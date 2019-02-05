Imports System.IO

Module Main
	Sub Main(args As String())
		Dim VBCPATH$ = GetVBCPath()

		Console.WriteLine("W Compiler Version 1.2.1" & vbCrLf)

#If DEBUG Then
		If args.Length = 0 Then args = {""}.Concat(Directory.EnumerateFiles(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\..\..\Tests"))).Where(Function(in$) [in].EndsWith(".w"))).ToArray()
#End If

		Dim cross = False, norun = False, [lib] = False
		Console.WriteLine("Reading input file(s).")
		For Each filename In args
			If String.IsNullOrWhiteSpace(filename) Then Continue For
			If filename.StartsWith("/") Then
				Select Case filename.ToLower
					Case "/norun"
						norun = True
						Console.WriteLine("Autorun suppressed")
					Case "/cross"
						cross = True
						Console.WriteLine("Compilation suppressed")
					Case "/lib"
						[lib] = True
						Console.WriteLine("Building to library")
					Case Else
						Console.WriteLine("Invalid program flag " & filename)
						Return
				End Select
				Continue For
			End If
			Try
				filename = Path.GetFullPath(filename)
				Console.WriteLine($"Building lexer for file <{filename}>.")
				Dim lex As New Lexer(File.ReadAllText(filename))

				Console.WriteLine("Parsing")
				Dim code() = Parse(lex, Path.GetFileNameWithoutExtension(filename))

				Dim emitpath$ = Path.ChangeExtension(filename, ".vb")
				Console.WriteLine("Emitting")
				File.WriteAllLines(emitpath, code)

				If Not cross Then
					CopyRuntime(emitpath)
					If [lib] Then
						Dim outpath$ = Path.ChangeExtension(filename, ".dll")
						Console.WriteLine($"Compiling to {outpath}.")
						Dim startInfo As New ProcessStartInfo With {.FileName = VBCPATH, .CreateNoWindow = True, .UseShellExecute = False, .RedirectStandardOutput = True, .Arguments = $"/out:""{outpath}"" /langversion:latest /t:library /r:Runtime.dll{References} /nologo {emitpath}"}
						Dim vbc = Process.Start(startInfo)
						Dim stdout = vbc.StandardOutput.ReadToEnd()
						File.Delete(emitpath)
					Else
						Dim outpath$ = Path.ChangeExtension(filename, ".exe")
						Console.WriteLine($"Compiling to {outpath}.")
						Dim startInfo As New ProcessStartInfo With {.FileName = VBCPATH, .CreateNoWindow = True, .UseShellExecute = False, .RedirectStandardOutput = True, .Arguments = $"/out:""{outpath}"" /langversion:latest /t:exe /r:Runtime.dll{References} /nologo {emitpath}"}
						Dim vbc = Process.Start(startInfo)
						Dim stdout = vbc.StandardOutput.ReadToEnd()
						File.Delete(emitpath)

						If String.IsNullOrWhiteSpace(stdout) Then
							If Not norun Then
								Console.WriteLine("Running…")
								Dim curDir = Environment.CurrentDirectory
#If DEBUG Then
								Environment.CurrentDirectory &= "..\..\..\..\Tests"
#End If
								Process.Start(outpath)
								Environment.CurrentDirectory = curDir
							End If
						Else
							Console.WriteLine(stdout)
						End If
					End If
				End If
				Console.WriteLine()
			Catch ex As FileNotFoundException
				Console.WriteLine($"File {filename} not found!")
				Continue For
			Catch ex As ArgumentException
				Console.WriteLine($"Error: {ex.Message}")
				Continue For
			End Try
		Next

		Console.WriteLine($"Done!{vbCrLf}Press any key to continue…")
		Console.ReadKey()
	End Sub

	Private Function GetRuntimePath() As String
		Dim Syspath$() = Environment.GetEnvironmentVariable("PATH").Split({";"c}, StringSplitOptions.RemoveEmptyEntries)

		Dim retval$ = (From s In Syspath
					   Where Directory.Exists(s) AndAlso Directory.EnumerateFiles(s).Contains(Path.Combine(s, "Runtime.dll"))
					   Select Path.Combine(s, "Runtime.dll")).FirstOrDefault()
		If Not File.Exists(retval) Then
			Console.WriteLine("Runtime.dll not found. Have you added the WCompiler to your PATH?")
			Console.WriteLine("Press any key to continue…")
			Console.ReadKey()
			End
		End If
		Return retval
	End Function

	Private Function GetVBCPath() As String
		Dim Syspath$() = Environment.GetEnvironmentVariable("PATH").Split({";"c}, StringSplitOptions.RemoveEmptyEntries)

		Dim retval$ = (From s In Syspath
					   Where Directory.Exists(s) AndAlso Directory.EnumerateFiles(s).Contains(Path.Combine(s, "vbc.exe"))
					   Select Path.Combine(s, "vbc.exe")).FirstOrDefault()
		If Not File.Exists(retval) Then
			Console.WriteLine("vbc.exe not found. Have you added C:\Windows\Microsoft.NET\Framework[64]\<version>\ to your PATH?")
			Console.WriteLine("Press any key to continue…")
			Console.ReadKey()
			End
		End If
		Return retval
	End Function

	Dim copyOnceFlag = False
	Private Sub CopyRuntime(emitpath$)
		If Not copyOnceFlag Then
			Dim rPath$ = GetRuntimePath(), rTarget$ = Path.Combine(Path.GetDirectoryName(emitpath), "Runtime.dll")
			If File.Exists(rPath) Then File.Delete(rTarget)
			File.Copy(rPath, rTarget)
			copyOnceFlag = True
		End If
	End Sub
End Module