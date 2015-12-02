public abstract class Stmt
{
}

// <ident> = <expr>
public class Assign : Stmt
{
	public string Ident;
	public Expr Expr;
}

public abstract class Blt : Stmt
{
}

public abstract class Color : Stmt
{
	public System.ConsoleColor color;
}

// var <ident> = <expr>
public class DeclareVar : Stmt
{
	public string Ident;
	public Expr Expr;
}

// Repeat <str> [ <Body> ]
public class ForLoop : Stmt
{
	public Stmt Body;
	public string To;
}

public class Conditional : Blt
{
	public Expr ExprA;
	public Expr ExprB;
	public BinComp Comp;
	public Stmt True;
	public Stmt False;
}

// pause <int>
public class Pause : Stmt
{
	public Expr Duration;
}

// type <expr>
public class Print : Stmt
{
	public Expr Expr;
}

// newline
public class PrintReturn : Stmt
{

}

// read <Ident>
public class Read : Stmt
{
	public string Ident;
}

// read_int <ident>
public class ReadNum : Stmt
{
	public string Ident;
}

public class Refresh : Stmt
{

}

public class ResetColor : Color
{

}

// <stmt> ; <stmt>
public class Sequence : Stmt
{
	public Stmt First;
	public Stmt Second;
}

public class TextBackColor : Color
{

}

public class TextForeColor : Color
{

}

/* <expr> := <string>
 *  | <int>
 *  | <arith_expr>
 *  | <ident>
 */
public abstract class Expr
{
}

// <bin_expr> := <expr> <bin_op> <expr>
public class BinExpr : Expr
{
	public Expr Left;
	public Expr Right;
	public BinOp Op;
}

// <bin_op> := + | - | * | /
public enum BinOp
{
	Add,
	Sub,
	Mul,
	Div
}

public enum BinComp
{
	Greater,
	Less,
	Equal,
	NotEqual
}

// <int> := <digit>+
public class IntLiteral : Expr
{
	public int Value;
}

// <string> := " <string_elem>* "
public class StringLiteral : Expr
{
	public string Value;
}

// <ident> := <char> <ident_rest>*
// <ident_rest> := <char> | <digit>
public class Variable : Expr
{
	public string Ident;
}
