
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

namespace net.yarukizero.vrchat.shizuku {
    public static partial class  Extension {
        /// <summary>新規シーケンスを開始</summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IConditonSequence Entry(
			this ITransitionHost @this,
			string sequenceName=null,
			string targetStage=null) {

            return new ShizukuSequence(
				@this,
				sequenceName: sequenceName,
				targetStage: targetStage);
        }

        // 仮置き
        public static IConditonSequence Entry(this IActionSequence @this) {
            return @this.ToNext();
        }

        // 仮置き
        public static ShizukuSequence Bool(this ShizukuSequence @this, string name) {
            return @this.SetLocalValiable(name, VrcType.Bool, default);
        }

        // 仮置き
        public static ShizukuSequence Int(this ShizukuSequence @this, string name) {
            return @this.SetLocalValiable(name, VrcType.Int, default);
        }


        // 仮置き
        public static ShizukuSequence Float(this ShizukuSequence @this, string name) {
            return @this.SetLocalValiable(name, VrcType.Float, default);
        }


		public static T Name<T>(this T @this, string name) where T: ISequence {
			@this.SetName(name);
			return @this;
		}

		/// <summary>条件を設定(ラムダ式に変数を指定した場合式を評価時点の値が使用されます)</summary>
		/// <param name="this"></param>
		/// <param name="exp"></param>
		/// <returns></returns>
		public static IConditonSequence Condition(this IConditonSequence @this, __ConditionExp exp) {
			ThrowIf<InvalidCastException>(exp != null);

            @this.AddCondition(exp);
            return @this;
        }

        public static IConditonSequence Or(this IConditonSequence @this, __ConditionExp exp) {
            ThrowIf<InvalidCastException>(exp != null);

            @this.AddOr(exp);
            return @this;
        }

        public static T LocalOnly<T>(this T @this, bool exp) where T:ISequence {
            @this.SetLocalOnly(exp);
            return @this;
        }

        public static IActionSequence Action(this IConditonSequence @this, __ActionExp exp) {
            return @this.ToAction().Action(exp);
        }

        public static IActionSequence Action(this IActionSequence @this, __ActionExp exp) {
            ThrowIf<InvalidCastException>(exp != null);

            @this.AddAction(exp);
            return @this;
        }

        public static ShizukuResult Result(this IActionSequence @this) {
            return @this.ToResult();
        }

        /*
        public static ShizukuSequence Debug(this ShizukuSequence @this, Action<string> output = null) {
            var debg = output;
            if (debg == null) {
                debg = (x) => Console.WriteLine(x);
            }

            debg($"Entry => {string.Join(" && ", @this.Conditions.Select(x => $"({x.Param.Name} {x.Op} {x.Value}"))})");


            debg($"IsLocalOnry => {@this.IsLocalOnly}");


            return @this;
        }
        */



        private static void ThrowIf<T>(bool condition, Expression exp=null) where T : Exception {
            if(!condition) {
                var exception = exp;
                if (exception == null) {
                    exception = Expression.New(typeof(T));
                }

                throw Expression.Lambda<T>(exception)
                    .Compile();
            }
        }

        private static bool TryToImpl<T, U>(this T @this, out U result)
            where T:class
            where U:class {

            result = @this as U;
            return (result == null);
        }

    }
}