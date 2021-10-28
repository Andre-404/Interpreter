using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter {
	class loxListClass : loxClass, LoxCallable {

		public loxListClass(string _name, loxClass _superClass, Dictionary<string, loxFunction> _methods, Func<LoxCallable, loxFunction> func) : base(_name, _superClass, _methods) {
			methods = new Dictionary<string, loxFunction>();
			methods.Add("push", func(new loxListPush()));
			methods.Add("set", func(new loxListSet()));
			methods.Add("remove", func(new loxListRemove()));
			methods.Add("length", func(new loxListLength()));
			methods.Add("get", func(new loxListGet()));
		}

		public new int arity() {
			return 0;
		}

		public new object call(interpreter inter, List<object> arguments, token Token) {
			//this calls the constructor(if any) and creates a new instance of the class
			loxInstance instance = new loxInstance(this);
			instance.fields.Add("___loxInternalList", new List<object>());
			instance.type = InstanceType.LIST;
			return instance;
		}
	}

	class loxArrayClass : loxClass, LoxCallable {

		public loxArrayClass(string _name, loxClass _superClass, Dictionary<string, loxFunction> _methods, Func<LoxCallable, loxFunction> func) : base(_name, _superClass, _methods) {
			methods = new Dictionary<string, loxFunction>();
			methods.Add("set", func(new loxArraySet()));
			methods.Add("length", func(new loxArrayLength()));
			methods.Add("get", func(new loxArrayGet()));
		}

		public new int arity() {
			return 1;
		}

		public new object call(interpreter inter, List<object> arguments, token Token) {
			//this calls the constructor(if any) and creates a new instance of the class
			if(!(arguments[0] is double)) {
				throw new RuntimeError(Token, "Array length must be a number.");
			}
			loxInstance instance = new loxInstance(this);
			instance.fields.Add("___loxInternalArray", new object[Convert.ToInt32(arguments[0])]);

			instance.type = InstanceType.LIST;
			return instance;
		}
	}
}
