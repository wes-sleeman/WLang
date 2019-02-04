Partial Module Parser
	Private Function Match(Type As TokenType, Optional Advance As Boolean = True) As String
		If Not Lexer.Current.Type = Type Then
			Throw New ArgumentException($"Expected {Type} but received {Lexer.Current.Type}.")
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
				filepath = IO.Path.GetFullPath(filepath & ".dll")
			References &= $",""{filepath}"""
			If typename Is Nothing Then
				Emit($"Types.AddRange(Assembly.LoadFile({filepath}).GetExportedTypes())")
			Else
				Emit($"Types.AddRange(Assembly.LoadFile({filepath}).GetType({typename}, False, True))")
			End If
		End If
    End Sub

	Private Sub Emit(output$)
		Dim tabBuffer$ = String.Empty
		For cntr% = 1 To IndentLevel
			tabBuffer &= vbTab
		Next
		outputbuffer.Add(tabBuffer & output)
	End Sub

	Private Sub Setup()
		Emit($"Imports System : Imports System.Collections.Generic : Imports System.Reflection
Public Module {Filename}
	Dim Register As Object
	Dim Parent As Object
	Dim Counter% = 0
	Dim LoopEnd% = 0
	Dim FuncArgs As New List(Of Object)
	ReadOnly Variable As New Dictionary(Of String, Object)
	ReadOnly Functions As New Dictionary(Of String, Object)
	ReadOnly Stack As New Stack(Of Object)
	ReadOnly Types As New List(Of Type)
	Function InvokeMethod(name$, args As List(Of Object)) As Object
		Dim ArgArr = args.ToArray()
		For Each type In Types
			Try
				Return type.GetMethod(name).Invoke(Nothing, ArgArr)
			Catch ex As TypeLoadException : Catch ex As NullReferenceException
			End Try
		Next
		Throw New MissingMethodException(""Unable to find method "" & name)
	End Function

	Public Sub Main(args As String())
		Variable(""args"") = args
		Types.AddRange(GetType({Filename}).Assembly.GetTypes())

		'BEGIN USER GENERATED CODE")
		IndentLevel = 2
	End Sub

	Private Sub Teardown()
		IndentLevel = 0
		Emit(
"		'END USER GENERATED CODE

		Console.WriteLine(""Press any key to continue..."")
		Console.ReadKey()
	End Sub
End Module")
	End Sub

	Private Sub Expr()
		Term()

		While Lexer.Current.Value Like "[+-]"
			Push()
			Dim op = Lexer.Current.Type
			Match(op)
			Term()
			Pop(op)
		End While

		While Lexer.Current.Value Like "[<>]=" OrElse Lexer.Current.Value Like "[<>=]"
			Push()
			Dim op = Lexer.Current.Type
			Match(op)
			Expr()
			Pop(op)
		End While
	End Sub

	Private Sub Term()
		SignedFactor()

		While Lexer.Current.Value Like "[*/%\]"
			Push()
			Dim op = Lexer.Current.Type
			Match(op)
			SignedFactor()
			Pop(op)
		End While
	End Sub

	Private Sub SignedFactor()
		Dim op = TokenType.Cross
		If Lexer.Current.Value Like "[+-]" Then
			op = Lexer.Current.Type
			Match(op)
		End If

		Factor()

		If op = TokenType.Hyphen Then
			Emit("Register = -Register")
		End If
	End Sub

	Private Sub Factor()
		Select Case Lexer.Current.Type
			Case TokenType.IntLiteral
				Emit($"Register = {Match(TokenType.IntLiteral)}")

			Case TokenType.StringLiteral
				Emit($"Register = ""{Match(TokenType.StringLiteral)}""")

			Case TokenType.Variable
				Dim name$ = Match(TokenType.Variable, False)
				If Varlist.Contains(name.ToLower) Then
					Lexer.Advance()
					Emit($"Register = Variable(""{name.ToLower}"")")
				Else
					FunctionCall()
				End If

			Case TokenType.Dollar
				Match(TokenType.Dollar)
				Emit("Register = Parent")

			Case TokenType.HashSign
				Match(TokenType.HashSign)
				Emit("Register = Counter")

			Case Else
				Match(TokenType.LeftParen)
				Expr()
				Match(TokenType.RightParen)
		End Select

		If Lexer.Current.Type = TokenType.Dot Then
			Match(TokenType.Dot)
			CheckProperty()
		End If
	End Sub

	Private Sub CheckProperty()
		Select Case Lexer.Current.Type
			Case TokenType.LeftParen
				Push()
				Expr()
				Emit("Register = (Stack.Pop())(Register)")

			Case TokenType.Variable
				Select Case Lexer.Current.Value.ToLower()
					Case "num"
						Emit("Try : Register = Register.Length : Catch : Register = Register.Count : End Try")
						Match(TokenType.Variable)
					Case "pos"
						Match(TokenType.Variable)
						Push()
						Expr()
						Emit("Register = New List(Of Object)(CType(Stack.Pop(), IEnumerable(Of Object))).IndexOf(Register)")
					Case Else
						Emit("Register = Register." & Match(TokenType.Variable))
				End Select

			Case TokenType.IntLiteral
				Emit($"Register = Register({Match(TokenType.IntLiteral)})")

			Case Else
				Throw New ArgumentException($"Got unexpected {Lexer.Current.Type} after dot.")
		End Select
	End Sub

	Private Sub BooleanExpr()
		If Lexer.Current.Type = TokenType.Boolean Then
			Emit($"Register = {Match(TokenType.Boolean)}")
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
		Emit($"Register = {Match(TokenType.Boolean)}")
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
			Case TokenType.Cross
				op = "+"
			Case TokenType.Hyphen
				op = "-"
			Case TokenType.Asterisk
				op = "*"
			Case TokenType.Slash
				op = "/"
			Case TokenType.BackSlash
				op = "\"
			Case TokenType.Percent
				op = "Mod"

			'Numeric -> Boolean
			Case TokenType.LeftAngle
				op = "<"
			Case TokenType.LeftAngleEquals
				op = "<="
			Case TokenType.RightAngle
				op = ">"
			Case TokenType.RightAngleEquals
				op = ">="
			Case TokenType.Equals
				op = "="

			'Boolean -> Boolean
			Case TokenType.Ampersand
				op = "And"
			Case TokenType.Pipe
				op = "Or"
			Case TokenType.And
				op = "AndAlso"
			Case TokenType.Or
				op = "OrElse"
		End Select
		Emit($"Register = Stack.Pop() {op} Register")
	End Sub
End Module