# 🚀 Migration Guide  

## Breaking changes  
* Rename Morpeh/XCrew.Morpeh -> Scellecs.Morpeh  
* Globals NextFrame -> Publish  
* int Entity.ID -> EntityID Entity.ID

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
