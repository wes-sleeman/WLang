Friend Class Lexer
	Public Property Current As IToken
	Dim _line% = 1
	Public Property Line As Integer
		Get
			Return _line
		End Get
		Private Set(value As Integer)
			_line = value
		End Set
	End Property

	Private ReadOnly Code As String
	Private Index As Integer = 0

	Sub New(Code$)
		Me.Code = Code
		Advance()
	End Sub

	Sub Advance()
		Try
			Current = TakeNext()
		Catch ex As ArgumentException
			Throw New ArgumentException($"Line {Line}: " & ex.Message)
		End Try
	End Sub

	Private Function TakeNext() As IToken
		If Index >= Code.Length Then Return New EOF

		Select Case Code(Index)
			Case "a"c To "z"c, "A"c To "Z"c, "_"
				Dim ident$ = TakeWhileLike("[a-zA-Z_1-9]")
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
				Dim num$ = TakeWhileLike("#")
				If Code(Index) = "."c AndAlso Code(Index + 1) Like "#" Then
					num &= "."
					Index += 1
					num &= TakeWhileLike("#")
				End If
				Return New IntLiteral(num)

			Case """"c
				Index += 1
				Dim retval As New StringLiteral(TakeWhileLike("[!""]"))
				Index += 1
				Return retval

			Case "'"c
				Index += 1
				Dim retval As New StringLiteral(TakeWhileLike("[!']"))
				Index += 1
				Return retval

			Case vbCr
				Index += 1
				Return TakeNext()
			Case vbLf, vbCrLf, Environment.NewLine
				Line += 1
				Index += 1
				Return TakeNext()

			Case vbTab, " "c
				TakeWhileIn(vbTab, " "c)
				Return TakeNext()

			Case "{"c
				TakeWhileLike("[!}]")
				Index += 1
				Return TakeNext()

			Case "?"c
				TakeWhileLike($"[!{Environment.NewLine}{vbLf}]")
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

	Private Function TakeWhileLike(pattern As String) As String
		Dim retval$ = ""
		Do
			retval &= Code(Index)
			Index += 1
			If Index >= Code.Length Then Exit Do
		Loop While Code(Index) Like pattern
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
End Class