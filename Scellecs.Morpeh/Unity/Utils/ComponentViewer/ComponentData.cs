#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System;
using UnityEngine;
namespace Scellecs.Morpeh.Utils.Editor {
    [Serializable]
    internal struct ComponentData {
        internal ExtendedComponentId.InternalTypeDefinition internalTypeDefinition;
        internal string niceName;
        internal Entity entity;

        internal bool IsMarker => this.internalTypeDefinition.isMarker;
        internal string FullName => this.internalTypeDefinition.type.FullName;
        internal string Name => this.niceName;
        internal bool IsValid => !this.entity.GetWorld().IsNullOrDisposed() && !this.entity.GetWorld().IsDisposed(this.entity);
        internal int TypeId => this.internalTypeDefinition.typeInfo.id;

        [HideLabel]
        [InlineProperty]
        [ShowInInspector]
        [DisableContextMenu]
        [HideReferenceObjectPicker]
        public object Data {
            get {
                if (this.internalTypeDefinition.isMarker || Application.isPlaying == false) {
                    return null;
                }

                if (!IsValid) {
                    return null;
                }

                return this.internalTypeDefinition.entityGetComponentBoxed(this.entity);
            }
            set {
                if (this.internalTypeDefinition.isMarker || Application.isPlaying == false)  {
                    return;
                }

                if (!IsValid) {
                    return;
                }

                this.internalTypeDefinition.entitySetComponentBoxed(this.entity, value);
            }
        }
    }
}
#endif
