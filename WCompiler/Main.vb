Imports System
Imports System.IO

Module Main
	Sub Main(args As String())
		Dim VBCPATH$ = GetVBCPath()

		Console.WriteLine("Reading input file(s).")
		For Each filename In args
			If filename.StartsWith("/") Then Continue For
			Try
				Console.WriteLine($"Building lexer for file <{filename}>.")
				Dim lex As New Lexer(File.ReadAllText(filename))

				Console.WriteLine("Parsing")
				Dim code() = Parse(lex)

				Dim emitpath$ = Path.ChangeExtension(filename, ".vb")
				Console.WriteLine("Emitting.")
				File.WriteAllLines(emitpath, code)

				Dim outpath$ = Path.ChangeExtension(filename, ".exe")
				Console.WriteLine($"Compiling to {outpath}.")
				Dim startInfo As New ProcessStartInfo With {.FileName = VBCPATH, .CreateNoWindow = True, .UseShellExecute = False, .RedirectStandardOutput = True, .Arguments = $"-out:""{outpath}"" -nologo " & emitpath}
				Dim vbc = Process.Start(startInfo)
				Dim stdout = vbc.StandardOutput.ReadToEnd()
				File.Delete(emitpath)

				If String.IsNullOrWhiteSpace(stdout) Then
					If Not args.Contains("/norun") Then
						Console.WriteLine("Running…")
						Process.Start(outpath)
					End If
				Else
					Console.WriteLine(stdout)
				End If
				Console.WriteLine()
			Catch ex As FileNotFoundException
				Console.WriteLine($"File {filename} not found!")
				Return
			Catch ex As ArgumentException
				Console.WriteLine($"Error: {ex.Message}")
				Return
			End Try
		Next

		Console.WriteLine($"Done!{vbCrLf}Press any key to continue…")
		Console.ReadKey()
	End Sub

	Private Function GetVBCPath() As String
		Dim Syspath$() = Environment.GetEnvironmentVariable("PATH").Split({";"c}, StringSplitOptions.RemoveEmptyEntries)

		Dim retval$ = (From s In Syspath
					   Where Directory.EnumerateFiles(s).Contains(Path.Combine(s, "vbc.exe"))
					   Select Path.Combine(s, "vbc.exe")).FirstOrDefault()
		If Not File.Exists(retval) Then
			Console.WriteLine("vbc.exe not found. Have you added C:\Windows\Microsoft.NET\Framework[64]\<version>\ to your PATH?")
			Console.WriteLine("Press any key to continue…")
			Console.ReadKey()
			End
		End If
		Return retval
	End Function
End Module