namespace Morpeh.Tasks {
    using System.Collections.Generic;
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

#if UNITY_EDITOR && ODIN_INSPECTOR
    [HideMonoScript]
#endif
    public class Scheduler : MonoProvider<ECS.TasksComponent> {
        
#if UNITY_EDITOR && ODIN_INSPECTOR
        [OnInspectorGUI]
        private void OnEditoGUI() {
            this.gameObject.transform.hideFlags = HideFlags.HideInInspector;
        }
#endif
        
#if UNITY_EDITOR
        [MenuItem("GameObject/ECS/Scheduler", false, 2)]
        private static void CreateInstaller(MenuCommand menuCommand) {
            var go = new GameObject("[Scheduler]");
            go.AddComponent<Scheduler>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
#endif
    }
    
    namespace ECS {
        using System;

        [Serializable]
        public struct TasksComponent : IComponent {
            public int order;
            public List<Task> tasks;
        }

        public class ProcessTasksSystem : ISystem {
            public World World { get; set; }

            private Filter filter;
            private SortedList<int, List<Task>> sortedList;

            public void OnAwake() {
                this.sortedList = new SortedList<int, List<Task>>();
                this.filter = this.World.Filter.With<TasksComponent>();
            }

            public void OnUpdate(float deltaTime) {
                this.sortedList.Clear();
                foreach (var tasksEntity in this.filter) {
                    ref var tasksComponent = ref tasksEntity.GetComponent<TasksComponent>();
                    this.sortedList.Add(tasksComponent.order, tasksComponent.tasks);
                }

                for (var index = 0; index < this.sortedList.Values.Count; index++) {
                    var tasks = this.sortedList.Values[index];
                    foreach (var task in tasks) {
                        task.Execute();
                    }
                }
            }

            public void Dispose() {
            }
        }
    }
}

namespace Morpeh {
    partial class World {
        partial void InitializeTasks() {
            var sg = this.CreateSystemsGroup();
            sg.AddSystem(new Tasks.ECS.ProcessTasksSystem());
            this.AddSystemsGroup(9001, sg);
        }
    }
}