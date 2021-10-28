using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Interpreter {
	class interpreter : expr.visitor<object>, stmt.visitor<object>{
		public enviroment globals = new enviroment();//the global enviroment
		private enviroment Enviroment;
		private Dictionary<expr, int> locals = new Dictionary<expr, int>();//the local variables that are inside specific scopes

		public interpreter() {
			Enviroment = globals;
			globals.define("systemClock", new clockClass());
			globals.define("systemReadLine", new readLineClass());
			globals.define("systemReadFile", new readFileClass());
			globals.define("List", new loxListClass("List", null, new Dictionary<string, loxFunction>(), convertNativeFunc));
			globals.define("Array", new loxArrayClass("Array", null, new Dictionary<string, loxFunction>(), convertNativeFunc));
		}

		private class clockClass : LoxCallable{
			
			public int arity() {
				return 0;
			}

			public object call(interpreter inter, List<object> args, token T) {
				return (double)DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
			}

			public string toString() {
				return "<native fn>";
			}

		}

		private class readLineClass : LoxCallable {

			public int arity() {
				return 0;
			}

			public object call(interpreter inter, List<object> args, token T) {
				return (string) Console.ReadLine();
			}

			public string toString() {
				return "<native fn>";
			}

		}

		private class readFileClass : LoxCallable {

			public int arity() {
				return 1;
			}

			public object call(interpreter inter, List<object> args, token T) {
				if(!(args[0] is string)) throw new RuntimeError(T, "File path must be string");
				string s = File.ReadAllText((string)args[0]);
				return s;
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
		private loxFunction convertNativeFunc(object func) {
			return new loxFunction(null, Enviroment, false, (nativeFunc)func);
		}

		public void resolve(expr expression, int depth) {
			//adds a local variable so that we know that in the provided "expression", the variable definition is depth hops away
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

		public object evaluate(expr expression) {
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
			//we see if the current expression contains a variable that inside the "locals" dictionary, if it isnt that means
			//its either not using variables or the variables it's using have global scope
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
			//if this variable needs to be assigned to a scope other than the current one, we use assignAt
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

			//applies to both AND and OR, here we short circut, if the operation is OR and the left side of the operation is true, we 
			//immediately return true, and if it isn't, we parse the right side of the expression
			if(expression.op.type == TokenType.OR) {
				if(isTruthy(left))
					return left;
			} else {
				//if we have a AND expression and the left side is false, we can immediately return false since there is no way this expression
				//will return true
				if(!isTruthy(left))
					return left;
			}

			return evaluate(expression.right);
		}

		public object visitCall(callExpr expression) {
			//a bit sucuffed, but it works
			object callee = evaluate(expression.callee);

			//first we evaluate the arguments and add them to a list
			List<object> arguments = new List<object>();
			foreach(expr argument in expression.arguments) {
				arguments.Add(evaluate(argument));
			}

			if(expression.paren.type == TokenType.RIGHT_PAREN){
				//if we have a function or a method, we first check if it's a child of lox callable
				if(!typeof(LoxCallable).IsInstanceOfType(callee)) {
					throw new RuntimeError(expression.paren, "Can only call functions and classes.");
				}
				LoxCallable function = (LoxCallable)callee;
				//next we check if the arg count is the same as func arity
				if(arguments.Count != function.arity()) {
					throw new RuntimeError(expression.paren, "Expected " +
						function.arity() + " arguments but got " +
						arguments.Count + ".");
				}

				//after all the checks we invoke the call method of the function
				return function.call(this, arguments, expression.paren);
			} else {
				//check if we got a internal type instance
				if(!(callee is loxInstance) || ((loxInstance)callee).type == InstanceType.CUSTOM) {
					throw new RuntimeError(expression.paren,
										   "Can only access type objects.");
				}
				loxInstance inst = (loxInstance)callee;

				//get the arguments(if any are needed)
				List<object> args = new List<object>();
				foreach(expr argument in expression.arguments) {
					args.Add(evaluate(argument));
				}
				loxFunction func = (loxFunction)inst.get(new token(TokenType.IDENTIFIER, "get", null, expression.paren.line));
				return func.call(this, args, expression.paren);

			}
		}

		public object visitGet(getExpr expression) {
			//we evaluate the expression we are trying to access
			object obj = evaluate(expression.obj);
			//we can only access instances, so we check if what we got from evaluating is a instance
			if(typeof(loxInstance).IsInstanceOfType(obj)) {
				return ((loxInstance)obj).get(expression.name);//we return the value we are looking for
			}

			throw new RuntimeError(expression.name,
				"Only instances have properties.");
		}

		public object visitSet(setExpr expression) {
			object obj = evaluate(expression.obj);
			//we check if the value if a instance
			if(!(obj is loxInstance)) {
				throw new RuntimeError(expression.name,
									   "Only instances have fields.");
			}
			//evaluate the expression of the value and then set it for the specified instance in the expression
			object value = evaluate(expression.value);
			((loxInstance)obj).set(expression.name, value);
			return value;
		}

		public object visitSetBracket(setExprBracket expression) {
			object obj = evaluate(expression.variable);//evaluate to get the internal type instance

			//check if we really got instance
			if(!(obj is loxInstance) || ((loxInstance)obj).type == InstanceType.CUSTOM) {
				throw new RuntimeError(expression.pos,
									   "Can only access type objects.");
			}
			loxInstance inst = (loxInstance)obj;

			//pack all the arguments,and put the value as the first argument
			List<object> args = new List<object>();
			args.Add(evaluate(expression.value));
			foreach(expr argument in expression.index) {
				args.Add(evaluate(argument));
			}
			//look for the "set" method in the instance and check it's arity
			loxFunction func = (loxFunction)inst.get(new token(TokenType.IDENTIFIER, "set", null, expression.pos.line));
			if(args.Count != func.arity()) {
				throw new RuntimeError(expression.pos, "Expected " +
					(func.arity() - 1) + " indexes inside brackets but got: " +
					(args.Count-1) + ".");
			}
			return func.call(this, args, expression.pos);
		}

		public object visitThis(thisExpr expression) {
			// looks up the definition of "this" for the current scope(meaning it will return the nearest instance if any)
			return lookUpVar(expression.keyword, expression);
		}

		public object visitSuper(superExpr expression) {
			//get the "super" keyword from a scope that's above the scope of "this" if there is any superclass
			int dist = locals[expression];
			loxClass superclass = (loxClass)(Enviroment.getAt(dist, "super"));

			loxInstance obj = (loxInstance)(Enviroment.getAt(dist - 1, "this"));

			loxFunction method = superclass.findMethod(expression.method.lexeme);//returns the method of the superclass

			if(method == null) {
				throw new RuntimeError(expression.method, "Undefined property '" + expression.method.lexeme + "'.");
			}

			return method.bind(obj);//binds the superclasses method to the current instance
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
			//checks if the condition inside the if is true, if it is, execute the "if" branch, if it isn't and we have a else branch, execute it instead
			if(isTruthy(evaluate(statement.condition))) {
				execute(statement.thenBranch);
			} else if(statement.elseBranch != null) {
				execute(statement.elseBranch);
			}
			return null;
		}

		public object visitWhile(whileStmt statement) {
			//self explanatory
			while(isTruthy(evaluate(statement.condition))) {
				execute(statement.body);
			}
			return null;
		}

		public object visitFunc(funcStmt statement) {
			//we make a new function object with the current environment as it's closure
			loxFunction function = new loxFunction(statement, Enviroment, false, null);
			Enviroment.define(statement.name.lexeme, function);//define the function in the current environment for later use
			return null;
		}

		public object visitReturn(returnStmt statement) {
			object value = null;
			if(statement.value != null) value = evaluate(statement.value);
			//we throw a exepction since we dont know how deep we are in the internal stack, we catch it where we call the function
			throw new Return(value);
		}

		public object visitClass(classStmt statement) {
			//if we are inheriting from a class, make sure it really is a class
			object superClass = null;
			if(statement.superClass != null) {
				superClass = evaluate(statement.superClass);
				if(!(superClass is loxClass)) {
					throw new RuntimeError(statement.superClass.name, "Superclass must be a class.");
				}
			}

			Enviroment.define(statement.name.lexeme, null);

			//creates a new environment to house the "super" keyword
			if(statement.superClass != null) {
				Enviroment = new enviroment(Enviroment);
				Enviroment.define("super", superClass);
			}
			//we loop over the body of the class and look for every method
			Dictionary<string, loxFunction> methods = new Dictionary<string, loxFunction>();
			foreach(funcStmt method in statement.methods) {
				//this makes sure we brand the "init" function as the constructor
				loxFunction function = new loxFunction(method, Enviroment, method.name.lexeme.Equals("init"), null);
				if(!methods.TryAdd(method.name.lexeme, function)) {
					methods[method.name.lexeme] = function;
				}
			}
			//create the new class
			loxClass klass = new loxClass(statement.name.lexeme, (loxClass)superClass, methods);
			//close the super environment
			if(superClass != null) {
				Enviroment = Enviroment.enclosing;
			}
			//assign the class to the current environment
			Enviroment.assign(statement.name, klass);
			return null;
		}

		public object visitForeach(foreachStmt statement) {
			//we evaluate the variable holding the collection we want to iterate over
			object collection = evaluate(statement.collection);
			if(!(collection is loxInstance)) {
				throw new RuntimeError(statement.keyword, "'foreach' Can only iterate over collections.");
			}
			loxInstance inst = (loxInstance)collection;
			//for every type of collection, we need to iterate differently
			switch(inst.type) {
				case InstanceType.LIST:
					foreachList(inst, statement);
					break;
				case InstanceType.ARRAY:
					break;
				case InstanceType.DICTIONARY:
					break;
				default:
					throw new RuntimeError(statement.keyword, "'foreach' Can only iterate over collections.");
					break;
			}

			return null;
		}

		#endregion

		#region Helpers

		private void foreachList(loxInstance inst, foreachStmt statement) {
			object tempL;
			//since ___loxInternalList is a property that people can change, we need to make sure it's still there
			if(!inst.fields.TryGetValue("___loxInternalList", out tempL)) {
				throw new RuntimeError(statement.keyword, "Does not contain a list.");
			}
			List<object> internalList = (List<object>)tempL;
			foreach(object o in internalList) {
				Enviroment.define(statement.declaration.lexeme, o);
				execute(statement.body);
			}
		}

		#endregion
	}
}
