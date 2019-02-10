Public Module Util
	Public Sub [Exit](ReturnCode As Object)
		Environment.Exit(ReturnCode)
	End Sub

	Public Function Collect(ParamArray Data() As Object) As Object
		Return Data.ToList()
	End Function

	Public Function Thread(ParamArray Data() As Object) As Object
		Dim Separator = Data(0)
		Data = Data.Skip(1).ToArray()
		If Data.Length = 1 AndAlso TypeOf Data(0) Is IEnumerable(Of Object) Then Return ThreadLocal(Separator, If(TypeOf Data(0) Is String, Data(0).ToString().ToCharArray(), Data(0)))
		Return Data.Aggregate(Function(threader, item) threader & Separator & If(TypeOf item Is IEnumerable(Of Object) AndAlso TypeOf item IsNot String, ThreadLocal(Separator, item), item))
	End Function

	Private Function ThreadLocal(Separator As Object, Data As IEnumerable(Of Object))
		Return Data.Aggregate(Function(threader, item) threader & Separator & If(TypeOf item Is IEnumerable(Of Object) AndAlso TypeOf item IsNot String, ThreadLocal(Separator, item), item))
	End Function
End Module