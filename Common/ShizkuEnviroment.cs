

namespace net.yarukizero.vrchat.shizuku {
    public static partial class ShizukuEnviromnet {
        public static string QualifiedName { get; } = "net.yarukizero.ndmf.shizuku";       
        public static string DisplayName { get; } = "Shizuku";


        public static string ParseLogName() => DisplayName;
        public static string ParseUnitySafe() => QualifiedName.Replace(".", "_");

    }
}