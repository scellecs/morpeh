#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
namespace Scellecs.Morpeh.WorldBrowser.Remote {
    internal unsafe struct Command {
        internal byte CommandType;
        internal byte CommandId;
        internal byte* Data;
        internal int Length;

        internal static Command FromPtr(byte* ptr, int length) => new Command() {
            CommandType = ptr[0],
            CommandId = ptr[1],
            Length = length - 2,
            Data = length > 2 ? ptr + 2 : null
        };
    }
}
#endif
