Public Module Util
	Public Sub [Exit](ReturnCode As Object)
		Environment.Exit(ReturnCode)
	End Sub

	Public Function Collect(ParamArray Data() As Object) As Object
		Return Data.ToList()
	End Function

	Public Function Thread(Separator As Object, ParamArray Data() As Object) As Object
		Return Data.Aggregate(Function(threader, item) threader & Separator & item)
	End Function
End Module