using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter {
	class loxClass : LoxCallable {
		public string name;
		public Dictionary<string, loxFunction> methods;
		public loxClass superClass;

		public loxClass(string _name, loxClass _superClass, Dictionary<string, loxFunction> _methods) {
			//when we create a new class we need 3 things, the name of the class, the superclass(if any) and a list of all the methods inside it
			name = _name;
			methods = _methods;
			superClass = _superClass;
		}

		public override string ToString() {
			return name;
		}

		public int arity() {
			//this figures out the arity of the constructor(if we have a constructor)
			loxFunction initializer = findMethod("init");
			if(initializer == null)
				return 0;
			return initializer.arity();
		}

		public object call(interpreter inter, List<object> arguments, token Token) {
			//this calls the constructor(if any) and creates a new instance of the class
			loxInstance instance = new loxInstance(this);
			loxFunction initializer = findMethod("init");
			if(initializer != null) {
				initializer.bind(instance).call(inter, arguments, Token);
			}
			return instance;
		}


		public loxFunction findMethod(string name) {
			// finds the method with the provided name inside the class(if it exists)
			if(methods.ContainsKey(name)) {
				loxFunction func;
				if(methods.TryGetValue(name, out func)) return func;
			}
			//if we didn't find the method in the class, check the superclass(if we have one)
			if(superClass != null) {
				return superClass.findMethod(name);
			}

			return null;
		}
	}
}
