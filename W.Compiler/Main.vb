Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

Public Module Main
	Sub Main(args As String())
		Console.WriteLine($"W {Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().Split({"."c}, StringSplitOptions.RemoveEmptyEntries).Take(3).Aggregate(Function(i, s) i & "." & s)} Compiler" & vbCrLf)

		If Not args.Where(Function(s) Not s.StartsWith("/")).Any() Then
			Console.WriteLine("Needs something to compile! Pass in a W file to get started.")
			Return
		End If
		args = args.Select(Function(s$) If(s.StartsWith("-"), "/" & s.TrimStart({"-"c}), s)).ToArray()

		Dim conf = False


		Dim cross = False, norun = False, [lib] = False, debug = False, errorOccurred = False
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
					Case "/noconf", "/quit"
						conf = False
						Console.WriteLine("Automatically quitting after compilation")
					Case "/conf", "/noquit"
						conf = True
						Console.WriteLine("Requiring confirmation before program termination")
					Case "/debug", "/test"
						debug = True
						Console.WriteLine("Building in debug mode")
					Case "/release", "/prod"
						debug = False
						Console.WriteLine("Building in release mode")
					Case "/lib"
						[lib] = True
					Case Else
						Console.WriteLine("Invalid program flag " & filename)
						Return
				End Select
				Continue For
			End If

			If Directory.Exists(".build") Then Directory.Delete(".build", True)

			'Create empty dotnet project to use as template.
			Call "dotnet new console -lang VB -o .build".Exec()

			Try
				filename = Path.GetFullPath(filename)
				Console.WriteLine($"Building lexer for file <{filename}>.")
				Dim code() = LexAndParse(File.ReadAllText(filename), filename, [lib], debug)

				File.Delete(Path.Combine(".build", "Program.vb"))

				Dim emitpath$ = Path.ChangeExtension(Path.Combine(".build", If([lib], Path.GetFileName(filename), "Program.vb")), ".vb")
				Console.WriteLine("Emitting")
				File.WriteAllLines(emitpath, code)

				Console.WriteLine()
			Catch ex As FileNotFoundException
				Console.WriteLine($"File {filename} not found!")
				errorOccurred = True
				Continue For
			Catch ex As ArgumentException
				Console.WriteLine($"Error: {ex.Message}")
				errorOccurred = True
				Continue For
			Catch ex As StackOverflowException
				Console.WriteLine("Error: Infinite recursion in expression. Are you sure this is valid W?")
				errorOccurred = True
				Continue For
			End Try

			If errorOccurred Then
				Directory.Delete(".build", True)
				Console.WriteLine("Compilation terminated due to errors. See above.")
				Return
			End If

			Call $"dotnet add {Path.Combine(".build", ".build.vbproj")} package W.Runtime".Exec()

			Dim projfile = File.ReadAllLines(Path.Combine(".build", ".build.vbproj")).ToList()
			Dim insertIndex% = projfile.FindIndex(Function(s$) s.Contains("<OutputType>"))
			projfile(insertIndex + 1) = projfile(insertIndex + 1).Replace("_build", Path.GetFileNameWithoutExtension(filename))
			If [lib] Then projfile(insertIndex) = projfile(insertIndex).Replace("Exe", "Library")
			projfile.Insert(insertIndex, $"    <AssemblyName>{Path.GetFileNameWithoutExtension(filename)}</AssemblyName>")

			insertIndex = projfile.FindIndex(Function(s$) s.Contains("<TargetFramework>"))
			Dim frameworkTarget$ = projfile(insertIndex).Substring(projfile(insertIndex).IndexOf("<Target") + "<TargetFramework>".Length)
			frameworkTarget = frameworkTarget.Substring(0, frameworkTarget.IndexOf("</Target"))

			File.WriteAllLines(Path.Combine(".build", ".build.vbproj"), projfile)

			If Not cross Then
				Dim outpath$ = Path.ChangeExtension(Path.GetFullPath(filename), ".dll")
				Console.WriteLine($"Compiling to {outpath}.")
				Dim stdout$ = $"dotnet publish .build -c {If(debug, "Debug", "Release")} -f {frameworkTarget} -v n -o ""{Path.GetDirectoryName(outpath)}""".Exec()

				'Clean up
				If Not debug Then
					File.Delete(Path.ChangeExtension(outpath, ".pdb"))
					If RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then File.Delete(Path.ChangeExtension(outpath, ".exe"))
					If RuntimeInformation.IsOSPlatform(OSPlatform.Linux) Then File.Delete(Path.GetFileNameWithoutExtension(outpath))
					File.Delete(Path.ChangeExtension(outpath, ".deps.json"))
				End If
				Directory.Delete(".build", True)

				If Not stdout.Contains("Build FAILED") Then
					If Not norun Then
						Console.WriteLine("Running…")
						Process.Start("dotnet", """" & outpath & """")
					End If
				Else
					Console.WriteLine(stdout)
				End If
			End If
		Next

		If conf Then
			Console.WriteLine($"Done!{vbCrLf}Press any key to continue…")
			Console.ReadKey()
		End If
	End Sub

	Public Function LexAndParse(code$, filename$, [lib] As Boolean, debug As Boolean) As String()
		Dim lex As New Lexer(code)

		Console.WriteLine("Parsing")
		Return Parse(lex, Path.GetFileNameWithoutExtension(filename), [lib], debug)
	End Function

	<Extension()>
	Friend Function Exec(cmd$) As String
		Using proc As New Process()
			If RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then
				proc.StartInfo = New ProcessStartInfo() With
			{
				.FileName = "cmd.exe",
				.Arguments = $"/C {cmd}",
				.RedirectStandardOutput = True,
				.UseShellExecute = False,
				.CreateNoWindow = True
			}
			Else
				Dim escapedargs$ = cmd.Replace("""", "\""")
				proc.StartInfo = New ProcessStartInfo() With
			{
				.FileName = "/bin/bash",
				.Arguments = $"-c ""{escapedargs}""",
				.RedirectStandardOutput = True,
				.UseShellExecute = False,
				.CreateNoWindow = True
			}
			End If

			proc.Start()
			Dim res$ = proc.StandardOutput.ReadToEnd()
			proc.WaitForExit()

			proc.Kill()

			Return res
		End Using
	End Function
End Module