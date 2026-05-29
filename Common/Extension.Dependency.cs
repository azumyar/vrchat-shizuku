
using net.yarukizero.vrchat.shizuku.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using __ActionExp = System.Linq.Expressions.Expression<
	System.Func<
		net.yarukizero.vrchat.shizuku.IShizukuStore<
			net.yarukizero.vrchat.shizuku.Linq.Actions.ShizukuParam>,
		net.yarukizero.vrchat.shizuku.Linq.Actions.ActionRecord>>;
using __ConditionExp = System.Linq.Expressions.Expression<
	System.Func<
		net.yarukizero.vrchat.shizuku.IShizukuStore<
			net.yarukizero.vrchat.shizuku.Linq.Conditions.ShizukuParam>,
		net.yarukizero.vrchat.shizuku.Linq.Conditions.ShizukuCondition>>;
using __TransitionDefine = net.yarukizero.vrchat.shizuku.Linq.Conditions.TransitionDefine;
using __ConditionsParam = net.yarukizero.vrchat.shizuku.Linq.Conditions.ShizukuParam;
using __ConditionsRecord = net.yarukizero.vrchat.shizuku.Linq.Conditions.ConditionRecord;
using __ActionRecord = net.yarukizero.vrchat.shizuku.Linq.Actions.ActionRecord;

using __ConditionExpEnumerable = System.Collections.Generic.IEnumerable<
    System.Linq.Expressions.Expression<
        System.Func<
            net.yarukizero.vrchat.shizuku.IShizukuStore<
                net.yarukizero.vrchat.shizuku.Linq.Conditions.ShizukuParam>,
            net.yarukizero.vrchat.shizuku.Linq.Conditions.ShizukuCondition>>>;
using __ActionExpEnumerable = System.Collections.Generic.IEnumerable<
    System.Linq.Expressions.Expression<
        System.Func<
            net.yarukizero.vrchat.shizuku.IShizukuStore<
                net.yarukizero.vrchat.shizuku.Linq.Actions.ShizukuParam>,
            net.yarukizero.vrchat.shizuku.Linq.Actions.ActionRecord>>>;


namespace net.yarukizero.vrchat.shizuku {
    public static partial class  Extension {
        /// <summary>新規シーケンスを開始</summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IConditonSequence Entry(
			this IDependencyHost @this,
			string sequenceName=null,
			Func<__TransitionDefine.Builder, __TransitionDefine> transitFrom=null) {

            return new DependencySequence(
				@this,
				sequenceName: sequenceName,
				transitFrom: transitFrom?.Invoke(new __TransitionDefine.Builder()));
        }

        // 仮置き
        public static IConditonSequence Entry(this IActionSequence @this) {
            return @this.ToNext();
        }

        // 仮置き
        public static DependencySequence Bool(this DependencySequence @this, string name) {
            return @this.SetLocalValiable(name, VrcType.Bool, default);
        }

        // 仮置き
        public static DependencySequence Int(this DependencySequence @this, string name) {
            return @this.SetLocalValiable(name, VrcType.Int, default);
        }


        // 仮置き
        public static DependencySequence Float(this DependencySequence @this, string name) {
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

		public static IActionSequence Nop(this IConditonSequence @this) {
			return @this.ToAction().Nop();
		}

		public static IActionSequence Nop(this IActionSequence @this) {
			@this.AddAction(x => __ActionRecord.Nop());
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

        public static DependencyResult Result(
			this IActionSequence @this,
			Func<__TransitionDefine.Builder, __TransitionDefine> transitTo=null) {

            return @this.ToResult(
				transitTo: transitTo?.Invoke(new()));
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


        // テスト中
        public static IEnumerable<DependencyResult> __Helper(
			this IDependencyHost @this,
			string sequenceName,
            (__ConditionExp Condition, __ActionExp Action) forward,
            (__ConditionExp Condition, __ActionExp Action) reverse) {
            
            return @this.__Helper(
                sequenceName: sequenceName,
                forward: (new __ConditionExp[] { forward.Condition }, new __ActionExp[] { forward.Action }),
                reverse: (new __ConditionExp[] { reverse.Condition }, new __ActionExp[] { reverse.Action })
            );
        }


        // テスト中
        public static IEnumerable<DependencyResult> __Helper(
			this IDependencyHost @this,
			string sequenceName,
            (__ConditionExpEnumerable Condition, __ActionExpEnumerable Action) forward,
            (__ConditionExpEnumerable Condition, __ActionExpEnumerable Action) reverse) {

            var active = $"{sequenceName}.active";
            var diactive = $"{sequenceName}.diactive";
            var toAct = @this.Entry(sequenceName: sequenceName)
                    .Name(active);
            var toDict = @this.Entry(sequenceName: sequenceName)
                    .Name(diactive);
            var act2Diact = @this.Entry(sequenceName: sequenceName, transitFrom: b => b.Name(active).Build())
                    .Name(diactive);
            var diact2Act = @this.Entry(sequenceName: sequenceName, transitFrom: b => b.Name(diactive).Build())
                    .Name(active);
            var actAct = default(IActionSequence);
            var diactAct = default(IActionSequence);
            foreach(var it in forward.Condition) {
                toAct = toAct.Condition(it);
                diact2Act = diact2Act.Condition(it);
            }
            foreach(var it in reverse.Condition) {
                toDict = toDict.Condition(it);
                act2Diact = act2Diact.Condition(it);
            }
            foreach(var it in forward.Action) {
                if(actAct == null) {
                    actAct = toAct.Action(it);
                } else {
                    actAct = actAct.Action(it);                    
                }
            }
            foreach(var it in reverse.Action) {
                if(diactAct == null) {
                    diactAct = toDict.Action(it);
                } else {
                    diactAct = diactAct.Action(it);                    
                }
            }

            return new DependencyResult[] {
                actAct.Result(transitTo: b => b.End().Build()),
                diactAct.Result(transitTo: b => b.End().Build()),
                act2Diact.Nop().Result(transitTo: b => b.End().Build()),
                diact2Act.Nop().Result(transitTo: b => b.End().Build()),
            };
        }


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