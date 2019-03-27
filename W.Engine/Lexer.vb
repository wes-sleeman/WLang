Friend Class Lexer
	Public Property Current As IToken

	Private Code As String = String.Empty
	Public Property Index As Integer = 0

	Public Sub New() : End Sub

	Public Sub New(Code$)
		Reset(Code)
	End Sub

	Public Sub New(other As Lexer)
		other.Copy(onto:=Me)
	End Sub

	Public Function Copy(Optional onto As Lexer = Nothing) As Lexer
		If onto Is Nothing Then onto = New Lexer()
		onto.Reset(Code)
		Do Until onto.Index = Index : onto.Advance() : Loop
		Return onto
	End Function

	Public Sub Reset(Code$)
		Me.Code = Code
		Index = 0
		Advance()
	End Sub

	Public Sub Feed(Code$)
		Me.Code &= Environment.NewLine & Code
		If Current.Type = TokenType._EOF Then Advance()
	End Sub

	Public Sub Advance()
		Current = TakeNext()
	End Sub

	Private Function TakeNext() As IToken
		If Index >= Code.Length Then Return New EOF

		Select Case Code(Index)
			Case "a"c To "z"c, "A"c To "Z"c
				Dim ident$ = ""
				Do
					ident &= Code(Index)
					Index += 1
					If Index >= Code.Length Then Exit Do
				Loop While Char.IsLetterOrDigit(Code(Index)) OrElse Code(Index) = "_"
				Try
					Return New [Boolean](ident)
				Catch ex As ArgumentException
					Try
						Return New Keyword(ident)
					Catch ex2 As ArgumentException
						Return New Variable(ident)
					End Try
				End Try

			Case "0"c To "9"c
				Dim num$ = TakeWhileNumeric()
				If Index < Code.Length - 1 AndAlso Code(Index) = "."c AndAlso IsNumeric(Code(Index + 1)) Then
					num &= "."
					Index += 1
					num &= TakeWhileNumeric()
				End If
				Return New IntLiteral(num)

			Case """"c
				Index += 1
				Dim retval As New StringLiteral(TakeWhileNotIn(""""c))
				Index += 1
				Return retval

			Case "'"c
				Index += 1
				Dim retval As New StringLiteral(TakeWhileNotIn("'"c))
				Index += 1
				Return retval

			Case vbCr, vbLf, vbCrLf, Environment.NewLine
				Index += 1
				Return TakeNext()

			Case vbTab, " "c
				TakeWhileIn(vbTab, " "c)
				Return TakeNext()

			Case "{"c
				TakeWhileNotIn("}"c)
				Index += 1
				Return TakeNext()

			Case "?"c
				TakeWhileNotIn(Environment.NewLine, vbLf)
				Return TakeNext()

			Case "<"c, ">"c
				If Index <= Code.Length AndAlso Code(Index + 1) = "="c Then
					Index += 2
					Return New Symbol(Code(Index - 2) & "=")
				ElseIf Index <= Code.Length AndAlso Code(Index) = "<"c AndAlso Code(Index + 1) = "["c Then
					Index += 2
					Return New Symbol("<[")
				Else
					Index += 1
					Return New Symbol(Code(Index - 1))
				End If

			Case "]"c, "="c
				Index += 1
				If Index < Code.Length AndAlso Code(Index) = ">" Then
					Index += 1
					Return New Symbol(Code(Index - 2) & ">")
				Else
					Return New Symbol(Code(Index - 1))
				End If

			Case Else
				Index += 1
				Return New Symbol(Code(Index - 1))
		End Select
	End Function

	Private Function TakeWhileNumeric() As String
		Dim retval$ = ""
		Do
			retval &= Code(Index)
			Index += 1
			If Index >= Code.Length Then Exit Do
		Loop While IsNumeric(Code(Index))
		Return retval
	End Function

	Private Function TakeWhileIn(ParamArray chars() As Char) As String
		Dim retval$ = ""
		Do
			retval &= Code(Index)
			Index += 1
			If Index >= Code.Length Then Exit Do
		Loop While chars.Contains(Code(Index))
		Return retval
	End Function

	Private Function TakeWhileNotIn(ParamArray chars() As Char) As String
		Dim retval$ = ""
		Do
			retval &= Code(Index)
			Index += 1
			If Index >= Code.Length Then Exit Do
		Loop Until chars.Contains(Code(Index))
		Return retval
	End Function
End Class