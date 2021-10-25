using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter {
	class interpreter : expr.visitor<object>, stmt.visitor<object>{
		public enviroment globals = new enviroment();//the global enviroment
		private enviroment Enviroment;
		private Dictionary<expr, int> locals = new Dictionary<expr, int>();

		public interpreter() {
			Enviroment = globals;
			globals.define("clock", new clockClass());

		}

		private class clockClass : LoxCallable{
			
			public int arity() {
				return 0;
			}

			public object call(interpreter inter, List<object> args) {
				return (double)DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
			}

			public string toString() {
				return "<native fn>";
			}

		}
		
		public void interpret(List<stmt> statements) {
			//executes all the statements that have been generated
			try {
				foreach(stmt statement in statements) {
					execute(statement);
				}
			} catch(RuntimeError error) {
				Lox.runtimeError(error);
			}
		}

		public void resolve(expr expression, int depth) {
			if(!locals.TryAdd(expression, depth)) {
				locals[expression] = depth;
			}
		}
		#region Expressions
		public object visitLiteral(literalExpr expression) {
			return expression.value;//returns the literal value of the expression
		}

		public object visitGrouping(groupingExpr expression) {
			return evaluate(expression.expression);//returns the value of the expression inside the parentheses
		}

		private object evaluate(expr expression) {
			return expression.accept(this);//this calls the different visit methods of the visitor interface that were implemented in this class
		}

		public object visitUnary(unaryExpr expression) {
			object right = evaluate(expression.right);//recursivly evaluetes the value of the right expression

			switch(expression.op.type) {
				case TokenType.BANG:
					return !isTruthy(right);
				case TokenType.MINUS:
					checkNumberOperand(expression.op, right);//checks if "right"(which we recursivly evaluated before) is a number
					return -(double)right;
			}

			return null;
		}

		private bool isTruthy(object obj) {
			//returns the bool of "right", and if the result is anything other than "null", we also return true
			if(obj == null)
				return false;
			if(typeof(bool).IsInstanceOfType(obj)) return (bool)obj;
			return true;
		}

		public object visitBinary(binaryExpr expression) {
			//first we recursivly evaluate both the left and the right side of the expression
			object left = evaluate(expression.left);
			object right = evaluate(expression.right);

			//next, based on the operand between the 2 expression, we do different things
			switch(expression.op.type) {
				case TokenType.GREATER:
					checkNumberOperands(expression.op, left, right);
					return (double)left > (double)right;

				case TokenType.GREATER_EQUAL:
					checkNumberOperands(expression.op, left, right);
					return (double)left >= (double)right;

				case TokenType.LESS:
					checkNumberOperands(expression.op, left, right);
					return (double)left < (double)right;

				case TokenType.LESS_EQUAL:
					checkNumberOperands(expression.op, left, right);
					return (double)left <= (double)right;

				case TokenType.MINUS:
					checkNumberOperands(expression.op, left, right);
					return (double)left - (double)right;

				case TokenType.PLUS:
					//here we have to do a bit of hacking in order to ensure we can add both number and strings
					if(typeof(double).IsInstanceOfType(left) && typeof(double).IsInstanceOfType(right)) {
						return (double)left + (double)right;
					}

					if(typeof(string).IsInstanceOfType(left) && typeof(string).IsInstanceOfType(right)) {
						return (string)left + (string)right;
					}
					throw new RuntimeError(expression.op, "Operands must be two numbers or two strings.");
					break;

				case TokenType.SLASH:
					checkNumberOperands(expression.op, left, right);
					return (double)left / (double)right;

				case TokenType.STAR:
					checkNumberOperands(expression.op, left, right);
					return (double)left * (double)right;

				case TokenType.BANG_EQUAL:
					return !isEqual(left, right);

				case TokenType.EQUAL_EQUAL:
					return isEqual(left, right);

			}

			// Unreachable.
			return null;
		}
		private bool isEqual(object a, object b) {
			if(a == null && b == null)
				return true;
			if(a == null)
				return false;

			return a.Equals(b);
		}

		private void checkNumberOperand(token op, object operand) {
			//if the operand isn't a number, throw a runtime error
			if(typeof(double).IsInstanceOfType(operand)) return;
			throw new RuntimeError(op, "Operand must be a number.");
		}

		private void checkNumberOperands(token op, object left, object right) {
			//both operands must be numbers in order for this to now throw a error
			if(typeof(double).IsInstanceOfType(left) && typeof(double).IsInstanceOfType(right)) return;

			throw new RuntimeError(op, "Operands must be numbers.");
		}

		private string stringify(object obj) {
			//converts the value of the object to a string
			if(obj == null)
				return "nil";

			if(typeof(double).IsInstanceOfType(obj)) {
				string text = obj.ToString();
				if(text.EndsWith(".0")) {
					text = text.Substring(0, text.Length - 2);
				}
				return text;
			}

			return obj.ToString();
		}

		public object visitVar(varExpr expression) {
			return lookUpVar(expression.name, expression);//returns the value of the variable specified in the current scope
		}

		private object lookUpVar(token name, expr expression) {
			int dist;
			if(locals.TryGetValue(expression, out dist)) {
				return Enviroment.getAt(dist, name.lexeme);
			} else {
				return globals.get(name);
			}
		}

		public object visitAssign(assignmentExpr expression) {
			//gets the value of the assigment(so the right side of the =)
			//this can be any expression
			object val = evaluate(expression.value);
			int dist;
			if(locals.TryGetValue(expression, out dist)) {
				Enviroment.assignAt(dist, expression.name, val);
			} else {
				globals.assign(expression.name, val);
			}
			return val;
		}

		public object visitLogical(logicalExpr expression) {
			object left = evaluate(expression.left);

			if(expression.op.type == TokenType.OR) {
				if(isTruthy(left))
					return left;
			} else {
				if(!isTruthy(left))
					return left;
			}

			return evaluate(expression.right);
		}

		public object visitCall(callExpr expression) {
			object callee = evaluate(expression.callee);

			List<object> arguments = new List<object>();
			foreach(expr argument in expression.arguments) {
				arguments.Add(evaluate(argument));
			}

			if(!typeof(LoxCallable).IsInstanceOfType(callee)) {
				throw new RuntimeError(expression.paren, "Can only call functions and classes.");
			}

			LoxCallable function = (LoxCallable)callee;
			if(arguments.Count != function.arity()) {
				throw new RuntimeError(expression.paren, "Expected " +
					function.arity() + " arguments but got " +
					arguments.Count + ".");
			}
			return function.call(this, arguments);
		}

		public object visitGet(getExpr expression) {
			object obj = evaluate(expression.obj);
			if(typeof(loxInstance).IsInstanceOfType(obj)) {
				return ((loxInstance)obj).get(expression.name);
			}

			throw new RuntimeError(expression.name,
				"Only instances have properties.");
		}

		public object visitSet(setExpr expression) {
			object obj = evaluate(expression.obj);

			if(!(obj is loxInstance)) {
				throw new RuntimeError(expression.name,
									   "Only instances have fields.");
			}

			object value = evaluate(expression.value);
			((loxInstance)obj).set(expression.name, value);
			return value;
		}

		public object visitThis(thisExpr expression) {
			return lookUpVar(expression.keyword, expression);
		}

		public object visitSuper(superExpr expression) {
			int dist = locals[expression];
			loxClass superclass = (loxClass)(Enviroment.getAt(dist, "super"));

			loxInstance obj = (loxInstance)(Enviroment.getAt(dist - 1, "this"));

			loxFunction method = superclass.findMethod(expression.method.lexeme);

			if(method == null) {
				throw new RuntimeError(expression.method, "Undefined property '" + expression.method.lexeme + "'.");
			}

			return method.bind(obj);
		}
		#endregion

		#region Statements
		public object visitExpression(expressionStmt statement) {
			evaluate(statement.expression);//since this statement is just a expression, we simply evaluate it
			return null;
		}
		
		public object visitPrint(printStmt statement) {
			//we evaluate the expression part of the statement, and then print it
			object value = evaluate(statement.expression);
			Console.WriteLine(stringify(value));

			return null;
		}

		public object visitVar(varStmt statement) {
			//this statement defines(creates) a new variable(or sets the value of the old one if it exists)
			object val = null;
			//if the initializer isn't null, we set evaluate the initializer expression and set value to it
			if(statement.initializer != null) {
				val = evaluate(statement.initializer);
			}
			Enviroment.define(statement.name.lexeme, val);
			return null;
		}

		public object visitBlock(blockStmt statement) {
			//we execute every statement inside the block in the new enviroment
			executeBlock(statement.statements, new enviroment(Enviroment));
			return null;
		}

		private void execute(stmt statement) {
			//this works on the visitor principle, "accepts" is different for every statement, and it executes the corresponding visit* func
			statement.accept(this);
		}

		public void executeBlock(List<stmt> statements, enviroment env) {
			//set the enviroment to the new one, execute every statement inside the block, and then return to the previous enviroment
			enviroment previous = Enviroment;
			try {
				Enviroment = env;

				foreach (stmt statement in statements) {
					execute(statement);
				}
			} finally {
				Enviroment = previous;
			}
		}

		public object visitIf(ifStmt statement) {
			if(isTruthy(evaluate(statement.condition))) {
				execute(statement.thenBranch);
			} else if(statement.elseBranch != null) {
				execute(statement.elseBranch);
			}
			return null;
		}

		public object visitWhile(whileStmt statement) {
			while(isTruthy(evaluate(statement.condition))) {
				execute(statement.body);
			}
			return null;
		}

		public object visitFunc(funcStmt statement) {
			loxFunction function = new loxFunction(statement, Enviroment, false);
			Enviroment.define(statement.name.lexeme, function);
			return null;
		}

		public object visitReturn(returnStmt statement) {
			object value = null;
			if(statement.value != null) value = evaluate(statement.value);

			throw new Return(value);
		}

		public object visitClass(classStmt statement) {

			object superClass = null;
			if(statement.superClass != null) {
				superClass = evaluate(statement.superClass);
				if(!(superClass is loxClass)) {
					throw new RuntimeError(statement.superClass.name, "Superclass must be a class.");
				}
			}

			Enviroment.define(statement.name.lexeme, null);

			if(statement.superClass != null) {
				Enviroment = new enviroment(Enviroment);
				Enviroment.define("super", superClass);
			}

			Dictionary<string, loxFunction> methods = new Dictionary<string, loxFunction>();
			foreach(funcStmt method in statement.methods) {
				loxFunction function = new loxFunction(method, Enviroment, method.name.lexeme.Equals("init"));
				if(!methods.TryAdd(method.name.lexeme, function)) {
					methods[method.name.lexeme] = function;
				}
			}

			loxClass klass = new loxClass(statement.name.lexeme, (loxClass)superClass, methods);

			if(superClass != null) {
				Enviroment = Enviroment.enclosing;
			}

			Enviroment.assign(statement.name, klass);
			return null;
		}

		#endregion
	}
}
