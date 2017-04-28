using System.Collections.Generic;
using System.IO;
using System.Text;

public sealed class Scanner
{
	private readonly IList<object> result;

	public Scanner(TextReader input)
	{
		result = new List<object>();
		Scan(input);
	}

	public IList<object> Tokens
	{
		get { return result; }
	}

	#region ArithmiticConstants

	// Constants to represent arithmitic tokens.
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

	private void Scan(TextReader input)
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
                    if (ch == '\r') input.Read();
				}
				else if (ch == '{')
				{
					while (!(ch == '}' || input.Peek() == -1))
					{
						ch = (char)input.Read();
					}
					if (char.IsControl((char)input.Peek()) && input.Peek() != -1) {
						ch = (char)input.Read();
                        if (ch == '\r') ch = (char)input.Read();
                    }
				}
			}
			else if (char.IsLetter(ch) || ch == '_' || ch == '[' || ch == ']' || ch == '$' || ch == '#')
			{
				// keyword or identifier

				StringBuilder accum = new StringBuilder();

				while (char.IsLetter(ch) || ch == '_' || ch == '[' || ch == ']' || ch == '$' || ch == '#')
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

				result.Add(accum.ToString());
			}
			else if (ch == '"')
			{
				// string literal
				StringBuilder accum = new StringBuilder();

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
				result.Add(accum);
			}
			else if (ch == '\'')
			{
				// string literal
				StringBuilder accum = new StringBuilder();

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
				result.Add(accum);
			}
			else if (char.IsDigit(ch))
			{
				// numeric literal

				StringBuilder accum = new StringBuilder();

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

				result.Add(int.Parse(accum.ToString()));
			}
			else if (char.IsControl(ch))
			{
				input.Read();
				if (ch == '\r' && input.Peek() == '\n')
				{
					input.Read();
				}
				result.Add(Semi);
			}
			else switch (ch)
			{
				case '+':
				input.Read();
				result.Add(Add);
				break;

				case '-':
				input.Read();
				result.Add(Sub);
				break;

				case '*':
				input.Read();
				result.Add(Mul);
				break;

				case '/':
				input.Read();
				result.Add(Div);
				break;

				case '=':
				input.Read();
				result.Add(Equal);
				break;

				case '.':
					input.Read();
					result.Add(Dot);
					break;

				case '<':
					input.Read ();
					result.Add (OpenAngle);
					break;

				case '>':
					input.Read ();
					result.Add (CloseAngle);
					break;

				case '!':
					input.Read ();
					result.Add (Bang);
					break;

				default:
				throw new System.Exception("Scanner encountered unrecognized character '" + ch + "'");
			}

		}
		if (result[result.Count - 1] != Semi)
		{
			result.Add(Semi);
		}
	}
}