
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using net.yarukizero.vrchat.shizuku.Linq;

using __ConditionsParam = net.yarukizero.vrchat.shizuku.Linq.Conditions.ShizukuParam;
using __ActionParam= net.yarukizero.vrchat.shizuku.Linq.Actions.ShizukuParam;

namespace net.yarukizero.vrchat.shizuku {
    public class ShizukuHost : IShizukuStore<__ConditionsParam>, IShizukuStore<__ActionParam> {
        // パラメータリスト返却用interface実装してるだけのクラス
        class Param: IVrcParameter {
            public string Name { get; }
            public VrcType Type { get; }

            public Param((string Name, VrcType Type) arg) {
                this.Name = arg.Name;
                this.Type = arg.Type;
            }
        }

        public bool IsLocalOnly { get; }
        private Dictionary<string, (string Name, VrcType Type)> dic = new();

        __ConditionsParam IShizukuStore<__ConditionsParam>.this[string name] {
            get {
                if(!this.dic.TryGetValue(name, out var r)) {
                    throw new InvalidOperationException($"{name}はVRCで定義されていません");
                }
                return new(r.Name, r.Type);
            }
        } 

        __ActionParam IShizukuStore<__ActionParam>.this[string name] {
            get {
                if(!this.dic.TryGetValue(name, out var r)) {
                    throw new InvalidOperationException($"{name}はVRCで定義されていません");
                }
                return new(r.Name, r.Type);
            }
        } 

        public ShizukuHost(IShizuku a, IEnumerable<(string Name, VrcType Type)> @params) {
            this.IsLocalOnly = a.IsLocalOnly;
            foreach(var it in @params) {
                this.dic.Add(it.Name, it);
            }
        }

        /// <summary>
        /// HOSTが管理しているVRCパラメータの一覧を取得
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IVrcParameter> GetParameter() {
            foreach(var it in this.dic.Keys) {
                if(this.dic.TryGetValue(it, out var v)) {
                    yield return new Param(v);
                }
            }
        }
    }
}