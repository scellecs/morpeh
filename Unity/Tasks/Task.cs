namespace Morpeh.Tasks {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Globals;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using Unity.IL2CPP.CompilerServices;

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
        private List<TaskAction> actions = default;

#if UNITY_EDITOR && ODIN_INSPECTOR
        private TaskAction AddAction() => new TaskAction {parent = this};

        private void RemoveAction(TaskAction action) {
            AssetDatabase.RemoveObjectFromAsset(action.Action);
            this.actions.Remove(action);
            AssetDatabase.SaveAssets();
        }

        private void RemoveActionByIndex(int index) {
            var value = this.actions[index];
            if (value != null && value.Action != null) {
                AssetDatabase.RemoveObjectFromAsset(value.Action);
            }

            this.actions.RemoveAt(index);
            AssetDatabase.SaveAssets();
        }
#endif

        public void Execute() {
            for (int i = 0, length = this.conditions.Count; i < length; i++) {
                if (!this.conditions[i].Compare()) {
                    return;
                }
            }

            foreach (var action in this.actions) {
                action.Action.Execute();
            }
        }

        [Serializable]
        public class TaskCondition {
            public BaseGlobal global = default;

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
            private bool IsInt    => this.global.GetValueType() == typeof(int);
            private bool IsFloat  => this.global.GetValueType() == typeof(float);
            private bool IsString => this.global.GetValueType() == typeof(string);
            private bool IsBool   => this.global.GetValueType() == typeof(bool);

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
                public          T    value;
                public abstract bool Compare(T other);
            }

            [Serializable]
            public abstract class EqualityComparison<T> : Comparison<T> where T : IEquatable<T> {
                public override bool Compare(T other) => other.Equals(this.value);
            }

            [Serializable]
            public sealed class StringComparison : EqualityComparison<string> {
            }

            [Serializable]
            public sealed class BoolComparison : EqualityComparison<bool> {
            }

            [Serializable]
            public class FloatComparison : Comparison<float> {
                public ComparisonType type;

                public override bool Compare(float other) {
                    if (this.type == ComparisonType.Equals) {
                        return other == this.value;
                    }

                    if (this.type == ComparisonType.Greater) {
                        return other > this.value;
                    }

                    if (this.type == ComparisonType.Less) {
                        return other < this.value;
                    }

                    if (this.type == ComparisonType.GreaterOrEquals) {
                        return other >= this.value;
                    }

                    if (this.type == ComparisonType.LessOrEquals) {
                        return other <= this.value;
                    }

                    return false;
                }
            }

            [Serializable]
            public class IntComparison : Comparison<int> {
                public ComparisonType type;

                public override bool Compare(int other) {
                    if (this.type == ComparisonType.Equals) {
                        return other == this.value;
                    }

                    if (this.type == ComparisonType.Greater) {
                        return other > this.value;
                    }

                    if (this.type == ComparisonType.Less) {
                        return other < this.value;
                    }

                    if (this.type == ComparisonType.GreaterOrEquals) {
                        return other >= this.value;
                    }

                    if (this.type == ComparisonType.LessOrEquals) {
                        return other <= this.value;
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
                        if (g.BatchedChanges.Count > 0) {
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
            private BaseAction action;

#if UNITY_EDITOR && ODIN_INSPECTOR
            [HideLabel]
            [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
            [ShowInInspector]
#endif
            public BaseAction Action {
                get => this.action;
                set {
#if UNITY_EDITOR
                    if (this.action != null) {
                        AssetDatabase.RemoveObjectFromAsset(this.action);
                    }
#endif
                    this.action           = Instantiate(value);
                    this.action.hideFlags = HideFlags.HideInHierarchy;
                    this.action.name      = value.name;
#if UNITY_EDITOR
                    AssetDatabase.AddObjectToAsset(this.action, this.parent);

                    AssetDatabase.SaveAssets();
#endif
                }
            }
        }
    }
}