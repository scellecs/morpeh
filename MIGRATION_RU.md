# 🚀 Migration Guide  

## Breaking changes  
* Rename Morpeh/XCrew.Morpeh -> Scellecs.Morpeh  
* Globals NextFrame -> Publish  
* int Entity.ID -> EntityID Entity.ID
* Filter.Length -> Filter.GetLengthSlow()

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
* Filter.IsEmpty()
* IMorpehLogger interface for custom loggers (Console.WriteLine for non-Unity environments by default)