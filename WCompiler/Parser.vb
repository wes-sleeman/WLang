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
		Do Until Lexer.Current.Type = TokenType.EOF
			Select Case Lexer.Current.Type
				Case TokenType.If
					[If]()

				Case TokenType.Repeat
					[Loop]()

				Case TokenType.Item
					Declaration()

				Case TokenType.Variable
					Assignment()
			End Select
		Loop
	End Sub

	Private Sub Assignment()
		Dim varname = Match(TokenType.Variable)
		Match(TokenType.Equals)
		Expr()
		Emit($"Variable(""{varname}"") = Register")
	End Sub
End Module