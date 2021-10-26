using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter {
	interface LoxCallable {
		public int arity();
		public object call(interpreter Interpreter, List<object> arguments, token Token);
	}
}
