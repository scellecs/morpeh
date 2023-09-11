# 🚀 Migration Guide  
## Migration from version 2022.2 to 2023.1

### Breaking changes
* `Filter` initialization should end with a `Build()` call, for example: `World.Filter.With<Component>().Build()`.
* `ComponentProvider` has been removed. If you still need it, you may copy it from older versions, or code your own alternative.

### New API
* Tri Inspector support is now available as a free alternative to Odin Inspector integration. The framework has no paid dependencies as of now, and both Tri Inspector and Odin Inspector can be chosen by the developers.
* Added Aspects, which are represented as a helper tool combining multiple components into one structure. Read README for more details.
* Stash API now includes `stash.RemoveAll()`, which can be used to remove a certain component from all entities in one single call. Useful with OneFrame components, e.g. when you have to make sure that no entities have a specific component at an exact point of time.
* Providers now have deinitialization methods the same way initialization existed.
* `World.JobHandle` allows combining Jobs within one `SystemsGroup`. Read README for more details.
* Added profiling metrics which allow tracking entities count, archetypes count and etc. Read README for details.
* Added a flag and a method to check that the world has been removed and cannot be used anymore: `World.IsDisposed` and `World.IsNullOrDisposed()`
* Added preprocessor definition `MORPEH_DISABLE_SET_ICONS` which disables automatic icon assignment to systems, providers, etc.
* `World.Commit()` now checks that it is not being called inside filter iterations.
* Added `World.DoNotDisableSystemOnException` which prevents system from stopping on exception in debug mode. Does not affect release builds.
* Added `Filter.IsNotEmpty()`.

### Odin Inspector
You can continue using Odin Inspector or switch to Tri Inspector.
To do this you need to add Tri Inspector via Git URL.
Remove Odin, and remove Odin Inspector defines from Player Settings.

## Migration from version 2022.1 to 2022.2

### Breaking changes
* Minimum Unity version up to 2020.3.* LTS
* Namespaces changed from `Morpeh` to `Scellecs.Morpeh`, `Scellecs.Morpeh.Systems`, `Scellecs.Morpeh.Providers`.
* Globals are separated into a separate package at https://github.com/scellecs/morpeh.globals.
* The `World.UpdateFilters()` method has been renamed to `World.Commit()`.
* The `ComponentsCache<>` class has been renamed to `Stash<>`. All methods for stashes have lost the `Component` prefix, now just `Add, Get, Set, Has, Remove`.
* Filters validate that they do not have duplicate types. For example, `Filter.With<A>().With<A>()` will throw an error.
* Removed `Filter` property from systems, use `World.Filter` instead.

### New API
* Added `ICleanupSystem` suitable for cleanup logic. Called by the most recent in LateUpdate by default.
* The mechanism for cleaning components has been redesigned. Now the component must implement `IDisposable`, and it is necessary to call the `AsDisposable` method of the stesh once in order for the cleanup to take place. For example, the shortest version is `World.GetStash<T>().AsDisposable()`.
* Added `World` method `GetReflectionStash` to get stash not via generic argument, but by `System.Type`.
* Added a `MORPEH_THREAD_SAFETY` define that forces the kernel to validate that all calls come from the same thread the world was created on. The binding to a thread can be changed using the `World.GetThreadId(), World.SetThreadId()` methods.
* Added API for plugins. To use it, you need to implement `IWorldPlugin`.

## Migration from version 2020.* to 2022.1

### Breaking changes  
* Assembly definition and all links have been replaced from `XCrew.Morpeh` to `Scellecs.Morpeh`. Namespace is the same.
* All Globals have lost method `NextFrame`. Use `Publish`, which works exactly as `NextFrame` used to. Current-frame Global events are not supported anymore, they are always deferred. The reason for this change is that most projects used both `Publish` + `NextFrame`, or `NextFrame` only.
* `Entity.ID` is not `int` anymore, but `EntityID` which contains `id` and `gen`. This is a mandatory change for Jobs/Burst support. `World.TryGetEntity(EntityId entityId, out Entity entity)` has been added, which allows checking if entity exists without storing direct reference to it.
* `Filter.Length` is removed completely. It has been replaced with `Filter.GetLengthSlow()` and `Filter.IsEmpty()`. This change is made because recalculating Filter length was quite costly, and most of the time filters length was not needed for developers, making it a useless overhead. It's important to note that most length comparisons were used to check if filter is empty or not, so such checks using `IsEmpty()` are as fast as they used to be, but with a different API. This allowed us to make Morpeh much faster in projects which have a lot of different filters.
* Using `entity.RemoveComponent()` clears components immediately now, and reading ref components data after removing said component is prohibited and will lead to unexpected behaviour.
* Iterating `Filter` does not guarantee any order now. If your logic depended on filters returning entities exactly in specific order, it has to be fixed before upgrading.
* `ComponentsBag` has been completely removed. It has been replaced with `ComponentsCache` which also provides full public access to raw components.

### New API  
* Added new component interfaces `IValidatable`, `IValidatableWithGameObject`. It allows calling default Unity method `OnValidate` for components, f.e. to initialize editor fields. Works only for components added via MonoProvider.
* Added support for native Api (Job/Burst). Details are available in [README](README.md#unity-jobs-and-burst). Most work is done via `AsNative()` for:
  * Archetype (NativeArchetype)
  * ComponentsCache (NativeCache)
  * FastList (NativeFastList)
  * IntFastList (NativeIntFastList)
  * Filter (NativeFilter)
  * IntHashMap (NativeIntHashMap)
  * World (NativeWorld)
* Added `IMorpehLogger` interface and static `MLogger` class. It allows overriding logging behaviour for Morpeh. Console.WriteLine is used for all environments by default, except Unity.
* Added `MORPEH_PROFILING` define. Adding it to Player Settings allows profiling systems without using Deep Profile.
* `Entity.Dispose()` is now public. It allows destroying entities without accessing the World it's in.
* Added viewing multiple worlds in World Browser
