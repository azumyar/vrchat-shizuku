using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace net.yarukizero.vrchat.shizuku.Linq {
	public interface IParameterizableSequence {
		void Define(string name, VrcType type, float? value, bool? localOnly, bool? save);
		IEnumerable<DefinedParamator> GetParamators();

		DefinedResult ToResult();
	}

	public class DefinedParamator {
		public string Name { get; }
		public VrcType Type { get; }
		public float DefaultValue { get; }
		public bool LocalOnly { get; }
		public bool Save { get; }

		public DefinedParamator(string name, VrcType type, float defaultValue, bool localOnly, bool save) {
			this.Name = name;
			this.Type = type;
			this.DefaultValue = defaultValue;
			this.LocalOnly = localOnly;
			this.Save = save;
		}
	}

	public class ShizukuParSequence : IParameterizableSequence {
		private readonly List<DefinedParamator> def = new();

		private IParameterizableHost Host { get; }

		public ShizukuParSequence(IParameterizableHost host) {
			this.Host = host;
		}

		public void Define(string name, VrcType type, float? value, bool? localOnly, bool? save) {
			float val = value ?? default;
			var lc = localOnly ?? this.Host.IsLocalOnly;
			var sv = save ?? false; // MAの既定的には標準保存のほうがいい？
			def.Add(new(name, type, val, lc, sv));
		}

		public IEnumerable<DefinedParamator> GetParamators() {
			return def.AsReadOnly();
		}

		public DefinedResult ToResult() {
			return new(this, this.Host);
		}

	}
}
