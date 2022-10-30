# 🚀 Migration Guide  

## Breaking changes  
* All files and all namespaces have been replaced from `XCrew.Morpeh` to `Scellecs.Morpeh`.
* All Globals have lost method `NextFrame`. Use `Publish`, which works exactly as `NextFrame` used to. Current-frame Global events are not supported anymore, they are always deferred. The reason for this change is that most projects used both `Publish` + `NextFrame`, or `NextFrame` only.
* `Entity.ID` is not `int` anymore, but `EntityID` which contains `id` and `gen`. This is a mandatory change for Jobs/Burst support.
* `Filter.Length` is removed completely. It has been replaced with `Filter.GetLengthSlow()` and `Filter.IsEmpty()`. This change is made because recalculating Filter length was quite costly, and most of the time filters length was not needed for developers, making it a useless overhead. It's important to note that most length comparisons were used to check if filter is empty or not, so such checks using `IsEmpty()` are as fast as they used to be, but with a different API. This allowed us to make Morpeh much faster in projects which have a lot of different filters.

## New API  
* IValidatable + IValidatableWithGameObject  
* .AsNative() for:
  * Archetype (NativeArchetype)
  * ComponentsCache (NativeCache)
  * FastList (NativeFastList)
  * IntFastList (NativeIntFastList)
  * Filter (NativeFilter)
  * IntHashMap (NativeIntHashMap)
  * World (NativeWorld)
* IMorpehLogger - interface for custom loggers (Console.WriteLine for non-Unity environments by default)
* MORPEH_PROFILING - define for automatic systems profiling
* World.TryGetEntity(EntityId entityId, out Entity entity) - returns true and entity if it exists, false otherwise
* entity.Dispose() - now public
* Displaying multiple worlds in World Browser
* Basic TriInspector support
* Fast Collections