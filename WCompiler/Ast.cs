#region AbstractClasses
public abstract class Stmt { }

public abstract class Expr { }

public abstract class Blt : Stmt { }

public abstract class Color : Stmt
{ public System.ConsoleColor color; }
#endregion

// <ident> = <expr>
public class Assign : Stmt
{
	public string Ident;
	public Expr Expr;
}

// if <ExprA> <BinOp> <ExprB> [ Body ]
//      if <ExprB> [ Body ] (if $ = <ExprB> [ Body ])
public class Conditional : Blt
{
    public Expr ExprA;
    public Expr ExprB;
    public BinComp Comp;
    public Stmt True;
    public Stmt False;
}

// item <ident> = <expr>
public class DeclareVar : Stmt
{
	public string Ident;
	public Expr Expr;
}

// repeat <ident> <expr> [ Body ]
//      repeat <expr> [ Body ] (repeat $ <expr> [ Body ])
public class ForLoop : Stmt
{
	public Stmt Body;
    public string Ident;
	public string To;
}

// (Empty Line)
public class NullStmt : Stmt { }

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
public class PrintReturn : Stmt { }

// read <ident>
//      read (read $)
public class Read : Stmt
{
	public string Ident;
}

// readnum <ident>
//      readnum (readnum $#)
public class ReadNum : Stmt
{
	public string Ident;
}

// refresh
// reset
public class Refresh : Stmt { }

// resetcolors
// resetcolours
public class ResetColor : Color { }

// (Used for chaining commands into program)
public class Sequence : Stmt
{
	public Stmt First;
	public Stmt Second;
}

// backcolor = "<color>"
// backcolour = "<colour>"
public class TextBackColor : Color { }

// forecolor = "<color>"
// forecolour = "<colour>"
public class TextForeColor : Color { }

#region BinExprs
public class BinExpr : Expr
{
	public Expr Left;
	public Expr Right;
	public BinOp Op;
}

// (+ - * /)
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
#endregion

#region LiteralsEtc
public class IntLiteral : Expr
{
	public float Value;
}

public class StringLiteral : Expr
{
	public string Value;
}


public class Variable : Expr
{
	public string Ident;
}
#endregion