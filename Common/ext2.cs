
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using net.yarukizero.vrchat.shizuku.Linq;

using __ConditionsParam = net.yarukizero.vrchat.shizuku.Linq.Conditions.ShizukuParam;
using __ConditionsRecord = net.yarukizero.vrchat.shizuku.Linq.Conditions.ConditionRecord;

using __ConditionExp = System.Linq.Expressions.Expression<
	System.Func<
		net.yarukizero.vrchat.shizuku.IShizukuStore<
			net.yarukizero.vrchat.shizuku.Linq.Conditions.ShizukuParam>,
		net.yarukizero.vrchat.shizuku.Linq.Conditions.ShizukuCondition>>;
using __ActionExp = System.Linq.Expressions.Expression<
	System.Func<
		net.yarukizero.vrchat.shizuku.IShizukuStore<
			net.yarukizero.vrchat.shizuku.Linq.Actions.ShizukuParam>,
		net.yarukizero.vrchat.shizuku.Linq.Actions.ActionRecord>>;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace net.yarukizero.vrchat.shizuku {
	public class Paramator<T> {
		public string name;
		public T value;
		public bool? localOnly;

		public Paramator(string name, T value=default, bool? localOnly=null) {
			this.name = name;
			this.value = value;
			this.localOnly = localOnly;
		}
	}

	public static partial class Extension {

		public static IParameterizableSequence Define(this IParameterizableHost @this) {
			return new ShizukuParSequence(@this);
		}

		public static IParameterizableSequence Bool(this IParameterizableSequence @this, Func<IParameterizableSequence, Paramator<bool?>> exp) {
			var v = exp?.Invoke(@this);
			ThrowIf<InvalidDataException>(v != null);

			@this.Define(v.name, VrcType.Bool, @float(v.value), v.localOnly);
			return @this;
		}

		public static IParameterizableSequence Int(this IParameterizableSequence @this, Func<IParameterizableSequence, Paramator<int?>> exp) {
			var v = exp?.Invoke(@this);
			ThrowIf<InvalidDataException>(v != null);

			@this.Define(v.name, VrcType.Int, @float(v.value), v.localOnly);
			return @this;
		}

		public static IParameterizableSequence Float(this IParameterizableSequence @this, Func<IParameterizableSequence, Paramator<float?>> exp) {
			var v = exp?.Invoke(@this);
			ThrowIf<InvalidDataException>(v != null);

			@this.Define(v.name, VrcType.Float, @float(v.value), v.localOnly);
			return @this;
		}

		// 内部的にはfloat==doubleなのでdoubleは初期化必須
		public static IParameterizableSequence Float(this IParameterizableSequence @this, Func<IParameterizableSequence, Paramator<double>> exp) {
			var v = exp?.Invoke(@this);
			ThrowIf<InvalidDataException>(v != null);

			@this.Define(v.name, VrcType.Float, @float(v.value), v.localOnly);
			return @this;
		}

		public static DefinedResult ToResult(this IParameterizableSequence @this) {
			return @this.ToResult();
		}



		private static float? @float(bool? v) {
			if(v.HasValue) {
				return v.Value ? 1f : 0f;
			} else {
				return null;
			}
		}
		private static float? @float(int? v) {
			if(v.HasValue) {
				return (float) v.Value;
			} else {
				return null;
			}
		}
		private static float? @float(float? v) {
			return v;
		}
		private static float? @float(double v) {
			return (float)v;
		}

	}


	public class DefinedResult {
		private readonly IEnumerable<DefinedParamator> def;

		public bool IsLocalOnly { get; }

		public DefinedResult(IParameterizableSequence sequence, IParameterizableHost host) {
			this.IsLocalOnly = host.IsLocalOnly;
			def = sequence.GetParamators()
				.ToArray();
		}

		public IEnumerable<DefinedParamator> GetDefinedParamators() {
			return def;
		}
	}
}

namespace net.yarukizero.vrchat.shizuku.Linq {
	public class DefinedParamator {
		public string name;
		public VrcType type;
		public float? value;
		public bool? localOnly;

		public DefinedParamator(string name, VrcType type, float? value, bool? localOnly) {
			this.name = name;
			this.type = type;
			this.value = value;
			this.localOnly = localOnly;
		}
	}

	public class ShizukuParSequence : IParameterizableSequence {
		private readonly List<DefinedParamator> def = new();

		private IParameterizableHost Host { get; }

		public ShizukuParSequence(IParameterizableHost host) {
			this.Host = host;
		}

		public void Define(string name, VrcType type, float? value, bool? localOnly) {
			def.Add(new(name, type, value, localOnly));
		}

		public IEnumerable<DefinedParamator> GetParamators() {
			return def.AsReadOnly();
		}

		public DefinedResult ToResult() {
			return new(this, this.Host);
		}

	}

	public interface IParameterizableSequence {
		void Define(string name, VrcType type, float? value, bool? localOnly);
		IEnumerable<DefinedParamator> GetParamators();

		DefinedResult ToResult();
	}
}