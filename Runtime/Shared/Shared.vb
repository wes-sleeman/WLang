Public Module SharedFunctions
    Public Function FormatArray(ParamArray Data() As Object) As String
        Dim retval$ = ""
        For Each item In Data
            If (Not TypeOf item Is String) AndAlso TypeOf item Is IEnumerable Then
                retval &= FormatArray(CType(item, IEnumerable(Of Object)).ToArray())
            Else
                retval &= Unescape(item)
            End If
        Next
        Return retval
    End Function

    Public Function Unescape(input$)
        Return input.Replace("\\", "ʍbɐcĸßℓɐßɥ").Replace("\r\n", vbCrLf).Replace("\r", vbCr).Replace("\n", Environment.NewLine).Replace("\b", vbBack).Replace("\t", vbTab).Replace("ʍbɐcĸßℓɐßɥ", "\")
    End Function
End Module