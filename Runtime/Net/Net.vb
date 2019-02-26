Imports System.Net
Imports Runtime.Shared

Public Module Net
	Public Function Wget(Url As Object) As Object
		Using cli As New WebClient()
			Try
				Return cli.DownloadString(Url.ToString())
			Catch
				Return cli.DownloadString("http://" & Url.ToString())
			End Try
		End Using
	End Function

	Public Sub Wpost(Data() As Object)
		Using cli As New WebClient()
			Try
				cli.UploadString(Data(0), FormatArray(Data.Skip(1).ToArray()))
			Catch
				cli.UploadString("http://" & Data(0), FormatArray(Data.Skip(1).ToArray()))
			End Try
		End Using
	End Sub
End Module