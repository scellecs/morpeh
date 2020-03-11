namespace Morpeh.Globals {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector;
    using Tasks;
    using Unity.IL2CPP.CompilerServices;
    using UnityEditor;
    using UnityEngine;
#if UNITY_EDITOR && ODIN_INSPECTOR
#endif
#if UNITY_EDITOR
#endif

#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "ECS/Task")]
#endif
#if UNITY_EDITOR && ODIN_INSPECTOR
    [HideMonoScript]
#endif
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class Task : ScriptableObject {
        [Space]
        [SerializeField]
        private List<TaskCondition> conditions = default;

        [Space]
        [SerializeField]
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ListDrawerSettings(
            CustomAddFunction           = nameof(AddAction),
            CustomRemoveElementFunction = nameof(RemoveAction),
            CustomRemoveIndexFunction   = nameof(RemoveActionByIndex))]
#endif
        internal List<TaskAction> actions = default;

#if UNITY_EDITOR && ODIN_INSPECTOR
        private TaskAction AddAction() => new TaskAction {parent = this};

        private void RemoveAction(TaskAction action) {
            AssetDatabase.RemoveObjectFromAsset(action.ActionSystem);
            this.actions.Remove(action);
            AssetDatabase.SaveAssets();
        }

        private void RemoveActionByIndex(int index) {
            var value = this.actions[index];
            if (value != null && value.ActionSystem != null) {
                AssetDatabase.RemoveObjectFromAsset(value.ActionSystem);
            }

            this.actions.RemoveAt(index);
            AssetDatabase.SaveAssets();
        }
#endif
        internal ICondition GetTaskCondition() => this.taskCommonCondition ?? (this.taskCommonCondition = new TaskCommonCondition(this.conditions));
        
        private TaskCommonCondition taskCommonCondition = default;

        private class TaskCommonCondition : ICondition {
            private readonly List<TaskCondition> conditions;

            public TaskCommonCondition(List<TaskCondition> taskConditions) => this.conditions = taskConditions;

            public bool Check() {
                for (int i = 0, length = this.conditions.Count; i < length; i++) {
                    if (!this.conditions[i].Compare()) {
                        return false;
                    }
                }
                return true;
            }

            public void Dispose() {
            }
        }

        [Serializable]
        public class TaskCondition {
            public BaseGlobal global = default;
            private bool firstCompare;

            [SerializeField]
#if UNITY_EDITOR && ODIN_INSPECTOR
            [InlineProperty]
            [ShowIf(nameof(IsVariable))]
            [HideLabel]
#endif
            private VariableComparisonMode variableComparisonMode = default;

            [SerializeField]
#if UNITY_EDITOR && ODIN_INSPECTOR
            [InlineProperty]
            [HideIf(nameof(IsVariable))]
            [HideLabel]
#endif
            private EventComparisonMode eventComparisonMode = default;

            [SerializeField]
#if UNITY_EDITOR && ODIN_INSPECTOR
            [ShowIf(nameof(IsInt))]
            [HideIf(nameof(DontCompare))]
            [InlineProperty]
            [HideLabel]
#endif
            private IntComparison intComparison = default;

            [SerializeField]
#if UNITY_EDITOR && ODIN_INSPECTOR
            [ShowIf(nameof(IsFloat))]
            [HideIf(nameof(DontCompare))]
            [InlineProperty]
            [HideLabel]
#endif
            private FloatComparison floatComparison = default;

            [SerializeField]
#if UNITY_EDITOR && ODIN_INSPECTOR
            [ShowIf(nameof(IsString))]
            [HideIf(nameof(DontCompare))]
            [InlineProperty]
            [HideLabel]
#endif
            private StringComparison stringComparison = default;

            [SerializeField]
#if UNITY_EDITOR && ODIN_INSPECTOR
            [ShowIf(nameof(IsBool))]
            [HideIf(nameof(DontCompare))]
            [InlineProperty]
            [HideLabel]
#endif
            private BoolComparison boolComparison = default;


#if UNITY_EDITOR && ODIN_INSPECTOR
            private bool IsInt    => this.global != null && this.global.GetValueType() == typeof(int);
            private bool IsFloat  => this.global != null && this.global.GetValueType() == typeof(float);
            private bool IsString => this.global != null && this.global.GetValueType() == typeof(string);
            private bool IsBool   => this.global != null && this.global.GetValueType() == typeof(bool);

            private bool IsVariable => this.global != null && InheritsFrom(this.global.GetType(), typeof(BaseGlobalVariable<>));

            private bool DontCompare =>
                this.IsVariable ? this.variableComparisonMode == VariableComparisonMode.DontCompareOnChange : this.eventComparisonMode == EventComparisonMode.DontCompareOnChange;
#endif

            public enum ComparisonType {
                Equals          = 0,
                Less            = 10,
                Greater         = 20,
                LessOrEquals    = 30,
                GreaterOrEquals = 40
            }

            public enum EventComparisonMode {
                DontCompareOnChange  = 0,
                CompareValueOnChange = 10,
            }

            public enum VariableComparisonMode {
                CompareValueOnChange  = 0,
                DontCompareOnChange   = 10,
                CompareValueEveryTime = 20,
            }

            [Serializable]
            public abstract class Comparison {
            }
            
            [Serializable]
            public abstract class Comparison<T> : Comparison where T : IEquatable<T> {
                public abstract bool Compare(T other);
            }

            [Serializable]
            public abstract class Comparison<TGV, T> : Comparison<T> where T : IEquatable<T> where TGV : BaseGlobalVariable<T> {
                public TGV value;
            }

            [Serializable]
            public abstract class EqualityComparison<TGV, T> : Comparison<TGV, T> where T : IEquatable<T> where TGV : BaseGlobalVariable<T> {
                public override bool Compare(T other) => other.Equals(this.value.Value);
            }

            [Serializable]
            public sealed class StringComparison : EqualityComparison<GlobalVariableString, string> {
            }

            [Serializable]
            public sealed class BoolComparison : EqualityComparison<GlobalVariableBool, bool> {
            }

            [Serializable]
            public class FloatComparison : Comparison<GlobalVariableFloat, float> {
                public ComparisonType type;

                public override bool Compare(float other) {
                    if (this.type == ComparisonType.Equals) {
                        return other == this.value.Value;
                    }

                    if (this.type == ComparisonType.Greater) {
                        return other > this.value.Value;
                    }

                    if (this.type == ComparisonType.Less) {
                        return other < this.value.Value;
                    }

                    if (this.type == ComparisonType.GreaterOrEquals) {
                        return other >= this.value.Value;
                    }

                    if (this.type == ComparisonType.LessOrEquals) {
                        return other <= this.value.Value;
                    }

                    return false;
                }
            }

            [Serializable]
            public class IntComparison : Comparison<GlobalVariableInt, int> {
                public ComparisonType type;

                public override bool Compare(int other) {
                    if (this.type == ComparisonType.Equals) {
                        return other == this.value.Value;
                    }

                    if (this.type == ComparisonType.Greater) {
                        return other > this.value.Value;
                    }

                    if (this.type == ComparisonType.Less) {
                        return other < this.value.Value;
                    }

                    if (this.type == ComparisonType.GreaterOrEquals) {
                        return other >= this.value.Value;
                    }

                    if (this.type == ComparisonType.LessOrEquals) {
                        return other <= this.value.Value;
                    }

                    return false;
                }
            }

            public bool Compare() {
                bool CompareVariables<T0, T1>(T0 g, Comparison<T1> c) where T0 : BaseGlobalVariable<T1> where T1 : IEquatable<T1> {
                    if (this.variableComparisonMode == VariableComparisonMode.DontCompareOnChange) {
                        return this.global.IsPublished;
                    }

                    var value = g.Value;
                    if (this.variableComparisonMode == VariableComparisonMode.CompareValueOnChange) {
                        if (!this.firstCompare) {
                            this.firstCompare = true;
                        }
                        else if (g.BatchedChanges.Count > 0) {
                            value = g.BatchedChanges.Peek();
                        }
                        else {
                            return false;
                        }
                    }

                    return c.Compare(value);
                }

                bool CompareEvents<T0, T1>(T0 g, Comparison<T1> c) where T0 : BaseGlobalEvent<T1> where T1 : IEquatable<T1> {
                    if (this.eventComparisonMode == EventComparisonMode.DontCompareOnChange) {
                        return this.global.IsPublished;
                    }

                    if (this.eventComparisonMode == EventComparisonMode.CompareValueOnChange) {
                        if (g.BatchedChanges.Count > 0) {
                            var value = g.BatchedChanges.Peek();
                            return c.Compare(value);
                        }

                        return false;
                    }

                    return false;
                }

                if (this.global is GlobalVariableInt gvi) {
                    return CompareVariables(gvi, this.intComparison);
                }

                if (this.global is GlobalVariableBool gvb) {
                    return CompareVariables(gvb, this.boolComparison);
                }

                if (this.global is GlobalVariableFloat gvf) {
                    return CompareVariables(gvf, this.floatComparison);
                }

                if (this.global is GlobalVariableString gvs) {
                    return CompareVariables(gvs, this.stringComparison);
                }

                if (this.global is GlobalEventInt gi) {
                    return CompareEvents(gi, this.intComparison);
                }

                if (this.global is GlobalEventBool gb) {
                    return CompareEvents(gb, this.boolComparison);
                }

                if (this.global is GlobalEventFloat gf) {
                    return CompareEvents(gf, this.floatComparison);
                }

                if (this.global is GlobalEventString gs) {
                    return CompareEvents(gs, this.stringComparison);
                }

                return this.global.IsPublished;
            }

            private static bool InheritsFrom(Type type, Type baseType) {
                if (baseType.IsAssignableFrom(type))
                    return true;
                if (type.IsInterface && !baseType.IsInterface)
                    return false;
                if (baseType.IsInterface)
                    return type.GetInterfaces().Contains(baseType);
                for (var type1 = type; type1 != null; type1 = type1.BaseType) {
                    if (type1 == baseType || baseType.IsGenericTypeDefinition && type1.IsGenericType && type1.GetGenericTypeDefinition() == baseType)
                        return true;
                }

                return false;
            }
        }

        [Serializable]
        public class TaskAction {
            [HideInInspector]
            [SerializeField]
            internal Task parent;

            [SerializeField]
            [HideInInspector]
            private ActionSystem actionSystem;

#if UNITY_EDITOR && ODIN_INSPECTOR
            [HideLabel]
            [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
            [ShowInInspector]
#endif
            public ActionSystem ActionSystem {
                get => this.actionSystem;
                set {
#if UNITY_EDITOR
                    if (this.actionSystem != null) {
                        AssetDatabase.RemoveObjectFromAsset(this.actionSystem);
                    }
#endif
                    this.actionSystem           = Instantiate(value);
                    this.actionSystem.hideFlags = HideFlags.HideInHierarchy;
                    this.actionSystem.name      = value.name;
#if UNITY_EDITOR
                    AssetDatabase.AddObjectToAsset(this.actionSystem, this.parent);

                    AssetDatabase.SaveAssets();
#endif
                }
            }
        }
    }
}