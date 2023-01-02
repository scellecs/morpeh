namespace Scellecs.Morpeh {
    public struct Aspect<T> where T : struct, IAspect {
        internal T value;
    }
}
