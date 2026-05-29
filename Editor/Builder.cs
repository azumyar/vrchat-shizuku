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
using net.yarukizero.vrchat.shizuku.Linq;
using UnityEditor.SceneManagement;
using __NextIs = net.yarukizero.vrchat.shizuku.Linq.Conditions.NextStage.NextIs;

namespace net.yarukizero.vrchat.shizuku.editor {
    internal class Builder {
        private class Env : IHostEnviroment {
            // フィールド公開はよくないけど内部クラスだしいいんじゃない？って感じもあり
            internal readonly List<(string Name, VrcType Type)> @params = new();

            public IEnumerable<(string Name, VrcType Type)> GetParameter() {
                return this.@params.AsReadOnly();
            }
        }

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

            var env = new Env();
            // 実行時に定義されている変数一覧を取得
            env.@params.AddRange(ParameterInfo.ForContext(ctx)
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
                .Select(x => x.Value));

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
                        proc.Add((shizuku, new ShizukuHost(shizuku, env)));
                    }
                }
                catch(Exception e) {
                    Debug.LogError(e);
                }
            }

            // 変数定義
            var parameters = ctx.AvatarRootObject.AddComponent<ModularAvatarParameters>();
            foreach(var it in proc.SelectMany(
                x => x.Shizuku.Parameters(x.Host) ??  Array.Empty<DefinedResult>()
                ).SelectMany(x => x.GetDefinedParamators())) {
                
                env.@params.Add((it.Name, it.Type));
                parameters.parameters.Add(new() {
                    nameOrPrefix = it.Name,
                    syncType = it.Type.ToMaType(),
                    localOnly = it.LocalOnly,
                    saved = it.Save,
                    defaultValue = it.DefaultValue,
                });
            }


            var clip = CreateEmptyAnimationClip();
            var animator = new AnimatorController();
            foreach(var it in env.@params) {
                animator.AddParameter(new AnimatorControllerParameter() {
                    name = it.Name,
                    type = it.Type.ToAnimationType(),
                });
            }

            // 処理
            foreach(var it in proc) {
                Apply(it.Shizuku.Dependencies(it.Host), animator, clip);
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


        private class NamedLayer {
            public string Name { get; }
            public AnimatorControllerLayer Layer { get; }
            public AnimatorState Idle { get; }
            public Dictionary<string, NamedState> Stages { get; } = new();

            public NamedLayer (string name, AnimatorControllerLayer layer, AnimatorState idle) {
                this.Name = name;
                this.Layer = layer;
                this.Idle = idle;
            }
        }


        private class NamedState {
            public string Name { get; }
            public AnimatorControllerLayer Layer { get; }
            public AnimatorState Idle { get; }
            public AnimatorState Active { get; }

            public NamedState (string name, AnimatorControllerLayer layer, AnimatorState idle, AnimatorState active) {
                this.Name = name;
                this.Layer = layer;
                this.Idle = idle;
                this.Active = active;
            }
        }
        private static void Apply(IEnumerable<DependencyResult> entries, AnimatorController animator, AnimationClip clip) {
            var namedLayers = new Dictionary<string, NamedLayer>();
            //var dic = new Dictionary<string, NamedState>();

            foreach(var it in entries.Select((x, i) => (Index: i, Value: x))) {
                var currentLayer = default(NamedLayer);
                AnimatorControllerLayer layer;
                AnimatorState idle;
                AnimatorState start;
                AnimatorState last;
                if(!string.IsNullOrEmpty(it.Value.SequenceName)) {
                    if(namedLayers.TryGetValue(it.Value.SequenceName, out var val)) {
                        currentLayer = val;
                        layer = val.Layer;
                        idle = val.Idle;
                        last = val.Idle;
                        if(string.IsNullOrEmpty(it.Value.StartStage)) {
                            start = idle;
                        } else {
                            if(!val.Stages.TryGetValue(it.Value.StartStage, out var v2)) {
                                throw new InvalidOperationException();
                            }
                            start = v2.Active;
                        }
                        goto start;
                    }
                }

                {
                    var hasName = !string.IsNullOrEmpty(it.Value.SequenceName);
                    var layerSuffix = hasName ? it.Value.SequenceName : $"{it.Index}";
                    var layerName = $"{ShizukuEnviromnet.ParseUnitySafe()}-{layerSuffix}";
                    animator.AddLayer($"{layerName}");
                    layer = animator.layers.Last();
                    idle = layer.stateMachine.NewState("idle", clip);
                    start = idle;
                    last = idle;
                    layer.defaultWeight = 1f;
                    layer.stateMachine.defaultState = idle;
                    if(hasName) {
                        currentLayer = new(layerName, layer, idle);
                        namedLayers.Add(layerSuffix, currentLayer);
                    }
                }
            start:
                var transactionTarget = default(AnimatorState);
                if(!string.IsNullOrEmpty(it.Value.StartStage)) {
                    if(currentLayer != null) {
                        if(!currentLayer.Stages.TryGetValue(it.Value.StartStage, out var target)) {
                            throw new InvalidOperationException($"Stage[{it.Value.StartStage}]は定義されていません");
                        }
                        last = target.Active;
                    }
                }

                switch(it.Value.EndStage.Next) {
                case __NextIs.Idle:
                    transactionTarget = idle;
                    break;
                case __NextIs.Name:
                    if(currentLayer == null) {
                        throw new InvalidOperationException($"Stage[{it.Value.EndStage.Name}]が指定されましたがシーケンス名が未定義です");                        
                    }
                    if(!currentLayer.Stages.TryGetValue(it.Value.StartStage, out var target)) {
                        throw new InvalidOperationException($"Stage[{it.Value.EndStage.Name}]は定義されていません");
                    }
                    transactionTarget = target.Active;
                    break;
                }

                foreach(var stage in it.Value.Stages.Select((x, i) => (Index: i, Value: x))) {
                    AnimatorState active;
                    var hasName = !string.IsNullOrEmpty(stage.Value.Name);
                    var stageName = hasName ? stage.Value.Name : $"stage-{it.Index}-{stage.Index}";
                    if((currentLayer != null)
                        && hasName
                        && currentLayer.Stages.TryGetValue(stageName, out var sotredStage)) {
                        
                        active = sotredStage.Active;
                        // TODO: VRC Avater Driver考える
                    } else {
                        active = layer.stateMachine.NewState(stageName, clip);
                        if((currentLayer != null) && hasName) {
                            currentLayer.Stages.Add(
                                stageName,
                                new(stageName, layer, idle, active));
                        }
                        active.behaviours = new StateMachineBehaviour[] {
                            stage.Value.CreateDriver(),
                        };
                    }

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
                if(!object.ReferenceEquals(transactionTarget, null) && !object.ReferenceEquals(transactionTarget, last)) {
                    last.AddTransition(transactionTarget).ToFree();
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