Module Program
	Sub Main(args As String())
		If args.Count = 0 OrElse args.Count(Function(s$) s.ToLower = "-i" OrElse s.ToLower = "--interactive" OrElse s.ToLower = "/i" OrElse s.ToLower = "-c") > 0 Then
			Interactive.Main(args)
		Else
			Compiler.Main.Main(args)
		End If
	End Sub
End Module