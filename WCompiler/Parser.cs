
using Collections = System.Collections.Generic;
using Text = System.Text;

public sealed class Parser
{
	private int index;
	private Collections.IList<object> tokens;
	private readonly Stmt result;
	private bool isTempVarNum = false;

	public Parser(Collections.IList<object> tokens)
	{
		this.tokens = tokens;
		this.index = 0;
		this.result = this.ParseStmt();

		if (this.index != this.tokens.Count)
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
		Variable temp = new Variable();
		temp.Ident = "$";
		Variable tempnum = new Variable();
		tempnum.Ident = "$#";

		//foreach (object item in this.tokens) System.Console.WriteLine (item.ToString());

		if (this.index == this.tokens.Count)
		{
			throw new System.Exception("expected statement, got EOF");
		}
		if (this.tokens [this.index].Equals ("backcolor") || this.tokens [this.index].Equals ("backcolour")) {
			this.index++;
			this.index++;
			TextBackColor backColor = new TextBackColor ();
			backColor.color = (System.ConsoleColor)System.Enum.Parse (typeof(System.ConsoleColor), this.tokens [this.index].ToString (), true);
			result = backColor;
			this.index++;
		} else if (this.tokens [this.index].Equals ("forecolor") || this.tokens [this.index].Equals ("forecolour")) {
			this.index++;
			this.index++;
			TextForeColor foreColor = new TextForeColor ();
			foreColor.color = (System.ConsoleColor)System.Enum.Parse (typeof(System.ConsoleColor), this.tokens [this.index].ToString (), true);
			result = foreColor;
			this.index++;
		} else if (this.tokens [this.index].Equals ("item")) {
			this.index++;
			DeclareVar declareVar = new DeclareVar ();

			if (this.index < this.tokens.Count &&
				this.tokens [this.index] is string) {
				declareVar.Ident = (string)this.tokens [this.index];
			} else {
				throw new System.Exception ("expected variable name after 'item'");
			}

			this.index++;

			if (this.index == this.tokens.Count || this.tokens [this.index] != Scanner.Equal) {
				// assigns to $
				this.index--;
				this.tokens [this.index] = "$";
				declareVar.Expr = this.ParseExpr();
				result = declareVar;
			} else {
				this.index++;

				declareVar.Expr = this.ParseExpr();
				result = declareVar;
			}
		}
		else if (this.tokens[this.index].Equals("newline"))
		{
			this.index++;
			PrintReturn newline = new PrintReturn();
			result = newline;
		}
		else if (this.tokens[this.index].Equals("pause"))
		{
			this.index++;
			Pause pause = new Pause();
			if (this.index < this.tokens.Count && this.tokens[this.index] is int && this.tokens[this.index + 1] == Scanner.Semi)
			{
				pause.Duration = this.intToExpr(((IntLiteral)this.intToExpr((int)this.tokens[this.index])).Value * 1000);
				result = pause;
				this.index--;
			}
			else if (this.index < this.tokens.Count && this.tokens[this.index] is int && this.tokens [this.index + 1] != Scanner.Semi)
			{
				pause.Duration = this.intToExpr(((IntLiteral)this.intToExpr((int)this.tokens[this.index])).Value * 1000);
				result = pause;
			}
			else
			{
				pause.Duration = intToExpr(int.MaxValue);
				result = pause;
			}
		}
		else if (this.tokens[this.index].Equals("read"))
		{
			this.index++;
			Read read = new Read();
			isTempVarNum = false;

			if (this.index < this.tokens.Count && this.tokens[this.index] is string)
			{
				read.Ident = (string)this.tokens[this.index++];
				result = read;
			}
			else
			{
				read.Ident = "$";
				result = read;
			}
		}
		else if (this.tokens[this.index].Equals("readnum"))
		{
			this.index++;
			ReadNum readnum = new ReadNum();
			isTempVarNum = true;

			if (this.index < this.tokens.Count && this.tokens[this.index] is string)
			{
				readnum.Ident = (string)this.tokens[this.index++];
				result = readnum;
			}
			else
			{
				readnum.Ident = "$#";
				result = readnum;
			}
		}
		else if (this.tokens[this.index].Equals("refresh") || this.tokens[this.index].Equals("reset"))
		{
			this.index++;
			Refresh refresh = new Refresh();
			result = refresh;
		}
		else if (this.tokens[this.index].Equals("repeat"))
		{
			this.index++;
			ForLoop forLoop = new ForLoop();

			Expr parsedExpr = this.ParseExpr();
			string tempForLoopToString = parsedExpr.ToString();

			if (tempForLoopToString == "Variable")
			{

				forLoop.To = ((Variable)parsedExpr).Ident.ToString();
			}
			else if (tempForLoopToString == "IntLiteral")
			{
				forLoop.To = ((IntLiteral)parsedExpr).Value.ToString();
			}

			if (this.index == this.tokens.Count ||
			    !this.tokens[this.index].Equals("["))
			{
				throw new System.Exception("expected opening \"[\" after expression in for loop");
			}

			this.index += 2;

			forLoop.Body = this.ParseStmt();
			result = forLoop;

			if (this.index == this.tokens.Count || !this.tokens[this.index].Equals("]"))
			{
				throw new System.Exception("unterminated loop body");
			}

			this.index++;
		}
		else if (this.tokens[this.index].Equals("resetcolors") || this.tokens[this.index].Equals("resetcolours"))
		{
			this.index++;
			ResetColor resetColor = new ResetColor();
			result = resetColor;
		}
		else if (this.tokens[this.index].Equals("type"))
		{
			this.index++;
			Print print = new Print();
			if (this.tokens[this.index] == Scanner.Semi)
			{
				if (isTempVarNum == true)
					print.Expr = tempnum;
				else
					print.Expr = temp;
			}
			else
			{
				print.Expr = this.ParseExpr();
			}
			result = print;
		}		
		else if (this.tokens[this.index].Equals("if"))
		{
			Conditional cond = new Conditional ();
			this.index++;
			if (this.index < this.tokens.Count && (this.tokens [this.index] == Scanner.Equal || this.tokens [this.index] == Scanner.CloseAngle || this.tokens [this.index] == Scanner.OpenAngle))// && this.tokens [this.index + 1] is string)
			{
				cond.ExprA = ConvToExpr ("$#");
			} else {
				cond.ExprA = ParseExpr ();
			}
			if (this.tokens [this.index] == Scanner.Equal) cond.Comp = BinComp.Equal;
			if (this.tokens [this.index] == Scanner.CloseAngle) cond.Comp = BinComp.Greater;
			if (this.tokens [this.index] == Scanner.OpenAngle) cond.Comp = BinComp.Less;
			if (this.tokens [this.index] == Scanner.Bang) cond.Comp = BinComp.NotEqual;

			this.index++;
			cond.ExprB = ParseExpr ();
			//this.index--;
			if (this.index == this.tokens.Count ||
			    !this.tokens[this.index].Equals("["))
			{
				throw new System.Exception("expected opening \"[\" after expression in if block");
			}

			this.index += 2;

			cond.True = this.ParseStmt();
			//System.Console.WriteLine ("CONDCONDCOND" + cond.ExprA.ToString() + cond.Comp.ToString() + cond.ExprB.ToString() + "\nCONDCONDCOND" + cond.True.ToString());
			result = cond;

			if (this.index == this.tokens.Count || !this.tokens[this.index].Equals("]"))
			{
				throw new System.Exception("unterminated if block");
			}

			this.index++;
		}
		else if (this.tokens[this.index] is string)
		{
			// assignment

			Assign assign = new Assign();
			assign.Ident = (string)this.tokens[this.index++];

			if (this.index == this.tokens.Count ||
			    this.tokens[this.index] != Scanner.Equal)
			{
				throw new System.Exception("expected '='");
			}

			this.index++;

			StringLiteral parsedExpr = (StringLiteral)this.ParseExpr();
			assign.Expr = parsedExpr;
			result = assign;
		}
		else
		{
			throw new System.Exception("parse error at token " + this.index + ": " + this.tokens[this.index]);
		}

		if (this.index < this.tokens.Count && this.tokens[this.index] == Scanner.Semi)
		{
			this.index++;

			if (this.index < this.tokens.Count &&
			    !this.tokens[this.index].Equals("]"))
			{
				Sequence sequence = new Sequence();
				sequence.First = result;
				sequence.Second = this.ParseStmt();
				result = sequence;
			}
		}

		return result;
	}

