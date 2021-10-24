using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter {
	class loxInstance {
		private loxClass klass;
		private Dictionary<string, object> fields = new Dictionary<string, object>();

		public loxInstance(loxClass _klass) {
			klass = _klass;
		}

		public object get(token name) {
			if(fields.ContainsKey(name.lexeme)) {
				object val;
				if(fields.TryGetValue(name.lexeme, out val)) return val;
			}

			loxFunction method = klass.findMethod(name.lexeme);
			if(method != null) return method.bind(this);

			throw new RuntimeError(name,
				"Undefined property '" + name.lexeme + "'.");
		}

		public void set(token name, object value) {
			if(!fields.TryAdd(name.lexeme, value)) {
				fields[name.lexeme] = value;
			}
		}


		public override string ToString() {
			return klass.name + " instance";
		}
	}
}
