Partial Module Parser
	Public References As String = String.Empty

	Private Property Lexer As Lexer
	Private outputbuffer As List(Of String)
	Private Filename As String

	Function Parse(Lexer As Lexer, filename As String) As String()
		outputbuffer = New List(Of String)
		Parser.Lexer = Lexer
		Parser.Filename = filename
		Program()
		Return outputbuffer.ToArray
	End Function

	Private Sub Program()
		Setup()
		Block()
		Teardown()
	End Sub

	Private Sub Block(Optional InLoop As Boolean = False, Optional InCond As Boolean = False)
		If InLoop OrElse InCond Then IndentLevel += 1
		Do Until Lexer.Current.Type = TokenType.EOF OrElse ((InLoop OrElse InCond) AndAlso Lexer.Current.Type = TokenType.RightSquare)
			Select Case Lexer.Current.Type
				Case TokenType.Escape
					Break()

				Case TokenType.If
					[If]()

				Case TokenType.Repeat
					[Loop]()

				Case TokenType.Item
					Declaration()

				Case TokenType.Variable
					Try
						Assignment()
					Catch ex As MissingFieldException
						FunctionCall()
					End Try

				Case TokenType.Ref
					Import()
			End Select
		Loop
		If InLoop OrElse InCond Then IndentLevel -= 1
	End Sub

	Private Sub [If]()
		Match(TokenType.If)
		BooleanExpr()
		Match(TokenType.LeftSquare)
		Emit("If Register Then")
		Block(InCond:=True)
		Match(TokenType.RightSquare)
		If Lexer.Current.Type = TokenType.LeftSquare Then
			Match(TokenType.LeftSquare)
			Emit("Else")
			Block(InCond:=True)
			Match(TokenType.RightSquare)
		End If
		Emit("End If")
	End Sub

	Private Sub [Loop]()
		Match(TokenType.Repeat)
		Dim inf As Boolean = True
		If Lexer.Current.Type <> TokenType.LeftSquare Then
			Expr()
			inf = False
		End If
		Match(TokenType.LeftSquare)
		Emit("Stack.Push(Register)")
		Emit("Stack.Push(LoopEnd) : LoopEnd = Register")
		Emit("Stack.Push(Counter) : Counter = 0")
		Emit(If(inf, "Do", $"Do While Counter < LoopEnd"))
		Block(InLoop:=True)
		Match(TokenType.RightSquare)
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
		Dim varname = Match(TokenType.Variable).ToLower()
		If Lexer.Current.Type = TokenType.Equals Then
			Match(TokenType.Equals)
			Expr()
			Emit($"Variable(""{varname}"") = Register")
		Else
			Emit($"Variable(""{varname}"") = Nothing")
		End If
		Varlist.Add(varname)
	End Sub

	Private Sub Assignment()
		Dim varname = Match(TokenType.Variable, False).ToLower()
		If Not Varlist.Contains(varname) Then Throw New MissingFieldException("No variable named " & varname)
		Lexer.Advance()
		Match(TokenType.Equals)
		Expr()
		Emit($"Variable(""{varname}"") = Register")
	End Sub

	Private Sub Import()
		Match(TokenType.Ref)
		Dim filenames As New List(Of String) From {Match(TokenType.StringLiteral)}
		While Lexer.Current.Type = TokenType.Comma
			Match(TokenType.Comma)
			filenames.Add(Match(TokenType.StringLiteral))
		End While
		If Lexer.Current.Type = TokenType.From Then
			Match(TokenType.From)
			Dim path$ = Match(TokenType.StringLiteral)
			For Each type In filenames
				AddLib(path, type)
			Next
		Else
			For Each Filename In filenames
				AddLib(Filename)
			Next
		End If
	End Sub

	Private Sub FunctionCall()
		Dim funcName$ = Match(TokenType.Variable)
		Match(TokenType.LeftParen)
		Emit("Stack.Push(FuncArgs.ToList())")
		Dim args As New List(Of String)
		Do
			Select Case Lexer.Current.Type
				Case TokenType.RightParen
					Exit Do

				Case Else
					Expr()
					Emit("FuncArgs.Add(Register)")
			End Select
		Loop
		Match(TokenType.RightParen)
		Emit($"Register = If(InvokeMethod(""{funcName}"", FuncArgs), Register)")
		Emit("FuncArgs = Stack.Pop()")
	End Sub
End Module