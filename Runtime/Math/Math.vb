Public Module Math
	Public Function Abs(Value As Object) As Object
		Return System.Math.Abs(Value)
	End Function

	Public Function Log(Value As Object, Base As Object) As Object
		Return System.Math.Log(Value, Base)
	End Function

	Public Function Pow(Base As Object, Exponent As Object) As Object
		Return System.Math.Pow(Base, If(System.Math.Round(Exponent) = Exponent, Exponent, CLng(Exponent)))
	End Function

	Public Function Sqrt(Value As Object) As Object
		Return System.Math.Sqrt(Value)
	End Function

	Public Function Sum(Data() As Object) As Object
		Dim retval As Object = 0
		For Each item In Data
			If TypeOf item Is String OrElse TypeOf item IsNot IEnumerable(Of Object) Then
				Try
					retval += item
				Catch ex As InvalidCastException
					retval &= item
				End Try
			Else
				retval += Sum(CType(item, IEnumerable(Of Object)).ToArray())
			End If
		Next
		Return retval
	End Function
End Module