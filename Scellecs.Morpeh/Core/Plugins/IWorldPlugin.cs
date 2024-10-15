namespace Scellecs.Morpeh {
    public interface IWorldPlugin {
        void Initialize(World world);
        void Deinitialize(World world);
    }
}