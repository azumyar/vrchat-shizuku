
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
		public string Name { get; }
		public T DefaultValue { get; }
		public bool? LocalOnly { get; }
		public bool? Save {  get; }

		public Paramator(string name, T defaultValue=default, bool? localOnly=null, bool? save=null) {
			this.Name = name;
			this.DefaultValue = defaultValue;
			this.LocalOnly = localOnly;
			this.Save = save;
		}
	}

	public static partial class Extension {

		public static IParameterizableSequence Define(this IParameterizableHost @this) {
			return new ShizukuParSequence(@this);
		}

		public static IParameterizableSequence Bool(this IParameterizableSequence @this, Func<IParameterizableSequence, Paramator<bool?>> exp) {
			var v = exp?.Invoke(@this);
			ThrowIf<InvalidDataException>(v != null);

			@this.Define(v.Name, VrcType.Bool, @float(v.DefaultValue), v.LocalOnly, v.Save);
			return @this;
		}

		public static IParameterizableSequence Int(this IParameterizableSequence @this, Func<IParameterizableSequence, Paramator<int?>> exp) {
			var v = exp?.Invoke(@this);
			ThrowIf<InvalidDataException>(v != null);

			@this.Define(v.Name, VrcType.Int, @float(v.DefaultValue), v.LocalOnly, v.Save);
			return @this;
		}

		public static IParameterizableSequence Float(this IParameterizableSequence @this, Func<IParameterizableSequence, Paramator<float?>> exp) {
			var v = exp?.Invoke(@this);
			ThrowIf<InvalidDataException>(v != null);

			@this.Define(v.Name, VrcType.Float, @float(v.DefaultValue), v.LocalOnly, v.Save);
			return @this;
		}

		// 内部的にはfloat==doubleなのでdoubleは初期化必須
		public static IParameterizableSequence Float(this IParameterizableSequence @this, Func<IParameterizableSequence, Paramator<double>> exp) {
			var v = exp?.Invoke(@this);
			ThrowIf<InvalidDataException>(v != null);

			@this.Define(v.Name, VrcType.Float, @float(v.DefaultValue), v.LocalOnly, v.Save);
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
}