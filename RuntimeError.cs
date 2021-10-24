using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter {
	//a runtime error that causes the internal stack to be cleared, meaning we start from a clean slate
	class RuntimeError : Exception{
		public token Token;

		public RuntimeError(token _Token, string msg) : base(msg){
			Token = _Token;
		}
	}
}
