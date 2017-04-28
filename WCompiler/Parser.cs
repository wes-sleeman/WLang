
using System.Collections.Generic;
using System.Text;

public sealed class Parser
{
	private int index;
	private IList<object> tokens;
	private readonly Stmt result;
	private bool isTempVarNum = false;

	public Parser(IList<object> tokens)
	{
		this.tokens = tokens;
		index = 0;
		result = ParseStmt();

		if (index != tokens.Count)
		{
			throw new System.Exception("expected newline after expression, are you using Windows?");
		}
	}

	public Stmt Result
	{
		get { return result; }
	}

	private Stmt ParseStmt()
	{
		Stmt result;
		Variable temp = new Variable()
        { Ident = "$" };
        Variable tempnum = new Variable()
        { Ident = "$#" };
        
		if (index == tokens.Count)
		{
			throw new System.Exception("expected statement, got EOF");
		}
		if (tokens[index].Equals ("backcolor") || tokens[index].Equals ("backcolour")) {
			index++;
			index++;
            TextBackColor backColor = new TextBackColor()
            { color = (System.ConsoleColor)System.Enum.Parse(typeof(System.ConsoleColor), tokens[index].ToString(), true) };
			result = backColor;
			index++;
		} else if (tokens [index].Equals ("forecolor") || tokens [index].Equals ("forecolour")) {
			index++;
			index++;
			TextForeColor foreColor = new TextForeColor()
            { color = (System.ConsoleColor)System.Enum.Parse(typeof(System.ConsoleColor), tokens[index].ToString(), true) };
			result = foreColor;
			index++;
		} else if (tokens [index].Equals ("item")) {
			index++;
			DeclareVar declareVar = new DeclareVar ();

			if (index < tokens.Count &&
				tokens [index] is string) {
				declareVar.Ident = (string)tokens [index];
			} else {
				throw new System.Exception ("expected variable name after 'item'");
			}

			index++;

			if (index == tokens.Count || tokens [index] != Scanner.Equal) {
				//throw new System.Exception ("expected = after 'item ident'");
				index--;
				tokens [index] = "$";
				declareVar.Expr = ParseExpr();
				result = declareVar;
			} else {
				index++;

				declareVar.Expr = ParseExpr();
				result = declareVar;
			}
		}
		else if (tokens[index].Equals("newline"))
		{
			index++;
			PrintReturn newline = new PrintReturn();
			result = newline;
		}
		else if (tokens[index].Equals("pause"))
		{
			index++;
			Pause pause = new Pause();
			if (index < tokens.Count && tokens[index] is int && tokens[index + 1] == Scanner.Semi)
			{
				pause.Duration = IntToExpr(((IntLiteral)IntToExpr((int)tokens[index])).Value * 1000);
				result = pause;
				index--;
			}
			else if (index < tokens.Count && tokens[index] is int && tokens [index + 1] != Scanner.Semi)
			{
				pause.Duration = IntToExpr(((IntLiteral)IntToExpr((int)tokens[index])).Value * 1000);
				result = pause;
			}
			else
			{
				pause.Duration = IntToExpr(1000);
				result = pause;
			}
		}
		else if (tokens[index].Equals("read"))
		{
			index++;
			Read read = new Read();
			isTempVarNum = false;

			if (index < tokens.Count && tokens[index] is string)
			{
				read.Ident = (string)tokens[index++];
				result = read;
			}
			else
			{
				read.Ident = "$";
				result = read;
			}
		}
		else if (tokens[index].Equals("readnum"))
		{
			index++;
			ReadNum readnum = new ReadNum();
			isTempVarNum = true;

			if (index < tokens.Count && tokens[index] is string)
			{
				readnum.Ident = (string)tokens[index++];
				result = readnum;
			}
			else
			{
				readnum.Ident = "$#";
				result = readnum;
			}
		}
		else if (tokens[index].Equals("refresh") || tokens[index].Equals("reset"))
		{
			index++;
			Refresh refresh = new Refresh();
			result = refresh;
		}
		else if (tokens[index].Equals("repeat"))
		{
			index++;
			ForLoop forLoop = new ForLoop();

			Expr parsedExpr = ParseExpr();
            if (tokens[index + 2].Equals("["))
            {
                forLoop.Ident = ((Variable)parsedExpr).Ident;

                parsedExpr = ParseExpr();
            }
            else forLoop.Ident = "$";

			string tempForLoopToString = parsedExpr.ToString();

			if (tempForLoopToString == "Variable")
			{

				forLoop.To = ((Variable)parsedExpr).Ident;
			}
			else if (tempForLoopToString == "IntLiteral")
			{
				forLoop.To = ((IntLiteral)parsedExpr).Value.ToString();
			}

			if (index == tokens.Count ||
			    !tokens[index].Equals("["))
			{
				throw new System.Exception("expected opening \"[\" after expression in for loop");
			}

			index += 2;

			forLoop.Body = ParseStmt();
			result = forLoop;

			if (index == tokens.Count || !tokens[index].Equals("]"))
			{
				throw new System.Exception("unterminated loop body");
			}

			index++;
		}
		else if (tokens[index].Equals("resetcolors") || tokens[index].Equals("resetcolours"))
		{
			index++;
			ResetColor resetColor = new ResetColor();
			result = resetColor;
		}
		else if (tokens[index].Equals("type"))
		{
			index++;
			Print print = new Print();
			if (tokens[index] == Scanner.Semi)
			{
				if (isTempVarNum == true)
					print.Expr = tempnum;
				else
					print.Expr = temp;
			}
			else
			{
				print.Expr = ParseExpr();
			}
			result = print;
		}		
		else if (tokens[index].Equals("if"))
		{
			Conditional cond = new Conditional ();
			index++;
			if (index < tokens.Count && (tokens [index] == Scanner.Equal || tokens [index] == Scanner.CloseAngle || tokens [index] == Scanner.OpenAngle))// && tokens [index + 1] is string)
			{
				cond.ExprA = ConvToExpr ("$#");
			} else {
				cond.ExprA = ParseExpr ();
			}
			if (tokens [index] == Scanner.Equal) cond.Comp = BinComp.Equal;
			if (tokens [index] == Scanner.CloseAngle) cond.Comp = BinComp.Greater;
			if (tokens [index] == Scanner.OpenAngle) cond.Comp = BinComp.Less;
			if (tokens [index] == Scanner.Bang) cond.Comp = BinComp.NotEqual;

			index++;
			cond.ExprB = ParseExpr ();
			//index--;
			if (index == tokens.Count ||
			    !tokens[index].Equals("["))
			{
				throw new System.Exception("expected opening \"[\" after expression in if block");
			}

			index += 2;

			cond.True = ParseStmt();
			result = cond;

			if (index == tokens.Count || !tokens[index].Equals("]"))
			{
				throw new System.Exception("unterminated if block");
			}

			index++;
		}
		else if (tokens[index] is string)
		{
            // assignment

            Assign assign = new Assign()
            { Ident = (string)tokens[index++] };

			if (index == tokens.Count ||
			    tokens[index] != Scanner.Equal)
			{
				throw new System.Exception("expected '='");
			}

			index++;

			StringLiteral parsedExpr = (StringLiteral)ParseExpr();
			assign.Expr = parsedExpr;
			result = assign;
		}
		else
		{
            // unrecongnised token, probably not supported. SKIP
            //throw new System.Exception("parse error at token " + index + ": " + tokens[index].ToString());
            result = new NullStmt();
		}

		if (index < tokens.Count && tokens[index] == Scanner.Semi)
		{
			index++;

			if (index < tokens.Count &&
			    !tokens[index].Equals("]"))
			{
                Sequence sequence = new Sequence()
                {
                    First = result,
                    Second = ParseStmt()
                };
                result = sequence;
			}
		}

		return result;
	}