	private Expr ParseExpr()
	{
		if (this.index == this.tokens.Count)
		{
			throw new System.Exception("expected expression, got EOF");
		}

		if (this.tokens[this.index + 1] == Scanner.Add)
		{

		}

		if (this.tokens[this.index] is Text.StringBuilder)
		{
			string value = ((Text.StringBuilder)this.tokens[this.index++]).ToString();
			StringLiteral stringLiteral = new StringLiteral();
			stringLiteral.Value = value;
			return stringLiteral;
		}
		else if (this.tokens[this.index] is int)
		{
			int intValue = (int)this.tokens[this.index++];
			if (this.tokens[this.index] == Scanner.Add || this.tokens[this.index] == Scanner.Sub || this.tokens[this.index] == Scanner.Mul || this.tokens[this.index] == Scanner.Div)
			{
				this.index++;
				this.Math(intValue, (int)this.tokens[this.index], (object)this.tokens[this.index - 1]);
			}
			IntLiteral intLiteral = new IntLiteral();
			intLiteral.Value = intValue;
			this.index++;
			return intLiteral;
		}
		else if (this.tokens[this.index] is string)
		{
			string ident = (string)this.tokens[this.index++];
			Variable var = new Variable();
			var.Ident = ident;
			return var;
		}
		/* Add support for function calls e.g. "item x = .read"
		 else if (this.tokens[this.index] == Scanner.Dot) {

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
			IntLiteral intLiteral = new IntLiteral();
			intLiteral.Value = (int)obj;
			return intLiteral;
		}
		else if (obj is string)
		{
			Variable var = new Variable();
			var.Ident = (string)obj;
			return var;
		}
		/* Add support for function calls e.g. "item x = .read"
		 else if (this.tokens[this.index] == Scanner.Dot) {

		}*/
		else
		{
			throw new System.Exception("expected string literal, int literal, or variable");
		}
	}

	private Expr intToExpr(int input)
	{
		int intValue = input;
		this.index++;
		if (this.tokens[this.index] == Scanner.Add || this.tokens[this.index] == Scanner.Sub|| this.tokens[this.index] == Scanner.Mul || this.tokens[this.index] == Scanner.Div)
		{
			this.index++;
			intValue = this.Math(intValue, (int)this.tokens[this.index], (object)this.tokens[this.index - 1]);
		}
		IntLiteral intLiteral = new IntLiteral();
		intLiteral.Value = intValue;
		return intLiteral;
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