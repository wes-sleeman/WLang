Partial Module Parser
	Public References As String = String.Empty

	Private Property Lexer As Lexer
	Private outputbuffer As List(Of String)
	Private functionBuffer As List(Of String)
	Private Filename As String
	Private [Lib] As Boolean
	Private Debug As Boolean

	Function Parse(Lexer As Lexer, filename As String, isLib As Boolean, debugBuild As Boolean) As String()
		References = String.Empty
		outputbuffer = New List(Of String)
		functionBuffer = New List(Of String)
		Parser.Lexer = Lexer

		While filename.Contains(".") OrElse filename(0) Like "#"
			filename = filename.Trim()
			If filename.Contains("."c) Then filename = filename.Substring(0, filename.IndexOf("."c))
			If filename(0) Like "#" Then filename = filename.Substring(1)
		End While

		Parser.Filename = filename
		[Lib] = isLib
		Debug = debugBuild
		Program()
		Return outputbuffer.ToArray
	End Function

	Private Sub Program()
		Setup()
		Block()
		Teardown()
	End Sub

	Private Sub Block(Optional InLoop As Boolean = False, Optional InCond As Boolean = False)
		IndentLevel += 1
		If Not InFunc Then Emit("LineNumber = " & Lexer.Line)
		Do Until Lexer.Current.Type = TokenType._EOF OrElse ((InLoop OrElse InCond OrElse InFunc) AndAlso Lexer.Current.Type = TokenType._RightSquare)
			Select Case Lexer.Current.Type
				Case TokenType.Escape
					Break()

				Case TokenType.If
					[If]()

				Case TokenType.Repeat
					[Loop]()

				Case TokenType.Item
					Declaration()

				Case TokenType._Variable
					Try
						Assignment()
					Catch ex As MissingFieldException
						FunctionCall()
					End Try

				Case TokenType.Ref
					Import()

				Case TokenType.Func, TokenType.Public
					Func()

				Case TokenType.Return
					[Return]()

				Case Else
					Match(TokenType._EOF)
			End Select
		Loop
		IndentLevel -= 1
	End Sub

	Private Sub [If]()
		Match(TokenType.If)
		BooleanExpr()
		Match(TokenType._LeftSquare)
		Emit("If Register Then")
		Block(InCond:=True)
		Match(TokenType._RightSquare)
		If Lexer.Current.Type = TokenType._LeftSquare Then
			Match(TokenType._LeftSquare)
			Emit("Else")
			Block(InCond:=True)
			Match(TokenType._RightSquare)
		End If
		Emit("End If")
	End Sub

	Private Sub [Loop]()
		Match(TokenType.Repeat)
		Dim inf As Boolean = True
		If Lexer.Current.Type <> TokenType._LeftSquare Then
			Expr()
			inf = False
		End If
		Match(TokenType._LeftSquare)
		Emit("Stack.Push(Register)")
		Emit($"Stack.Push(LoopEnd){ If(inf, "", ": LoopEnd = Register")}")
		Emit("Stack.Push(Counter) : Counter = 0")
		Emit(If(inf, "Do", $"Do While Counter < LoopEnd"))
		Block(InLoop:=True)
		Match(TokenType._RightSquare)
		Emit(vbTab & "Counter += 1")
		Emit("Loop")
		Emit("Counter = Stack.Pop()")
		Emit("LoopEnd = Stack.Pop()")
		Emit("Register = Stack.Pop()")
	End Sub

	Private Sub Break()
		Match(TokenType.Escape)
		Emit("Exit Do")
	End Sub

	Private ReadOnly Varlist As New List(Of String) From {"args"}
	Private Sub Declaration()
		Match(TokenType.Item)
		Dim varname = Match(TokenType._Variable).ToLower()
		If Lexer.Current.Type = TokenType._Equals Then
			Match(TokenType._Equals)
			Expr()
			Emit($"Variable(""{varname}"") = Register")
		Else
			Emit($"Variable(""{varname}"") = Nothing")
		End If
		Varlist.Add(varname)
	End Sub

	Private Sub Assignment()
		Dim varname = Match(TokenType._Variable, False).ToLower()
		If Not Varlist.Contains(varname) Then Throw New MissingFieldException("No variable named " & varname)
		Lexer.Advance()
		If Lexer.Current.Type = TokenType._Dot Then
			CheckProperty(True, varname)
		Else
			Match(TokenType._Equals)
			Expr()
			Emit($"Variable(""{varname}"") = Register")
		End If
	End Sub

	Private Sub Import()
		Match(TokenType.Ref)
		Dim filenames As New List(Of String) From {Match(TokenType._StringLiteral)}
		While Lexer.Current.Type = TokenType._Comma
			Match(TokenType._Comma)
			filenames.Add(Match(TokenType._StringLiteral))
		End While
		If Lexer.Current.Type = TokenType.From Then
			Match(TokenType.From)
			Dim path$ = Match(TokenType._StringLiteral)
			For Each type In filenames
				AddLib(path, type)
			Next
		Else
			For Each Filename In filenames
				AddLib(Filename)
			Next
		End If
	End Sub

	Private Sub Func()
		Dim callable = False
		If Lexer.Current.Type = TokenType.Public Then
			Match(TokenType.Public)
			callable = True
		End If
		Match(TokenType.Func)
		Dim funcName$ = Match(TokenType._Variable)
		Dim inFuncCache As Boolean = InFunc, indentCache% = IndentLevel
		InFunc = True
		IndentLevel = 1
		Emit($"{If(callable, "Public", "Friend")} Function {funcName}(ParamArray args() As Object) As Object")
		Match(TokenType._LeftSquare)
		IndentLevel = 2
		Emit("Stack.Push(New Dictionary(Of String, Object)(Variable)) : Variable.Clear() : Variable(""args"") = args")
		Emit("Try")
		Block()
		Emit("Finally : Variable = Stack.Pop() : End Try")
		Match(TokenType._RightSquare)
		IndentLevel = 1
		Emit($"End Function")
		InFunc = inFuncCache
		IndentLevel = indentCache
	End Sub

	Private Sub [Return]()
		Match(TokenType.Return)
		Try
			Expr()
			Emit("Return Register")
		Catch
			If InFunc OrElse [Lib] Then
				Emit("Return Nothing")
			Else
				Emit("Return")
			End If
		End Try
	End Sub

	Private Sub FunctionCall()
		Dim funcName$ = Match(TokenType._Variable)
		Try
			Match(TokenType._LeftParen)
		Catch ex As ArgumentException
			If Lexer.Current.Type = TokenType._Equals Then
				Throw New ArgumentException($"Assignment to undeclared variable '{funcName}' on line {Lexer.Line}.")
			End If
		End Try
		Emit("Stack.Push(FuncArgs.ToList()) : FuncArgs.Clear()")
		Dim args As New List(Of String)
		Do
			Select Case Lexer.Current.Type
				Case TokenType._RightParen
					Exit Do

				Case TokenType._Comma
					Match(TokenType._Comma)

				Case Else
					Expr()
					Emit("FuncArgs.Add(If(TypeOf Register Is List(Of Object), Register.ToArray(), Register))")
			End Select
		Loop
		Match(TokenType._RightParen)
		Emit($"Register = InvokeMethod(""{funcName}"", FuncArgs)")
		Emit("FuncArgs = Stack.Pop()")
	End Sub
End Module