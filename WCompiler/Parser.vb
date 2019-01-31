Partial Module Parser
	Private Property Lexer As Lexer
	Private outputbuffer As List(Of String)

	Function Parse(Lexer As Lexer) As String()
		outputbuffer = New List(Of String)()
		Parser.Lexer = Lexer
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
				Case TokenType.If
					[If]()

				Case TokenType.Repeat
					[Loop]()

				Case TokenType.Item
					Declaration()

				Case TokenType.Variable
					Assignment()

				Case Else
					Statement()
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
		Emit("Stack.Push(Counter) : Counter = 1")
		Emit(If(inf, "Do", $"Do While Counter <= LoopEnd"))
		Block(InLoop:=True)
		Match(TokenType.RightSquare)
		Emit(vbTab & "Counter += 1")
		Emit("Loop")
		Emit("Counter = Stack.Pop()")
		Emit("LoopEnd = Stack.Pop()")
		Emit("Register = Stack.Pop()")
	End Sub

	Private Sub Declaration()
		Match(TokenType.Item)
		Dim varname = Match(TokenType.Variable)
		If Lexer.Current.Type = TokenType.Equals Then
			Match(TokenType.Equals)
			Expr()
			Emit($"Variable(""{varname}"") = Register")
		Else
			Emit($"Variable(""{varname}"") = Nothing")
		End If
	End Sub

	Private Sub Assignment()
		Dim varname = Match(TokenType.Variable)
		Match(TokenType.Equals)
		Expr()
		Emit($"Variable(""{varname}"") = Register")
	End Sub

	Private Sub Statement()
		Dim token = Lexer.Current
		Match(token.Type)
		Select Case token.Type
			Case TokenType.Type
				Expr()
				Emit("Print(Register)")
			Case Else
				Throw New ArgumentException($"Invalid statement {Lexer.Current.Value}.")
		End Select
	End Sub
End Module