	private Expr ParseExpr()
	{
		if (index == tokens.Count)
		{
			throw new System.Exception("expected expression, got EOF");
		}

		if (tokens[index + 1] == Scanner.Add)
		{

		}

		if (tokens[index] is StringBuilder)
		{
			string value = ((StringBuilder)tokens[index++]).ToString();
			return new StringLiteral() { Value = value };
		}
		else if (tokens[index] is int)
		{
			int intValue = (int)tokens[index++];
			if (tokens[index] == Scanner.Add || tokens[index] == Scanner.Sub || tokens[index] == Scanner.Mul || tokens[index] == Scanner.Div)
			{
				index++;
				Math(intValue, (int)tokens[index], (object)tokens[index - 1]);
			}
			index++;
			return new IntLiteral { Value = intValue };
		}
		else if (tokens[index] is string)
		{
			string ident = (string)tokens[index++];
			return new Variable { Ident = ident };
		}
		/* Add support for lambda-type function calls e.g. "item x = .read"
		 else if (tokens[index] == Scanner.Dot) {

		}*/
		else
		{
			throw new System.Exception("expected string literal, int literal, or variable");
		}
	}

	private Expr ConvToExpr(object obj)
	{
		if (obj is int)
		{
			return new IntLiteral { Value = (int)obj };
		}
		else if (obj is string)
		{
			return new Variable { Ident = (string)obj };
		}
		/* Add support for inline function calls e.g. "item x = .read"
		 else if (tokens[index] == Scanner.Dot) {

		}*/
		else
		{
			throw new System.Exception("expected string literal, int literal, or variable");
		}
	}

	private Expr IntToExpr(int input)
	{
		int intValue = input;
		index++;
		if (tokens[index] == Scanner.Add || tokens[index] == Scanner.Sub|| tokens[index] == Scanner.Mul || tokens[index] == Scanner.Div)
		{
			index++;
			intValue = Math(intValue, (int)tokens[index], (object)tokens[index - 1]);
		}
		return new IntLiteral { Value = intValue };
	}
	private int Math(int a, int b, object op)
	{
		if (op == Scanner.Add)
		{
			return a + b;
		}
		else if (op == Scanner.Sub)
		{
			return a - b;
		}
		else if (op == Scanner.Mul)
		{
			return a * b;
		}
		else if (op == Scanner.Div)
		{
			try
			{
				return a / b;
			}
			catch (System.DivideByZeroException)
			{  
				return 1;
			}
		}
		else
		{
			throw new System.Exception("Unrecognised operator '" + op + "'.");
		}

	}

}