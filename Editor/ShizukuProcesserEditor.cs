using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using VRC.SDKBase;
using VRC.SDK3.Avatars.ScriptableObjects;

using net.yarukizero.vrchat.shizuku.runtime;

namespace net.yarukizero.vrchat.shizuku.editor {
    [CustomEditor(typeof(ShizukuProcesser))]
    public class ShizukuProcesserEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
        }
    }
}