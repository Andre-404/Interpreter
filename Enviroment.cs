using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter {
	class enviroment {
		private Dictionary<string, object> values = new Dictionary<string, object>();
		public enviroment enclosing;

		public enviroment() {
			enclosing = null;//the enviroment that this one is enclosing(meaning that the "enclosing" env is shadowed by this one
		}

		public enviroment(enviroment _enclosing) {
			enclosing = _enclosing;
		}

		public void define(string name, object val) {
			//if we don't have a variable already stored, create a new one, if we do, we simply change it's value
			//this is done because using "var variable = value" on a variable that already exists will just reassign the value to the variable
			if(!values.ContainsKey(name))
				values.Add(name, val);
			else
				values[name] = val;
		}

		public object get(token name) {
			//retrives the variable of the current enviroment or any ones below it
			if(values.ContainsKey(name.lexeme)) {
				return values.GetValueOrDefault(name.lexeme);
			}

			if(enclosing != null) return enclosing.get(name);

			//if even the enviroments below this one don't have the variable, throw a error
			throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
		}

		public void assign(token name, Object value) {
			//if the variable we're accesing exits, we set it's value
			if(values.ContainsKey(name.lexeme)) {
				values[name.lexeme] =  value;
				return;
			}
			//if the variable doesn't exists in the current enviroment, but it does exits in the ones below this one
			//assign the value to the first enviroment that contains it
			if(enclosing != null) {
				enclosing.assign(name, value);
				return;
			}
			//if the variable doesn't exist in any enviroment, throw error
			throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
		}

		public object getAt(int dist, string name) {
			return ancestor(dist).values[name];
		}

		public enviroment ancestor(int dist) {
			enviroment Env = this;

			for(int i = 0; i < dist; i++) {
				Env = Env.enclosing;
			}

			return Env;
		}

		public void assignAt(int dist, token name, object value) {
			if(!ancestor(dist).values.TryAdd(name.lexeme, value)) {
				ancestor(dist).values[name.lexeme] = value;
			}
		}
	}
}
