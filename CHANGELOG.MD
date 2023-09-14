# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2023.1.0] - 09.10.2023
### Fixed
- Fix `NativeFilter` allocations.
- Calling `World.InitializeDefaultWorld()` do not create new Update Loop Runner.
- Avoid defines modification by @vanifatovvlad.
- Small bugfix providers, when entity was destroyed.
### Added
- Aspects API. `IAspect`, `IFilterExtensions`, `Filter.Extend()`.
- New method for stashes `RemoveAll()` by @SH42913.
- Deinitilize method for `EntityProvider`, `MonoProvider`.
- `World.JobHandle`.
- Metrics for Unity Profiler.
- TriInspector Integration for replacing paid Odin Inspector.
- `World.IsDisposed` and `World.IsNullOrDisposed()`
- `MORPEH_DISABLE_SET_ICONS` define for disabling set icons in Project Window.
- Safety check for `World.Commit()` inside Filter foreach loop.
- New Pinned Collections.
- `World.DoNotDisableSystemOnException`
- `Filter.IsNotEmpty()`
### Changed
- **[Breaking]** Filters must be initialized with `Build()` method.
- Overrided Entity.ToString() will output ID by @SH42913.
- Rework types warmup for fast start of application.
- All archetypes are reworked. Performance has been significantly improved and RAM consumption has been reduced.
- All Native API changed from internal to public.
- `Entity` and `World` now has partial modifier.
### Removed
- Discover window.
- **[Breaking]** Removed `ComponentProvider`.

## [2022.2.3] - 18.05.2023
### Fixed
- RemoveEntityOnDestroy doesnt work properly.
- Disable LFS

## [2022.2.2] - 26.02.2023
### Fixed
- Stashes were cleaned incorrectly. https://github.com/scellecs/morpeh/issues/170

## [2022.2.1] - 16.01.2023
### Changed
- Change visibility of `EntityProvider.map` from private to public.

### Fixed
- Disable warnings for Unity 2021+

## [2022.2.0] - 28.12.2022
### Added
- Added `ICleanupSystem` suitable for cleanup logic. Called by the most recent in LateUpdate by default.
- Added to `World` method `GetReflectionStash` to get stash not via generic argument, but by `System.Type`.
- Added a `MORPEH_THREAD_SAFETY` define that forces the kernel to validate that all calls come from the same thread the world was created on. The binding to a thread can be changed using the `World.GetThreadId(), World.SetThreadId()` methods.
- Added API for plugins. To use it, you need to implement `IWorldPlugin`.
- Unmanaged Collections for Jobs & Burst.

### Changed
- **[Breaking]** Minimal Unity version up to 2020.3.*
- **[Breaking]** Rename namespaces `Morpeh/XCrew.Morpeh` -> `Scellecs.Morpeh`
- **[Breaking]** Globals placed in a separate package via the link https://github.com/scellecs/morpeh.globals
- **[Breaking]** Method `World.UpdateFilters()` renamed to `World.Commit()`.
- **[Breaking]** Class `ComponentsCache<>` renamed to `Stash<>`. All stash methods lost prefix `Component`, now it's `Add, Get, Set, Has, Remove`.
- **[Breaking]** Filters validate that there are no duplicate types in them. For example, `Filter.With<A>().With<A>()` will throw an error.
- **[Breaking]** Removed the `Filter` property from systems, use `World.Filter` instead.
- The mechanism for cleaning components has been redesigned. Now the component must implement `IDisposable`, and it is necessary to call the `AsDisposable` method of the stash once in order for the cleanup to take place. For example, the shortest version is `World.GetStash<T>().AsDisposable()`.
- For `Installer` property `BasePair.System` now has public setter.
- `EntityProvider` and `MonoProvider` are now optimized in methods `OnEnable(), OnDisable()` 

### Fixed
- `GetLengthSlow()` sometimes gave the wrong length.
- `IconsSetter` no longer gives a warning in Unity 2021+
- Cannot add validate method for menu item warning.
- `ComponentsCacheDisposable` doesn't work with AOT fixed by redesign.


## [2022.1.3] - 09.12.2022
### Fixed
- Fix BurstDetector. Domain reload performance has been significantly degraded prior to the patch. Now it is reworked to fastest version.

## [2022.1.2] - 14.11.2022
### Fixed
- Fix MigrateTo() method. Entity archetypes were not changed when the method was called. Fix by @vanifatovvlad.

## [2022.1.1] - 31.10.2022
### Added
- Add .meta file for CHANGELOG.MD

### Fixed
- Fix syntax errors for Unity 2019.4

## [2022.1.0] - 31.10.2022
### Added
- Basic Jobs/Burst support:
  - Changing components data (`ref T nativeCache.GetComponent(EntityID entityId)` / `ref T nativeCache.GetComponent(EntityID entityId, out bool exists)`)
  - Checking if components exist on an entity (`bool nativeCache.HasComponent(EntityID entityId)`)
- `.AsNative()` API for Jobs/Burst:
  - `Archetype` (`NativeArchetype`)
  - `ComponentsCache` (`NativeCache`)
  - `FastList` (`NativeFastList`)
  - `IntFastList` (`NativeIntFastList`)
  - `Filter` (`NativeFilter`)
  - `IntHashMap` (`NativeIntHashMap`)
  - `World` (`NativeWorld`)
- `Filter.IsEmpty()` to check if filter has no entities
- `IMorpehLogger` interface for custom loggers (`Console.WriteLine` for non-Unity environments by default)
- `MORPEH_PROFILING` define for automatic systems profiling
- `World.TryGetEntity(EntityId entityId, out Entity entity)` - returns true and entity if it exists, false otherwise
- `MORPEH_NON_SERIALIZED` define to avoid serialization of Morpeh components
- Displaying multiple worlds in World Browser
- Search support in World Browser
- `MORPEH_BURST` define to determine if Burst is enabled
- `World.SetFriendlyName(string name)` / `string World.GetFriendlyName()` + integration with World Browser
- `Entity.MigrateTo(Entity otherEntity)` to migrate components from one entity to another
- `EXTERNAL_IL2CPP_ATTRS` define support for external IL2CPP attributes

### Changed
- **[Breaking]** Globals `NextFrame` -> `Publish`
- **[Breaking]** `int Entity.ID` -> `EntityID Entity.ID`
- **[Breaking]** `Filter.Length` -> `Filter.GetLengthSlow()`
- **[Breaking]** Components caches now cleanup after removing components (Reading ref components data is not allowed after removing components)
- **[Breaking]** `Filter` iteration is not order-deterministic
- **[Breaking]** `FastList` allows removing elements during iteration (current and next elements only)
- `Entity.Dispose()` is now public
- `Installer` can be inherited from
- License ownership changed to `Oleg Morozov, Igor Boyko`
- Internal optimizations and improvements

### Fixed
- Extra filter update https://github.com/scellecs/morpeh/pull/129
- Systems disposed twice https://github.com/scellecs/morpeh/pull/128
- Entity is detected as alive when it's not https://github.com/scellecs/morpeh/pull/123
- World.Default takes 0-th index with null value if initialized outside of UnityRuntimeHelper https://github.com/scellecs/morpeh/pull/138
- Errors in Pure C# projects
- Memory leaks in Collections

### Removed
- **[Breaking]** `ComponentsBag` (replaced by `ComponentsCache`)