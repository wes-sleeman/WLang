Public Module SharedFunctions
	Public Function FormatArray(Data() As Object) As String
		Dim Sterilise = Function(inp$)
							Return inp.Replace("\\", "ʍbɐcĸßℓɐßɥ").Replace("\r\n", vbCrLf).Replace("\r", vbCr).Replace("\n", Environment.NewLine).Replace("\b", vbBack).Replace("\t", vbTab).Replace("ʍbɐcĸßℓɐßɥ", "\")
						End Function

		Dim retval$ = ""
		For Each item In Data
			If (Not TypeOf item Is String) AndAlso TypeOf item Is IEnumerable Then
				retval &= FormatArray(CType(item, IEnumerable(Of Object)).ToArray())
			Else
				retval &= Sterilise(item)
			End If
		Next
		Return retval
	End Function
End Module