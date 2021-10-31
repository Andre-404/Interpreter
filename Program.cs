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
		STAR_EQUALS, SLASH_EQUALS,

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
		public  static string source = "";
		public  static scanner Scanner = new scanner(source);
		public  static parser Parser = new parser(new List<token>());
		public  static resolver Resolver = new resolver(Interpreter);

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
			Console.WriteLine("--------------\n" + 
				"Error: " + error.Message + 
				"\n[line " + error.Token.line + "]\n" + 
				printLine(error.Token) +
				"\n--------------");
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

		public static void run() {
			hadError = false;
			hadRuntimeError = false;
			if(source == "") {
				Console.WriteLine("Source path not provided.");
				return;
			}
			Console.WriteLine("\nOutput:");
			//get the tokens from the source code
			Scanner.source = source;
			List<token> tokens = Scanner.scanTokens();
			//parse the tokens into statements
			List<stmt> statements = Parser.parse(tokens);


			if(hadError) return;
			if(hadRuntimeError) return;
			
			//resolve variables
			Resolver.scopes = new Stack<Dictionary<string, bool>>();
			Resolver.resolve(statements);

			// Stop if there was a resolution error.
			if(hadError) return;

			//executes the syntax tree that's made out of statements and expressions
			Interpreter.interpret(statements);
		}
	}

	class Program {
		static string readFromFile(string _fileName) {
			if(File.Exists(_fileName)){

				return File.ReadAllText(_fileName);
			} else {
				Console.WriteLine("Couldn't find the file.");
				return "";
			}
		}

		static void printHelp() {
			Console.WriteLine(
				"\nTypes:\n" +
				"\"string\" : string\n" +
				"[any number] : represents any number, can be a integer or a float\n" +
				"nil : null value\n" +
				"true : true value\n" +
				"false : false value\n" +
				"[class name] : represents a class type\n" +
				"function : represents any function\n" +
				"\nvar [name] = [value]; : creates a variable and assigns it a value, if '= [value]' is omitted the value defaults to 'nil'\n" +
				"[variable]++ : can only be used with numbers, increments the number by 1\n" +
				"[variable]-- : can only be used with numbers, increments the number by -1\n" +
				"[variable] += [value] : can only be used with numbers, increases the number by [value]\n" +
				"[variable] -= [value] : can only be used with numbers, decreases the number by [value]\n" +
				"[variable] *= [value] : can only be used with numbers, multiples the number by [value]\n" +
				"[variable] /= [value] : can only be used with numbers, divides the number by [value]\n" +
				"\nforeach( var [name] in [collection name] ){ [body] } : iterates over every element of the collection and executes the body for each iteration\n" +
				"for([optional: variable declaration]; [optional, defaults to 'true': condition]; [optional: iteration]{ [body] } : creates a for statement\n" +
				"while([optional, defaults to 'true': condition]){ [body] } : creates a while statement\n" +
				"if([condition]) [body] else [body] : creates a if statement, 'else' clause is optional\n" +
				"|| or 'or' : or\n" +
				"&& or 'and' : and\n" +
				"\nfunc [name]( [params] ){ [body] } : creates a global function\n" +
				"return [value] : can only be used inside a function, returns [value] from inside the function\n" +
				"\nclass [name] { [body] } : creates a new class\n" +
				"Inside the class body:\n"+
				"init( [params] ){ [body] } : creates a constructor\n" +
				"[method name]( [params] ){ [body] } : creates a class method\n" +
				"Inside the methods and constructor\n" +
				"this.[variable] : refrences a variable of the current instance\n" +
				"super.[method] : refrences a method of the parent class(if there is any)\n" +
				"\nBuilt in classes\n" +
				"List() : creates a list(mutable array)\n" +
				"Array([int : array number]) : creates a imutable array\n" +
				"Hash([key types]) : creates a hash map with the specified type to be used for keys\n" +
				"\nBuilt in functions\n" +
				"print [value] : prints a value\n" +
				"systemClock() : returns time(in ms) in the unix standard\n" +
				"systemReadLine() : waits for user input to the console and returns the string of the line that was inputted\n" +
				"systemReadFile([filepath]) : returns the string that is contained inside the file\n" +
				"getType([value]) : returns the type of the provided value\n" +
				"toNumber([value]) : attempts to convert the value to a number and if it suceeds returns the number\n" +
				"toString([value]) : converts the value to a string");
		}
		
		static void Main(string[] args) {
			Console.WriteLine("Commands: " + "\n -help or -h : provides the rules of the syntax" +
				"\n -filepath [filepath]: filepath to the source code" +
				"\n -run : runs the source code" +
				"\n -exit : exits the console");
			Lox interpreter = new Lox();
			string path = "";
			string input = "";
			string source = "";
			while(input != "-exit") {
				input = Console.ReadLine();
				if(input == "-help" || input == "-h") {
					printHelp();
				}else if(input.StartsWith("-filepath")) {
					source = readFromFile(input.Remove(input.IndexOf('-'), 10));
					if(source != "") Console.WriteLine("Path found, source code loaded.");
					path = input.Remove(input.IndexOf('-'), 10);
				} else if(input == "-run") {
					Lox.source = readFromFile(path);
					Lox.run();
				}else if(input != "-exit") {
					Console.WriteLine(input + " is not a valid command.");
				}
			}
			
		}

	}
}
