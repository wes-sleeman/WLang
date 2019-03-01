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
End Module