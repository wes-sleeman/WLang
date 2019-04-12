Imports W.Engine

Module Program
    Sub Main()
		Console.WriteLine($"W {Reflection.Assembly.GetAssembly(GetType(Runtime.Util)).GetName().Version.ToString().Split({"."c}, StringSplitOptions.RemoveEmptyEntries).Take(3).Aggregate(Function(i, s) i & "." & s)} Interactive")
		Dim ExecutionEngine As New Engine()
        Do
            Console.Write(Environment.NewLine & ">>> ")
            Try
                Dim output$ = ExecutionEngine.Eval(Console.ReadLine)

                Console.Write(output & If(String.IsNullOrEmpty(output), String.Empty, Environment.NewLine))
            Catch ex As Exception
                Console.WriteLine("Error " & ex.Message)
		End Try
        Loop
    End Sub
End Module