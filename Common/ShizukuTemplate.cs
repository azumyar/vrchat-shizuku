using net.yarukizero.vrchat.shizuku.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using __ActionParam = net.yarukizero.vrchat.shizuku.Linq.Actions.ShizukuParam;
using __ConditionsParam = net.yarukizero.vrchat.shizuku.Linq.Conditions.ShizukuParam;

namespace net.yarukizero.vrchat.shizuku {
	/// <summary>interfaceを実装しているヘルパークラス</summary>
	public class ShizukuTemplate : IShizuku {
		public virtual bool IsLocalOnly {
			get {
				return false;
			}
		}

		public virtual IEnumerable<DefinedResult> Parameters(IParameterizableHost host) {
			return Array.Empty<DefinedResult>();
		}

		public virtual IEnumerable<DependencyResult> Dependencies(IDependencyHost host) {
			return Array.Empty<DependencyResult>();
		}

	}
}
