using Collections = System.Collections.Generic;
using Reflect = System.Reflection;
using Emit = System.Reflection.Emit;
using IO = System.IO;

public sealed class CodeGen
{
	Emit.ILGenerator il = null;
	Collections.Dictionary<string, Emit.LocalBuilder> symbolTable;

	public CodeGen(Stmt stmt, string moduleName)
	{
		if (IO.Path.GetFileName(moduleName) != moduleName)
		{
			throw new System.Exception("can only output into current directory!");
		}

		Reflect.AssemblyName name = new Reflect.AssemblyName(IO.Path.GetFileNameWithoutExtension(moduleName));
		Emit.AssemblyBuilder asmb = System.AppDomain.CurrentDomain.DefineDynamicAssembly(name, Emit.AssemblyBuilderAccess.Save);
		Emit.ModuleBuilder modb = asmb.DefineDynamicModule(moduleName);
		Emit.TypeBuilder typeBuilder = modb.DefineType("Program");

		Emit.MethodBuilder methb = typeBuilder.DefineMethod("Main", Reflect.MethodAttributes.Static, typeof(void), System.Type.EmptyTypes);

		// CodeGenerator
		this.il = methb.GetILGenerator();
		this.symbolTable = new Collections.Dictionary<string, Emit.LocalBuilder>();

		// Go Compile!
		this.declareTemp();
		this.GenStmt(stmt);

		il.Emit(Emit.OpCodes.Ret);
		typeBuilder.CreateType();
		modb.CreateGlobalFunctions();
		asmb.SetEntryPoint(methb);
		asmb.Save(moduleName);
		this.symbolTable = null;
		this.il = null;
	}


