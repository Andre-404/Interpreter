using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Interpreter {
	partial class scanner {
		private string source;
		private List<token> tokens = new List<token>();
		private int start = 0;
		private int current = 0;
		private int line = 1;
		static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>();

		public scanner(string _source) {
			source = _source;
			//keywords
			keywords.Add("and", TokenType.AND);
			keywords.Add("class", TokenType.CLASS);
			keywords.Add("else", TokenType.ELSE);
			keywords.Add("false", TokenType.FALSE);
			keywords.Add("for", TokenType.FOR);
			keywords.Add("fun", TokenType.FUN);
			keywords.Add("if", TokenType.IF);
			keywords.Add("nil", TokenType.NIL);
			keywords.Add("or", TokenType.OR);
			keywords.Add("print", TokenType.PRINT);
			keywords.Add("return", TokenType.RETURN);
			keywords.Add("super", TokenType.SUPER);
			keywords.Add("this", TokenType.THIS);
			keywords.Add("true", TokenType.TRUE);
			keywords.Add("var", TokenType.VAR);
			keywords.Add("while", TokenType.WHILE);
		}

		public List<token> scanTokens() {
			//scans every character in the source code string
			while(!isAtEnd()) {
				start = current;
				scanToken();
			}
			tokens.Add(new token(TokenType.EOF, "", null, line));
			return tokens;
		}

		private bool isAtEnd() {
			return current >= source.Length;
		}

		private void scanToken() {
			char c = nextChar();
			switch(c) {
				//various keyowrds
				case '(':
					addToken(TokenType.LEFT_PAREN);
					break;
				case ')':
					addToken(TokenType.RIGHT_PAREN);
					break;
				case '{':
					addToken(TokenType.LEFT_BRACE);
					break;
				case '}':
					addToken(TokenType.RIGHT_BRACE);
					break;
				case ',':
					addToken(TokenType.COMMA);
					break;
				case '.':
					addToken(TokenType.DOT);
					break;
				case '-':
					addToken(TokenType.MINUS);
					break;
				case '+':
					addToken(TokenType.PLUS);
					break;
				case ';':
					addToken(TokenType.SEMICOLON);
					break;
				case '*':
					addToken(TokenType.STAR);
					break;
				case '!':
					addToken(match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
					break;
				case '=':
					addToken(match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
					break;
				case '<':
					addToken(match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
					break;
				case '>':
					addToken(match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
					break;
				case '/':
					if(match('/')) {
						// A comment goes until the end of the line.
						while(peek() != '\n' && !isAtEnd()) nextChar();
					} else {
						addToken(TokenType.SLASH);
					}
					break;
				case ' ':
				case '\r':
				case '\t':
					// Ignore whitespace.
					break;

				case '\n':
					line++;
					break;
				case '"':
					generateString();
					break;
				default:
					//in the default case we detect numbers, keywords and variables
					if(isDigit(c)) {
						generateNumber();
					} else if(isAlpha(c)) {
						generateIdentifier();
					} else {
						Lox.error(line, "Unexpected character: " + "'" + c.ToString() + "'.");
						;
					}
					break;
			}
		}

		private char nextChar() {
			//moves onto the next character
			return source.ElementAt(current++);
		}

		private void addToken(TokenType _type) {
			addToken(_type, null);
		}

		private void addToken(TokenType _type, object _literal) {
			//used for strings and numbers
			string text = source.Substring(start, current - start);
			tokens.Add(new token(_type, text, _literal, line));
		}

		private bool match(char expected) {
			if(isAtEnd())
				return false;
			if(source.ElementAt(current) != expected)
				return false;
			//if the current char is the one we expect, update the string pos
			current++;
			return true;
		}

		private char peek() {
			//retrives the current char
			if(isAtEnd()) return '\0';
			return source.ElementAt(current);
		}

		private void generateString() {
			//since we have detected the " char, loop until we find another one that completes the string
			while(peek() != '"' && !isAtEnd()) {
				if(peek() == '\n')
					line++;
				nextChar();
			}

			if(isAtEnd()) {
				Lox.error(line, "Unterminated string.");
				return;
			}

			// The closing ".
			nextChar();

			// Trim the surrounding quotes.
			string value = source.Substring(start + 1, current - 2 -start);
			addToken(TokenType.STRING, value);
		}

		private bool isDigit(char c) {
			return c >= '0' && c <= '9';
		}

		private void generateNumber() {
			//look at the next character and determine if its a number, if it is, update the string pos
			while(isDigit(peek())) nextChar();

			// Look for a fractional part.
			if(peek() == '.' && isDigit(peekNext())) {
				// Consume the "."
				nextChar();

				while(isDigit(peek())) nextChar();
			}
			//invariant culture means the fractional part is ALWAYS seperated by "."
			addToken(TokenType.NUMBER, Convert.ToDouble(source.Substring(start, current-start), CultureInfo.InvariantCulture));
		}

		private char peekNext() {
			if(current + 1 >= source.Length) return '\0';
			return source.ElementAt(current + 1);
		}

		private void generateIdentifier() {
			//we have detected a letter first, now we check if there are letter OR numbers following it
			while(isAlphaNumeric(peek())) nextChar();

			string text = source.Substring(start, current-start);
			TokenType type = TokenType.IDENTIFIER;
			//if the identifier is one of the keywords, replace the type of the current token
			if(keywords.ContainsKey(text)) type = keywords[text];

			addToken(type);
		}
		
		private bool isAlpha(char c) {
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
		}
		private bool isAlphaNumeric(char c) {
			return isAlpha(c) || isDigit(c);
		}
	}
}
