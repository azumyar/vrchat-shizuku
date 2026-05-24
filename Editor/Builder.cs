using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using VRC.SDKBase;
using UnityEngine.Animations;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using net.yarukizero.vrchat.shizuku;
using net.yarukizero.vrchat.shizuku.runtime;

using @ref = System.Reflection;

namespace net.yarukizero.vrchat.shizuku.editor {
    internal class Builder {
        private @ref.Assembly assembly { get; }
        
        public Builder() {
			try {
				this.assembly = @ref.Assembly.Load("Assembly-CSharp");
			}
			catch(System.IO.FileNotFoundException e) {
				throw new NotSupportedException("Assembly-CSharpが見つかりません", e);
			}
        }

        public void BuildShizuku(BuildContext ctx) {
            var pulgin = ctx.AvatarRootObject
                .GetComponentsInChildren<ShizukuProcesser>();
            if(pulgin.Count() == 0) {
                return;
            }

            // 実行時に定義されている変数一覧を取得
            var @params = ParameterInfo.ForContext(ctx)
                .GetParametersForObject(ctx.AvatarRootObject)
                .SelectMany(x => x.SubParameters())
                .Select<ProvidedParameter, (string Name, VrcType Type)?>(x => {
                    switch(x.ParameterType) {
                    case AnimatorControllerParameterType.Bool:
                        return (x.EffectiveName, VrcType.Bool);
                    case AnimatorControllerParameterType.Int:
                        return (x.EffectiveName, VrcType.Int);
                    case AnimatorControllerParameterType.Float:
                        return (x.EffectiveName, VrcType.Float);
                    default:
                        return null;
                    }
                }).Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToArray();

            var clip = CreateEmptyAnimationClip();
            var animator = new AnimatorController();
            foreach(var it in @params) {
                animator.AddParameter(new AnimatorControllerParameter() {
                    name = it.Name,
                    type = it.Type.ToAnimationType(),
                });
            }

            // IShizukuのインスタンス化
            var proc = new List<(IShizuku Shizuku, ShizukuHost Host)>();
            foreach(var it in assembly.GetTypes()
                .Where(x => Attribute
                    .GetCustomAttributes(x)
                    .Any(y => y is ShizukuClientAttribute))
                .ToArray()) {
                
                try {
                    var shizuku = System.Activator.CreateInstance(it) as IShizuku;
                    if(shizuku != null) {
                        proc.Add((shizuku, new ShizukuHost(shizuku, @params)));
                    }
                }
                catch(Exception e) {
                    Debug.LogError(e);
                }
            }

            // 処理
            foreach(var it in proc) {
                Apply(it.Shizuku.Define(it.Host), animator, clip);
            }

            // 結合
            var mergeAnimator = ctx.AvatarRootObject.AddComponent<ModularAvatarMergeAnimator>();
            mergeAnimator.animator = animator;
            mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            mergeAnimator.matchAvatarWriteDefaults = true;

            foreach(var it in pulgin) {
                UnityEngine.Object.DestroyImmediate(it);
            }
        }


        private static void Apply(IEnumerable<ShizukuResult> entries, AnimatorController animator, AnimationClip clip) {
            foreach(var it in entries.Select((x, i) => (Index: i, Value: x))) {
                animator.AddLayer($"{ShizukuEnviromnet.ParseUnitySafe()}-{it.Index}");
                var layer = animator.layers.Last();
                var idle = layer.stateMachine.NewState("idle", clip);
                var last = idle;
                layer.defaultWeight = 1f;
                layer.stateMachine.defaultState = idle;
                foreach(var stage in it.Value.Stages.Select((x, i) => (Index: i, Value: x))) {
                    var active = layer.stateMachine.NewState($"stage{stage.Index}", clip);
                    active.behaviours = new StateMachineBehaviour[] {
                        stage.Value.CreateDriver(),
                    };

                    foreach(var or in stage.Value.Or) {
                        var transition = last.AddTransition(active).ToNext();
                        foreach(var con in or) {
                            transition.AddCondition(
                                con.ToMode(),
                                con.Value,
                                con.Param.Name);

                        }
                    }
                    last = active;
                }
                if(!object.ReferenceEquals(idle, last)) {
                    last.AddTransition(idle).ToFree();
                }
            }
            
        }

        private static AnimationClip CreateEmptyAnimationClip() {
            var result = new AnimationClip();
            result.SetCurve(
                $"__{ShizukuEnviromnet.ParseLogName()}_Empty__", 
                typeof(GameObject),
                "localPosition.x",
                new AnimationCurve {
                    keys = new Keyframe[] {
                        new Keyframe {
                            time = 0f,
                            value = 0f
                        },
                        new Keyframe {
                            time = 1f / 60f,
                            value = 0f
                        }
                    }
                });
            return result;
        }
    }
}