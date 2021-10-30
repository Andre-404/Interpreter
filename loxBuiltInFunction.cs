using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
			tempArr = inst.fields["___loxInternalList"];
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
			tempArr = inst.fields["___loxInternalList"];
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
			tempArr = inst.fields["___loxInternalList"];
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
			object value = args[1];
			if(!(args[0] is double)) {
				throw new RuntimeError(Token, "Index must be a number.");
			}
			int index = Convert.ToInt32(args[0]);
			loxInstance inst = (loxInstance)obj;
			tempArr = inst.fields["___loxInternalList"];
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
			tempArr = inst.fields["___loxInternalList"];
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
			tempArr = inst.fields["___loxInternalArray"];
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
			object value = args[1];
			if(!(args[0] is double)) {
				throw new RuntimeError(Token, "Index must be a number.");
			}
			int index = Convert.ToInt32(args[0]);
			loxInstance inst = (loxInstance)obj;
			tempArr = inst.fields["___loxInternalArray"];
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
			tempArr = inst.fields["___loxInternalArray"];
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

	#region Hashes
	class loxHashLength : nativeFunc {

		public override int arity() {
			return 0;
		}

		public override object call(interpreter inter, List<object> args, token Token) {
			object tempArr;
			loxInstance inst = (loxInstance)obj;
			tempArr = inst.fields["___loxInternalHash"];
			if(!(tempArr is Dictionary<object, object>))
				throw new RuntimeError(Token, "Field '___loxInternalHash' is not a hash.");
			return ((Dictionary<object, object>)tempArr).Count;
		}

		public string toString() {
			return "<native fn>";
		}

	}

	class loxHashSet : nativeFunc {
		public override int arity() {
			return 2;
		}

		public override object call(interpreter inter, List<object> args, token Token) {
			object tempHash;
			object value = args[1];

			loxInstance inst = (loxInstance)obj;
			tempHash = inst.fields["___loxInternalHash"];
			string keyType = (string)inst.fields["___loxInternalKeyType"];
			if(!(inter.getType(args[0], Token).name == keyType)) {
				throw new RuntimeError(Token, "Index isn't of the correct type.");
			}

			if(!(tempHash is Dictionary<object, object>))
				throw new RuntimeError(Token, "Field '___loxInternalHash' is not a hash.");
			Dictionary<object, object> tempH = ((Dictionary<object, object>)tempHash);
			if(!tempH.TryAdd(args[0], value)) {
				tempH[args[0]] = value;
			}
			return null;
		}

		public string toString() {
			return "<native fn>";
		}

	}

	class loxHashGet : nativeFunc {

		public override int arity() {
			return 1;
		}

		public override object call(interpreter inter, List<object> args, token Token) {
			object tempHash;

			loxInstance inst = (loxInstance)obj;
			tempHash = inst.fields["___loxInternalHash"];
			string keyType = (string)inst.fields["___loxInternalKeyType"];
			if(!(inter.getType(args[0], Token).name == keyType)) {
				throw new RuntimeError(Token, "Index isn't of the correct type.");
			}

			if(!(tempHash is Dictionary<object, object>))
				throw new RuntimeError(Token, "Field '___loxInternalHash' is not a hash.");
			Dictionary<object, object> tempH = ((Dictionary<object, object>)tempHash);
			object val;
			if(!tempH.TryGetValue(args[0], out val)) {
				throw new RuntimeError(Token, "Field " + args[0].ToString() + " not defined.");
			}
			return val;
		}

		public string toString() {
			return "<native fn>";
		}

	}

	#endregion

	#region Misc funcs
	class clockClass : LoxCallable {

		public int arity() {
			return 0;
		}

		public object call(interpreter inter, List<object> args, token T) {
			return (double)DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		}

		public string toString() {
			return "<native fn>";
		}

	}

	class readLineClass : LoxCallable {

		public int arity() {
			return 0;
		}

		public object call(interpreter inter, List<object> args, token T) {
			return (string)Console.ReadLine();
		}

		public string toString() {
			return "<native fn>";
		}

	}

	class readFileClass : LoxCallable {

		public int arity() {
			return 1;
		}

		public object call(interpreter inter, List<object> args, token T) {
			if(!(args[0] is string))
				throw new RuntimeError(T, "File path must be string");
			string s = File.ReadAllText((string)args[0]);
			return s;
		}

		public string toString() {
			return "<native fn>";
		}

	}

	class getType : LoxCallable {
		public int arity() {
			return 1;
		}

		public object call(interpreter inter, List<object> args, token T) {
			return inter.getType(args[0], T);
		}

		public string toString() {
			return "<native fn>";
		}
	}

	#endregion
}
