/*
   Copyright 2017 Wes Sleeman

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
using System.IO;

namespace WCompiler
{
    class Program
	{
		static void TestMain(string[] args)
		{
            args = new string[] { "example.w" };
            Main(args);
        }
		static void Main(string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine("Usage: W.exe source.w");
                TestMain(args);
				return;
			} else {
				try {
					Scanner scanner = null;
					using (TextReader input = File.OpenText(args[0])) {
                        Console.WriteLine("Scanning...");
						scanner = new Scanner(input);
					}

                    Console.WriteLine("Parsing...");
					Parser parser = new Parser(scanner.Tokens);

                    Console.WriteLine("Generating code...");
					CodeGen codeGen = new CodeGen(parser.Result, Path.GetFileNameWithoutExtension(args [0]) + ".exe");

                    Console.WriteLine("Complete!");
                } catch (Exception e) {
					Console.Error.WriteLine(e.Message);
                    while (true) ;
				}
			}
		}
	}
}