using System;
using System.Collections.Generic;
using System.Text;

using __ConditionsParam = net.yarukizero.vrchat.shizuku.Linq.Conditions.ShizukuParam;
using __ActionParam = net.yarukizero.vrchat.shizuku.Linq.Actions.ShizukuParam;
using System.Linq;

namespace net.yarukizero.vrchat.shizuku {
	public interface ITransitionHost : IShizukuHost, IShizukuStore<__ConditionsParam>, IShizukuStore<__ActionParam> {
		IEnumerable<IVrcParameter> GetParameter();
	}

	public partial class ShizukuHost : ITransitionHost {
		// パラメータリスト返却用interface実装してるだけのクラス
		class Param : IVrcParameter {
			public string Name { get; }
			public VrcType Type { get; }

			public Param((string Name, VrcType Type) arg) {
				this.Name = arg.Name;
				this.Type = arg.Type;
			}
		}

		private Dictionary<string, (string Name, VrcType Type)> dic = new();

		__ConditionsParam IShizukuStore<__ConditionsParam>.this[string name] {
			get {
				var v = ((ITransitionHost)this).GetParameter()
					.Where(x => x.Name == name)
					.FirstOrDefault();
				if(v == null) {
					throw new InvalidOperationException($"{name}はVRCで定義されていません");
				}

				return new(v.Name, v.Type);
			}
		}

		__ActionParam IShizukuStore<__ActionParam>.this[string name] {
			get {
				var v = ((ITransitionHost)this).GetParameter()
					.Where(x => x.Name == name)
					.FirstOrDefault();
				if(v == null) {
					throw new InvalidOperationException($"{name}はVRCで定義されていません");
				}

				return new(v.Name, v.Type);
			}
		}

		partial void InitSequence(IShizuku template, IHostEnviroment env) {}

		/// <summary>
		/// HOSTが管理しているVRCパラメータの一覧を取得
		/// </summary>
		/// <returns></returns>
		IEnumerable<IVrcParameter> ITransitionHost.GetParameter() {
			foreach(var it in this.Enviroment.GetParameter()) {
				yield return new Param(it);
			}

			// TODO: シーケンスローカル変数の処理
		}
	}
}
