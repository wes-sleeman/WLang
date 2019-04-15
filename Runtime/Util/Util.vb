Public Module Util
	Public Sub [Exit](ReturnCode As Object)
		Environment.Exit(ReturnCode)
	End Sub

	Public Function Collect(ParamArray Data() As Object) As Object
		If Data Is Nothing Then Return New List(Of Object)
		Return Data.ToList()
	End Function

	Public Function Dedup(ParamArray Data() As Object) As Object
		Return Data.ToHashSet()
	End Function

	Public Function Thread(ParamArray Data() As Object) As Object
		Dim Separator = Data(0)
		Data = Data.Skip(1).ToArray()
		If (Data.Length = 1) AndAlso (TypeOf Data(0) Is IEnumerable(Of Object) OrElse TypeOf Data(0) Is String) Then
			Return ThreadLocal(Separator, If(TypeOf Data(0) Is String, Data(0).ToString().ToCharArray().Select(Function(c) CObj(c)).ToArray(), Data(0)))
		Else
			Return Data.Aggregate(Function(threader, item) threader & Separator & If(TypeOf item Is IEnumerable(Of Object) AndAlso TypeOf item IsNot String, ThreadLocal(Separator, item), item))
		End If
	End Function

	Private Function ThreadLocal(Separator As Object, Data As IEnumerable(Of Object))
		Return Data.Aggregate(Function(threader, item) threader & Separator & If(TypeOf item Is IEnumerable(Of Object) AndAlso TypeOf item IsNot String, ThreadLocal(Separator, item), item))
	End Function
End Module