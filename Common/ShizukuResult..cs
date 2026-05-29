using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using net.yarukizero.vrchat.shizuku.Linq;

using __ConditionsRecord = net.yarukizero.vrchat.shizuku.Linq.Conditions.ConditionRecord;
using __ConditionsNext = net.yarukizero.vrchat.shizuku.Linq.Conditions.NextStage;
using __ActionRecord = net.yarukizero.vrchat.shizuku.Linq.Actions.ActionRecord;

namespace net.yarukizero.vrchat.shizuku {
    public interface IResultStage {
		string Name { get; }
		bool IsLocalOnly { get; }
		IEnumerable<IEnumerable<__ConditionsRecord>> Or { get; }
        IEnumerable<__ActionRecord> Actions { get; }
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



	public class DependencyResult {
        class Stage : IResultStage {
			public string Name { get; }
            public IEnumerable<IEnumerable<__ConditionsRecord>> Or { get; }
            public IEnumerable<__ActionRecord> Actions { get; }
			/// <summary>シーケンスをローカルで実行するか否か</summary>
            public bool IsLocalOnly { get; }
            public Stage(SequenceStage stage, IDependencyHost host) {
                this.Or = stage.CompileOr();
                this.Actions = stage.CompileActions();
				this.Name = stage.Name;
                this.IsLocalOnly = stage.IsLocalOnly ?? host.IsLocalOnly;
            }
        }
        /// <summary>使用する変数一覧</summary>
        public IEnumerable<IVrcParameter> Parameters { get; }
        public IEnumerable<IResultStage> Stages { get; }
		public string SequenceName { get; }
		public string StartStage { get; }
		public bool IsTransactEndStage { get; }
		public __ConditionsNext EndStage { get; }


		internal DependencyResult(
			DependencySequence sequence,
			bool? transactFirst=null,
			__ConditionsNext endStage =null) {

			this.SequenceName = sequence.Name;
			this.StartStage = sequence.TargetStage;
			this.IsTransactEndStage = transactFirst.HasValue ? transactFirst.Value : false;
			this.EndStage = endStage ?? __ConditionsNext.Idle();
            this.Stages = sequence.Stages.Select(x => new Stage(x, sequence.Host))
                .ToList()
                .AsReadOnly();

            var list = new List<(IVrcParameter Param, int _stab)>();
            var @in = new List<IVrcParameter>();
            //TODO: ローカル変数
            //list.AddRange(sequence.);
            foreach(var it in this.Stages) {
                list.AddRange(it.Or.SelectMany(x => x).Select(x => (x.Param, 0)).ToArray());            
                list.AddRange(it.Actions.Select(x => (x.Param, 0)).ToArray());
            }
            foreach(var it in list) {
                if(@in.Any(x => x.Name == it.Param.Name)) {
                    continue;
                }
                @in.Add(it.Param);
            }
            this.Parameters = @in.AsReadOnly();
        }
    }
}