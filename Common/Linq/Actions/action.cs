using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace net.yarukizero.vrchat.shizuku.Linq.Actions {
    public enum VrcAction {
		Nop,
        Set,
        Add,
        Random,
		Copy,
    }
    public class ActionRecord {
		private class NopParameter : IVrcParameter {
			public string Name => "__nop__";
			public VrcType Type => VrcType.Float;
		}


		public IVrcParameter Param { get; }
        public VrcAction Action { get; }
        public float Value { get; }

        public ActionRecord(IVrcParameter param, VrcAction action, float value) {
            this.Param = param;
            this.Action = action;
            this.Value = value;
        }

		public static ActionRecord Nop() => new ActionRecord(new NopParameter(), VrcAction.Nop, 0f);
	}

    public class ShizukuParam : IVrcParameter {
        public string Name { get; }
        public VrcType Type { get; }

        public ShizukuParam(string name, VrcType type) {
            this.Name = name;
            this.Type = type;
        }

        public ActionRecord Set(bool v) {
            return this.Set((v ? 1f : 0f));
        }
        public ActionRecord Set(int v) {
            return this.Set((float)v);
        }
        public ActionRecord Set(float v) {
            return new ActionRecord(this, VrcAction.Set, v);
        }
        public ActionRecord Set(double v) {
            return this.Set((float)v);
        }

        // Add, Random, Copy
    }
}