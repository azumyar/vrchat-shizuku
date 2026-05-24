using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using net.yarukizero.vrchat.shizuku.Linq;

using __ConditionsRecord = net.yarukizero.vrchat.shizuku.Linq.Conditions.ConditionRecord;
using __ActionRecord = net.yarukizero.vrchat.shizuku.Linq.Actions.ActionRecord;

namespace net.yarukizero.vrchat.shizuku {
    public interface IResultStage {
        IEnumerable<IEnumerable<__ConditionsRecord>> Or { get; }
        IEnumerable<__ActionRecord> Actions { get; }
        bool IsLocalOnly { get; }
    }

    public class ShizukuResult {
        class Stage : IResultStage {
            public IEnumerable<IEnumerable<__ConditionsRecord>> Or { get; }
            public IEnumerable<__ActionRecord> Actions { get; }
        /// <summary>シーケンスをローカルで実行するか否か</summary>
            public bool IsLocalOnly { get; }
            public Stage(SequenceStage stage, ShizukuHost host) {
                this.Or = stage.CompileOr();
                this.Actions = stage.CompileActions();
                this.IsLocalOnly = stage.IsLocalOnly ?? host.IsLocalOnly;
            }
        }
        /// <summary>使用する変数一覧</summary>
        public IEnumerable<IVrcParameter> Parameters { get; }
        public IEnumerable<IResultStage> Stages { get; }

        internal ShizukuResult(ShizukuSequence sequence) {
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