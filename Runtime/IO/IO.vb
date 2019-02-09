Imports System.IO

Public Module IO
	Public Sub Type(ParamArray Data() As Object)
		Dim Sterilise = Function(inp$)
							Return inp.Replace("\\", "ʍbɐcĸßℓɐßɥ").Replace("\r\n", vbCrLf).Replace("\r", vbCr).Replace("\n", Environment.NewLine).Replace("\b", vbBack).Replace("\t", vbTab).Replace("ʍbɐcĸßℓɐßɥ", "\")
						End Function

		For Each item In Data
			If (Not TypeOf item Is String) AndAlso TypeOf item Is IEnumerable Then
				For Each index In item
					Console.Write(Sterilise(index))
				Next
			Else
				Console.Write(Sterilise(item))
			End If
		Next
		Console.WriteLine()
	End Sub

	Public Function Read()
		Return Console.ReadLine()
	End Function

	Public Function FileContents(Filepath As Object)
		Return File.ReadAllLines(Path.GetFullPath(Filepath.ToString()))
	End Function
End Module