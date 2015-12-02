using Collections = System.Collections.Generic;
using IO = System.IO;
using Text = System.Text;

public sealed class Scanner
{
	private readonly Collections.IList<object> result;

	public Scanner(IO.TextReader input)
	{
		this.result = new Collections.List<object>();
		this.Scan(input);
	}

	public Collections.IList<object> Tokens
	{
		get { return this.result; }
	}

	#region ArithmiticConstants

	// Constants to represent arithmitic tokens. This could
	// be alternatively written as an enum.
	public static readonly object Add = new object();
	public static readonly object Sub = new object();
	public static readonly object Mul = new object();
	public static readonly object Div = new object();
	public static readonly object Semi = new object();
	public static readonly object Equal = new object();
	public static readonly object Dot = new object();

	#endregion

	#region ComparisonOperators

	// Comparison operators, pretty self-explanitory
	public static readonly object OpenAngle = new object();
	public static readonly object CloseAngle = new object();
	public static readonly object Bang = new object();
	#endregion

	private void Scan(IO.TextReader input)
	{
		while (input.Peek() != -1)
		{
			char ch = char.ToLower((char)input.Peek());

			// Scan individual tokens
			if ((char.IsWhiteSpace(ch) && !char.IsControl(ch)) || ch == '?' || ch == '{')
			{
				// eat the current char and skip ahead!
				input.Read();
				if (ch == '?')
				{
					while (!(char.IsControl(ch) || input.Peek() == -1))
					{
						ch = (char)input.Read();
					}
				}
				else if (ch == '{')
				{
					while (!(ch == '}' || input.Peek() == -1))
					{
						ch = (char)input.Read();
					}
					if (char.IsControl((char)input.Peek()) && input.Peek() != -1) {
						ch = (char)input.Read();
					}
				}
			}
			else if (char.IsLetter(ch) || ch == '_' || ch == '[' || ch == ']')
			{
				// keyword or identifier

				Text.StringBuilder accum = new Text.StringBuilder();

				while (char.IsLetter(ch) || ch == '_' || ch == '[' || ch == ']')
				{
					accum.Append(ch);
					input.Read();

					if (input.Peek() == -1)
					{
						break;
					}
					else
					{
						ch = char.ToLower((char)input.Peek());
					}
				}

				this.result.Add(accum.ToString());
			}
			else if (ch == '"')
			{
				// string literal
				Text.StringBuilder accum = new Text.StringBuilder();

				input.Read(); // skip the '"'

				if (input.Peek() == -1)
				{
					throw new System.Exception("unterminated string literal");
				}

				while ((ch = (char)input.Peek()) != '"' || char.IsControl(ch))
				{
					accum.Append(ch);
					input.Read();

					if (input.Peek() == -1)
					{
						throw new System.Exception("unterminated string literal");
					}
				}

				// skip the terminating "
				input.Read();
				this.result.Add(accum);
			}
			else if (ch == '\'')
			{
				// string literal
				Text.StringBuilder accum = new Text.StringBuilder();

				input.Read(); // skip the '"'

				if (input.Peek() == -1)
				{
					throw new System.Exception("unterminated string literal");
				}

				while ((ch = (char)input.Peek()) != '\'' || char.IsControl(ch))
				{
					accum.Append(ch);
					input.Read();

					if (input.Peek() == -1)
					{
						throw new System.Exception("unterminated string literal");
					}
				}

				// skip the terminating "
				input.Read();
				this.result.Add(accum);
			}
			else if (char.IsDigit(ch))
			{
				// numeric literal

				Text.StringBuilder accum = new Text.StringBuilder();

				while (char.IsDigit(ch))
				{
					accum.Append(ch);
					input.Read();

					if (input.Peek() == -1)
					{
						break;
					}
					else
					{
						ch = (char)input.Peek();
					}
				}

				this.result.Add(int.Parse(accum.ToString()));
			}
			else if (char.IsControl(ch))
			{
				input.Read();
				if (ch == '\r' && input.Peek() == '\n')
				{
					input.Read();
				}
				this.result.Add(Scanner.Semi);
			}
			else switch (ch)
			{
				case '+':
				input.Read();
				this.result.Add(Scanner.Add);
				break;

				case '-':
				input.Read();
				this.result.Add(Scanner.Sub);
				break;

				case '*':
				input.Read();
				this.result.Add(Scanner.Mul);
				break;

				case '/':
				input.Read();
				this.result.Add(Scanner.Div);
				break;

				case '=':
				input.Read();
				this.result.Add(Scanner.Equal);
				break;

				case '.':
					input.Read();
					this.result.Add(Scanner.Dot);
					break;

				case '<':
					input.Read ();
					this.result.Add (Scanner.OpenAngle);
					break;

				case '>':
					input.Read ();
					this.result.Add (Scanner.CloseAngle);
					break;

				case '!':
					input.Read ();
					this.result.Add (Scanner.Bang);
					break;

				default:
				throw new System.Exception("Scanner encountered unrecognized character '" + ch + "'");
			}

		}
		if (this.result[this.result.Count - 1] != Scanner.Semi)
		{
			this.result.Add(Scanner.Semi);
		}
	}
}