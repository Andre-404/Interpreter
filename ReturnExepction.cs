using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter {
	class Return : Exception{
		public object value;

		public Return(object _val) {
			value = _val;
		}
	}
}
