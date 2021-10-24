namespace Interpreter {
	partial class token {
		public TokenType type;
		public string lexeme;
		public object literal;
		public int line;

		public token(TokenType _type, string _lexeme, object _literal, int _line) {
			type = _type;
			lexeme = _lexeme;
			literal = _literal;
			line = _line;

		}
		public string toString() {
			return type + " " + lexeme + " " + literal;
		}
	}
}
