using System;
using System.IO;
using System.Collections.Generic;

namespace Interpreter {
	enum TokenType {
		// Single-character tokens.
		LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
		COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR, COLON,
		LEFT_BRACK, RIGHT_BRACK,

		// One or two character tokens.
		BANG, BANG_EQUAL,
		EQUAL, EQUAL_EQUAL,
		GREATER, GREATER_EQUAL,
		LESS, LESS_EQUAL, P_INCREMENT,
		N_INCREMENT, PLUS_EQUALS, MINUS_EQUALS,

		// Literals.
		IDENTIFIER, STRING, NUMBER,

		// Keywords.
		AND, CLASS, ELSE, FALSE, FUN, FOR, IF, NIL, OR,
		PRINT, RETURN, SUPER, THIS, TRUE, VAR, WHILE,
		FOREACH, IN,

		EOF
	}
	enum FunctionType {
		NONE,
		FUNCTION,
		INIT,
		METHOD
	}

	enum ClassType {
		NONE,
		CLASS,
		SUBCLASS
	}

	enum InstanceType {
		CUSTOM,
		ARRAY,
		LIST,
		DICTIONARY
	}

	enum CallType {
		DEFAULT,
		INTERNAL
	}

	class Lox{
		private static bool hadError = false;
		private static bool hadRuntimeError = false;
		private static interpreter Interpreter = new interpreter();
		private static string source;

		public static void error(int line, string msg) {
			report(line, "", msg);
		}
		public static void error(token t, string msg) {
			//reports a error based on the tokens line, lexeme(name) and the message we have passed to it
			if(t.type == TokenType.EOF) {
				report(t.line, " at end", msg);
			} else {
				report(t.line, " at '" + t.lexeme + "'", msg);
			}
		}
		public static void runtimeError(RuntimeError error) {
			Console.WriteLine("Error: " + error.Message + "\n[line " + error.Token.line + "]\n" + printLine(error.Token));
			hadRuntimeError = true;
		}

		private static void report(int line, string where, string msg) {
			Console.WriteLine("[line " + line.ToString() + "] Error" + where + ": " + msg);
			hadError = true;//ensures we don't run code that doesn't work
		}

		private static string printLine(token t) {
			//prints error line
			int line = t.line;
			string s = "";
			int tempL = 1;
			for(int i = 0; i < source.Length; i++) {
				char c = source[i];
				if(c == '\n'){
					tempL++;
					continue;
				}
				if(tempL == line) {
					s += c;
				}
				
			}
			return s;
		}

		public static void run(string _source) {
			//get the tokens from the source code
			source = _source;
			scanner sc = new scanner(source);
			List<token> tokens = sc.scanTokens();
			for(int i = 0; i < tokens.Count; i++) {
				Console.WriteLine(tokens[i].toString());
			}
			Console.WriteLine("---------------");
			//parse the tokens into statements
			parser p = new parser(tokens);
			List<stmt> statements = p.parse();


			if(hadError) return;
			if(hadRuntimeError) return;

			resolver Resolver = new resolver(Interpreter);
			Resolver.resolve(statements);

			// Stop if there was a resolution error.
			if(hadError) return;

			//executes the syntax tree that's made out of statements and expressions
			Interpreter.interpret(statements);
		}
	}

	class Program {
		static string readFromFile(string _fileName) {
			return File.ReadAllText(_fileName);
		}
		
		static void Main(string[] args) {
			Console.WriteLine("File path: "+ "C:\\Temp\\test.txt");
			Lox interpreter = new Lox();
			//string fp = Console.ReadLine();
			string sourceCode = readFromFile(@"C:\Temp\test.txt");
			Lox.run(sourceCode);
			Console.ReadKey();
			
		}

	}
}
