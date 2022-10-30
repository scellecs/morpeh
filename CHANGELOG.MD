# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2022.1.0] - 01.11.2022
### Added
- Basic Jobs/Burst support:
  - Changing components data
  - Checking if components exists on an entity
  - Accessing entities by `EntityID` struct
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
- Basic TriInspector support (Add `TRI_INSPECTOR` define to enable)

### Changed
- [Breaking] Rename `Morpeh/XCrew.Morpeh` -> `Scellecs.Morpeh`
- [Breaking] Globals `NextFrame` -> `Publish`
- [Breaking] `int Entity.ID` -> `EntityID Entity.ID`
- [Breaking] `Filter.Length` -> `Filter.GetLengthSlow()`
- [Breaking] Collections now cleanup after removing components (Reading ref components data is not allowed after removing components)
- [Breaking] `Filter` iteration is not order-deterministic
- [Breaking] `FastList` allows removing elements during iteration (current and next elements only)
- `Entity.Dispose()` is now public
- `Installer` can be inherited from
- License ownership changed to `Oleg Morozov, Igor Boyko`

### Fixed
- Extra filter update https://github.com/scellecs/morpeh/pull/129
- Systems disposed twice https://github.com/scellecs/morpeh/pull/128
- Entity is detected as alive when it's not https://github.com/scellecs/morpeh/pull/123
- World.Default takes 0-th index with null value if initialized outside of UnityRuntimeHelper https://github.com/scellecs/morpeh/pull/138
- Errors in Pure C# projects

### Removed
- [Breaking] `ComponentsBag` (replaced by `ComponentsCache`)