Partial Module Parser
	Private Function Match(Type As TokenType, Optional Advance As Boolean = True) As String
		If Not Lexer.Current.Type = Type Then
			Throw New ArgumentException($"Expected {Type} but received {Lexer.Current.Type} on line {Lexer.Line}.")
		End If

		Dim retval$ = Lexer.Current.Value
		If Advance Then Lexer.Advance()
		Return retval
	End Function

	Private IndentLevel% = 0

	Private Sub AddLib(filepath$, Optional typename$ = Nothing)
		If filepath.ToLower = "runtime" Then
			If typename Is Nothing Then
				Emit("Types.AddRange(Assembly.Load(""Runtime"").GetExportedTypes())")
			Else
				Emit($"Types.Add(GetType(Runtime.{typename}))")
			End If
		Else
			filepath = IO.Path.GetFullPath(If(IO.File.Exists(filepath & ".dll"), filepath & ".dll", filepath))
			References &= $",""{filepath}"""
			If typename Is Nothing Then
				Emit($"Types.AddRange(Assembly.LoadFile(""{filepath}"").GetExportedTypes())")
			Else
				Emit($"Types.AddRange(Assembly.LoadFile(""{filepath}"").GetType(""{typename}"", False, True))")
			End If
		End If
	End Sub

	Private Sub Emit(output$)
		Dim tabBuffer$ = String.Empty
		For cntr% = 1 To IndentLevel
			tabBuffer &= vbTab
		Next
		If InFunc Then
			functionBuffer.Add(tabBuffer & output)
		Else
			outputbuffer.Add(tabBuffer & output)
		End If
	End Sub

	Private Sub Setup()
		Emit($"Imports System : Imports System.Collections.Generic : Imports System.Reflection
Module {Filename}
	Dim Register As Object
	Dim Counter% = 0
	Dim LoopEnd% = 0
	Private FuncArgs As New List(Of Object)
	Private Variable As New Dictionary(Of String, Object)
	Private ProjectionOutput As New List(Of Object)
	ReadOnly Stack As New Stack(Of Object)
	ReadOnly Types As New List(Of Type)
	Function Defined(ParamArray Data() As Object) As Object
		If Data.Length = 1 AndAlso TypeOf Data(0) IsNot String AndAlso TypeOf Data(0) Is IEnumerable(Of Object) Then Data = New List(Of Object)(CType(Data(0), IEnumerable(Of Object))).ToArray()
		If Data.Length = 1 AndAlso TypeOf Data(0) Is String Then
			Return Variable.ContainsKey(Data(0).ToLower())
		Else
			Dim retval As New List(Of Object)
			For Each item In Data
				retval.Add(Defined(item))
			Next
			Return retval
		End If
	End Function
	Function _Concat() As Object
		If TypeOf Stack.Peek() Is String Then
			Return If(Stack.Pop(), String.Empty).ToString() & If(Register, String.Empty).ToString()
		Else
			If Stack.Peek() Is Nothing Then
				Stack.Pop()
				If Register Is Nothing Then Return Nothing
				Return CType(If(TypeOf Register Is IEnumerable(Of Object), Register, {{Register}}), IEnumerable(Of Object)).ToList()
			Else
				If Register Is Nothing Then Return New List(Of Object)(CType(If(TypeOf Stack.Peek() Is IEnumerable(Of Object) AndAlso Not TypeOf Stack.Peek() Is String, Stack.Pop(), {{Stack.Pop()}}), IEnumerable(Of Object))).ToList()
				Return New List(Of Object)(CType(If(TypeOf Stack.Peek() Is IEnumerable(Of Object) AndAlso Not TypeOf Stack.Peek() Is String, Stack.Pop(), {{Stack.Pop()}}), IEnumerable(Of Object))).Concat(If(TypeOf Register Is IEnumerable(Of Object), Register, {{Register}})).ToList()
			End If
		End If
	End Function
	Function _Equality(Item1 As Object, Item2 As Object) As Boolean
		Dim a As Object() = If(TypeOf Item1 Is IEnumerable(Of Object), If(TypeOf Item1 Is Object(), Item1, Item1.ToArray()), {{Item1}})
		Dim b As Object() = If(TypeOf Item2 Is IEnumerable(Of Object), If(TypeOf Item2 Is Object(), Item2, Item2.ToArray()), {{Item2}})
		If a.Length <> b.Length Then Return False
		If a.SequenceEqual(b) Then Return True
		For i% = 0 To a.Length - 1
			If (IsNumeric(a(i)) AndAlso IsNumeric(b(i))) OrElse (TypeOf a(i) Is String OrElse TypeOf a(i) Is Char) AndAlso (TypeOf b(i) Is String OrElse TypeOf b(i) Is Char) Then
				If CStr(a(i)) <> CStr(b(i)) Then Return False
			ElseIf TypeOf a(i) Is IEnumerable(Of Object) AndAlso TypeOf b(i) Is IEnumerable(Of Object) Then
				If Not _Equality(a(i), b(i)) Then Return False
			End If
        Next
		Return True
	End Function
	Function _InvokeMethod(name$, args As IEnumerable(Of Object)) As Object
		Dim ArgArr = args.ToArray()

		For Each type In GetType({Filename}).Assembly.GetTypes()
			Try
				Try
					Return type.GetMethod(name, BindingFlags.NonPublic Or BindingFlags.Static).Invoke(Nothing, ArgArr)
				Catch e As Exception When TypeOf e Is TargetParameterCountException OrElse TypeOf e Is ArgumentException
					Return type.GetMethod(name, BindingFlags.NonPublic Or BindingFlags.Static).Invoke(Nothing, {{ArgArr}})
				End Try
			Catch ex As TypeLoadException : Catch ex As NullReferenceException : Catch ex As TargetParameterCountException
			End Try
			Try
				Try
					Return type.GetMethod(name, BindingFlags.Public Or BindingFlags.Static).Invoke(Nothing, ArgArr)
				Catch e As Exception When TypeOf e Is TargetParameterCountException OrElse TypeOf e Is ArgumentException
					Return type.GetMethod(name, BindingFlags.Public Or BindingFlags.Static).Invoke(Nothing, {{ArgArr}})
				End Try
			Catch ex As TypeLoadException : Catch ex As NullReferenceException : Catch ex As TargetParameterCountException
			End Try
		Next

		For Each type In Types
			Try
				Try
					Return type.GetMethod(name).Invoke(Nothing, ArgArr)
				Catch e As Exception When TypeOf e Is TargetParameterCountException OrElse TypeOf e Is ArgumentException OrElse TypeOf e Is NullReferenceException
					Return type.GetMethod(name).Invoke(Nothing, {{ArgArr}})
				Catch e As AmbiguousMatchException
					Try
						Return type.GetMethod(name, ArgArr.Select(Function(obj) If(TypeOf obj Is IEnumerable(Of Object), GetType(Object()), GetType(Object))).ToArray()).Invoke(Nothing, ArgArr)
					Catch e2 As Exception When TypeOf e2 Is TargetParameterCountException OrElse TypeOf e2 Is ArgumentException OrElse TypeOf e2 Is NullReferenceException
						Return type.GetMethod(name, {{ArgArr.GetType()}}).Invoke(Nothing, {{ArgArr}})
					End Try
				End Try
			Catch ex As TypeLoadException : Catch ex As NullReferenceException : Catch ex As TargetParameterCountException
			End Try
		Next
		Throw New MissingMethodException(""Unable to find method "" & name & "". Did you forget a reference?"")
	End Function

	Public {If([Lib], $"Function {Filename}(Optional args As Object() = Nothing) As Object", "Sub Main(args As String())")}
		Dim LineNumber As ULong = 0
		Variable(""args"") = args

		Try

			'BEGIN USER GENERATED CODE")
		IndentLevel = 2
	End Sub

	Private Sub Teardown()
		Emit(
$"		'END USER GENERATED CODE

	Catch ex As Exception
" &
If(Debug,
"		Dim exMessage$() = ex.Message.Split({"" ""c }, StringSplitOptions.RemoveEmptyEntries)
		If TypeOf ex Is InvalidCastException Then
				Console.WriteLine(""Exception on line "" & LineNumber & "": Impossible operation on data "" & exMessage(3))
		ElseIf TypeOf ex Is TargetInvocationException Then 
				Console.WriteLine(""Exception on line "" & LineNumber & "": "" & ex.InnerException.Message)
		Else
				Console.WriteLine(""Exception on line "" & LineNumber & "": "" & ex.Message)
		End If
",
"		Console.WriteLine(""The application encountered an error. Please inform the developers."")
") &
"		Environment.Exit(1)
	End Try")
		If [Lib] Then
			Emit("End Function")
		Else
			outputbuffer.Add(String.Empty)
			Emit(vbTab & "Console.WriteLine(""Press any key to continue..."") : Console.ReadKey()")
			Emit("End Sub")
		End If
		IndentLevel = 0
		For Each line In functionBuffer
			If line.TrimStart().StartsWith("Friend") OrElse line.TrimStart().StartsWith("Public") Then outputbuffer.Add(String.Empty)
			Emit(line)
		Next
		Emit("End Module")
	End Sub

	Private Sub Expr(Optional inProp As Boolean = False)
		CompExpr(inProp)

		While "|&".Contains(Lexer.Current.Value) OrElse Lexer.Current.Type = TokenType.And OrElse Lexer.Current.Type = TokenType.Or
			Push()
			Dim op = Lexer.Current.Type
			Match(op)
			CompExpr(inProp)
			Pop(op)
		End While
	End Sub

	Private Sub CompExpr(inProp As Boolean)
		MathExpr(inProp)

		While {">", "<", "=", ">=", "<="}.Contains(Lexer.Current.Value)
			Push()
			Dim op = Lexer.Current.Type
			Match(op)
			MathExpr(inProp)
			Pop(op)
		End While
	End Sub

	Private Sub MathExpr(inProp As Boolean)
		Term(inProp)

		While "+-".Contains(Lexer.Current.Value)
			Push()
			Dim op = Lexer.Current.Type
			Match(op)
			Term(inProp)
			Pop(op)
		End While
	End Sub

	Private Sub Term(inProp As Boolean)
		SignedFactor(inProp)

		While "*/%\".Contains(Lexer.Current.Value)
			Push()
			Dim op = Lexer.Current.Type
			Match(op)
			SignedFactor(inProp)
			Pop(op)
		End While
	End Sub

	Private Sub SignedFactor(inProp As Boolean)
		Dim op = TokenType._Cross
		If "+-".Contains(Lexer.Current.Value) Then
			op = Lexer.Current.Type
			Match(op)
		End If

		Factor(inProp)

		If op = TokenType._Hyphen Then
			Emit("Register = -Register")
		End If
	End Sub

	Private Sub Factor(inProp As Boolean)
		Select Case Lexer.Current.Type
			Case TokenType._IntLiteral
				Emit($"Register = {Match(TokenType._IntLiteral)}")

			Case TokenType._StringLiteral
				Emit($"Register = ""{Match(TokenType._StringLiteral)}""")

			Case TokenType._Variable
				Dim name$ = Match(TokenType._Variable, False)
				If Varlist.Contains(name.ToLower) Then
					Lexer.Advance()
					Emit($"Register = Variable(""{name.ToLower}"")")
				Else
					FunctionCall()
				End If

			Case TokenType.Not, TokenType._Boolean
				BooleanExpr()

			Case TokenType._Dollar
				Match(TokenType._Dollar)
				Emit("Register = ProjectionIterator")

			Case TokenType._HashSign
				Match(TokenType._HashSign)
				Emit("Register = Counter")

			Case TokenType._LeftAngleSquare
				Projection()

			Case Else
				Match(TokenType._LeftParen)
				Expr()
				Match(TokenType._RightParen)
		End Select

		If Not inProp AndAlso Lexer.Current.Type = TokenType._Dot Then
			CheckProperty()
		End If
	End Sub

	Private Sub CheckProperty(Optional Assignment As Boolean = False, Optional Varname As String = "")
		If Assignment AndAlso String.IsNullOrWhiteSpace(Varname) Then Throw New ArgumentNullException("Varname must be supplied assigning property check.")

		Dim indexers As New List(Of String)

		If Assignment Then Push()
		Do While Lexer.Current.Type = TokenType._Dot
			Match(TokenType._Dot)
			If Not Assignment Then Push()
			Select Case Lexer.Current.Type
				Case TokenType._LeftParen
					Match(TokenType._LeftParen)
					Expr()
					Match(TokenType._RightParen)
					If Assignment Then Push()
					indexers.Add(If(Assignment, $"(Stack.Pop())", "Register = (Stack.Pop())(Register)"))

				Case TokenType._Variable
					If Assignment Then
						indexers.Add($".{Match(TokenType._Variable)}")
					Else
						Select Case Lexer.Current.Value.ToLower()
							Case "num"
								Match(TokenType._Variable)
								indexers.Add("Try : Try : Register = (Stack.Peek()).Length : Catch : Register = (Stack.Peek()).Count : End Try : Catch : Register = If(Register Is Nothing, 0, 1) : End Try : Stack.Pop()")
							Case "pos"
								Match(TokenType._Variable)
								Expr(inProp:=True)
								indexers.Add("Try : Register = New List(Of Object)(CType(Stack.Pop(), IEnumerable(Of Object))).IndexOf(Register) : Catch : Register = -1 : End Try")
							Case "concat"
								Match(TokenType._Variable)
								Expr(inProp:=True)
								indexers.Add("Register = _Concat()")
							Case Else
								indexers.Add("Register = (Stack.Pop())." & Match(TokenType._Variable))
						End Select
					End If

				Case TokenType._IntLiteral
					indexers.Add(If(Assignment, $"({Match(TokenType._IntLiteral)})", $"Register = (Stack.Pop())({Match(TokenType._IntLiteral)})"))

				Case Else
					Throw New ArgumentException($"Unexpected {Lexer.Current.Value} after dot on line {Lexer.Line}. Did you forget to bracket a dynamic indexer?")
			End Select
			If Not Assignment Then
				While indexers.Count > 0
					Emit(indexers(0))
					indexers.RemoveAt(0)
				End While
				indexers.Clear()
			End If
		Loop
		If Assignment Then
			Dim retval = $"Variable(""{Varname}"")"
			For Each item In indexers
				retval &= item
			Next
			Match(TokenType._Equals)
			Expr()
			Emit(retval & " = Register")
			Pop()
		End If
	End Sub

	Private Sub Projection()
		Match(TokenType._LeftAngleSquare)
		Expr()
		Push()
		Match(TokenType._RightEqualsAngle)
		Emit("Stack.Push(New List(Of Object)(ProjectionOutput)) : ProjectionOutput.Clear()")
		Emit("For Each ProjectionIterator As Object In _Concat()")
		IndentLevel += 1
		Expr()
		Emit("ProjectionOutput.Add(Register)")
		IndentLevel -= 1
		Emit("Next")
		Emit("Register = ProjectionOutput : ProjectionOutput = Stack.Pop()")
		Match(TokenType._RightSquareAngle)
	End Sub

	Private Sub BooleanExpr()
		If Lexer.Current.Type = TokenType._Boolean Then
			Emit($"Register = {Match(TokenType._Boolean)}")
		ElseIf Lexer.Current.Type = TokenType.Not Then
			Match(TokenType.Not)
			BooleanExpr()
			Emit($"Register = Not Register")
		Else
			Try
				Try
					[Boolean]()
				Catch ex As ArgumentException
					Expr()
				End Try
			Catch ex As ArgumentException
				BooleanExpr()
				Push()
				Dim op = Lexer.Current.Type
				Match(op)
				BooleanExpr()
				Pop(op)
			End Try
		End If
	End Sub

	Private Sub [Boolean]()
		Emit($"Register = {Match(TokenType._Boolean)}")
	End Sub

	Private Sub Push()
		Emit("Stack.Push(Register)")
	End Sub

	Private Sub Pop(Optional optype As TokenType? = Nothing)
		If optype Is Nothing Then
			Emit("Register = Stack.Pop()")
			Return
		End If
		Dim op$ = ""
		Select Case optype.Value
			'Numeric -> Numeric type operators
			Case TokenType._Cross
				op = "+"
			Case TokenType._Hyphen
				op = "-"
			Case TokenType._Asterisk
				op = "*"
			Case TokenType._Slash
				op = "/"
			Case TokenType._BackSlash
				op = "\"
			Case TokenType._Percent
				op = "Mod"

			'Numeric -> Boolean
			Case TokenType._LeftAngle
				op = "<"
			Case TokenType._LeftAngleEquals
				op = "<="
			Case TokenType._RightAngle
				op = ">"
			Case TokenType._RightAngleEquals
				op = ">="
			Case TokenType._Equals
				Emit("Register = _Equality(Stack.Pop(), Register)")
				Return

			'Boolean -> Boolean
			Case TokenType._Ampersand
				op = "And"
			Case TokenType._Pipe
				op = "Or"
			Case TokenType._Caret
				op = "Xor"
			Case TokenType.And
				op = "AndAlso"
			Case TokenType.Or
				op = "OrElse"
		End Select
		Emit($"Register = Stack.Pop() {op} Register")
	End Sub
End Module