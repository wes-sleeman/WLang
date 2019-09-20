Imports Xunit

Namespace W.Tests.Runtime
    Public Class MathUnitTests
        <Theory>
        <InlineData(5, 5)>
        <InlineData(-5, 5)>
        <InlineData(0, 0)>
        <InlineData(42, 42)>
        <InlineData(5.5, 5.5)>
        <InlineData(-5.5, 5.5)>
        <InlineData(Integer.MinValue + 1, Integer.MaxValue)>
        Sub TestAbs(data#, result#)
            Assert.Equal(result, Global.Runtime.Abs(data))
        End Sub

        <Theory>
        <InlineData(0, 1)>
        <InlineData(1, 0.5403)>
        <InlineData(-1, 0.5403)>
        <InlineData(2, -0.41614)>
        <InlineData(-2, -0.41614)>
        <InlineData(-50, 0.96496)>
        <InlineData(100, 0.86231)>
        <InlineData(Math.PI, -1)>
        <InlineData(-Math.PI, -1)>
        Sub TestCos(data#, result#)
            Assert.Equal(result, Global.Runtime.Cos(data), 4)
        End Sub

        <Theory>
        <InlineData(1, 57.2958)>
        <InlineData(50, 2864.78898)>
        <InlineData(-2, -114.59156)>
        <InlineData(1.5, 85.9437)>
        <InlineData(Math.PI, 180)>
        Sub TestDegrees(data#, result#)
            Assert.Equal(result, Global.Runtime.Degrees(data), 4)
        End Sub

        <Theory>
        <InlineData(1, 2, 0)>
        <InlineData(2, 2, 1)>
        <InlineData(30, 2, 4.90689)>
        <InlineData(64, 2, 6)>
        <InlineData(1, 3, 0)>
        <InlineData(1, 5, 0)>
        <InlineData(1, 10, 0)>
        <InlineData(10, 10, 1)>
        <InlineData(57, 10, 1.75587)>
        <InlineData(100, 10, 2)>
        Sub TestLog(data#, base#, result#)
            Assert.Equal(result, Global.Runtime.Log(data, base), 4)
        End Sub

        <Theory>
        <InlineData(0, 1, 0)>
        <InlineData(0, 3.7, 0)>
        <InlineData(0, 100, 0)>
        <InlineData(1, 1, 1)>
        <InlineData(1, 3.7, 1)>
        <InlineData(1, 5, 1)>
        <InlineData(1, 60, 1)>
        <InlineData(1, -1, 1)>
        <InlineData(6, -2, 0.027777)>
        <InlineData(6, -5, 0.000128)>
        <InlineData(6, 20, 3656158440062976)>
        <InlineData(16, 0, 1)>
        <InlineData(16, 0.56, 4.72397)>
        <InlineData(16, 1, 16)>
        <InlineData(16, -2, 0.003906)>
        Sub TestPow(data#, exponent#, result#)
            Try
                Assert.Equal(result, Global.Runtime.Pow(data, exponent), 4)
            Catch
                Throw
            End Try
        End Sub
    End Class
End Namespace