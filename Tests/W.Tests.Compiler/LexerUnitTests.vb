Imports W.Compiler
Imports Xunit

Namespace W.Tests.Compiler
	Public Class LexerUnitTests
		Private Lex As New Lexer()

		<Fact>
		Sub TestConfiguration()
			Lex = New Lexer()

			Assert.Equal(String.Empty, Lex.Code)
			Assert.Equal(TokenType._EOF, Lex.Current.Type)
			Assert.Equal(0, Lex.Depth)
		End Sub

		<Theory>
		<InlineData("", 0, 0)>
		<InlineData("[]", 1, 0)>
		<InlineData("[", 1, 1)>
		<InlineData("[[[]]", 3, 1)>
		<InlineData("[this[is]some[[us]e[[les]]]s info[]]", 4, 0)>
		Sub TestDepth(code$, anticipatedMaxDepth%, anticipatedEndingDepth%)
			Lex.Reset(code)
			Dim maxDepth% = lex.Depth

			Do Until lex.Current.Type = TokenType._EOF
				lex.Advance()
				If lex.Depth > maxDepth Then maxDepth = lex.Depth
			Loop

			Assert.Equal(anticipatedMaxDepth, maxDepth)
			Assert.Equal(anticipatedEndingDepth, Lex.Depth)
		End Sub

		<Theory>
		<InlineData("", 1)>
		<InlineData("This
		Is
		A
		Test", 4)>
		<InlineData("This" & vbCr & vbCr & "Mi
		ght" & vbCr & "
		mess with" & vbLf & "things", 4)>
		Sub TestLineNumbering(code$, anticipatedLines%)
			Lex.Reset(code)
			Do Until Lex.Current.Type = TokenType._EOF
				Lex.Advance()
			Loop
			Assert.Equal(anticipatedLines%, Lex.Line)
		End Sub

		<Theory>
		<InlineData("{This should be treated as a [ { single ] comment}", 0)>
		<InlineData("{comments don't count} plus three ""symbols""", 3)>
		<InlineData("""A string is one, and so is a number"" 225", 2)>
		Sub TestNumTokens(code$, anticipatedLength%)
			Lex.Reset(code)

			For cntr% = 1 To anticipatedLength
				Assert.NotEqual(TokenType._EOF, Lex.Current.Type)
				Lex.Advance()
			Next
			Assert.Equal(TokenType._EOF, Lex.Current.Type)
		End Sub
	End Class
End Namespace