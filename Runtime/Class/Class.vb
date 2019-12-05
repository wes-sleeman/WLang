Imports System.Dynamic
Imports Runtime.Shared

Public Module [Class]
    Public Function P(ParamArray args())
        Return Prop(args)
    End Function

    Public Function Prop(ParamArray args())
        If args.Length < 2 Then Throw New ArgumentException("Prop must have at least two arguments.")

        Try
            Dim Target As IDictionary(Of String, Object) = args(0)
            args = args.Skip(1).ToArray()

            If args.Length = 1 Then
                If Target.ContainsKey(args(0)) Then
                    Target(args(0)) = Nothing
                Else
                    Target.Add(New KeyValuePair(Of String, Object)(args(0), Nothing))
                End If
            ElseIf args.Length = 2 Then
                If Target.ContainsKey(args(0)) Then
                    Target(args(0)) = args(1)
                Else
                    Target.Add(New KeyValuePair(Of String, Object)(args(0), args(1)))
                End If
            End If

            Return Target
        Catch ex As Exception
            Throw New ArgumentException("First argument to Prop must be created by a call to Dynamic().")
        End Try
    End Function

    Public Function Dynamic()
        Return New ExpandoObject()
    End Function
End Module