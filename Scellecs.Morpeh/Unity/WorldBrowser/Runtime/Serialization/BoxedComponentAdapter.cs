#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Serialization.Binary;
using static Scellecs.Morpeh.WorldBrowser.Serialization.SerializationUtility;

namespace Scellecs.Morpeh.WorldBrowser.Serialization {
    internal unsafe sealed class BoxedComponentAdapter : IBinaryAdapter<ComponentDataBoxed> {
        public void Serialize(in BinarySerializationContext<ComponentDataBoxed> context, ComponentDataBoxed value) {
            var startLength = context.Writer->Length;
            context.Writer->Add(0);
            var contentStart = context.Writer->Length;

            WriteString(context, value.data.GetType().AssemblyQualifiedName);
            context.Writer->Add(value.isMarker);
            context.Writer->Add(value.typeId);
            context.SerializeValue(value.data);

            var totalSize = context.Writer->Length - contentStart;
            UnsafeUtility.MemCpy(context.Writer->Ptr + startLength, &totalSize, sizeof(int));
        }

        public ComponentDataBoxed Deserialize(in BinaryDeserializationContext<ComponentDataBoxed> context) {
            var blockSize = context.Reader->ReadNext<int>();
            var startOffset = context.Reader->Offset;

            var typeName = ReadString(context);
            var isMarker = context.Reader->ReadNext<bool>();
            var typeId = context.Reader->ReadNext<int>();
            var componentData = default(IComponent);

            try {
                componentData = context.DeserializeValue<IComponent>();
            }
            catch (Exception) {
                context.Reader->Offset = startOffset + blockSize;
                var type = Type.GetType(typeName);
                componentData = (IComponent)Activator.CreateInstance(type);
            }

            return new ComponentDataBoxed() {
                data = componentData,
                isMarker = isMarker,
                typeId = typeId,
            };
        }
    }
}
#endif
