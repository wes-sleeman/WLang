Module Tokens
	Interface IToken
		ReadOnly Property Type As TokenType
		ReadOnly Property Value As String
	End Interface

	Structure Keyword
		Implements IToken
		Public ReadOnly Property Type As TokenType Implements IToken.Type
		Public ReadOnly Property Value As String Implements IToken.Value
			Get
				Return [Enum].GetName(GetType(TokenType), Type)
			End Get
		End Property

		Public Sub New(kw$)
			If Not [Enum].TryParse(kw, True, Type) Then Throw New ArgumentException($"Invalid keyword {kw}.")
		End Sub
	End Structure

	Structure [Boolean]
		Implements IToken
		Public ReadOnly Property Type As TokenType Implements IToken.Type
		Public ReadOnly Property Value As String Implements IToken.Value

		Public Sub New(bool$)
			Select Case bool.ToLower()
				Case "true"
					Value = "True"
				Case "false"
					Value = "False"
				Case Else
					Throw New ArgumentException($"Invalid boolean {bool}.")
			End Select
			Type = TokenType.Boolean
		End Sub
	End Structure

	Structure IntLiteral
		Implements IToken
		Public ReadOnly Property Type As TokenType Implements IToken.Type
		Public Property Value As String Implements IToken.Value

		Public Sub New(number$)
			Value = number
			Type = TokenType.IntLiteral
		End Sub
	End Structure

	Structure StringLiteral
		Implements IToken
		Public ReadOnly Property Type As TokenType Implements IToken.Type
		Public ReadOnly Property Value As String Implements IToken.Value

		Public Sub New(content$)
			Value = content
			Type = TokenType.StringLiteral
		End Sub
	End Structure

	Structure Symbol
		Implements IToken

		Public ReadOnly Property Type As TokenType Implements IToken.Type

		Public ReadOnly Property Value As String Implements IToken.Value

		Public Sub New(input As String)
			Value = input

			Select Case input
				Case "."
					Type = TokenType.Dot
				Case "?"
					Type = TokenType.QuestionMark
				Case "{"
					Type = TokenType.LeftCurly
				Case "}"
					Type = TokenType.RightCurly
				Case "["
					Type = TokenType.LeftSquare
				Case "]"
					Type = TokenType.RightSquare
				Case "("
					Type = TokenType.LeftParen
				Case ")"
					Type = TokenType.RightParen

				Case "+"
					Type = TokenType.Cross
				Case "-"
					Type = TokenType.Hyphen
				Case "*"
					Type = TokenType.Asterisk
				Case "/"
					Type = TokenType.Slash
				Case "\"
					Type = TokenType.BackSlash
				Case "%"
					Type = TokenType.Percent
				Case "="
					Type = TokenType.Equals
				Case "<"
					Type = TokenType.LeftAngle
				Case ">"
					Type = TokenType.RightAngle
				Case "<="
					Type = TokenType.LeftAngleEquals
				Case ">="
					Type = TokenType.RightAngleEquals

				Case "!"
					Type = TokenType.ExclamationPoint
				Case "@"
					Type = TokenType.AtSign
				Case "#"
					Type = TokenType.HashSign
				Case "$"
					Type = TokenType.Dollar
				Case "^"
					Type = TokenType.Caret
				Case "&"
					Type = TokenType.Ampersand
				Case "|"
					Type = TokenType.Pipe
				Case ","
					Type = TokenType.Comma

				Case Else
					Throw New ArgumentException($"{input} is not a valid symbol.")
			End Select
		End Sub
	End Structure

	Structure Variable
		Implements IToken
		Public ReadOnly Property Type As TokenType Implements IToken.Type
		Public ReadOnly Property Value As String Implements IToken.Value

		Public Sub New(name$)
			Value = name
			Type = TokenType.Variable
		End Sub
	End Structure

	Structure EOF
		Implements IToken
		Public ReadOnly Property Type As TokenType Implements IToken.Type
			Get
				Return TokenType.EOF
			End Get
		End Property

		Public ReadOnly Property Value As String Implements IToken.Value
			Get
				Return [Enum].GetName(GetType(TokenType), TokenType.EOF)
			End Get
		End Property
	End Structure


	Enum TokenType
		'Categories
		Variable
		IntLiteral
		StringLiteral

		'Comments and blocks
		QuestionMark
		LeftParen
		RightParen
		LeftSquare
		RightSquare
		LeftCurly
		RightCurly
		Dot

		'Operators
		Cross
		Hyphen
		Asterisk
		Slash
		BackSlash
		Percent
		[And]
		[Or]
		[Not]

		'Comparisons
		Equals
		LeftAngle
		RightAngle
		LeftAngleEquals
		RightAngleEquals

		'Other symbols
		ExclamationPoint
		AtSign
		HashSign
		Dollar
		Caret
		Ampersand
		Pipe
		Comma
		EOF

		'Control flow
		[If]
		Repeat
		[Boolean]
		Escape

		'Declaration
		Item
		Ref
		From
	End Enum
End Module