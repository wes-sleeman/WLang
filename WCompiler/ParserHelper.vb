Partial Module Parser
	Private Function Match(Type As TokenType, Optional Advance As Boolean = True) As String
		If Not Lexer.Current.Type = Type Then
			Throw New ArgumentException($"Error: Expected {Type} but received {Lexer.Current.Type}.")
		End If

		Dim retval$ = Lexer.Current.Value
		If Advance Then Lexer.Advance()
		Return retval
	End Function

	Private IndentLevel% = 0

	Private Sub AddLib(filepath$)
		If filepath.ToLower.StartsWith("runtime.") Then
			Emit($"Assemblies.Add(GetType(Runtime.{filepath.Substring(filepath.IndexOf("."c) + 1)}).Assembly)")
		Else
			filepath = IO.Path.GetFullPath(filepath & ".dll")
			References &= $",""{filepath}"""
			Emit($"Assemblies.Add(Assembly.LoadFile({filepath}))")
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
Module {Filename}
	Dim Register As Object
	Dim Parent As Object
	Dim Counter% = 0
	Dim LoopEnd% = 0
	ReadOnly Variable As New Dictionary(Of String, Object)
	ReadOnly Functions As New Dictionary(Of String, Object)
	ReadOnly Stack As New Stack(Of Object)
	ReadOnly Assemblies As New List(Of Assembly) From {{ GetType({Filename}).Assembly }}
	Function InvokeMethod(name$, ParamArray args As Object()) As Object
		For Each asm In Assemblies
			For Each type In asm.GetTypes()
				Try
					Dim AsmName$ = Asm.GetName().Name
					Return Asm.GetType(AsmName & ""."" & type.Name, True, True).GetMethod(name).Invoke(Nothing, args)
				Catch ex As TypeLoadException : Catch ex As NullReferenceException
				End Try
			Next
		Next
		Throw New MissingMethodException(""Unable to find method "" & name)
	End Function

	Sub Main()

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

		While Lexer.Current.Value Like "[*/%]"
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
				Emit($"Register = Variable(""{Match(TokenType.Variable)}"")")

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
					Emit($"Register = CBool(Register)")
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