using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter {
	abstract class expr {
		//this implements the visitor interface, which has a different function for each expression,
		//ensuring we can do different things with each of them by just changing the functions in the class that's using the interface
		public abstract T accept<T>(visitor<T> vis);
		public interface visitor<T> {
			public T visitBinary(binaryExpr expression);
			public T visitGrouping(groupingExpr expression);
			public T visitLiteral(literalExpr expression);
			public T visitUnary(unaryExpr expression);
			public T visitVar(varExpr expression);
			public T visitAssign(assignmentExpr expression);
			public T visitLogical(logicalExpr expression);
			public T visitCall(callExpr expression);
			public T visitGet(getExpr expression);
			public T visitSet(setExpr expression);
			public T visitArraySet(setArrayExpr expression);
			public T visitThis(thisExpr expression);
			public T visitSuper(superExpr expression);

		}
	}

	class binaryExpr : expr {
		public expr left;
		public token op;
		public expr right;
		public binaryExpr(expr _left, token _op, expr _right) {
			left = _left;
			op = _op;
			right = _right;
		}

		public override T accept<T>(visitor<T> vis) {
			return vis.visitBinary(this);
		}
	}

	class groupingExpr : expr {
		public expr expression;
		public groupingExpr(expr _expression) {
			expression = _expression;
		}

		public override T accept<T>(visitor<T> vis) {
			return vis.visitGrouping(this);
		}
	}

	class literalExpr : expr {
		public object value;//can be either bool, null, number or string
		public literalExpr(object _value) {
			value = _value;
		}

		public override T accept<T>(visitor<T> vis) {
			return vis.visitLiteral(this);
		}
	}

	class unaryExpr : expr {
		public token op;
		public expr right;

		public unaryExpr(token _op, expr _right) {
			op = _op;
			right = _right;
		}
		public override T accept<T>(visitor<T> vis) {
			return vis.visitUnary(this);
		}
	}

	class varExpr : expr {
		public token name;

		public varExpr(token _name) {
			name = _name;
		}
		public override T accept<T>(visitor<T> vis) {
			return vis.visitVar(this);
		}
	}

	class assignmentExpr : expr {
		public token name;
		public expr value;

		public assignmentExpr(token _name, expr _value) {
			name = _name;
			value = _value;
		}
		public override T accept<T>(visitor<T> vis) {
			return vis.visitAssign(this);
		}
	}

	class logicalExpr : expr {
		public expr left;
		public token op;
		public expr right;
		
		public logicalExpr(expr _left, token _op, expr _right) {
			left = _left;
			op = _op;
			right = _right;
		}

		public override T accept<T>(visitor<T> vis) {
			return vis.visitLogical(this);
		}
	}

	class callExpr : expr {
		public expr callee;
		public token paren;
		public List<expr> arguments;

		public callExpr(expr _callee, token _paren, List<expr> _args) {
			callee = _callee;
			paren = _paren;
			arguments = _args;
		}

		public override T accept<T>(visitor<T> vis) {
			return vis.visitCall(this);
		}
	}

	class getExpr : expr {
		public expr obj;
		public token name;

		public getExpr(expr _obj, token _name) {
			obj = _obj;
			name = _name;
		}
		public override T accept<T>(visitor<T> vis) {
			return vis.visitGet(this);
		}
	}
	class setExpr : expr {
		public expr obj;
		public token name;
		public expr value;

		public setExpr(expr _obj, token _name, expr _val) {
			obj = _obj;
			name = _name;
			value = _val;
		}
		public override T accept<T>(visitor<T> vis) {
			return vis.visitSet(this);
		}
	}
	class thisExpr : expr {
		public token keyword;

		public thisExpr(token _keyword) {
			keyword = _keyword;
		}
		public override T accept<T>(visitor<T> vis) {
			return vis.visitThis(this);
		}
	}
	class superExpr : expr {
		public token keyword;
		public token method;

		public superExpr(token _keyword, token _method) {
			keyword = _keyword;
			method = _method;
		}

		public override T accept<T>(visitor<T> vis) {
			return vis.visitSuper(this);
		}
	}
	class setArrayExpr : expr {
		public expr arr;
		public expr index;
		public expr value;
		public token pos;

		public setArrayExpr(expr _arr, expr _val, expr _index, token _pos) {
			arr = _arr;
			value = _val;
			index = _index;
			pos = _pos;
		}
		public override T accept<T>(visitor<T> vis) {
			return vis.visitArraySet(this);
		}
	}

	class AstPrinter : expr.visitor<string> {

		public string print(expr expression) {
			return expression.accept(this);
		}
		public string visitBinary(binaryExpr expression) {
			expr[] exprs = { expression.left, expression.right };
			return parenthesize(expression.op.lexeme, exprs);

		}
		public string visitGrouping(groupingExpr expression) {
			expr[] exprs = { expression.expression };
			return parenthesize("group", exprs);
		}
		public string visitLiteral(literalExpr expression) {
			if(expression.value == null) return "nil";
			return expression.value.ToString();
		}
		public string visitUnary(unaryExpr expression) {
			expr[] exprs = { expression.right };
			return parenthesize(expression.op.lexeme, exprs);
		}

		public string visitVar(varExpr expression) {
			return expression.name.lexeme;
		}
		public string visitAssign(assignmentExpr expression) {
			return expression.name.lexeme + " = " + expression.value.ToString();
		}

		public string visitLogical(logicalExpr expression) {
			expr[] exprs = { expression.left, expression.right };
			return parenthesize(expression.op.lexeme, exprs);
		}

		public string visitCall(callExpr expression) {
			string s = "";
			foreach(expr Expr in expression.arguments) {
				s += Expr.accept(this);
			}
			return expression.callee.accept(this) + s;
		}

		public string visitGet(getExpr expression) {
			return "Getting var from:" + expression.name.lexeme;
		}

		public string visitSet(setExpr expression) {
			return "Setting var to:" + expression.name.lexeme + expression.value.ToString();
		}
		public string visitThis(thisExpr expression) {
			return "This";
		}

		public string visitSuper(superExpr expression) {
			return "Called the superclass's method: " + expression.method.lexeme;
		}

		public string visitArraySet(setArrayExpr expression) {
			return "Setting array value";
		}

		private string parenthesize(string name, expr[] exprs) {
			string s = "(";
			s += name;
			for(int i = 0; i < exprs.Length; i++) {
				s += " ";
				s += exprs[i].accept(this);
			}
			s += ")";

			return s;
		}

	}

}
