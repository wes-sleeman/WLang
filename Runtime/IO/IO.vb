Imports System.IO

Public Module IO
	Public Sub Type(ParamArray Data() As Object)
		For Each item In Data
			If (Not TypeOf item Is String) AndAlso TypeOf item Is IEnumerable Then
				For Each index In item
					Console.Write(index)
				Next
			Else
				Console.Write(item)
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