Imports System.IO
Imports Runtime.Shared

Public Module IO
	Public Function Type(ParamArray Data() As Object) As Object
		Dim output$ = FormatArray(Data)
		Console.WriteLine(output)
		Return output
	End Function

	Public Function Read() As Object
		Return Console.ReadLine()
	End Function

	Public Function Load(Filepath As Object) As Object
		Return File.ReadAllLines(Path.GetFullPath(Filepath.ToString())).ToList()
	End Function

	Public Function Save(ParamArray Data() As Object) As Object
		If Data.Length = 0 Then
			Return False
		ElseIf Data.Length = 1 Then
			File.Create(Data(0)).Close()
			Return False
		Else
			File.WriteAllText(Data(0), FormatArray(Data.Skip(1).ToArray()))
			Return True
		End If
	End Function
End Module