	private void GenStmt(Stmt stmt)
	{
		if (stmt is Sequence)
		{
			Sequence seq = (Sequence)stmt;
			this.GenStmt(seq.First);
			this.GenStmt(seq.Second);
		}
		else if (stmt is Assign)
		{
			Assign assign = (Assign)stmt;
			this.GenExpr(assign.Expr, this.TypeOfExpr(assign.Expr));
			this.Store(assign.Ident, this.TypeOfExpr(assign.Expr));
		}
		else if (stmt is Color)
		{
			// Sub-group to deal with colors

			if (stmt is ResetColor)
			{
				this.il.Emit(Emit.OpCodes.Call, typeof(System.Console).GetMethod("ResetColor"));
			}
			else if (stmt is TextBackColor)
			{
				convertColor((System.ConsoleColor)(((TextBackColor)stmt).color));
				this.il.Emit(Emit.OpCodes.Call, typeof(System.Console).GetMethod("set_BackgroundColor", new System.Type[] { typeof(System.ConsoleColor) }));

			}
			else if (stmt is TextForeColor)
			{
				convertColor((System.ConsoleColor)(((TextForeColor)stmt).color));
				this.il.Emit(Emit.OpCodes.Call, typeof(System.Console).GetMethod("set_ForegroundColor", new System.Type[] { typeof(System.ConsoleColor) }));
			}
		}
		else if (stmt is DeclareVar)
		{
			// declare a local
			DeclareVar declare = (DeclareVar)stmt;
			this.symbolTable[declare.Ident] = this.il.DeclareLocal(this.TypeOfExpr(declare.Expr));

			// set the initial value
			Assign assign = new Assign();
			assign.Ident = declare.Ident;
			assign.Expr = declare.Expr;
			this.GenStmt(assign);
		}
		else if (stmt is Pause)
		{
			// "Pause" is used to delay the program.
			// it uses System.Threading.Thread.Sleep
			this.GenExpr(((Pause)stmt).Duration, typeof(int));
			this.il.Emit(Emit.OpCodes.Call, typeof(System.Threading.Thread).GetMethod("Sleep", new System.Type[] { typeof(int) }));
		}
		else if (stmt is Print)
		{
			// the "type" statement is an alias for System.Console.Write 
			// it uses the string case
			this.GenExpr(((Print)stmt).Expr, typeof(string));
			this.il.Emit(Emit.OpCodes.Call, typeof(System.Console).GetMethod("Write", new System.Type[] { typeof(string) }));
		}
		else if (stmt is PrintReturn)
		{
			// the "newline" statement is an alias for System.Console.WriteLine 
			// it uses the string case
			this.il.Emit(Emit.OpCodes.Ldstr, "");
			this.il.Emit(Emit.OpCodes.Call, typeof(System.Console).GetMethod("WriteLine", new System.Type[] { typeof(string) }));
		}
		else if (stmt is ReadNum)
		{
			this.il.Emit(Emit.OpCodes.Call, typeof(System.Console).GetMethod("ReadLine", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new System.Type[] { }, null));
			try
			{
				this.il.Emit(Emit.OpCodes.Call, typeof(int).GetMethod("Parse", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new System.Type[] { typeof(string) }, null));
			}
			catch(System.ArgumentException)
			{

			}
			this.Store(((ReadNum)stmt).Ident, typeof(int));
		}
		else if (stmt is Read)
		{
			this.il.Emit(Emit.OpCodes.Call, typeof(System.Console).GetMethod("ReadLine", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new System.Type[] { }, null));
			this.Store(((Read)stmt).Ident, typeof(string));
		}
		else if (stmt is Refresh)
		{
			this.il.Emit(Emit.OpCodes.Call, typeof(System.Console).GetMethod("Clear"));
		}

		else if (stmt is ForLoop)
		{
			// example: 
			// repeat 10 [
			//   print "hello"
			// ]

			// x = 0
			ForLoop forLoop = (ForLoop)stmt;
			int cntr = 0;
			int loopTo = 0;
			if (!int.TryParse(forLoop.To, out loopTo))
			{
				string parseTest = this.symbolTable[forLoop.To].ToString().Substring(this.symbolTable[forLoop.To].ToString().IndexOf("(") + 1, (this.symbolTable[forLoop.To].ToString().Length - 1) - (this.symbolTable[forLoop.To].ToString().IndexOf("(") + 1));
				if (!int.TryParse(forLoop.To, out loopTo))
				{
					throw new System.Exception("Cannot evaluate variable " + forLoop.To + " to a number to be repeated. Got:" + parseTest);
				}
			}

			// jump to the test
			Emit.Label test = this.il.DefineLabel();
			this.il.Emit(Emit.OpCodes.Br, test);

			// statements in the body of the for loop
			Emit.Label body = this.il.DefineLabel();
			this.il.MarkLabel(body);
			this.GenStmt(forLoop.Body);

			//System.Console.WriteLine(cntr);

			// **test** does x equal 100? (do the test)
			this.il.MarkLabel(test);
			this.il.Emit(Emit.OpCodes.Ldloc, cntr);
			this.il.Emit(Emit.OpCodes.Ldc_I4, loopTo);
			this.il.Emit(Emit.OpCodes.Blt, body);
			//}
		}

		else if (stmt is Conditional)
		{
			// example: 
			// if x = 1 [
			//   print "hello"
			// ]

			Conditional cond = (Conditional)stmt;
			byte exprA, exprB;
			int ExprA = 0;
			int ExprB = 0;
			exprA = 0;
			exprB = 0;
			
			//System.Console.WriteLine (cond.ExprA.ToString());
			if (cond.ExprA is StringLiteral) {
				exprA = (byte)(char)((StringLiteral)cond.ExprA).Value.ToCharArray().GetValue(0);
			}
			else if (!int.TryParse(TypeOfExpr(cond.ExprA).ToString(), out ExprA))
			{
				string parseTest = this.symbolTable[((Variable)cond.ExprA).Ident].ToString().Substring(this.symbolTable[((Variable)cond.ExprA).Ident].ToString().IndexOf("(") + 1, (this.symbolTable[((Variable)cond.ExprA).Ident].ToString().Length - 1) - (this.symbolTable[((Variable)cond.ExprA).Ident].ToString().IndexOf("(") + 1));
				if (!int.TryParse(parseTest, out ExprA))
				{
					throw new System.Exception("Cannot evaluate variable " + cond.ExprA + " to a number to be tested. Got:" + parseTest);
				}
			}
			else if (int.TryParse(TypeOfExpr(cond.ExprA).ToString(), out ExprA))
			{
				exprA = (byte)ExprA;
			}
			else throw new System.Exception("Cannot evaluate if statement part a.");

			//System.Console.WriteLine (cond.ExprB.ToString());

			if (cond.ExprB is StringLiteral) {
				exprB = (byte)int.Parse(((StringLiteral)cond.ExprB).Value.ToString());
			}
			else if (!int.TryParse(TypeOfExpr(cond.ExprB).ToString(), out ExprB))
			{
				string parseTest = this.symbolTable[((Variable)cond.ExprB).Ident].ToString().Substring(this.symbolTable[((Variable)cond.ExprB).Ident].ToString().IndexOf("(") + 1, (this.symbolTable[((Variable)cond.ExprB).Ident].ToString().Length - 1) - (this.symbolTable[((Variable)cond.ExprB).Ident].ToString().IndexOf("(") + 1));
				if (!int.TryParse(parseTest, out ExprB))
				{
					throw new System.Exception("Cannot evaluate variable " + cond.ExprB + " to a number to be tested. Got:" + parseTest);
				}
			}
			else if (int.TryParse(TypeOfExpr(cond.ExprB).ToString(), out ExprB))
			{
				exprB = (byte)ExprB;
			}
			else throw new System.Exception("Cannot evaluate if statement part b.");

			//System.Console.WriteLine(exprA + exprB);

			// jump to the test
			Emit.Label test = this.il.DefineLabel();
			Emit.Label End = this.il.DefineLabel();
			Emit.Label True = this.il.DefineLabel();
			this.il.Emit(Emit.OpCodes.Br, test);

			// **test** does x equal 100? (do the test)
			this.il.MarkLabel(test);
			this.il.Emit(Emit.OpCodes.Ldloc, exprA);
			this.il.Emit(Emit.OpCodes.Ldc_I4, exprB);
			if (cond.Comp == BinComp.Equal) {
				this.il.Emit(Emit.OpCodes.Beq, True);
			}
			else if (cond.Comp == BinComp.Greater) {
				this.il.Emit(Emit.OpCodes.Bgt, True);
			}
			else if (cond.Comp == BinComp.Less) {
				this.il.Emit(Emit.OpCodes.Blt, True);
			}
			else if (cond.Comp == BinComp.NotEqual) {
				this.il.Emit(Emit.OpCodes.Bne_Un, True);
			}

			// statements in the body of the if block
			this.il.MarkLabel(True);
			this.GenStmt(cond.True);
			this.il.Emit(Emit.OpCodes.Br, End);
			//}
			this.il.MarkLabel(End);
		}
		else
		{
			throw new System.Exception("don't know how to gen a " + stmt.GetType().Name);
		}




	}    

