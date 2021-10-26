using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter {
	class loxFunction : LoxCallable{
		private funcStmt declaration;
		private enviroment closure;
		private bool isInit;

		public loxFunction(funcStmt _declaration, enviroment _closure, bool _isInit) {
			declaration = _declaration;
			closure = _closure;
			isInit = _isInit;
		}

		public object call(interpreter Interpreter, List<object> arguments, token Token) {
			enviroment Enviroment = new enviroment(closure);
			for(int i = 0; i < declaration.param.Count; i++) {
				Enviroment.define(declaration.param[i].lexeme, arguments[i]);
			}
			try{
				Interpreter.executeBlock(declaration.body, Enviroment);
			}catch(Return returnValue) {
				if(isInit) return closure.getAt(0, "this");
				return returnValue.value;
			}
			if(isInit) return closure.getAt(0, "this");
			return null;
		}

		public int arity() {
			return declaration.param.Count;
		}

		public loxFunction bind(loxInstance inst) {
			enviroment Enviroment = new enviroment(closure);
			Enviroment.define("this", inst);
			return new loxFunction(declaration, Enviroment, isInit);
		}

		public override string ToString() {
			return "<fn " + declaration.name.lexeme + ">";
		}
	}
}
