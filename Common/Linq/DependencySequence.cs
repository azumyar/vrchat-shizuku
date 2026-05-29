using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using net.yarukizero.vrchat.shizuku;

using __ConditionParam = net.yarukizero.vrchat.shizuku.Linq.Conditions.ShizukuParam;
using __ConditionRecord = net.yarukizero.vrchat.shizuku.Linq.Conditions.ConditionRecord;
using __ConditionsNext = net.yarukizero.vrchat.shizuku.Linq.Conditions.NextStage;
using __ActionParam = net.yarukizero.vrchat.shizuku.Linq.Actions.ShizukuParam;
using __ActionRecord = net.yarukizero.vrchat.shizuku.Linq.Actions.ActionRecord;

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

namespace net.yarukizero.vrchat.shizuku.Linq {
    internal interface ICombineStore : IShizukuStore<__ConditionParam>, IShizukuStore<__ActionParam> {}

    public interface ISequence {
		void SetName(string name);
        void SetLocalOnly(bool localOnly);
    }

    public interface IConditonSequence : ISequence {
        void AddCondition(__ConditionExp condition);
        void AddOr(__ConditionExp condition);

        IActionSequence ToAction();
    }

    public interface IActionSequence : ISequence {
        void AddAction(__ActionExp action);
        IConditonSequence ToNext(string targetStage);
        DependencyResult ToResult(__ConditionsNext transactStage);
    }

    public enum ShizukuOprator {
        Equals,
        NotEquals,
        LessThan,
        GreaterThan,
    }

    public class DependencySequence : IConditonSequence, IActionSequence {
        private class CombineStore : ICombineStore {
            private DependencySequence sequence;
            public CombineStore(DependencySequence sequence) {
                this.sequence = sequence;
            }

			__ConditionParam IShizukuStore<__ConditionParam>.this[string name] {
                get {
                    return getParam(name, this.sequence.Host);
                }
            }

			__ActionParam IShizukuStore<__ActionParam>.this[string name] {
                get {
                    return getApply(name, this.sequence.Host);
                }
            } 


            private __ConditionParam getParam(string name, IShizukuStore<__ConditionParam> global) {
                if(this.sequence.localVal.TryGetValue(name, out var v)) {
                    return new(v.Name, v.Type);
                }
                return global[name];
            }

            private __ActionParam getApply(string name, IShizukuStore<__ActionParam> global) {
                if(this.sequence.localVal.TryGetValue(name, out var v)) {
                    return new(v.Name, v.Type);
                }
                return global[name];
            }
            /*
            private T get<T>(string name, Expression<T> @new) {
                if(this.sequence.localVal.TryGetValue(name out var v)) {
                    var c = typeof(T).GetConstructor(new Type[] {typeof(string), typeof(VrcType)});
                    return Expression.Lambda<T>(Expression.New(c), Expression.l(v.Name), v.Type);
                }
                return this.sequence.Host[name];
            }
            */

        }


		public string Name { get; }
		public string TargetStage { get; }
		private SequenceStage currentStage;
        private readonly List<SequenceStage> stages = new();
        private Dictionary<string, (string Name, VrcType Type, float? Default)> localVal = new();

        private SequenceStage CurrentStage {
            get {
				//this.currentStage ??= new SequenceStage(this.Store);
				if(this.currentStage == null) {
					this.currentStage = new SequenceStage(this.Store);
				}
                return this.currentStage;
            }
        }

        internal ICombineStore Store { get; }
        internal IDependencyHost Host { get; }
        internal IEnumerable<SequenceStage> Stages {
             get {
                return this.stages.AsReadOnly();
            }
        }

        internal DependencySequence(IDependencyHost host, string sequenceName, string targetStage) {
            this.Store = new CombineStore(this);
            this.Host = host;
			this.Name = sequenceName;
			this.TargetStage = targetStage;
		}

        internal DependencySequence SetLocalValiable(string name, VrcType type, float? defaultVal) {
            this.localVal.Add(name, (name, type, defaultVal));
            return this;
        }

		public void SetName(string name) {
			this.CurrentStage.Name = name;
		}

        public void SetLocalOnly(bool localOnly) {
            this.CurrentStage.IsLocalOnly = localOnly;
        }

        public void AddCondition(__ConditionExp condition) {
            this.CurrentStage.Conditions.Add(condition);
        }

        public void AddOr(__ConditionExp condition) {
            this.CurrentStage.Or.Add(condition);
        }
    
        public IActionSequence ToAction() => this;

        public void AddAction(__ActionExp action) {
            this.CurrentStage.Actions.Add(action);
        }

        public IConditonSequence ToNext(string targetStage) {
            this.Staging();
			this.CurrentStage.Name = targetStage;
            return this;
        }

        public DependencyResult ToResult(__ConditionsNext transactStage) {
            this.Staging();
            return new(
				this,
				endStage: transactStage);
        }

        private void Staging() {
            if(this.currentStage != null) {
                this.stages.Add(this.currentStage);
                this.currentStage = null;
            }
        }
    }

    internal class SequenceStage {
        private readonly ICombineStore store;
        internal readonly List<__ConditionExp> Conditions = new();
        internal readonly List<__ConditionExp> Or = new();
        internal readonly List<__ActionExp> Actions = new();
        public bool? IsLocalOnly { get; set; }
		public string Name { get; set; }

        public SequenceStage(
            ICombineStore store) {

            this.store = store;
        }

        internal IEnumerable<__ConditionRecord> CompileConditions() {
            return this.Conditions
                .SelectMany(x => x.Compile()(this.store).Records)
                .ToList()
                .AsReadOnly();
        }
        internal IEnumerable<IEnumerable<__ConditionRecord>> CompileOr() {
            var result = new List<IEnumerable<__ConditionRecord>>();
            result.Add(this.CompileConditions());
            result.AddRange(this.Or
                .SelectMany(x => x.Compile()(this.store).Records)
                .Select(x => new __ConditionRecord[] { x }));
            return result.AsReadOnly();
        }
        internal IEnumerable<__ActionRecord> CompileActions() {
            return this.Actions
                .Select(x => x.Compile()(this.store))
                .ToList()
                .AsReadOnly();
        }
    }
}