Imports System.IO

Public Module IO
    Public Sub Type(ParamArray Data() As Object)
        TypeLocal(Data)
        Console.WriteLine()
    End Sub

    Private Sub TypeLocal(Data() As Object)
        Dim Sterilise = Function(inp$)
                            Return inp.Replace("\\", "ʍbɐcĸßℓɐßɥ").Replace("\r\n", vbCrLf).Replace("\r", vbCr).Replace("\n", Environment.NewLine).Replace("\b", vbBack).Replace("\t", vbTab).Replace("ʍbɐcĸßℓɐßɥ", "\")
                        End Function

        For Each item In Data
            If (Not TypeOf item Is String) AndAlso TypeOf item Is IEnumerable Then
                TypeLocal(CType(item, IEnumerable(Of Object)).ToArray())
            Else
                Console.Write(Sterilise(item))
            End If
        Next
    End Sub

    Public Function Read() As Object
		Return Console.ReadLine()
	End Function

	Public Function Read(Filepath As Object) As Object
		Return File.ReadAllLines(Path.GetFullPath(Filepath.ToString())).ToList()
	End Function
End Module