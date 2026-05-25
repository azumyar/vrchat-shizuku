using System;
using System.Collections.Generic;
using System.Text;

namespace net.yarukizero.vrchat.shizuku {
	public class Util {

		/// <summary>LocalOnlyの設定をHostの基盤設定と各objの設定から選択</summary>
		/// <param name="hostLocal"></param>
		/// <param name="objLocal"></param>
		/// <returns></returns>
		public static bool IsLocal(bool hostLocal, bool? objLocal) {
			if(!objLocal.HasValue) {
				return hostLocal;
			}
			return objLocal.Value;
		}
	}
}
