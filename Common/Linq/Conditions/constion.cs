using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace net.yarukizero.vrchat.shizuku.Linq.Conditions {
    public class ConditionRecord {
        public IVrcParameter Param { get; }
        public ShizukuOprator Op { get; }
        public float Value { get; }

        public ConditionRecord(IVrcParameter param, ShizukuOprator op, float value) {
            this.Param = param;
            this.Op = op;
            this.Value = value;
        }
    }
    
    public class ShizukuParam : IVrcParameter {
        public string Name { get; }
        public VrcType Type { get; }

        public ShizukuParam(string name, VrcType type) {
            this.Name = name;
            this.Type = type;
        }

        public static ShizukuCondition operator ==(ShizukuParam a, bool b) { return OpBool(a, b, ShizukuOprator.Equals); }
        public static ShizukuCondition operator !=(ShizukuParam a, bool b) { return OpBool(a, b, ShizukuOprator.NotEquals); }

        public static ShizukuCondition operator ==(ShizukuParam a, int b) { return OpInt(a, b, ShizukuOprator.Equals); }
        public static ShizukuCondition operator !=(ShizukuParam a, int b) { return OpInt(a, b, ShizukuOprator.NotEquals); }
        public static ShizukuCondition operator <(ShizukuParam a, int b) { return OpInt(a, b, ShizukuOprator.LessThan); }
        public static ShizukuCondition operator >(ShizukuParam a, int b) { return OpInt(a, b, ShizukuOprator.GreaterThan); }

        public static ShizukuCondition operator <(ShizukuParam a, double b) { return OpFloat(a, b, ShizukuOprator.LessThan); }
        public static ShizukuCondition operator >(ShizukuParam a, double b) { return OpFloat(a, b, ShizukuOprator.GreaterThan); }

        private static ShizukuCondition OpBool(ShizukuParam a, bool b, ShizukuOprator op) {
            ThrowIf(a, VrcType.Bool);

            return new(a, op, b);
        }

        private static ShizukuCondition OpInt(ShizukuParam a, int b, ShizukuOprator op) {
            ThrowIf(a, VrcType.Int); 

            return new(a, op, b);
        }

        private static ShizukuCondition OpFloat(ShizukuParam a, float b, ShizukuOprator op) {
            ThrowIf(a, VrcType.Float);

            return new(a, op, b);
        }

        private static ShizukuCondition OpFloat(ShizukuParam a, double b, ShizukuOprator op) {            
            return OpFloat(a, (float)b, op);
        }

        private static void ThrowIf(ShizukuParam a, VrcType type) {
            if (a.Type != type) {
                throw new ExpressionException($"右辺{a.Name}:{a.Type }と左辺{type}の型が異なります");
            }
        }
    }

    public class ShizukuCondition {

        private readonly List<ConditionRecord> records = new();

        public IEnumerable<ConditionRecord> Records {
             get {
                return records.AsReadOnly();
            }
        }

        public ShizukuCondition(ShizukuParam param, ShizukuOprator op, bool value) : this(param, op, (value ? 1f : 0)) { }

        public ShizukuCondition(ShizukuParam param, ShizukuOprator op, int value) : this(param, op, (float)value) { }

        public ShizukuCondition(ShizukuParam param, ShizukuOprator op, float value) {
            this.records.Add(new(param, op, value));
        }

        public ShizukuCondition(ShizukuCondition a, ShizukuCondition b) {
            records.AddRange(a.Records);
            records.AddRange(b.Records);
        }

		public static bool operator true(ShizukuCondition _) => true;
		public static bool operator false(ShizukuCondition _) => false;

		public static ShizukuCondition operator &(ShizukuCondition a, ShizukuCondition b) { return new(a, b); }
		public static ShizukuCondition operator |(ShizukuCondition a, ShizukuCondition b) { throw new NotSupportedException(); }
	}
}