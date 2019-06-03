Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

Public Module Main
	Sub Main(args As String())
		Console.WriteLine($"W {Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().Split({"."c}, StringSplitOptions.RemoveEmptyEntries).Take(3).Aggregate(Function(i, s) i & "." & s)} Compiler" & vbCrLf)

		args = args.Select(Function(s$) If(s.StartsWith("-"), "/" & s.TrimStart({"-"c}), s)).ToArray()
#If DEBUG Then
		If args.Length = 0 Then
			Main({"tmp.w"})
			Return
		End If
#End If
		Dim conf = False

		'Create empty dotnet project to use as template.
		Call "dotnet new console -lang VB -o .build".Exec()

		Dim cross = False, norun = False, [lib] = False, debug = True
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
				Dim code() = Parse(lex, Path.GetFileNameWithoutExtension(filename), [lib], debug)

				Dim emitpath$ = Path.ChangeExtension(Path.Combine(".build", If([lib], Path.GetFileName(filename), "Program.vb")), ".vb")
				Console.WriteLine("Emitting")
				File.WriteAllLines(emitpath, code)

				Console.WriteLine()
				[lib] = True
			Catch ex As FileNotFoundException
				Console.WriteLine($"File {filename} not found!")
				Continue For
			Catch ex As ArgumentException
				Console.WriteLine($"Error: {ex.Message}")
				Continue For
			End Try
		Next

		Call $"dotnet add {Path.Combine(".build", ".build.vbproj")} package W.Runtime".Exec()

		Dim executableFile$ = args.Where(Function(s$) Not s.StartsWith("/")).First()
		Dim projfile = File.ReadAllLines(Path.Combine(".build", ".build.vbproj")).ToList()
		Dim insertIndex% = projfile.FindIndex(Function(s$) s.Contains("<OutputType>"))
		projfile(insertIndex + 1) = projfile(insertIndex + 1).Replace("_build", Path.GetFileNameWithoutExtension(executableFile))
		projfile.Insert(insertIndex, $"    <AssemblyName>{Path.GetFileNameWithoutExtension(executableFile)}</AssemblyName>")
		File.WriteAllLines(Path.Combine(".build", ".build.vbproj"), projfile)

		If Not cross Then
			Dim outpath$ = Path.ChangeExtension(Path.GetFullPath(executableFile), ".dll")
			Console.WriteLine($"Compiling to {outpath}.")
			Dim stdout$ = $"dotnet publish .build -c {If(debug, "Debug", "Release")} -f netcoreapp3.0 -v n -o ""{Path.GetDirectoryName(outpath)}""".Exec()

			'Clean up
			If debug Then File.Delete(Path.ChangeExtension(outpath, ".pdb"))
			If RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then File.Delete(Path.ChangeExtension(outpath, ".exe"))
			If RuntimeInformation.IsOSPlatform(OSPlatform.Linux) Then File.Delete(Path.GetFileNameWithoutExtension(outpath))
			File.Delete(Path.ChangeExtension(outpath, ".deps.json"))
			Directory.Delete(".build", True)

			If stdout.Contains("Build succeeded.") Then
				If Not norun Then
					Console.WriteLine("Running…")
					Process.Start("dotnet", """" & outpath & """")
				End If
			Else
				Console.WriteLine(stdout)
			End If
		End If

		If conf Then
			Console.WriteLine($"Done!{vbCrLf}Press any key to continue…")
			Console.ReadKey()
		End If
	End Sub

	<Extension()>
	Friend Function Exec(cmd$) As String
		Dim proc As New Process()

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
				.Arguments = $"-c '{escapedargs}'",
				.RedirectStandardOutput = True,
				.UseShellExecute = False,
				.CreateNoWindow = True
			}
		End If

		proc.Start()
		Dim res$ = proc.StandardOutput.ReadToEnd()
		proc.WaitForExit()

		Return res
	End Function
End Module