using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace net.yarukizero.vrchat.shizuku {
    /* TODO:未実装
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class ShizukuParameterAttribute : Attribute {
        public string Name {get;}
        public VrcType Type {get;}
        public float DefaultValue { get; }

        public bool IsLoacl { get; set;}
        public bool IsSync { get; set; }

        public ShizukuParameterAttribute(string name, VrcType type, float? defaultValue=null) {
            this.Name = name;
            this.Type = type;
            this.DefaultValue = defaultValue ?? 0;
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class ShizukuIntParameterAttribute : ShizukuParameterAttribute {
        public ShizukuIntParameterAttribute(string name) : base(name, VrcType.Int) {}
        public ShizukuIntParameterAttribute(string name, int defaultValue) : base(name, VrcType.Int) {}
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class ShizukuBoolParameterAttribute : ShizukuParameterAttribute {
        public ShizukuBoolParameterAttribute(string name) : base(name, VrcType.Bool) {}
        public ShizukuBoolParameterAttribute(string name, bool defaultValue) : base(name, VrcType.Bool) {}
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class ShizukuFloatParameterAttribute : ShizukuParameterAttribute {
        public ShizukuFloatParameterAttribute(string name) : base(name, VrcType.Float) {}
        public ShizukuFloatParameterAttribute(string name, float defaultValue) : base(name, VrcType.Float) {}
        public ShizukuFloatParameterAttribute(string name, double defaultValue) : base(name, VrcType.Float) {}
    }
    */

    public class ShizukuClientAttribute : Attribute {}
}