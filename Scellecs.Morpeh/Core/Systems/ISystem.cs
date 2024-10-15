namespace Scellecs.Morpeh {
    public interface ISystem : IInitializer {
        void OnUpdate(float deltaTime);
    }
}