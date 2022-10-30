# 🚀 Migration Guide  

## Breaking changes  
* Переименовано: Morpeh/XCrew.Morpeh -> Scellecs.Morpeh  
* Globals NextFrame -> Publish  
* int Entity.ID -> EntityID Entity.ID
* Filter.Length -> Filter.GetLengthSlow()

## New API  
* IValidatable + IValidatableWithGameObject  
* .AsNative() для:
  * Archetype (NativeArchetype)
  * ComponentsCache (NativeCache)
  * FastList (NativeFastList)
  * IntFastList (NativeIntFastList)
  * Filter (NativeFilter)
  * IntHashMap (NativeIntHashMap)
  * World (NativeWorld)
* Filter.IsEmpty() - для проверки что в фильтре нет ни одного Entity
* IMorpehLogger - интерфейс для кастомных логгеров (Console.WriteLine для окружений кроме Unity по дефолту)
* MORPEH_PROFILING - дефайн для автоматического профайлинга всех систем
* World.TryGetEntity(EntityId entityId, out Entity entity) - возвращает true и энтити если он существует, false и default(Entity) в противном случае