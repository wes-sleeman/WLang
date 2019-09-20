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
        Sub TestCos(data#, result#)
            Assert.Equal(result, Global.Runtime.Cos(data))
        End Sub

        <Theory>
        <InlineData(1, 57.2958)>
        <InlineData(50, 2864.78898)>
        <InlineData(-2, -114.59156)>
        <InlineData(1.5, 85.9437)>
        <InlineData(Math.PI, 180)>
        Sub TestDegrees(data#, result#)
            Assert.Equal(Math.Round(result, 4), Math.Round(Global.Runtime.Degrees(data), 4))
        End Sub

        <Theory>
        Sub TestLog(data#, base#, result#)
            Assert.Equal(result, Global.Runtime.Log(data, base))
        End Sub

        <Theory>
        Sub TestPow(data#, exponent#, result#)
            Assert.Equal(result, Global.Runtime.Pow(data, exponent))
        End Sub
    End Class
End Namespace