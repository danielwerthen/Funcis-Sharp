using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FuncisSharp
{
	public class Parser
	{
		private string Input { get; set; }
		private int Lineno { get; set; }
		private int IndentLvl { get; set; }
		private int IndentPrev { get; set; }


		public Parser(string str)
		{
			this.Input = str;
			this.Lineno = 1;
			this.IndentLvl = 0;
			this.IndentPrev = 0;
		}

		private Token Tok(Types type, string val)
		{
			return new Token()
			{
				Type = type,
				Line = this.Lineno,
				Val = val,
				Indent = this.IndentLvl
			};
		}

		private void Consume(int idx) 
		{
			this.Input = this.Input.Substring(idx);
		}

		private Token Scan(string reg, Types type)
		{
			var cap = Regex.Match(this.Input, reg);
			if (cap.Success)
			{
				this.Consume(cap.Groups[0].Length);
				return this.Tok(type, cap.Groups[1].Value);
			}
			return null;
		}

		private Token Func()
		{
			return this.Scan(@"^(\w+) *", Types.Func);
		}

		private Token Selector()
		{
			return this.Scan(@"^([\w\.]+)\.", Types.Selector);
		}

		private Token Param()
		{
			var par = this.Scan("^[\\(,] *({.*}|\\w+|\'.*\'|\".*\") *", Types.Param);
			if (par != null)
			{
				par.Val = Regex.Replace(par.Val, @"( *{ *| *, *)\'?(\w+)\'? *: *", "$1\"$2\": ", RegexOptions.Multiline);
			}
			return par;
		}

		private Token ParamStop()
		{
			var cap = this.Scan(@"^(\(?\) *)( *=> *| *?)", Types.ParamStop);
			if (cap != null)
				cap.Val = null;
			return cap;
		}

		private Token Indents()
		{
			var cap = Regex.Match(this.Input, @"^(\r?\n?)(\t*) *");
			if (cap.Success)
			{
				if (cap.Groups[0].Length == 0)
					return null;
				this.Consume(cap.Groups[0].Length);
				if (cap.Groups[1].Length > 0)
					++this.Lineno;
				if (cap.Groups[2].Length > this.IndentLvl + 1)
				{
					throw new Exception("Bad indent at " + this.Lineno);
				}
				else
				{
					this.IndentPrev = this.IndentLvl;
					this.IndentLvl = cap.Groups[2].Length;
					return this.Tok(Types.Newline, "");
				}
			}
			return null;
		}

		private IEnumerable<Token> _next()
		{
			yield return this.Selector();
			yield return this.Func();
			yield return this.Param();
			yield return this.ParamStop();
			yield return this.Indents();
		}

		private Token Next()
		{
			return _next().FirstOrDefault(row => row != null);
		}

		private IEnumerable<Token> Parse()
		{
			Token tok;
			while (null != (tok = Next())) yield return tok;
		}

		public IEnumerable<Function> Signalify()
		{
			var toks = this.Parse().ToList();
			while (toks.Count() > 0)
			{
				var f = toks.GetFunction();
				if (f != null)
					yield return f;
			}
		}
	}

	#region Types

	internal enum Types
	{
		Func,
		Selector,
		Param,
		ParamStop,
		Newline
	}

	internal class Token
	{
		public Types Type { get; set; }
		public int Line { get; set; }
		public string Val { get; set; }
		public int Indent { get; set; }
	}

	public class Function
	{
		public string Name { get; set; }
		public string Selector { get; set; }
		public List<Parameter> Parameters { get; set; }
		public List<Parameter> CallbackParameters { get; set; }
		public List<Function> Callbacks { get; set; }

		public Function(string selector, string name, List<Parameter> parameters)
		{
			this.Name = name;
			this.Selector = selector;
			this.Parameters = parameters;
			if (this.Parameters == null)
				this.Parameters = new List<Parameter>();
			this.CallbackParameters = new List<Parameter>();
			this.Callbacks = new List<Function>();
		}
	}

	public class Parameter
	{
	}

	public class StringParameter : Parameter
	{
		public string Value { get; set; }
		public StringParameter(string str)
		{
			this.Value = str;
		}
	}

	public class ConstantParameter : Parameter
	{
		public JObject Value { get; set; }
		public ConstantParameter(JObject jo) 
		{
			this.Value = jo;
		}
	}

	public class ArgumentParameter : Parameter
	{
		public string Value { get; set; }
		public ArgumentParameter(string str)
		{
			Value = str;
		}
	}

	#endregion

	public static class Tokenizer
	{
		internal static Function GetFunction(this List<Token> tokens)
		{
			//Delete all tokens until the good part starts
			while (tokens.Count > 0 
				&& tokens.Select(row => row.Type).FirstOrDefault() != Types.Func
				&& tokens.Select(row => row.Type).FirstOrDefault() != Types.Selector)
			{
				tokens.RemoveAt(0);
			}
			if (tokens.Count > 0)
			{
				Token name = null, selector = null;
				int lineno = tokens[0].Line;
				int indent = tokens[0].Indent;
				if (tokens[0].Type == Types.Selector)
				{
					selector = tokens[0];
					tokens.RemoveAt(0);
				}
				if (tokens.Count > 0 && tokens[0].Type == Types.Func)
				{
					name = tokens[0];
					tokens.RemoveAt(0);
				}
				if (name == null)
					throw new Exception("Function is missing a name at line: " + lineno);
				var paras = tokens.GetParams();
				var f = new Function(selector != null ? selector.Val : null, name.Val, paras);
				while (tokens.Count > 0 && tokens[0].Indent > indent)
				{
					if (tokens[0].Type == Types.Param)
					{
						var args = tokens.GetParams();
						if (f.CallbackParameters.Count > 0)
						{
							throw new Exception("Unknown syntax error at line: " + tokens[0].Line);
						}
						f.CallbackParameters = args;
					}
					else if (tokens[0].Type == Types.Func || tokens[0].Type == Types.Selector)
					{
						var c = tokens.GetFunction();
						if (c != null)
							f.Callbacks.Add(c);
						else
							throw new Exception("Unknown syntax error at line: " + tokens[0].Line);
					}
					else
					{
						tokens.RemoveAt(0);
					}
				}
				return f;
			}
			return null;
		}

		internal static List<Parameter> GetParams(this List<Token> tokens)
		{
			var paras = new List<Parameter>();
			while (tokens.Count > 0 && tokens[0].Type == Types.Param)
			{
				var param = tokens[0];
				tokens.RemoveAt(0);
				try
				{
					var str = Regex.Match(param.Val, "^\'(.*)\'$|^\"(.*)\"$");
					if (str.Success)
					{
						if (!string.IsNullOrEmpty(str.Groups[1].Value))
							paras.Add(new StringParameter(str.Groups[1].Value));
						else if (!string.IsNullOrEmpty(str.Groups[2].Value))
							paras.Add(new StringParameter(str.Groups[2].Value));
					}
					else
					{
						var jo = JObject.Parse(param.Val);
						paras.Add(new ConstantParameter(jo));
					}
				}
				catch (Exception)
				{
					paras.Add(new ArgumentParameter(param.Val));
				}
			}
			if (tokens.Count > 0 && tokens[0].Type == Types.ParamStop)
				tokens.RemoveAt(0);
			return paras;
		}
	}
}
