Imports System.IO

Public Module IO
	Public Sub Type(Data As Object)
		If (Not TypeOf Data Is String) AndAlso TypeOf Data Is IEnumerable Then
			For Each item In Data
				Console.WriteLine(item)
			Next
		Else
			Console.WriteLine(Data)
		End If
	End Sub

	Public Function Read()
		Return Console.ReadLine()
	End Function

	Public Function FileContents(filepath As Object)
		Return File.ReadAllLines(Path.GetFullPath(filepath.ToString()))
	End Function
End Module