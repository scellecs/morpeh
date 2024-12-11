# 🚀 Migration Guide  

## Migration from version 2023.1 to 2024.1

### Breaking Changes

* When installing Morpeh as a package in Unity (via package manager or manifest.json),
you now need to additionally specify `?path=Scellecs.Morpeh`.
Installation link has been updated in the `README.md`.
* Active `World` count is now limited to 256. If you have more than
256 worlds active at the same time, consider merging them into one world.
* `Entity` is now a struct instead of a class and no longer has `partial` modifier.
`default(Entity)` is reserved as an invalid entity.
In case you have some external plugins or code that relies on no
more existing data, you may need to update it. Some data
you may need (e.g. for plugins, workarounds, extensions, etc.)
could still be available through the internal data (like `EntityData`
in the `World`, or `Archetype` class in general).
Pooling `Entity` may also make no sense anymore.
* Since `Entity` is now a struct and can be used in jobs,
`EntityId` and `World.TryGetEntity` have been removed - native APIs
can use `Entity` directly.
* `EntityExtensions` (`Entity.Add<T>()`, etc.) methods are now marked
as Obsolete because of a potential removal in future versions of Morpeh.
It will still be available throughout the entire current major Morpeh version
(2024) but *may* be removed in Morpeh 2025 release. Prefer using
`Stash` API which is guaranteed to stay and is faster anyway.
* `Installer`, `UpdateSystem`, `FixedSystem` and other ScriptableObject-based
systems are now marked as Obsolete because of a potential removal
in future versions of Morpeh. It will still be available throughout
the entire current major Morpeh version (2024) but *may* be removed in
Morpeh 2025 release. Prefer using `SystemGroup` + `ISystem`
(`IFixedSystem`, etc.) API for easier migration later on.
* `Stash<T>` initial size functionality is now a part of
`ComponentId`. Use `ComponentId<T>.StashSize` to modify the initial size
of the stash before the first call to `World.GetStash<T>()`
(which includes `EntityExtensions` API and different providers / external code).
* `ComponentId` and `ExtendedComponentId` are now split into
different use cases to reduce IL2CPP metadata size. `ExtendedComponentId`
is now stripped out of non-UnityEditor runtimes unless
`MORPEH_GENERATE_ALL_EXTENDED_IDS` is specified. `ExtendedComponentId` may
be required for reflection-based code where `IStash` interface is not enough.
* Removed `UniversalProvider`. Consider using `MonoProvider` instead.
`UniversalProvider` *may* return in a form of source-generated class
with defined-in-code components in future versions (aka "`MonoProvider`
but with multiple components").
* Removed `BitMap<T>`. If applicable, use `IntHashMap<T>` or backport
`BitMap<T>` from Morpeh 2023.1 to your project.
* Removed `UnmanagedList<T>`. As it was broken anyway, this should not affect any projects.
* Removed `UnmanagedArray<T>`. As it was broken anyway, this should not affect any projects.
* Removed `Stash<T>.Empty()`. This was used for internal purposes and should
not affect any projects. In case it was used somewhere, just use `default(T)`
for the same effect.
* `FastList<T>` and multiple other collections were fixed to be more
reliable and less error-prone. This may affect some projects that relied
on the previous behaviour of these collections. Please refer to the
source code and adapt your usage to the renamed methods or new overloads.
Beware that HashMaps in general still don't work with negative
values as keys inside `foreach` calls.
* `bool Stash<T>.Add(Entity entity, in T value)` is now a `void` method
and throws an exception if the entity already has the component.
This is to prevent accidental retaining of old data if the component
already exists where logic expects `Add` to always restore data to the defaulted state. This decision has been made due to lots of projects using `Add` and discarding the return value, leading to potentially super-hard-to-find bugs.
* All `Stash<T>` methods always throw an exception if an entity
is invalid or the operation makes no sense to make (e.g. `Get` if
an entity has no such component), both in Debug and Release mode.
This may increase exception rate in your project instead of silently
ignoring them as it was before. These exceptions have to be addressed
and fixed in the project code.

### New API

* Refer to the [Changelog](CHANGELOG.MD) for the full list of changes.

### Next Major Version remarks

We are evaluating a possibility of heavy usage of Source Generators in future
versions of Morpeh, potentially even in 2025 release. 
This may pose more restrictions on what we may or may not support in the future.

One likely change is potential introduction of tag stashes (non-generic) for tag
components, which would reduce IL2CPP overhead, memory overhead, and increase
general performance. We are willing to make it user-agnostic so that the user
does not have to worry about the specific implementation of the stash, if possible.
If that's the case, you may want to avoid iterating over stashes with tag components
as this operation may slow down due to necessity to count trailing zeroes if bitset
is chosen as an underlying storage type. This may or not be a bitset after all
(e.g. if we use a hashset or something else), but the general idea is to be
cautious about iterating over large stashes.

One of the most likely changes is the removal of `EntityExtensions` API in favor
of `Stash` API, if it becomes possible to make a convenient replacement for it. We
do acknowledge that `EntityExtensions` is a very convenient API, but it has
a large overhead (both CPU and IL2CPP metadata amount) and is not very flexible,
especially if we want to implement tag stashes for tag components
described above.

We have been using a completely separate system runner in our projects for a while now,
and it has been working quite well, allowing us to avoid interface/virtual calls
to system update methods, improving overall performance due to lower idle systems
cost (systems that do not have any entities to process, but the update method is still
called). This may lead to a complete removal of ScriptableObject-based systems in
favor of source-generated systems, as well as source-generated "features" which
would replace `SystemGroup`. For an easier migration later on, we recommend
sticking to pure-C# systems implementing `ISystem` interface instead of 
ScriptableObject-based systems.

Providers (`EntityProvider`/`MonoProvider`) are also subject to possible changes in
future versions. We are considering a possibility of merging them into one
source-generated class, which would allow defining multiple components in one
place. This would also allow us to create a well-defined functionality for
entity disposal process, as currently `EntityProvider` does not *actually*
remove entities from the world, but just removes its component from the entity,
and entity disposal is handled by removing the last component from the entity during
the next `World.Commit()` call. This sometimes leads to confusion and unexpected behaviour
when the entity has components set from the outside of the providers,
potentially leaking unused entities.

All the changes described above will definitely affect external plugins, workarounds,
extensions and other code that relies on Morpeh internals or even some public APIs.

Please note that these are just plans and may be or may not be implemented in the future.
The list may also be incomplete and may not cover all the changes that are planned.

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
