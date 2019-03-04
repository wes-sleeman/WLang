Module Program
	Sub Main()
		Console.WriteLine("W 1.5.1 Interactive")
		Dim ExecutionEngine As New Engine()
		Do
			Console.Write(Environment.NewLine & ">>> ")
			Try
				Dim output$ = ExecutionEngine.Eval(Console.ReadLine)

				Console.Write(output & If(String.IsNullOrEmpty(output), String.Empty, Environment.NewLine))
			Catch ex As Exception
				Console.WriteLine("Error: " & ex.Message)
			End Try
		Loop
	End Sub
End Module
