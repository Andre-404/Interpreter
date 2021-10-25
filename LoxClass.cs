using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter {
	class loxClass : LoxCallable {
		public string name;
		private Dictionary<string, loxFunction> methods;
		public loxClass superClass;

		public loxClass(string _name, loxClass _superClass, Dictionary<string, loxFunction> _methods) {
			name = _name;
			methods = _methods;
			superClass = _superClass;

		}

		public override string ToString() {
			return name;
		}

		public int arity() {
			loxFunction initializer = findMethod("init");
			if(initializer == null)
				return 0;
			return initializer.arity();
		}

		public object call(interpreter inter, List<object> arguments) {
			loxInstance instance = new loxInstance(this);
			loxFunction initializer = findMethod("init");
			if(initializer != null) {
				initializer.bind(instance).call(inter, arguments);
			}
			return instance;
		}


		public loxFunction findMethod(string name) {
			if(methods.ContainsKey(name)) {
				loxFunction func;
				if(methods.TryGetValue(name, out func)) return func;
			}

			if(superClass != null) {
				return superClass.findMethod(name);
			}

			return null;
		}
	}
}
