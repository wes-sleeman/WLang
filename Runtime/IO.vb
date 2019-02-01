Imports System.IO

Public Module IO
	Public Sub Type(Data As Object)
		Console.WriteLine(Data)
	End Sub

	Public Function FileContents(filepath As Object)
		Return File.ReadAllLines(Path.GetFullPath(filepath.ToString()))
	End Function
End Module