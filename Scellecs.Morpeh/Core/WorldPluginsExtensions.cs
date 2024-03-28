using JetBrains.Annotations;

namespace Scellecs.Morpeh {
    using System.Runtime.CompilerServices;
    using Scellecs.Morpeh.Collections;
    
    public static class WorldPluginsExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddWorldPlugin<T>(T plugin) where T : class, IWorldPlugin {
            if (World.plugins == null) {
                World.plugins = new FastList<IWorldPlugin>();
            }
            World.plugins.Add(plugin);
        }
        
        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddPluginSystemsGroup(this World world, SystemsGroup systemsGroup) {
            world.ThreadSafetyCheck();
            
            world.newPluginSystemsGroups.Add(systemsGroup);
        }
    }
}