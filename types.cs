using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter {
	class loxType {
		public string name;

		public loxType(string _name) {
			name = _name;
		}

		public override string ToString() {
			return name;
		}
	}
}
