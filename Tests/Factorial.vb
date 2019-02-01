Imports System : Imports System.Collections.Generic : Imports System.Reflection
Module Program
	Dim Register As Object
	Dim Parent As Object
	Dim Counter% = 0
	Dim LoopEnd% = 0
	ReadOnly Variable As New Dictionary(Of String, Object)
	ReadOnly Functions As New Dictionary(Of String, Object)
	Dim Stack As New Stack(Of Object)
	ReadOnly Assemblies As New List(Of Assembly) From { GetType(Program).Assembly }
	Function InvokeMethod(name$, ParamArray args As Object()) As Object
		For Each asm In Assemblies
			Try
				Dim t = Asm.GetType(Asm.GetName().Name & "." & "Program", True, True)
				Dim m = t.GetMethod(name)
				Return m.Invoke(Nothing, args)
			Catch ex As TypeLoadException
			End Try
		Next
		Throw New MissingMethodException("Unable to find method " & name)
	End Function
	Function ReadStr() As String
		Return Console.ReadLine()
	End Function
	Function ReadInt() As Integer
		Dim input = ReadStr()
		Try
			Return Convert.ToInt32(input)
		Catch ex As FormatException
			Return 0
		End Try
	End Function
	Sub Print(data As Object)
		Console.WriteLine(data.ToString())
	End Sub

	Sub Main()

		'BEGIN USER GENERATED CODE
		Register = 15
		Stack.Push(Register)
		Stack.Push(LoopEnd) : LoopEnd = Register
		Stack.Push(Counter) : Counter = 1
		Do While Counter <= LoopEnd
			Register = 1
			Variable("x") = Register
			Register = Counter
			Stack.Push(Register)
			Stack.Push(LoopEnd) : LoopEnd = Register
			Stack.Push(Counter) : Counter = 1
			Do While Counter <= LoopEnd
				Register = Variable("x")
				Stack.Push(Register)
				Register = Counter
				Register = Stack.Pop() * Register
				Variable("x") = Register
				Counter += 1
			Loop
			Counter = Stack.Pop()
			LoopEnd = Stack.Pop()
			Register = Stack.Pop()
			Register = Variable("x")
			Print(Register)
			Counter += 1
		Loop
		Counter = Stack.Pop()
		LoopEnd = Stack.Pop()
		Register = Stack.Pop()
  'END USER GENERATED CODE

	Console.WriteLine("Press any key to continue...")
	Console.ReadKey()
End Sub
End Module
