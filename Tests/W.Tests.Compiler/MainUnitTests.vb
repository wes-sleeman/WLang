Imports System.IO
Imports W.Compiler
Imports Xunit

Namespace W.Tests.Compiler
	Public Class MainUnitTests
		<Fact>
		Sub TestEmptyMain()
            Global.W.Compiler.Main.Main(Array.Empty(Of String)())
        End Sub

		<Theory>
		<InlineData("", "test.w", False, False)>
		<InlineData("", "test.w", False, True)>
		<InlineData("", "test.w", True, False)>
        <InlineData("", "test.w", True, True)>
        Sub TestEmptyProgram(code$, filename$, [lib] As Boolean, debug As Boolean)
            Dim RemoveSpace = Function(s$) New String(s.Where(Function(x) Not Char.IsWhiteSpace(x)).ToArray()).ToLower()
            Dim validResult$ = $"Imports System : Imports System.Collections.Generic : Imports System.Reflection
Module {Path.GetFileNameWithoutExtension(filename)}
	Dim Register As Object
	Dim Counter% = 0
	Dim LoopEnd% = 0
	Private FuncArgs As New List(Of Object)
	Private Variable As New Dictionary(Of String, Object)
	Private ProjectionOutput As New List(Of Object)
	ReadOnly Stack As New Stack(Of Object)
	ReadOnly Types As New List(Of Type)
	Function Defined(ParamArray Data() As Object) As Object
		If Data.Length = 1 AndAlso TypeOf Data(0) IsNot String AndAlso TypeOf Data(0) Is IEnumerable(Of Object) Then Data = New List(Of Object)(CType(Data(0), IEnumerable(Of Object))).ToArray()
		If Data.Length = 1 AndAlso TypeOf Data(0) Is String Then
			Return Variable.ContainsKey(Data(0).ToLower())
		Else
			Dim retval As New List(Of Object)
			For Each item In Data
				retval.Add(Defined(item))
			Next
			Return retval
		End If
	End Function
	Function _Concat() As Object
		If TypeOf Stack.Peek() Is String Then
			Return If(Stack.Pop(), String.Empty).ToString() & If(Register, String.Empty).ToString()
		Else
			If Stack.Peek() Is Nothing Then
				Stack.Pop()
				If Register Is Nothing Then Return Nothing
				Return CType(If(TypeOf Register Is IEnumerable(Of Object), Register, {{Register}}), IEnumerable(Of Object)).ToList()
			Else
				If Register Is Nothing Then Return New List(Of Object)(CType(If(TypeOf Stack.Peek() Is IEnumerable(Of Object) AndAlso Not TypeOf Stack.Peek() Is String, Stack.Pop(), {{Stack.Pop()}}), IEnumerable(Of Object))).ToList()
				Return New List(Of Object)(CType(If(TypeOf Stack.Peek() Is IEnumerable(Of Object) AndAlso Not TypeOf Stack.Peek() Is String, Stack.Pop(), {{Stack.Pop()}}), IEnumerable(Of Object))).Concat(If(TypeOf Register Is IEnumerable(Of Object), Register, {{Register}})).ToList()
			End If
		End If
	End Function
	Function _Equality(Item1 As Object, Item2 As Object) As Boolean
		Dim a As Object() = If(TypeOf Item1 Is IEnumerable(Of Object), If(TypeOf Item1 Is Object(), Item1, Item1.ToArray()), {{Item1}})
		Dim b As Object() = If(TypeOf Item2 Is IEnumerable(Of Object), If(TypeOf Item2 Is Object(), Item2, Item2.ToArray()), {{Item2}})
		If a.Length <> b.Length Then Return False
		If a.SequenceEqual(b) Then Return True
		For i% = 0 To a.Length - 1
			If (IsNumeric(a(i)) AndAlso IsNumeric(b(i))) OrElse (TypeOf a(i) Is String OrElse TypeOf a(i) Is Char) AndAlso (TypeOf b(i) Is String OrElse TypeOf b(i) Is Char) Then
				If CStr(a(i)) <> CStr(b(i)) Then Return False
			ElseIf TypeOf a(i) Is IEnumerable(Of Object) AndAlso TypeOf b(i) Is IEnumerable(Of Object) Then
				If Not _Equality(a(i), b(i)) Then Return False
			End If
        Next
		Return True
	End Function
	Function _InvokeMethod(name$, args As IEnumerable(Of Object)) As Object
		Dim ArgArr = args.ToArray()

		For Each type In GetType(test).Assembly.GetTypes()
			Try
				Try
					Return type.GetMethod(name, BindingFlags.NonPublic Or BindingFlags.Static).Invoke(Nothing, ArgArr)
				Catch e As Exception When TypeOf e Is TargetParameterCountException OrElse TypeOf e Is ArgumentException
					Return type.GetMethod(name, BindingFlags.NonPublic Or BindingFlags.Static).Invoke(Nothing, {{ArgArr}})
				End Try
			Catch ex As TypeLoadException : Catch ex As NullReferenceException : Catch ex As TargetParameterCountException
			End Try
			Try
				Try
					Return type.GetMethod(name, BindingFlags.Public Or BindingFlags.Static).Invoke(Nothing, ArgArr)
				Catch e As Exception When TypeOf e Is TargetParameterCountException OrElse TypeOf e Is ArgumentException
					Return type.GetMethod(name, BindingFlags.Public Or BindingFlags.Static).Invoke(Nothing, {{ArgArr}})
				End Try
			Catch ex As TypeLoadException : Catch ex As NullReferenceException : Catch ex As TargetParameterCountException
			End Try
		Next

		For Each type In Types
			Try
				Try
					Return type.GetMethod(name).Invoke(Nothing, ArgArr)
				Catch e As Exception When TypeOf e Is TargetParameterCountException OrElse TypeOf e Is ArgumentException OrElse TypeOf e Is NullReferenceException
					Return type.GetMethod(name).Invoke(Nothing, {{ArgArr}})
				Catch e As AmbiguousMatchException
					Try
						Return type.GetMethod(name, ArgArr.Select(Function(obj) If(TypeOf obj Is IEnumerable(Of Object), GetType(Object()), GetType(Object))).ToArray()).Invoke(Nothing, ArgArr)
					Catch e2 As Exception When TypeOf e2 Is TargetParameterCountException OrElse TypeOf e2 Is ArgumentException OrElse TypeOf e2 Is NullReferenceException
						Return type.GetMethod(name, {{ArgArr.GetType()}}).Invoke(Nothing, {{ArgArr}})
					End Try
				End Try
			Catch ex As TypeLoadException : Catch ex As NullReferenceException : Catch ex As TargetParameterCountException
			End Try
		Next
		Throw New MissingMethodException(""Unable to find method "" & name & "". Did you forget a reference?"")
	End Function"
            validResult = RemoveSpace(validResult)
            Dim emission$ = RemoveSpace(LexAndParse(code, filename, [lib], debug).Aggregate(Function(s, i) s & vbLf & i).Replace(vbLf, Environment.NewLine))
            Assert.Equal(validResult, emission.Substring(0, validResult.Length))
            Assert.True(emission.StartsWith(validResult))
        End Sub
    End Class
End Namespace