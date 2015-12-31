/*
   Copyright 2015 Wes Sleeman

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
   */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace WCompiler
{
	class Program
	{
		static void testMain(string[] args)
		{
			string input = "34";
			int output = 0;
			int.TryParse(input, out output);
			Console.WriteLine(output.ToString());
			//args[0] = "test.w";
			//realMain(args);
		}
		static void Main(string[] args)
		{
			if (args.Length == 0) {
				Console.WriteLine ("Usage: W.exe source.w");
				return;
			} else {
				try {
					Scanner scanner = null;
					using (TextReader input = File.OpenText(args[0])) {
						System.Console.WriteLine ("Scanning...");
						scanner = new Scanner (input);
					}
					System.Console.WriteLine ("Parsing...");
					Parser parser = new Parser (scanner.Tokens);
					System.Console.WriteLine ("Generating code...");
					CodeGen codeGen = new CodeGen (parser.Result, Path.GetFileNameWithoutExtension (args [0]) + ".exe");
					System.Console.WriteLine ("Complete!");
				} catch (Exception e) {
					Console.Error.WriteLine (e.Message);
					for (; ;)
						;
				}
			}
		}
	}
}