	private void Store(string name, System.Type type)
	{
		if (this.symbolTable.ContainsKey(name))
		{
			Emit.LocalBuilder locb = this.symbolTable[name];

			if (locb.LocalType == type)
			{
				this.il.Emit(Emit.OpCodes.Stloc, this.symbolTable[name]);
			}
			else
			{
				throw new System.Exception("'" + name + "' is of type " + locb.LocalType.Name + " but attempted to store value of type " + type.Name);
			}
		}
		else
		{
			throw new System.Exception("undeclared variable '" + name + "'");
		}
	}



	private void GenExpr(object expr, System.Type expectedType)
	{
		System.Type deliveredType;

		if (expr is StringLiteral)
		{
			deliveredType = typeof(string);
			this.il.Emit(Emit.OpCodes.Ldstr, ((StringLiteral)expr).Value);
		}
		else if (expr is IntLiteral)
		{
			deliveredType = typeof(int);
			this.il.Emit(Emit.OpCodes.Ldc_I4, ((IntLiteral)expr).Value);
		}        
		else if (expr is Variable)
		{
			string ident = ((Variable)expr).Ident;
			deliveredType = this.TypeOfExpr(expr);

			if (!this.symbolTable.ContainsKey(ident))
			{
				throw new System.Exception("undeclared variable '" + ident + "'");
			}
			//System.Console.WriteLine(this.symbolTable[ident]);

			this.il.Emit(Emit.OpCodes.Ldloc, this.symbolTable[ident]);
		}
		else if (expr.ToString().Contains("+"))
		{
			int signIndex = expr.ToString().IndexOf('+');
			StringLiteral tempStringLit = new StringLiteral();
			tempStringLit.Value = expr.ToString().Substring(0, signIndex);
			GenExpr(tempStringLit, typeof(string));
			tempStringLit.Value = expr.ToString().Substring(signIndex + 1);
			GenExpr(tempStringLit, typeof(string));
			this.il.Emit(Emit.OpCodes.Add);
			deliveredType = typeof(string);
		}
		else
		{
			throw new System.Exception("don't know how to generate " + expr.GetType().Name);
		}

		if (deliveredType != expectedType)
		{
			if (deliveredType == typeof(int) &&
			    expectedType == typeof(string))
			{
				this.il.Emit(Emit.OpCodes.Box, typeof(int));
				this.il.Emit(Emit.OpCodes.Callvirt, typeof(object).GetMethod("ToString"));
			}
			else
			{
				throw new System.Exception("can't coerce a " + deliveredType.Name + " to a " + expectedType.Name);
			}
		}

	}



