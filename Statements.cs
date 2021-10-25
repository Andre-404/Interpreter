using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter {
	abstract class stmt {
		//visitor interface for the statements, works in the same way as the expression vistior
		public abstract T accept<T>(visitor<T> vis);
		public interface visitor<T> {
			public T visitExpression(expressionStmt statement);
			public T visitPrint(printStmt statement);
			public T visitVar(varStmt statement);
			public T visitBlock(blockStmt statement);
			public T visitIf(ifStmt statement);
			public T visitWhile(whileStmt statement);
			public T visitFunc(funcStmt statement);
			public T visitReturn(returnStmt statement);
			public T visitClass(classStmt statement);
		}
	}

	class expressionStmt : stmt {
		//a statement that contains a expression
		public expr expression;
		public expressionStmt(expr _expression) {
			expression = _expression;
		}

		public override T accept<T>(visitor<T> vis) {
			return vis.visitExpression(this);
		}
	}

	class printStmt : stmt {
		//prints the expression
		public expr expression;
		public printStmt(expr _expression) {
			expression = _expression;
		}

		public override T accept<T>(visitor<T> vis) {
			return vis.visitPrint(this);
		}
	}

	class varStmt : stmt {
		//name: token whose lexeme is the name of the variable
		//initializer: if there is a expression for the initializer, set the variable to the value of the expression, if not, set it to null
		public token name;
		public expr initializer;

		public varStmt(token _name , expr _expression) {
			name = _name;
			initializer = _expression;
		}

		public override T accept<T>(visitor<T> vis) {
			return vis.visitVar(this);
		}
	}

	class blockStmt : stmt {
		//a block of code that contains a list of statements that will be executed in the new enviroment
		public List<stmt> statements;

		public blockStmt(List<stmt> _statements) {
			statements = _statements;
		}

		public override T accept<T>(visitor<T> vis) {
			return vis.visitBlock(this);
		}
	}

	class ifStmt : stmt {
		//contains a expression for the condition, and then 1 or 2 statements, the first one is mandatory, while the second one isn't
		public expr condition;
		public stmt thenBranch;
		public stmt elseBranch;

		public ifStmt(expr _condition, stmt _thenBranch, stmt _elseBranch) {
			condition = _condition;
			thenBranch = _thenBranch;
			elseBranch = _elseBranch;
		}

		public override T accept<T>(visitor<T> vis) {
			return vis.visitIf(this);
		}
	}

	class whileStmt : stmt {
		public expr condition;
		public stmt body;

		public whileStmt(expr _condition, stmt _body) {
			condition = _condition;
			body = _body;
		}

		public override T accept<T>(visitor<T> vis) {
			return vis.visitWhile(this);
		}

	}

	class funcStmt : stmt{
		public token name;
		public List<token> param;
		public List<stmt> body;

		public funcStmt(token _name, List<token> _params, List<stmt> _body) {
			name = _name;
			param = _params;
			body = _body;
		}

		public override T accept<T>(visitor<T> vis) {
			return vis.visitFunc(this);
		}
	}

	class returnStmt : stmt {
		public token keyword;
		public expr value;

		public returnStmt(token _keyword, expr _val) {
			keyword = _keyword;
			value = _val;

		}

		public override T accept<T>(visitor<T> vis) {
			return vis.visitReturn(this);
		}
	}

	class classStmt : stmt{
		public token name;
		public List<stmt> methods;
		public varExpr superClass;

		public classStmt(token _name, List<stmt> _methods, varExpr _superClass) {
			name = _name;
			methods = _methods;
			superClass = _superClass;
		}

		public override T accept<T>(visitor<T> vis) {
			return vis.visitClass(this);
		}
	}
}
