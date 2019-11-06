Imports System.IO
Imports Runtime.Shared

Public Module IO
	Public Function CurDir()
		Return Environment.CurrentDirectory.Replace("\", "\\")
	End Function

	Public Function DirList(ParamArray Paths())
		If Paths.Length = 1 AndAlso TypeOf Paths(0) IsNot String AndAlso TypeOf Paths(0) Is IEnumerable(Of Object) Then Paths = Paths(0)

		If Paths.Length = 0 Then
			Return DirList(CurDir())
		ElseIf Paths.Length = 1 Then
			Try
				Return Directory.EnumerateFileSystemEntries(Path.GetFullPath(Paths(0).ToString())).Select(Function(s) s.Replace("\", "\\")).ToList()
			Catch
				Return New List(Of String)
			End Try
		Else
			Dim retval As New List(Of List(Of String))
			For Each Path In Paths
				Try
					retval.Add(Directory.EnumerateFileSystemEntries(Path.GetFullPath(Path.ToString())).Select(Function(s) s.Replace("\", "\\")))
				Catch
					retval.Add(New List(Of String))
				End Try
			Next
			Return retval
		End If
	End Function

	Public Function FileExists(ParamArray Paths())
		If Paths.Length = 1 AndAlso TypeOf Paths(0) IsNot String AndAlso TypeOf Paths(0) Is IEnumerable(Of Object) Then Paths = Paths(0)

		If Paths.Length = 0 Then
			Return New List(Of Boolean)
		ElseIf Paths.Length = 1 Then
			Try
				Return File.Exists(Path.GetFullPath(Paths(0).ToString()))
			Catch
				Return False
			End Try
		Else
			Dim retval As New List(Of Boolean)
			For Each Path In Paths
				Try
					retval.Add(File.Exists(Path.GetFullPath(Path.ToString())))
				Catch
					retval.Add(False)
				End Try
			Next
			Return retval
		End If
	End Function

	Public Function Read(ParamArray Paths())
		If Paths.Length = 1 AndAlso TypeOf Paths(0) IsNot String AndAlso TypeOf Paths(0) Is IEnumerable(Of Object) Then Paths = Paths(0)

		If Paths.Length = 0 Then
			Return Console.ReadLine()
		ElseIf Paths.Length = 1 Then
			Try
				Return File.ReadAllLines(Path.GetFullPath(Paths(0).ToString())).ToList()
			Catch
				Return New List(Of Object)
			End Try
		Else
			Dim retval As New List(Of Object)
			For Each Path In Paths
				Try
					retval.Add(File.ReadAllLines(Path.GetFullPath(Path.ToString())).ToList())
				Catch
					retval.Add(New List(Of Object))
				End Try
			Next
			Return retval
		End If
	End Function

	Public Function Delete(ParamArray Paths())
		If Paths.Length = 1 AndAlso TypeOf Paths(0) IsNot String AndAlso TypeOf Paths(0) Is IEnumerable(Of Object) Then Paths = Paths(0)

		If Paths.Length = 0 Then
			Return New List(Of Object)
		ElseIf Paths.Length = 1 Then
			Try
				File.Delete(Paths(0))
				Return True
			Catch
				Return False
			End Try
		Else
			Dim retval As New List(Of Object)
			For Each Path In Paths
				Try
					File.Delete(Path)
					retval.Add(True)
				Catch
					retval.Add(False)
				End Try
			Next
			Return retval
		End If
	End Function

	Public Sub Save(ParamArray Data())
		If Data.Length = 1 Then
			File.Create(Data(0)).Close()
		Else
			File.WriteAllText(Data(0), FormatArray(Data.Skip(1).ToArray()))
		End If
	End Sub

	Public Function Type(ParamArray Data())
		Dim output$ = FormatArray(Data)
		Console.WriteLine(output)
		Return output
	End Function
End Module