Partial Public Class Engine
	Private Sub BooleanExpr(Optional recursionDepth = 0, Optional recursionLimit = 25)
		If Lexer.Current.Type = TokenType._Boolean Then
			[Boolean]()
		ElseIf Lexer.Current.Type = TokenType.Not Then
			Match(TokenType.Not)
			BooleanExpr()
			Register = Not Register
		Else
			Try
				Expr()
			Catch ex As ArgumentException When recursionDepth < recursionLimit
				BooleanExpr(recursionDepth + 1, recursionLimit)
				Push()
				Dim op = Lexer.Current.Type
				Match(op)
				BooleanExpr(recursionDepth + 1, recursionLimit)
				Pop(op)
			Catch ex As ArgumentException When recursionDepth >= recursionLimit
				Throw New StackOverflowException("Invalid expression")
			End Try
		End If
	End Sub

	Private Sub CompExpr(inProp As Boolean)
		MathExpr(inProp)

		While {">", "<", ">=", "<=", "="}.Contains(Lexer.Current.Value)
			Push()
			Dim op = Lexer.Current.Type
			Match(op)
			MathExpr(inProp)
			Pop(op)
		End While
	End Sub

	Private Sub Expr(Optional inProp As Boolean = False)
		CompExpr(inProp)

		While {"|", "&"}.Contains(Lexer.Current.Value) OrElse Lexer.Current.Type = TokenType.And OrElse Lexer.Current.Type = TokenType.Or
			Push()
			Dim op = Lexer.Current.Type
			Match(op)
			CompExpr(inProp)
			Pop(op)
		End While
	End Sub

	Private Sub Factor(inProp As Boolean)
		Select Case Lexer.Current.Type
			Case TokenType._IntLiteral
				Register = Double.Parse(Match(TokenType._IntLiteral))

			Case TokenType._StringLiteral
				Register = Match(TokenType._StringLiteral)

			Case TokenType._Variable
				Dim name$ = Match(TokenType._Variable, False)
                If Variable.ContainsKey(name.ToLower) Then
                    Lexer.Advance()
                    Register = Variable(name.ToLower())
                Else
                    FunctionCall()
				End If

			Case TokenType.Not, TokenType._Boolean
				BooleanExpr()

			Case TokenType._Dollar
				Match(TokenType._Dollar)
				Register = ProjectionIterator

			Case TokenType._HashSign
				Match(TokenType._HashSign)
				Register = Counter

			Case TokenType._LeftAngleSquare
				Projection()

			Case Else
				Match(TokenType._LeftParen)
				Expr()
				Match(TokenType._RightParen)
		End Select

		If Not inProp AndAlso Lexer.Current.Type = TokenType._Dot Then
			CheckProperty()
		End If
	End Sub

	Private Sub MathExpr(inProp As Boolean)
		Term(inProp)

		While {"+", "-"}.Contains(Lexer.Current.Value)
			Push()
			Dim op = Lexer.Current.Type
			Match(op)
			Term(inProp)
			Pop(op)
		End While
	End Sub

	Private Sub SignedFactor(inProp As Boolean)
		Dim op = TokenType._Cross
		If {"+", "-"}.Contains(Lexer.Current.Value) Then
			op = Lexer.Current.Type
			Match(op)
		End If

		Factor(inProp)

		If op = TokenType._Hyphen Then
			Register = -Register
		End If
	End Sub

	Private Sub Term(inProp As Boolean)
		SignedFactor(inProp)

		While {"*", "/", "%", "\"}.Contains(Lexer.Current.Value)
			Push()
			Dim op = Lexer.Current.Type
			Match(op)
			SignedFactor(inProp)
			Pop(op)
		End While
	End Sub
End Class