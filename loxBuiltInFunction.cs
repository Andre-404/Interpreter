using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter {

	abstract class nativeFunc : LoxCallable {
		public object obj;

		public abstract int arity();

		public abstract object call(interpreter inter, List<object> args, token Token);

		}
	#region ListHandling
	class loxListLength : nativeFunc {

		public override int arity() {
			return 0;
		}

		public override object call(interpreter inter, List<object> args, token Token) {
			object tempArr;
			loxInstance inst = (loxInstance)obj;
			if(!(inst.fields.TryGetValue("___loxInternalList", out tempArr))){
				throw new RuntimeError(Token, "Field 'array' not defined.");
			}
			if(!(tempArr is List<object>)) throw new RuntimeError(Token, "Field 'array' is not a array.");
			return ((List<object>)tempArr).Count;
		}

		public string toString() {
			return "<native fn>";
		}

	}

	class loxListPush : nativeFunc {
		public override int arity() {
			return 1;
		}

		public override object call(interpreter inter, List<object> args, token Token) {
			object tempArr;
			loxInstance inst = (loxInstance)obj;
			if(!(inst.fields.TryGetValue("___loxInternalList", out tempArr))) {
				throw new RuntimeError(Token, "Field 'array' not defined.");
			}
			if(!(tempArr is List<object>))
				throw new RuntimeError(Token, "Field 'array' is not a array.");
			((List<object>)tempArr).Add(args[0]);
			return null;
		}

		public string toString() {
			return "<native fn>";
		}

	}

	class loxListRemove : nativeFunc {

		public override int arity() {
			return 1;
		}

		public override object call(interpreter inter, List<object> args, token Token) {
			object tempArr;
			loxInstance inst  = (loxInstance)obj;
			if(!(inst.fields.TryGetValue("___loxInternalList", out tempArr))) {
				throw new RuntimeError(Token, "Field 'array' not defined.");
			}
			if(!(tempArr is List<object>))
				throw new RuntimeError(Token, "Field 'array' is not a array.");
			((List<object>)tempArr).RemoveAt(Convert.ToInt32(args[0]));
			return null;
		}

		public string toString() {
			return "<native fn>";
		}

	}

	class loxListSet : nativeFunc {
		private int realArgCount = 1;
		public override int arity() {
			return 2;
		}

		public override object call(interpreter inter, List<object> args, token Token) {
			object tempArr;
			object value = args[0];
			if(!(args[1] is double)) {
				throw new RuntimeError(Token, "Index must be a number.");
			}
			int index = Convert.ToInt32(args[1]);
			loxInstance inst = (loxInstance)obj;
			if(!(inst.fields.TryGetValue("___loxInternalList", out tempArr))) {
				throw new RuntimeError(Token, "Field 'array' not defined.");
			}
			if(!(tempArr is List<object>))
				throw new RuntimeError(Token, "Field 'array' is not a array.");
			if(index >= ((List<object>)tempArr).Count) {
				throw new RuntimeError(Token, "Index " + index.ToString() + " outside of range: " + ((List<object>)tempArr).Count.ToString());
			}
			((List<object>)tempArr)[index] = value;
			return null;
		}

		public string toString() {
			return "<native fn>";
		}

	}

	class loxListGet : nativeFunc {

		public override int arity() {
			return 1;
		}

		public override object call(interpreter inter, List<object> args, token Token) {
			object tempArr;
			if(!(args[0] is double)) {
				throw new RuntimeError(Token, "Index must be a number.");
			}
			int index = Convert.ToInt32(args[0]);
			loxInstance inst = (loxInstance)obj;
			if(!(inst.fields.TryGetValue("___loxInternalList", out tempArr))) {
				throw new RuntimeError(Token, "Field 'array' not defined.");
			}
			if(!(tempArr is List<object>))
				throw new RuntimeError(Token, "Field 'array' is not a array.");
			if(index >= ((List<object>)tempArr).Count) {
				throw new RuntimeError(Token, "Index " + index.ToString() + " outside of range: " + ((List<object>)tempArr).Count.ToString());
			}
			return ((List<object>)tempArr)[index];
		}

		public string toString() {
			return "<native fn>";
		}

	}

	#endregion

	#region Arrays
	class loxArrayLength : nativeFunc {

		public override int arity() {
			return 0;
		}

		public override object call(interpreter inter, List<object> args, token Token) {
			object tempArr;
			loxInstance inst = (loxInstance)obj;
			if(!(inst.fields.TryGetValue("___loxInternalArray", out tempArr))) {
				throw new RuntimeError(Token, "Field 'array' not defined.");
			}
			if(!(tempArr is object[]))
				throw new RuntimeError(Token, "Field 'array' is not a array.");
			return ((object[])tempArr).Length; 
		}

		public string toString() {
			return "<native fn>";
		}

	}


	class loxArraySet : nativeFunc {
		public override int arity() {
			return 2;
		}

		public override object call(interpreter inter, List<object> args, token Token) {
			object tempArr;
			object value = args[0];
			if(!(args[1] is double)) {
				throw new RuntimeError(Token, "Index must be a number.");
			}
			int index = Convert.ToInt32(args[1]);
			loxInstance inst = (loxInstance)obj;
			if(!(inst.fields.TryGetValue("___loxInternalArray", out tempArr))) {
				throw new RuntimeError(Token, "Field 'array' not defined.");
			}
			if(!(tempArr is object[]))
				throw new RuntimeError(Token, "Field 'array' is not a array.");
			if(index >= ((object[])tempArr).Length) {
				throw new RuntimeError(Token, "Index " + index.ToString() + " outside of range: " + ((object[])tempArr).Length.ToString());
			}
			((object[])tempArr)[index] = value;
			return null;
		}

		public string toString() {
			return "<native fn>";
		}

	}

	class loxArrayGet : nativeFunc {

		public override int arity() {
			return 1;
		}

		public override object call(interpreter inter, List<object> args, token Token) {
			object tempArr;
			if(!(args[0] is double)) {
				throw new RuntimeError(Token, "Index must be a number.");
			}
			int index = Convert.ToInt32(args[0]);
			loxInstance inst = (loxInstance)obj;
			if(!(inst.fields.TryGetValue("___loxInternalArray", out tempArr))) {
				throw new RuntimeError(Token, "Field 'array' not defined.");
			}
			if(!(tempArr is object[]))
				throw new RuntimeError(Token, "Field 'array' is not a array.");
			if(index >= ((object[])tempArr).Length) {
				throw new RuntimeError(Token, "Index " + index.ToString() + " outside of range: " + ((object[])tempArr).Length.ToString());
			}
			return ((object[])tempArr)[index];
		}

		public string toString() {
			return "<native fn>";
		}

	}

	#endregion
}
