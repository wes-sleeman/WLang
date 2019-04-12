Imports System.Reflection

Partial Public Class Engine
	Private Sub AddLib(filepath$, Optional typename$ = Nothing)
		If filepath.ToLower = "runtime" Then
			If typename Is Nothing Then
				Types.AddRange(Assembly.Load("Runtime").GetExportedTypes())
			Else
				Types.Add(Assembly.Load("Runtime").GetType(typename, False, True))
			End If
		Else
			filepath = IO.Path.GetFullPath(If(IO.File.Exists(filepath & ".dll"), filepath & ".dll", filepath))
			If typename Is Nothing Then
				Types.AddRange(Assembly.LoadFile(filepath).GetExportedTypes())
			Else
				Types.AddRange(Assembly.LoadFile(filepath).GetType(typename, False, True))
			End If
		End If
	End Sub

	Private Sub [Boolean]()
		Register = Match(TokenType._Boolean)
	End Sub

	Private Sub CheckAssignmentProperty(Varname As String)
		If TypeOf Register IsNot IEnumerable(Of Object) Then Register = {Register}
		Register = CheckAssignmentRecursion(Register)
		Match(TokenType._Equals)
		Push()
		Expr()
		Variable(Varname) = Register(Stack.Pop())
	End Sub

	Private Function CheckAssignmentRecursion(ByRef WorkingRegister As Object) As Object
		If Lexer.Current.Type <> TokenType._Dot Then Return WorkingRegister
		If TypeOf WorkingRegister IsNot IEnumerable(Of Object) Then Register = {Register}
		Match(TokenType._Dot)
		Select Case Lexer.Current.Type
			Case TokenType._LeftParen
				Match(TokenType._LeftParen)
				Expr()
				Match(TokenType._RightParen)
				Push()
				Return CheckAssignmentRecursion(WorkingRegister(Stack.Pop()))

			Case TokenType._IntLiteral
				Return CheckAssignmentRecursion(WorkingRegister(Double.Parse(Match(TokenType._IntLiteral))))

			Case Else
				Throw New ArgumentException($"Unexpected {Lexer.Current.Value} after dot. Did you forget to bracket a dynamic indexer?")
		End Select
	End Function

	Private Sub CheckProperty()
		Do While Lexer.Current.Type = TokenType._Dot
			Match(TokenType._Dot)
			Push()
			Select Case Lexer.Current.Type
				Case TokenType._LeftParen
					Match(TokenType._LeftParen)
					Expr()
					Match(TokenType._RightParen)
					Register = Stack.Pop()(Register)

				Case TokenType._Variable
					Select Case Lexer.Current.Value.ToLower()
						Case "num"
							Match(TokenType._Variable)
							Try : Try : Register = (Stack.Peek()).Length : Catch : Register = (Stack.Peek()).Count : End Try : Catch : Register = If(Register Is Nothing, 0, 1) : End Try : Stack.Pop()
						Case "pos"
							Match(TokenType._Variable)
							Expr(inProp:=True)
							Try : Register = New List(Of Object)(CType(Stack.Pop(), IEnumerable(Of Object))).IndexOf(Register) : Catch : Register = -1 : End Try
						Case "concat"
							Match(TokenType._Variable)
							Expr(inProp:=True)
							Register = _Concat()
						Case Else
							Register = Stack.Peek().GetType().GetProperty(Match(TokenType._Variable)).GetValue(Stack.Pop())
					End Select

				Case TokenType._IntLiteral
					Register = Stack.Pop()(Double.Parse(Match(TokenType._IntLiteral)))

				Case Else
					Throw New ArgumentException($"Unexpected {Lexer.Current.Value} after dot. Did you forget to bracket a dynamic indexer?")
			End Select
		Loop
	End Sub

	Private Function Match(Type As TokenType, Optional Advance As Boolean = True) As String
		If Not Lexer.Current.Type = Type Then
			Throw New ArgumentException($"Expected {Type} but received {Lexer.Current.Type}.")
		End If

		Dim retval$ = Lexer.Current.Value
		If Advance Then Lexer.Advance()
		Return retval
	End Function

	Private Sub Pop(Optional optype As TokenType? = Nothing)
        If optype Is Nothing Then
            Register = Stack.Pop()
            Return
        End If
        If Stack.Peek Is Nothing Then
            Stack.Pop()
            Stack.Push(False)
        End If
        Select Case optype.Value
            'Numeric -> Numeric type operators
            Case TokenType._Cross
                Dim res As Double = 0
                If Double.TryParse(Stack.Peek().ToString(), res) Then
                    Stack.Pop()
                    Register = res + Register
                Else
                    Register = Stack.Pop() + Register
                End If
            Case TokenType._Hyphen
                Register = Stack.Pop() - Register
            Case TokenType._Asterisk
                Register = Stack.Pop() * Register
            Case TokenType._Slash
                Register = Stack.Pop() / Register
            Case TokenType._BackSlash
                Register = Stack.Pop() \ Register
            Case TokenType._Percent
                Register = Stack.Pop() Mod Register

            'Numeric -> Boolean
            Case TokenType._LeftAngle
                Register = Stack.Pop() < Register
            Case TokenType._LeftAngleEquals
                Register = Stack.Pop() <= Register
            Case TokenType._RightAngle
                Register = Stack.Pop() > Register
            Case TokenType._RightAngleEquals
                Register = Stack.Pop() >= Register
            Case TokenType._Equals
                Register = _Equality(Stack.Pop(), Register)

            'Boolean -> Boolean
            Case TokenType._Ampersand
                Register = Stack.Pop() And Register
            Case TokenType._Pipe
                Register = Stack.Pop() Or Register
            Case TokenType._Caret
                Register = Stack.Pop() Xor Register
            Case TokenType.And
                Register = Stack.Pop() AndAlso Register
            Case TokenType.Or
                Register = Stack.Pop() OrElse Register
        End Select
    End Sub

	Private ProjectionIterator As Object
	Private Sub Projection()
		Match(TokenType._LeftAngleSquare)
		Expr()

		Dim lexerCache = Lexer.Index
		Match(TokenType._RightEqualsAngle)
		Stack.Push(New List(Of Object)(ProjectionOutput))
		ProjectionOutput.Clear()
		For Each PI As Object In CType(If(TypeOf Register Is IEnumerable(Of Object), Register, {Register}), IEnumerable(Of Object)).ToList()
			ProjectionIterator = PI
			Lexer.Index = lexerCache
			Lexer.Advance()
			Expr()
			ProjectionOutput.Add(Register)
		Next
		Register = ProjectionOutput
		ProjectionOutput = Stack.Pop()
		Match(TokenType._RightSquareAngle)
	End Sub

	Private Sub Push()
		Stack.Push(Register)
	End Sub
End Class