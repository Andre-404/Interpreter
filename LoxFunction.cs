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
		private nativeFunc nativeFunc;

		public loxFunction(funcStmt _declaration, enviroment _closure, bool _isInit, nativeFunc _nativeFunc) {
			declaration = _declaration;
			closure = _closure;
			isInit = _isInit;
			nativeFunc = _nativeFunc;
		}

		public object call(interpreter Interpreter, List<object> arguments, token Token) {
			//first we check if this is just a wrap for a native function
			if(nativeFunc == null){
				//if it isn't we create a new environment for the code to execute in, we stored it's closure when we created the function
				enviroment Enviroment = new enviroment(closure);
				for(int i = 0; i < declaration.param.Count; i++) {
					//define all the arguements
					Enviroment.define(declaration.param[i].lexeme, arguments[i]);
				}
				try{
					Interpreter.executeBlock(declaration.body, Enviroment);//executes the function
				}catch(Return returnValue) {
					//if we have a return value, we return it, if the current function is a constructor, we return the id of the instance
					if(isInit) return closure.getAt(0, "this");
					return returnValue.value;
				}
				if(isInit) return closure.getAt(0, "this");
			} else {
				//we execute the native function but first we provide it with the current obj
				nativeFunc.obj = closure.getAt(0, "this");
				return nativeFunc.call(Interpreter, arguments, Token);
			}
			return null;
		}

		public int arity() {
			return nativeFunc == null ? declaration.param.Count : nativeFunc.arity();
		}

		public loxFunction bind(loxInstance inst) {
			//this binds the function to a instance, it creates a new environment with "this" defined, and it encapsulates the function
			enviroment Enviroment = new enviroment(closure);
			Enviroment.define("this", inst);
			return new loxFunction(declaration, Enviroment, isInit, nativeFunc);
		}

		public override string ToString() {
			return "<fn " + declaration.name.lexeme + ">";
		}
	}
}
