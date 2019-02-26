Imports System.IO
Imports Runtime.Shared

Public Module IO
	Public Sub Type(ParamArray Data() As Object)
		Console.WriteLine(FormatArray(Data))
	End Sub

	Public Function Read() As Object
		Return Console.ReadLine()
	End Function

	Public Function Load(Filepath As Object) As Object
		Return File.ReadAllLines(Path.GetFullPath(Filepath.ToString())).ToList()
	End Function

	Public Sub Save(ParamArray Data() As Object)
		If Data.Length = 0 Then
			Return
		ElseIf Data.Length = 1 Then
			File.Create(Data(0)).Close()
		Else
			File.WriteAllText(Data(0), FormatArray(Data.Skip(1).ToArray()))
		End If
	End Sub
End Module