using System;
using System.Collections.Generic;
using System.Linq.Expressions;

#if false
// TODO: まだ使ってない
namespace net.yarukizero.vrchat.shizuku {
    public struct ShizukuParameter {
        public string Name { get; }
        public VrcType VrcType { get; }
        public float DefaultValue { get; }

        internal ShizukuParameter(string name, VrcType type, float defaultValue) {
            this.Name = name;
            this.VrcType = type;
            this.DefaultValue = defaultValue;
        }
    }
}
#endif