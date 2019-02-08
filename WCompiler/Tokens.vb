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
			Type = TokenType._IntLiteral
		End Sub
	End Structure

	Structure StringLiteral
		Implements IToken
		Public ReadOnly Property Type As TokenType Implements IToken.Type
		Public ReadOnly Property Value As String Implements IToken.Value

		Public Sub New(content$)
			Value = content
			Type = TokenType._StringLiteral
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
					Type = TokenType._Dot
				Case "?"
					Type = TokenType._QuestionMark
				Case "{"
					Type = TokenType._LeftCurly
				Case "}"
					Type = TokenType._RightCurly
				Case "["
					Type = TokenType._LeftSquare
				Case "]"
					Type = TokenType._RightSquare
				Case "("
					Type = TokenType._LeftParen
				Case ")"
					Type = TokenType._RightParen

				Case "+"
					Type = TokenType._Cross
				Case "-"
					Type = TokenType._Hyphen
				Case "*"
					Type = TokenType._Asterisk
				Case "/"
					Type = TokenType._Slash
				Case "\"
					Type = TokenType._BackSlash
				Case "%"
					Type = TokenType._Percent
				Case "="
					Type = TokenType._Equals
				Case "<"
					Type = TokenType._LeftAngle
				Case ">"
					Type = TokenType._RightAngle
				Case "<="
					Type = TokenType._LeftAngleEquals
				Case ">="
					Type = TokenType._RightAngleEquals

				Case "!"
					Type = TokenType._ExclamationPoint
				Case "@"
					Type = TokenType._AtSign
				Case "#"
					Type = TokenType._HashSign
				Case "$"
					Type = TokenType._Dollar
				Case "^"
					Type = TokenType._Caret
				Case "&"
					Type = TokenType._Ampersand
				Case "|"
					Type = TokenType._Pipe
				Case ","
					Type = TokenType._Comma

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
			Type = TokenType._Variable
		End Sub
	End Structure

	Structure EOF
		Implements IToken
		Public ReadOnly Property Type As TokenType Implements IToken.Type
			Get
				Return TokenType._EOF
			End Get
		End Property

		Public ReadOnly Property Value As String Implements IToken.Value
			Get
				Return [Enum].GetName(GetType(TokenType), TokenType._EOF)
			End Get
		End Property
	End Structure


	Enum TokenType
		'Categories
		_Variable
		_IntLiteral
		_StringLiteral

		'Comments and blocks
		_QuestionMark
		_LeftParen
		_RightParen
		_LeftSquare
		_RightSquare
		_LeftCurly
		_RightCurly
		_Dot

		'Operators
		_Cross
		_Hyphen
		_Asterisk
		_Slash
		_BackSlash
		_Percent
		[And]
		[Or]
		[Not]

		'Comparisons
		_Equals
		_LeftAngle
		_RightAngle
		_LeftAngleEquals
		_RightAngleEquals

		'Other symbols
		_ExclamationPoint
		_AtSign
		_HashSign
		_Dollar
		_Caret
		_Ampersand
		_Pipe
		_Comma
		_EOF

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