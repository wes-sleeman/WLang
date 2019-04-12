Public Module Program
	Sub Main(args As String())
		Console.WriteLine($"W {Reflection.Assembly.GetAssembly(GetType(Runtime.Util)).GetName().Version.ToString().Split({"."c}, StringSplitOptions.RemoveEmptyEntries).Take(3).Aggregate(Function(i, s) i & "." & s)} Interactive")
		Dim ExecutionEngine As New Engine.Engine()

		Dim killflag As Boolean = False

		args = args.Where(Function(s$) Not (s.ToLower = "-i" OrElse s.ToLower = "--interactive" OrElse s.ToLower = "/i")).ToArray()
		If args.Contains("-c") Then
			args = args.Aggregate(Function(i$, s$) i & " " & s).Split("-c", StringSplitOptions.RemoveEmptyEntries)
			For Each line$ In args
				Print(ExecutionEngine.Eval(line))
			Next
			Return
		End If

		For Each arg$ In args
			If arg.ToLower = "-k" OrElse arg.ToLower = "/k" Then killflag = True

			If Not IO.File.Exists(arg) Then
				Console.WriteLine($"Cannot find file {arg}.")
				Continue For
			End If

			For Each line In IO.File.ReadAllLines(arg)
				ExecutionEngine.Eval(line)
			Next
		Next

		If killflag Then Return

		Do
			Console.Write(Environment.NewLine & ">>> ")
			Try
				Print(ExecutionEngine.Eval(Console.ReadLine))
			Catch ex As Exception
				Console.WriteLine("Error " & ex.Message)
			End Try
		Loop
	End Sub

	Private Sub Print(output$)
		Console.Write(output & If(String.IsNullOrEmpty(output), String.Empty, Environment.NewLine))
	End Sub
End Module