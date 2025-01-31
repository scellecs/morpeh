#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using Unity.Serialization.Binary;

namespace Scellecs.Morpeh.WorldBrowser.Serialization {
    internal unsafe sealed class EntityAdapter : IBinaryAdapter<Entity> {
        public Entity Deserialize(in BinaryDeserializationContext<Entity> context) => context.Reader->ReadNext<Entity>();
        public void Serialize(in BinarySerializationContext<Entity> context, Entity value) => context.Writer->Add(value);
    }
}
#endif
