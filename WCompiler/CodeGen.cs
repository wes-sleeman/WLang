using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;

public sealed class CodeGen
{
	ILGenerator il = null;
	Dictionary<string, LocalBuilder> symbolTable;

	public CodeGen(Stmt stmt, string moduleName)
	{
		if (Path.GetFileName(moduleName) != moduleName)
		{
			throw new System.Exception("can only output into current directory!");
		}

		AssemblyName name = new AssemblyName(Path.GetFileNameWithoutExtension(moduleName));
		AssemblyBuilder asmb = System.AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Save);
		ModuleBuilder modb = asmb.DefineDynamicModule(moduleName);
		TypeBuilder typeBuilder = modb.DefineType("Program");

		MethodBuilder methb = typeBuilder.DefineMethod("Main", MethodAttributes.Static, typeof(void), System.Type.EmptyTypes);

        // CodeGenerator
        il = methb.GetILGenerator();
        symbolTable = new Dictionary<string, LocalBuilder>();

        // Go Compile!
        DeclareTemp();
		GenStmt(stmt);

		il.Emit(OpCodes.Ret);
		typeBuilder.CreateType();
		modb.CreateGlobalFunctions();
		asmb.SetEntryPoint(methb);
		asmb.Save(moduleName);
		symbolTable = null;
		il = null;
	}


	private void GenStmt(Stmt stmt)
	{
		if (stmt is Sequence seq)
		{
            GenStmt(seq.First);
			GenStmt(seq.Second);
		}
		else if (stmt is Assign assign)
		{
			GenExpr(assign.Expr, TypeOfExpr(assign.Expr));
			Store(assign.Ident, TypeOfExpr(assign.Expr));
		}
		else if (stmt is Color)
		{
			// sub-group to deal with colors

			if (stmt is ResetColor)
			{
				il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("ResetColor"));
			}
			else if (stmt is TextBackColor)
			{
				ConvertColor((System.ConsoleColor)(((TextBackColor)stmt).color));
				il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("set_BackgroundColor", new System.Type[] { typeof(System.ConsoleColor) }));

			}
			else if (stmt is TextForeColor)
			{
				ConvertColor(((TextForeColor)stmt).color);
				il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("set_ForegroundColor", new System.Type[] { typeof(System.ConsoleColor) }));
			}
		}
		else if (stmt is DeclareVar declare)
		{
			// example:
			// Item var
			// ?Creates a new variable named var
			symbolTable[declare.Ident] = il.DeclareLocal(TypeOfExpr(declare.Expr));

            // set the initial value
            // sets to $ if nothing provided
            Assign tmpAssign = new Assign()
            {
                Ident = declare.Ident,
                Expr = declare.Expr
            };
            GenStmt(tmpAssign);
		}
		else if (stmt is Pause)
		{
			// example:
			// Pause 5
			// ?Pauses for 5 seconds
			GenExpr(((Pause)stmt).Duration, typeof(int));
			il.Emit(OpCodes.Call, typeof(System.Threading.Thread).GetMethod("Sleep", new System.Type[] { typeof(int) }));
		}
		else if (stmt is Print)
		{
			// example:
			// Type "Hello, world"
			// ?Writes "Hello, world" to console
			GenExpr(((Print)stmt).Expr, typeof(string));
			il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("Write", new System.Type[] { typeof(string) }));
		}
		else if (stmt is PrintReturn)
		{
			// used without arguments, prints newline char
			il.Emit(OpCodes.Ldstr, "");
			il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("WriteLine", new System.Type[] { typeof(string) }));
		}
		else if (stmt is ReadNum)
		{
			// reads from console and parses as decimal
			il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("ReadLine", BindingFlags.Public | BindingFlags.Static, null, new System.Type[] { }, null));
			try
			{
				il.Emit(OpCodes.Call, typeof(decimal).GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new System.Type[] { typeof(string) }, null));
			}
			catch(System.ArgumentException)
			{

			}
			Store(((ReadNum)stmt).Ident, typeof(decimal));
		}
		else if (stmt is Read)
		{
			// reads from console, accepts $
			// example:
			// read x
			// ?Puts input into variable x
			il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("ReadLine", BindingFlags.Public | BindingFlags.Static, null, new System.Type[] { }, null));
            Store(((Read)stmt).Ident, typeof(string));
		}
		else if (stmt is Refresh)
		{
			il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("Clear"));
		}

		else if (stmt is ForLoop forLoop)
        {
          // example: 
          // repeat 10 [
          //   print "hello"
          // ]
          // x = 0
            int cntr = 0;
            if (!int.TryParse(forLoop.To, out int loopTo))
            {
                string parseTest = symbolTable[forLoop.To].ToString().Substring(symbolTable[forLoop.To].ToString().IndexOf("(") + 1, (symbolTable[forLoop.To].ToString().Length - 1) - (symbolTable[forLoop.To].ToString().IndexOf("(") + 1));
                if (!int.TryParse(forLoop.To, out loopTo))
                {
                    throw new System.Exception("Cannot evaluate variable " + forLoop.To + " to a number to be repeated. Got:" + parseTest);
                }
            }

            // jump to the test
            Label test = il.DefineLabel();
            il.Emit(OpCodes.Br, test);

            // statements in the body of the for loop
            Label body = il.DefineLabel();
            il.MarkLabel(body);
            GenStmt(forLoop.Body);

            // performs actual test
            il.MarkLabel(test);
            il.Emit(OpCodes.Ldloc, cntr);
            il.Emit(OpCodes.Ldc_I4, loopTo);
            il.Emit(OpCodes.Blt, body);
        }

        else if (stmt is Conditional cond)
        {
            // example: 
            // if x = 1 [
            //   print "hello"
            // ]
            
            string exprA = "", exprB = "";
            int ExprA = 0, ExprB = 0;

            if (cond.ExprA is StringLiteral)
            {
                exprA = ((StringLiteral)cond.ExprA).Value;
            }
            else if (!int.TryParse(TypeOfExpr(cond.ExprA).ToString(), out ExprA))
            {
                string parseTest = symbolTable[((Variable)cond.ExprA).Ident].ToString().Substring(symbolTable[((Variable)cond.ExprA).Ident].ToString().IndexOf("(") + 1, (symbolTable[((Variable)cond.ExprA).Ident].ToString().Length - 1) - (symbolTable[((Variable)cond.ExprA).Ident].ToString().IndexOf("(") + 1));
                if (!int.TryParse(parseTest, out ExprA))
                {
                    throw new System.Exception("Cannot evaluate variable " + cond.ExprA + " to a number to be tested. Got:" + parseTest);
                }
                exprA = parseTest;
            }
            else if (int.TryParse(TypeOfExpr(cond.ExprA).ToString(), out ExprA))
            {
                exprA = ((byte)ExprA).ToString();
            }
            else throw new System.Exception("Cannot evaluate if statement part a.");

            if (cond.ExprB is StringLiteral)
            {
                exprB = ((StringLiteral)cond.ExprB).Value;
            }
            else if (!int.TryParse(TypeOfExpr(cond.ExprB).ToString(), out ExprB))
            {
                string parseTest = symbolTable[((Variable)cond.ExprB).Ident].ToString().Substring(symbolTable[((Variable)cond.ExprB).Ident].ToString().IndexOf("(") + 1, (symbolTable[((Variable)cond.ExprB).Ident].ToString().Length - 1) - (symbolTable[((Variable)cond.ExprB).Ident].ToString().IndexOf("(") + 1));
                if (!int.TryParse(parseTest, out ExprB))
                {
                    throw new System.Exception("Cannot evaluate variable " + cond.ExprB + " to a number to be tested. Got:" + parseTest);
                }
                exprB = parseTest;
            }
            else if (int.TryParse(TypeOfExpr(cond.ExprB).ToString(), out ExprB))
            {
                exprB = ((byte)ExprB).ToString();
            }
            else throw new System.Exception("Cannot evaluate if statement part b.");


            // jump to the test
            Label test = il.DefineLabel();
            Label End = il.DefineLabel();
            Label True = il.DefineLabel();
            il.Emit(OpCodes.Br, test);

            // perform the actual test
            il.MarkLabel(test);

            // checks for the type of the expressions to load the proper value
            if (cond.ExprA is Variable)
            {
                il.Emit(OpCodes.Ldloc_S, byte.Parse(exprA));
            }
            else if (int.TryParse(exprA, out int value))
            {
                il.Emit(OpCodes.Ldc_I4, value);
            }
            else if (cond.ExprA is StringLiteral)
            {
                il.Emit(OpCodes.Ldstr, exprA);
            }
            else throw new System.Exception("Cannot parse if statement part a.");

            if (cond.ExprB is Variable)
            {
                il.Emit(OpCodes.Ldloc_S, byte.Parse(exprB));
            }
            else if (int.TryParse(exprB, out int value))
            {
                il.Emit(OpCodes.Ldc_I4, value);
            }
            else if (cond.ExprB is StringLiteral)
            {
                il.Emit(OpCodes.Ldstr, exprB);
            }
            else throw new System.Exception("Cannot parse if statement part b.");

            // drops in the proper comparator
            if (cond.Comp == BinComp.Equal)
            {
                il.Emit(OpCodes.Beq, True);
                il.Emit(OpCodes.Br, End);
            }
            else if (cond.Comp == BinComp.Greater)
            {
                il.Emit(OpCodes.Bgt, True);
                il.Emit(OpCodes.Br, End);
            }
            else if (cond.Comp == BinComp.Less)
            {
                il.Emit(OpCodes.Blt, True);
                il.Emit(OpCodes.Br, End);
            }
            else if (cond.Comp == BinComp.NotEqual)
            {
                il.Emit(OpCodes.Bne_Un, True);
                il.Emit(OpCodes.Br, End);
            }

            // statements in the body of the if block
            il.MarkLabel(True);
            GenStmt(cond.True);
            il.Emit(OpCodes.Br, End);
            il.MarkLabel(End);
        }
        else if (stmt is NullStmt)
        {
            il.Emit(OpCodes.Nop);
        }
        else throw new System.Exception("don't know how to gen a " + stmt.GetType().Name);




    }    

	private void Store(string name, System.Type type)
	{
		if (symbolTable.ContainsKey(name))
		{
			LocalBuilder locb = symbolTable[name];

			if (locb.LocalType == type)
			{
				il.Emit(OpCodes.Stloc, symbolTable[name]);
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
			il.Emit(OpCodes.Ldstr, ((StringLiteral)expr).Value);
		}
		else if (expr is IntLiteral)
		{
			deliveredType = typeof(int);
			il.Emit(OpCodes.Ldc_I4, ((IntLiteral)expr).Value);
		}        
		else if (expr is Variable)
		{
			string ident = ((Variable)expr).Ident;
			deliveredType = TypeOfExpr(expr);

			if (!symbolTable.ContainsKey(ident))
			{
				throw new System.Exception("undeclared variable '" + ident + "'");
			}
			//System.Console.WriteLine(symbolTable[ident]);

			il.Emit(OpCodes.Ldloc, symbolTable[ident]);
		}
		else if (expr.ToString().Contains("+"))
		{
			int signIndex = expr.ToString().IndexOf('+');

            StringLiteral tempStringLit = new StringLiteral()
            {Value = expr.ToString().Substring(0, signIndex)};

            GenExpr(tempStringLit, typeof(string));
			tempStringLit.Value = expr.ToString().Substring(signIndex + 1);
			GenExpr(tempStringLit, typeof(string));
			il.Emit(OpCodes.Add);
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
				il.Emit(OpCodes.Box, typeof(int));
				il.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString"));
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
		else if (expr is Variable var)
		{
			if (symbolTable.ContainsKey(var.Ident))
			{
				LocalBuilder locb = symbolTable[var.Ident];
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
		else if (expr is Variable var)
		{
			if (symbolTable.ContainsKey(var.Ident))
			{
				LocalBuilder locb = symbolTable[var.Ident];
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
        IntLiteral intLiteral = new IntLiteral()
        { Value = intValue };
		return intLiteral;
	}
	private string VarName(Expr input)
	{
		Variable var = (Variable)input;
		return var.Ident;
	}
    private void DeclareTemp()
    {
        DeclareVar stmt = new DeclareVar();

        StringLiteral strLit = new StringLiteral()
        { Value = ""};

		stmt.Ident = "$";
		stmt.Expr = strLit;
		GenStmt(stmt);

		IntLiteral intLit = new IntLiteral()
        { Value = 0};
		stmt.Ident = "$#";
		stmt.Expr = intLit;
		GenStmt(stmt);
	}
	private void ConvertColor(System.ConsoleColor input)
	{
		switch (input)
		{
			case System.ConsoleColor.Black:
			il.Emit(OpCodes.Ldc_I4_0);
			break;
			case System.ConsoleColor.DarkBlue:
			il.Emit(OpCodes.Ldc_I4_1);
			break;
			case System.ConsoleColor.DarkGreen:
			il.Emit(OpCodes.Ldc_I4_2);
			break;
			case System.ConsoleColor.DarkCyan:
			il.Emit(OpCodes.Ldc_I4_3);
			break;
			case System.ConsoleColor.DarkRed:
			il.Emit(OpCodes.Ldc_I4_4);
			break;
			case System.ConsoleColor.DarkMagenta:
			il.Emit(OpCodes.Ldc_I4_5);
			break;
			case System.ConsoleColor.DarkYellow:
			il.Emit(OpCodes.Ldc_I4_6);
			break;
			case System.ConsoleColor.Gray:
			il.Emit(OpCodes.Ldc_I4_7);
			break;
			case System.ConsoleColor.DarkGray:
			il.Emit(OpCodes.Ldc_I4_8);
			break;
			case System.ConsoleColor.Blue:
			il.Emit(OpCodes.Ldc_I4_S, 9);
			break;
			case System.ConsoleColor.Green:
			il.Emit(OpCodes.Ldc_I4_S, 10);
			break;
			case System.ConsoleColor.Cyan:
			il.Emit(OpCodes.Ldc_I4_S, 11);
			break;
			case System.ConsoleColor.Red:
			il.Emit(OpCodes.Ldc_I4_S, 12);
			break;
			case System.ConsoleColor.Magenta:
			il.Emit(OpCodes.Ldc_I4_S, 13);
			break;
			case System.ConsoleColor.Yellow:
			il.Emit(OpCodes.Ldc_I4_S, 14);
			break;
			case System.ConsoleColor.White:
			il.Emit(OpCodes.Ldc_I4_S, 15);
			break;
			default:
			throw new System.Exception("Unrecognised color.");
		}
	}
}
