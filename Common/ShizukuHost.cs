
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

using net.yarukizero.vrchat.shizuku.Linq;

namespace net.yarukizero.vrchat.shizuku {
	public interface IHostEnviroment {
		IEnumerable<(string Name, VrcType Type)> GetParameter();
	}

	public interface IShizukuHost {
		bool IsLocalOnly { get; }
	}

	public partial class ShizukuHost {
		private readonly bool isLocalOnly;

		private IHostEnviroment Enviroment { get; }

		bool IShizukuHost.IsLocalOnly {
			get {
				return false;
			}
		}
		public ShizukuHost(IShizuku template, IHostEnviroment env) {
			this.Enviroment = env;
			this.isLocalOnly = template.IsLocalOnly;

			InitParamator(template, env);
			InitDependency(template, env);
		}

		partial void InitParamator(IShizuku template, IHostEnviroment env);
		partial void InitDependency(IShizuku template, IHostEnviroment env);
	}
}