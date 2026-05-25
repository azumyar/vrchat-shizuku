using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEngine.Animations;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;

using net.yarukizero.vrchat.shizuku ;
using net.yarukizero.vrchat.shizuku.runtime;
using net.yarukizero.vrchat.shizuku.Linq;

using __Action = net.yarukizero.vrchat.shizuku.Linq.Actions.VrcAction;
using __ActionRecord = net.yarukizero.vrchat.shizuku.Linq.Actions.ActionRecord;
using __ConditionRecord = net.yarukizero.vrchat.shizuku.Linq.Conditions.ConditionRecord;
using System.IO;

namespace net.yarukizero.vrchat.shizuku.editor {
    internal static class InternalExtension {
        public static ParameterSyncType ToMaType(this VrcType @this) {
            switch(@this) {
            case VrcType.Bool:
                return ParameterSyncType.Bool;
            case VrcType.Int:
                return ParameterSyncType.Int;
            case VrcType.Float:
                return ParameterSyncType.Float;
            default:
                throw new InvalidDataException();
            }
        }

        public static VRCAvatarParameterDriver CreateDriver(this IResultStage @this) {
            var d = ScriptableObject.CreateInstance<VRCAvatarParameterDriver>();
            d.parameters = @this.Actions.Select(x => x.ToDriverParameter()).ToList();
            d.localOnly = @this.IsLocalOnly;
            return d;
        }

        public static VRC_AvatarParameterDriver.Parameter ToDriverParameter(this __ActionRecord @this) {
            switch(@this.Action) {
            case __Action.Set:
                return new VRC_AvatarParameterDriver.Parameter() {
                    type = VRC_AvatarParameterDriver.ChangeType.Set,
                    name =  @this.Param.Name,
                    value = @this.Value
                };
            // TODO: 未実装
            case __Action.Add:
            case __Action.Random:
            case __Action.Copy:
            default:
                throw new InvalidOperationException($"不明なアクション{@this.Action}");
            }
        }
        public static AnimatorControllerParameterType ToAnimationType(this VrcType @this) {
            switch(@this) {
            case VrcType.Bool:
                return AnimatorControllerParameterType.Bool;
            case VrcType.Int:
                return AnimatorControllerParameterType.Int;
            case VrcType.Float:
                return AnimatorControllerParameterType.Float;
            }
            throw new InvalidOperationException($"不明な入力{@this}");           
        }


        public static AnimatorState NewState(this AnimatorStateMachine @this, string name, Motion motion) {
            var state = @this.AddState(name);
            state.writeDefaultValues = false;
            state.motion = motion;
            return state;
        }

        public static AnimatorStateTransition ToNext(this AnimatorStateTransition @this) {
            @this.hasExitTime = false;
            @this.hasFixedDuration = true;
            @this.duration = 0f;
            @this.exitTime = 0f;
            return @this;
        }

        public static AnimatorStateTransition ToFree(this AnimatorStateTransition @this) {
            @this.hasExitTime = true;
            @this.hasFixedDuration = true;
            @this.duration = 0f;
            @this.exitTime = 0f;
            return @this;
        }

        public static AnimatorConditionMode ToMode(this __ConditionRecord @this) {
            if(@this.Param.Type == VrcType.Bool) {
                if(@this.Op == ShizukuOprator.Equals) {
                    if(@this.Value != 0) {
                        return AnimatorConditionMode.If;
                    } else {
                        return AnimatorConditionMode.IfNot;                    
                    }
                } else if(@this.Op == ShizukuOprator.NotEquals) {
                    if(@this.Value != 0) {
                        return AnimatorConditionMode.IfNot;
                    } else {
                        return AnimatorConditionMode.If;                    
                    }
                }
                throw new InvalidOperationException("boolと予期しない組み合わせ");
            }

            switch(@this.Op) {
            case ShizukuOprator.Equals:
                return AnimatorConditionMode.Equals;
            case ShizukuOprator.NotEquals:
                return AnimatorConditionMode.NotEqual;
            case ShizukuOprator.LessThan:
                return AnimatorConditionMode.Less;
            case ShizukuOprator.GreaterThan:
                return AnimatorConditionMode.Greater;
            default:
                throw new InvalidOperationException("予期しないOp");
            }
        }
    }
}