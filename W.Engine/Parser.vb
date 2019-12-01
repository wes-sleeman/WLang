Imports System.Reflection

Partial Public Class Engine

	Private Sub Assignment()
		Dim varname = Match(TokenType._Variable, False).ToLower()
		If Not Variable.ContainsKey(varname) Then Throw New MissingFieldException("No variable named " & varname)
		Lexer.Advance()
		If Lexer.Current.Type = TokenType._Dot Then
			CheckAssignmentProperty(varname)
		Else
			Match(TokenType._Equals)
			Expr()
			Variable(varname) = Register
		End If
	End Sub

	Private Sub Block(Optional InLoop As Boolean = False, Optional InCond As Boolean = False)
		Do Until Lexer.Current.Type = TokenType._EOF OrElse ((InLoop OrElse InCond OrElse InFunc) AndAlso Lexer.Current.Type = TokenType._RightSquare)
			Select Case Lexer.Current.Type
				Case TokenType.Escape
					Match(TokenType.Escape)
					Throw New EscapeException()

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
					Match(TokenType.Return)
					Try
						Expr()
						Throw New ReturnException(Register)
					Catch Ex As ReturnException
						'Propgate me!
						Throw Ex
					Catch
						Register = Nothing
						Throw New ReturnException(Register)
					End Try

				Case Else
					Match(TokenType._EOF)
			End Select
		Loop
	End Sub

	Private Sub Declaration()
		Match(TokenType.Item)
		Dim varname = Match(TokenType._Variable).ToLower()
		If Lexer.Current.Type = TokenType._Equals Then
			Match(TokenType._Equals)
			BooleanExpr()
			Variable(varname) = Register
		Else
			Variable(varname) = Nothing
		End If
	End Sub

	Private Sub ExecFunc(name$, args As Object)
		Dim lexCache = Lexer
		Lexer = New Lexer(Lexer)

		Stack.Push(New Dictionary(Of String, Object)(Variable))
		Variable.Clear()
		Variable("args") = args
		Lexer.Reset(Functions(name))
		Try
			Block()
		Catch ex As ReturnException
			Register = ex.ReturnValue
		Finally
			Variable = Stack.Pop()
		End Try

		Lexer = lexCache
	End Sub

	Private Sub Func()
		Match(TokenType.Func)
		Dim funcName$ = Match(TokenType._Variable)
		Dim inFuncCache As Boolean = InFunc
		InFunc = True

		If Lexer.Current.Type = TokenType._EOF Then
			GetBlock()
		End If
		Match(TokenType._LeftSquare)
		If Lexer.Current.Type = TokenType._EOF Then
			GetBlock()
		End If

		Dim code$ = Lexer.Code.Substring(Lexer.Code.IndexOf("["c) + 1)
		code = code.Substring(0, code.LastIndexOf("]"c)).Trim() & vbCrLf & "Return"

		Do Until Lexer.Current.Type = TokenType._EOF
			Match(Lexer.Current.Type)
		Loop
		InFunc = inFuncCache

		If Functions.ContainsKey(funcName.ToLower()) Then
			Functions(funcName.ToLower()) = code
		Else
			Functions.Add(funcName.ToLower(), code)
		End If
	End Sub

	Private Sub FunctionCall()
		Dim funcName$ = Match(TokenType._Variable)
		Try
			Match(TokenType._LeftParen)
		Catch ex As ArgumentException
			If Lexer.Current.Type = TokenType._Equals Then
				Throw New ArgumentException($"Assignment to undeclared variable '{funcName}'.")
			End If
		End Try
		Stack.Push(FuncArgs.ToList())
		FuncArgs.Clear()
		Do
			Select Case Lexer.Current.Type
				Case TokenType._RightParen
					Exit Do

				Case TokenType._Comma
					Match(TokenType._Comma)

				Case Else
					BooleanExpr()
					FuncArgs.Add(If(TypeOf Register Is List(Of Object), Register.ToArray(), Register))
			End Select
		Loop
		Match(TokenType._RightParen)
		If funcName.ToLower() = "defined" Then
			Register = Defined(FuncArgs)
			FuncArgs = Stack.Pop()
		ElseIf Functions.ContainsKey(funcName.ToLower()) Then
			ExecFunc(funcName.ToLower(), New List(Of Object)(FuncArgs))
			FuncArgs = Stack.Pop()
		Else
			Dim ArgArr = FuncArgs.ToArray()
			For Each type In Types
				Try
					Try
						Register = type.GetMethods().Where(Function(mi) mi.Name.ToLower() = funcName.ToLower()).FirstOrDefault().Invoke(Nothing, ArgArr)
						FuncArgs = Stack.Pop()
						Return
					Catch e As Exception When TypeOf e Is TargetParameterCountException OrElse TypeOf e Is ArgumentException
						Register = type.GetMethods().Where(Function(mi) mi.Name.ToLower() = funcName.ToLower()).FirstOrDefault().Invoke(Nothing, {ArgArr})
						FuncArgs = Stack.Pop()
						Return
					Catch e As AmbiguousMatchException
						Register = type.GetMethod(funcName, FuncArgs.Select(Function(obj) If(TypeOf obj Is IEnumerable(Of Object), GetType(Object()), GetType(Object))).ToArray()).Invoke(Nothing, ArgArr)
						FuncArgs = Stack.Pop()
						Return
					End Try
				Catch ex As TypeLoadException : Catch ex As NullReferenceException
				Catch ex As TargetInvocationException
					If TypeOf ex.InnerException Is InvalidCastException Then
						Throw New Exception("Invalid operation on data.")
					Else
						Throw ex.InnerException
					End If
				End Try
			Next
			Throw New MissingMethodException("Unable To find method " & funcName & ". Did you forget a reference?")
		End If
	End Sub

	Private Sub [If]()
		Match(TokenType.If)
		BooleanExpr()

		If Lexer.Current.Type = TokenType._EOF Then
			GetBlock()
		End If
		Match(TokenType._LeftSquare)
		If Lexer.Current.Type = TokenType._EOF Then
			GetBlock()
		End If

		Dim AdvanceLoop As Action = Sub()
										Do Until Lexer.Current.Type = TokenType._RightSquare OrElse Lexer.Current.Type = TokenType._EOF
											If Lexer.Current.Type = TokenType._LeftSquare Then
												Lexer.Advance()
												AdvanceLoop()
											End If
											Lexer.Advance()
										Loop
									End Sub
		Dim bool = Register
		If bool Then
			Block(InCond:=True)
		Else
			AdvanceLoop()
		End If
		Match(TokenType._RightSquare)
		If Lexer.Current.Type = TokenType._LeftSquare Then
			Match(TokenType._LeftSquare)
			If Not bool Then
				Block(InCond:=True)
			Else
				AdvanceLoop()
			End If
			Match(TokenType._RightSquare)
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

	Private Sub [Loop]()
		Match(TokenType.Repeat)
		Dim inf As Boolean = True
		If Lexer.Current.Type <> TokenType._LeftSquare AndAlso Lexer.Current.Type <> TokenType._EOF Then
			Expr()
			inf = False
		End If

		If Lexer.Current.Type = TokenType._EOF Then
			GetBlock()
		End If

		Dim lexerCache = Lexer.Index
		Match(TokenType._LeftSquare)

		If Lexer.Current.Type = TokenType._EOF Then
			GetBlock()
		End If

		Stack.Push(Register)
		Stack.Push(LoopEnd)
		If inf Then LoopEnd = Integer.MaxValue Else LoopEnd = Register
		Stack.Push(Counter) : Counter = 0
		Try
			Do While Counter < LoopEnd
				Lexer.Index = lexerCache
				Lexer.Advance()
				Block(InLoop:=True)
				Counter += 1
			Loop
		Catch esc As EscapeException
			Lexer.Index = lexerCache
			Dim depth = 1
			Do Until depth = 0 AndAlso Lexer.Current.Type = TokenType._RightSquare
				Lexer.Advance()
				If Lexer.Current.Type = TokenType._LeftSquare Then depth += 1
				If Lexer.Current.Type = TokenType._RightSquare Then depth -= 1
			Loop
		End Try

		Match(TokenType._RightSquare)
		Counter = Stack.Pop()
		LoopEnd = Stack.Pop()
		Register = Stack.Pop()
	End Sub
End Class