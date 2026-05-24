using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace net.yarukizero.vrchat.shizuku {
    public class ExpressionException : Exception {
        public ExpressionException() {}
        public ExpressionException(string msg) : base(msg) {}
    }
}



