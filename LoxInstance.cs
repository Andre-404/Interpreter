using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter {
	class loxInstance {
		private loxClass klass;
		public Dictionary<string, object> fields = new Dictionary<string, object>();
		public InstanceType type = InstanceType.CUSTOM;//to differentiate between custom and built-in instances

		public loxInstance(loxClass _klass) {
			klass = _klass;
		}

		public object get(token name) {
			//we try to find the value in the fields dictionary, if we can't we check for the methods inside the parent
			if(fields.ContainsKey(name.lexeme)) {
				object val;
				if(fields.TryGetValue(name.lexeme, out val)) return val;
			}
			if(klass != null) {
				loxFunction method = klass.findMethod(name.lexeme);
				if(method != null) return method.bind(this);//bind the function we're calling to the this instance
			}
			throw new RuntimeError(name,
				"Undefined property '" + name.lexeme + "'.");
		}

		public void set(token name, object value) {
			//sets the value to the dictionary
			if(!fields.TryAdd(name.lexeme, value)) {
				fields[name.lexeme] = value;
			}
		}


		public override string ToString() {
			return klass.name + " instance";
		}
	}
}
