using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter {
	class resolver : stmt.visitor<object>, expr.visitor<object> {
		private interpreter Interpreter;
		private Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();
		private FunctionType currentFunc = FunctionType.NONE;
		private ClassType currentClass = ClassType.NONE;

		public resolver(interpreter _interpreter) {
			Interpreter = _interpreter;
		}

		public object visitBlock(blockStmt statement) {
			beginScope();
			resolve(statement.statements);
			endScope();
			return null;
		}

		public void beginScope() {
			scopes.Push(new Dictionary<string, bool>());
		}

		public void resolve(List<stmt> statements) {
			foreach(stmt statement in statements) {
				resolve(statement);
			}
		}

		private void resolve(stmt statement) {
			statement.accept(this);
		}

		private void resolve(expr expression) {
			expression.accept(this);
		}

		private void endScope() {
			scopes.Pop();
		}
		private void declare(token name) {
			if(scopes.Count == 0)
				return;

			Dictionary<string, bool> dict = scopes.Peek();
			if(dict.ContainsKey(name.lexeme)) {
				Lox.error(name, "Already a variable with this name in this scope.");
			}
			dict.Add(name.lexeme, false);
		}

		private void define(token name) {
			if(scopes.Count == 0)
				return;

			if(!scopes.Peek().TryAdd(name.lexeme, true)) {
				scopes.Peek()[name.lexeme] = true;

			}
		}

		private void resolveLocal(expr expression, token name) {
			for(int i = 0; i < scopes.Count; i++) {
				if(scopes.ElementAt(i).ContainsKey(name.lexeme)) {
					Interpreter.resolve(expression, i);
					return;
				}
			}
		}

		#region Statements
		public object visitVar(varStmt statement) {
			declare(statement.name);
			if(statement.initializer != null) {
				resolve(statement.initializer);
			}
			define(statement.name);

			return null;
		}
		public object visitFunc(funcStmt statement) {
			declare(statement.name);
			define(statement.name);

			resolveFunction(statement, FunctionType.FUNCTION);
			return null;
		}

		private void resolveFunction(funcStmt statement, FunctionType type) {
			FunctionType enclosingFunc = currentFunc;
			currentFunc = type;
			beginScope();
			foreach(token param in statement.param) {
				declare(param);
				define(param);
			}
			resolve(statement.body);
			endScope();
			currentFunc = enclosingFunc;
		}

		public object visitExpression(expressionStmt statement) {
			resolve(statement.expression);
			return null;
		}

		public object visitIf(ifStmt statement) {
			resolve(statement.condition);
			resolve(statement.thenBranch);
			if(statement.elseBranch != null)
				resolve(statement.elseBranch);
			return null;
		}

		public object visitPrint(printStmt statement) {
			resolve(statement.expression);
			return null;
		}

		public object visitReturn(returnStmt statement) {
			if(currentFunc == FunctionType.NONE) {
				Lox.error(statement.keyword, "Can't return from top-level code.");
			}
			if(statement.value != null) {
				if(currentFunc == FunctionType.INIT) {
					Lox.error(statement.keyword,
						"Can't return a value from an initializer.");
				}
				resolve(statement.value);
			}

			return null;
		}

		public object visitWhile(whileStmt statement) {
			resolve(statement.condition);
			resolve(statement.body);
			return null;
		}

		public object visitClass(classStmt statement) {
			ClassType enclosingClass = currentClass;
			currentClass = ClassType.CLASS;

			declare(statement.name);
			define(statement.name);

			if(statement.superClass != null && statement.name.lexeme.Equals(statement.superClass.name.lexeme)) {
				Lox.error(statement.superClass.name, "A class can't inherit from itself.");
			}

			if(statement.superClass != null) {
				currentClass = ClassType.SUBCLASS;
				resolve(statement.superClass);
			}

			if(statement.superClass != null) {
				beginScope();
				scopes.Peek().Add("super", true);
			}

			beginScope();
			scopes.Peek().Add("this", true);

			foreach(funcStmt method in statement.methods) {
				FunctionType type = FunctionType.METHOD;
				if(method.name.lexeme.Equals("init")) {
					type = FunctionType.INIT;
				}
				resolveFunction(method, type);
			}
			endScope();
			if(statement.superClass != null) endScope();
			currentClass = enclosingClass;

			return null;
		}
		#endregion

		#region Expressions
		public object visitVar(varExpr expression) {
			bool tryVal;
			if(scopes.Count > 0 && scopes.Peek().TryGetValue(expression.name.lexeme, out tryVal) && tryVal == false){
				Lox.error(expression.name, "Can't read local variable in its own initializer.");
			}
			resolveLocal(expression, expression.name);
			return null;
		}

		public object visitAssign(assignmentExpr expression) {
			resolve(expression.value);
			resolveLocal(expression, expression.name);

			return null;
		}
		public object visitBinary(binaryExpr expression) {
			resolve(expression.left);
			resolve(expression.right);
			return null;
		}

		public object visitCall(callExpr expression) {
			resolve(expression.callee);

			foreach(expr argument in expression.arguments) {
				resolve(argument);
			}

			return null;
		}

		public object visitGrouping(groupingExpr expression) {
			resolve(expression.expression);
			return null;
		}

		public object visitLiteral(literalExpr expression) {
			return null;
		}

		public object visitLogical(logicalExpr expression) {
			resolve(expression.left);
			resolve(expression.right);
			return null;
		}
		public object visitUnary(unaryExpr expression) {
			resolve(expression.right);
			return null;
		}

		public object visitGet(getExpr expression) {
			resolve(expression.obj);
			return null;
		}

		public object visitSet(setExpr expression) {
			resolve(expression.obj);
			resolve(expression.value);

			return null;
		}

		public object visitThis(thisExpr expression) {
			resolveLocal(expression, expression.keyword);
			if(currentClass == ClassType.NONE) {
				Lox.error(expression.keyword,
					"Can't use 'this' outside of a class.");
				return null;
			}
			return null;
		}

		public object visitSuper(superExpr expression) {
			if(currentClass == ClassType.NONE) {
				Lox.error(expression.keyword,
					"Can't use 'super' outside of a class.");
			} else if(currentClass != ClassType.SUBCLASS) {
				Lox.error(expression.keyword,
					"Can't use 'super' in a class with no superclass.");
			}
			resolveLocal(expression, expression.keyword);
			return null;
		}

		#endregion
	}
}
