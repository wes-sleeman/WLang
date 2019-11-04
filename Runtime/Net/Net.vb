Imports System.Net
Imports Runtime.Shared

Public Module Net
	Public Function Wget(Url As Object) As Object
		Using cli As New WebClient()
			Try
                Return cli.DownloadString(Url.ToString()).Replace(vbCrLf, vbLf).Replace(vbLf, Environment.NewLine)
            Catch
                Return cli.DownloadString("http://" & Url.ToString()).Replace(vbCrLf, vbLf).Replace(vbLf, Environment.NewLine)
            End Try
		End Using
	End Function

    Public Function Wpost(Data() As Object) As Object
        Using cli As New WebClient()
            Try
                Return cli.UploadString(Data(0), FormatArray(Data.Skip(1).ToArray()))
            Catch
                Return cli.UploadString("http://" & Data(0), FormatArray(Data.Skip(1).ToArray()))
            End Try
        End Using
    End Function
End Module