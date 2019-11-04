Public Module Math
    ''' <summary>
    ''' Takes the absolute value of the provided input
    ''' </summary>
    ''' <param name="Value">The input value</param>
    ''' <returns>The absolute value of the provided input</returns>
    Public Function Abs(Value As Object) As Object
        Return System.Math.Abs(Value)
    End Function

    ''' <summary>
    ''' Finds the cosine of the specified input angle
    ''' </summary>
    ''' <param name="Value">The input in radians</param>
    ''' <returns>Cos(input)</returns>
    Public Function Cos(Value As Object) As Object
        Return System.Math.Cos(Value)
    End Function

    ''' <summary>
    ''' Calculates the degree representation of the provided angle
    ''' </summary>
    ''' <param name="Radians">The input in radians</param>
    ''' <returns>The angle in degrees</returns>
    Public Function Degrees(Radians As Object) As Object
        Return Radians * (180.0 / System.Math.PI)
    End Function

    ''' <summary>
    ''' Takes the specified base logarithm of the provided value.
    ''' </summary>
    ''' <param name="Value">The value to take the logarithm of</param>
    ''' <param name="Base">The base of the logarithm</param>
    ''' <returns></returns>
    Public Function Log(Value As Object, Base As Object) As Object
        Return System.Math.Log(Value, Base)
    End Function

    ''' <summary>
    ''' Raises a provided base to a provided exponent
    ''' </summary>
    ''' <param name="Base">The number to be exponentiated</param>
    ''' <param name="Exponent">The power to exponentiate to</param>
    ''' <returns><paramref name="Base"/>^<paramref name="Exponent"/></returns>
    Public Function Pow(Base As Object, Exponent As Object) As Object
        Return System.Math.Pow(Base, If(System.Math.Round(Exponent) = Exponent, Exponent, CDbl(Exponent)))
    End Function


    ''' <summary>
    ''' Calculates the radian representation of the provided angle
    ''' </summary>
    ''' <param name="Degrees">The input in degrees</param>
    ''' <returns>The angle in radians</returns>
    Public Function Radians(Degrees As Object) As Object
        Return System.Math.PI * Degrees / 180.0
    End Function


    ''' <summary>
    ''' Finds the sine of the specified input angle
    ''' </summary>
    ''' <param name="Value">The input in radians</param>
    ''' <returns>Sin(input)</returns>
    Public Function Sin(Value As Object) As Object
        Return System.Math.Sin(Value)
    End Function

    ''' <summary>
    ''' Finds the square root of the provided value
    ''' </summary>
    ''' <param name="Value">The input to be sqrt'd</param>
    ''' <returns>The square root of the input</returns>
    Public Function Sqrt(Value As Object) As Object
        Return System.Math.Sqrt(Value)
    End Function

    ''' <summary>
    ''' Sums the provided values
    ''' </summary>
    ''' <param name="Data">A collection of values to sum</param>
    ''' <returns>The sum of all provided values</returns>
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


    ''' <summary>
    ''' Finds the tangent of the specified input angle
    ''' </summary>
    ''' <param name="Value">The input in radians</param>
    ''' <returns>Tan(input)</returns>
    Public Function Tan(Value As Object) As Object
        Return System.Math.Tan(Value)
    End Function
End Module