namespace Morpeh {
    using System.Collections.Generic;
    using Globals;
#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [CreateAssetMenu(menuName = "ECS/Configurator")]
    public class Configurator : Initializer {
        public List<Configuration> configurations;

        private void OnValidate() {
            foreach (var configuration in this.configurations) {
                if (configuration.variable == null) {
                    configuration.wrapper = null;
                }
                else if (configuration.variable is IDataVariable variable ) {
                    if (configuration.wrapper == null) {
                        configuration.wrapper = variable.Wrapper;
                    }
                }
                else {
                    configuration.variable = null;
                    configuration.wrapper = null;
                }
            }
        }

        public override void OnAwake() {
            foreach (var configuration in this.configurations) {
                if (configuration.variable == null) {
                    continue;
                }
                if (configuration.wrapper != null && configuration.variable is IDataVariable variable) {
                    variable.Wrapper = configuration.wrapper;
                }
            }
        }

        [System.Serializable]
        public class Configuration {
            public BaseGlobal variable;
            [SerializeReference]
#if UNITY_EDITOR && ODIN_INSPECTOR
            [HideIf("@variable == null")]
            [HideReferenceObjectPicker]
#endif
            public DataWrapper wrapper;
        }
    }
}