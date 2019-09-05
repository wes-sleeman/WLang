Public Module Math
	Public Function Abs(Value As Object) As Object
		Return System.Math.Abs(Value)
	End Function

	Public Function Cos(Value As Object) As Object
		Return System.Math.Cos(Value)
	End Function

	Public Function Degrees(Radians As Object) As Object
		Return Radians * (180.0 / System.Math.PI)
	End Function

	Public Function Log(Value As Object, Base As Object) As Object
		Return System.Math.Log(Value, Base)
	End Function

	Public Function Pow(Base As Object, Exponent As Object) As Object
		Return System.Math.Pow(Base, If(System.Math.Round(Exponent) = Exponent, Exponent, CDbl(Exponent)))
	End Function

	Public Function Radians(Degrees As Object) As Object
		Return System.Math.PI * Degrees / 180.0
	End Function

	Public Function Sin(Value As Object) As Object
		Return System.Math.Sin(Value)
	End Function

	Public Function Sqrt(Value As Object) As Object
		Return System.Math.Sqrt(Value)
	End Function

	Public Function Sum(ParamArray Data() As Object) As Object
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

	Public Function Tan(Value As Object) As Object
		Return System.Math.Tan(Value)
	End Function
End Module