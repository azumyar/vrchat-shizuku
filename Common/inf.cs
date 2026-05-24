using System;
using System.Collections.Generic;

namespace net.yarukizero.vrchat.shizuku {
    /// <summary>NDMFビルドする際に目印になるinterface</summary>
    public interface IShizuku {
		/// <summary>シーケンス全体の標準スコープを決定します。シーケンスで指定しない場合このスコープが採用されます</summary>
        public bool IsLocalOnly { get; }
		/// <summary>シーケンス定義いい名前が思つかないのでざつざつ</summary>
		/// <param name="host"></param>
		/// <returns></returns>
        public IEnumerable<ShizukuResult> Define(ShizukuHost host);
    }


	/// <summary>interfaceを実装しているヘルパークラス</summary>
    public class ShizukuTemplate  : IShizuku {
        public virtual bool IsLocalOnly {
            get {
                return false;
            }
        }
        
        
        public IEnumerable<ShizukuResult> Define(ShizukuHost host) {
            // null は許可しない
            return this.DoDefine(host) ?? Array.Empty<ShizukuResult>();
        }

        protected virtual IEnumerable<ShizukuResult> DoDefine(ShizukuHost host) {
            return Array.Empty<ShizukuResult>();
        }
    }


    /// <summary>VRCパラメータ保管庫</summary>
    public interface IShizukuStore<T> where T:IVrcParameter {
        public T this[string name] { get; }        
    }

    /// <summary>VRCパラメータ</summary>
    public interface IVrcParameter {
        string Name { get; }
        VrcType Type { get; }
    }

    /// <summary>VRCパラメータ型</summary>
    public enum VrcType {
        Bool,
        Int,
        Float,
    }
}