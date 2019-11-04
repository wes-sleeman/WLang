Imports Xunit

Namespace W.Tests.Runtime
    Public Class UtilUnitTests
        <Theory>
        <InlineData("this is a test", {" "}, {"this", "is", "a", "test"})>
        Sub TestSplit(data$, sep$(), expected$())
            Assert.Equal(Global.Runtime.Shared.FormatArray(expected), Global.Runtime.Shared.FormatArray(Global.Runtime.Split(data, sep)))
        End Sub
    End Class
End Namespace