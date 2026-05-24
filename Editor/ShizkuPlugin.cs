using System;
using System.Linq;
using System.Collections.Generic;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;

namespace net.yarukizero.vrchat.shizuku.editor {
    public class ShizkuPlugin : Plugin<ShizkuPlugin> {
		public override string QualifiedName => ShizukuEnviromnet.QualifiedName;
        public override string DisplayName => ShizukuEnviromnet.DisplayName;

        protected override void Configure() {
            var builder = default(Builder);
            try {
                builder = new Builder();
            }
            catch(NotSupportedException _) {}

            if(builder == null) {
                return;
            }

            InPhase(BuildPhase.Generating)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run(
                    ShizukuEnviromnet.ParseLogName(),
                    (ctx) => {
                        builder.BuildShizuku(ctx);
                    });
        }
    }
}