	private System.Type TypeOfExpr(object expr)
	{
		if (expr is StringLiteral)
		{
			return typeof(string);
		}
		else if (expr is IntLiteral)
		{
			return typeof(int);
		}
		else if (expr is Variable)
		{
			Variable var = (Variable)expr;
			if (this.symbolTable.ContainsKey(var.Ident))
			{
				Emit.LocalBuilder locb = this.symbolTable[var.Ident];
				return locb.LocalType;
			}
			else
			{
				throw new System.Exception("undeclared variable '" + var.Ident + "'");
			}
		}
		else
		{
			throw new System.Exception("don't know how to calculate the type of " + expr.GetType().Name);
		}
	}
	private object TypeOfExprEquivalent(object expr)
	{
		if (expr is StringLiteral)
		{
			return (string)(((StringLiteral)expr).Value);
		}
		else if (expr is IntLiteral)
		{
			return (int)(((IntLiteral)expr).Value);
		}
		else if (expr is Variable)
		{
			Variable var = (Variable)expr;
			if (this.symbolTable.ContainsKey(var.Ident))
			{
				Emit.LocalBuilder locb = this.symbolTable[var.Ident];
				return locb;
			}
			else
			{
				throw new System.Exception("undeclared variable '" + var.Ident + "'");
			}
		}
		else
		{
			throw new System.Exception("don't know how to calculate the type of " + expr.GetType().Name);
		}
	}
	private Expr IntToExpr(int input)
	{
		int intValue = input;
		IntLiteral intLiteral = new IntLiteral();
		intLiteral.Value = intValue;
		return intLiteral;
	}
	private string varName(Expr input)
	{
		Variable var = (Variable)input;
		return var.Ident;
	}
	private void declareTemp()
	{
		DeclareVar stmt = new DeclareVar();
		StringLiteral strLit = new StringLiteral();
		strLit.Value = "";
		stmt.Ident = "temp";
		stmt.Expr = strLit;
		GenStmt(stmt);

		IntLiteral intLit = new IntLiteral();
		intLit.Value = 0;
		stmt.Ident = "tempnum";
		stmt.Expr = intLit;
		GenStmt(stmt);
	}
	private void convertColor(System.ConsoleColor input)
	{
		switch (input)
		{
			case System.ConsoleColor.Black:
			this.il.Emit(Emit.OpCodes.Ldc_I4_0);
			break;
			case System.ConsoleColor.DarkBlue:
			this.il.Emit(Emit.OpCodes.Ldc_I4_1);
			break;
			case System.ConsoleColor.DarkGreen:
			this.il.Emit(Emit.OpCodes.Ldc_I4_2);
			break;
			case System.ConsoleColor.DarkCyan:
			this.il.Emit(Emit.OpCodes.Ldc_I4_3);
			break;
			case System.ConsoleColor.DarkRed:
			this.il.Emit(Emit.OpCodes.Ldc_I4_4);
			break;
			case System.ConsoleColor.DarkMagenta:
			this.il.Emit(Emit.OpCodes.Ldc_I4_5);
			break;
			case System.ConsoleColor.DarkYellow:
			this.il.Emit(Emit.OpCodes.Ldc_I4_6);
			break;
			case System.ConsoleColor.Gray:
			this.il.Emit(Emit.OpCodes.Ldc_I4_7);
			break;
			case System.ConsoleColor.DarkGray:
			this.il.Emit(Emit.OpCodes.Ldc_I4_8);
			break;
			case System.ConsoleColor.Blue:
			this.il.Emit(Emit.OpCodes.Ldc_I4_S, 9);
			break;
			case System.ConsoleColor.Green:
			this.il.Emit(Emit.OpCodes.Ldc_I4_S, 10);
			break;
			case System.ConsoleColor.Cyan:
			this.il.Emit(Emit.OpCodes.Ldc_I4_S, 11);
			break;
			case System.ConsoleColor.Red:
			this.il.Emit(Emit.OpCodes.Ldc_I4_S, 12);
			break;
			case System.ConsoleColor.Magenta:
			this.il.Emit(Emit.OpCodes.Ldc_I4_S, 13);
			break;
			case System.ConsoleColor.Yellow:
			this.il.Emit(Emit.OpCodes.Ldc_I4_S, 14);
			break;
			case System.ConsoleColor.White:
			this.il.Emit(Emit.OpCodes.Ldc_I4_S, 15);
			break;
			default:
			throw new System.Exception("Unrecognised color.");
		}
	}
//	public string ConvVarIntToString (
}
