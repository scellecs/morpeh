# Morpeh [![License](https://img.shields.io/github/license/scellecs/morpeh?color=3750c1&style=flat-square)](LICENSE.md) [![Unity](https://img.shields.io/badge/Unity-2020.3+-2296F3.svg?color=3750c1&style=flat-square)](https://unity.com/) [![Version](https://img.shields.io/github/package-json/v/scellecs/morpeh?color=3750c1&style=flat-square&filename=Scellecs.Morpeh/package.json)](Scellecs.Morpeh/package.json)
🎲 **ECS Framework for Unity Game Engine and .Net Platform**  

* Simple Syntax.  
* Plug & Play Installation.  
* No code generation.  
* Structure-Based and Cache-Friendly.  

## 📖 Table of Contents
* [Introduction](#-introduction)
  * [Base concept of ECS pattern](#-base-concept-of-ecs-pattern)
  * [Advanced](#-advanced)
    * [Filter Extensions](#-filter-extensions)
    * [Filter Disposing](#-filter-disposing)
    * [Component Disposing](#-component-disposing)
    * [Stash size](#-stash-size)
* [Examples](#-examples)
* [Games](#-games)
* [License](#-license)
* [Contacts](#-contacts)

## 📖 Introduction
### 📘 Base concept of ECS pattern

#### 🔖 Entity
An identifier for components, which does not store any data but can be used to
access components.
It is a value type, and is trivially copyable. Underlying identifiers (IDs) are
reused, but each reused ID is guaranteed to have a new generation, making each new
Entity unique.

```c#
var healthStash = this.World.GetStash<HealthComponent>();
var entity = this.World.CreateEntity();

ref var addedHealthComponent  = ref healthStash.Add(entity);
ref var gottenHealthComponent = ref healthStash.Get(entity);

//if you remove the last entity component, it will be destroyed during the next world.Commit() call
bool removed = healthStash.Remove(entity);
healthStash.Set(entity, new HealthComponent {healthPoints = 100});

bool hasHealthComponent = healthStash.Has(entity);

var debugString = entity.ToString();

//remove entity
this.World.RemoveEntity(entity);

//check disposal
bool isDisposed = this.World.IsDisposed(entity);

//alternatively
bool has = this.World.Has(entity);
```


#### 🔖 Component
Components are types which include only data.  
In Morpeh components are value types for performance purposes.
```c#
public struct HealthComponent : IComponent {
    public int healthPoints;
}
```

#### 🔖 System

Types that process entities with a specific set of components.  
Entities are selected using a filter.

All systems are represented by interfaces.

```c#
public class HealthSystem : ISystem {
    public World World { get; set; }

    private Filter filter;
    private Stash<HealthComponent> healthStash;

    public void OnAwake() {
        this.filter = this.World.Filter.With<HealthComponent>().Build();
        this.healthStash = this.World.GetStash<HealthComponent>();
    }

    public void OnUpdate(float deltaTime) {
        foreach (var entity in this.filter) {
            ref var healthComponent = ref healthStash.Get(entity);
            healthComponent.healthPoints += 1;
        }
    }

    public void Dispose() {
    }
}
```

All systems types:
* `IInitializer` - have only OnAwake and Dispose methods, convenient for executing startup logic
* `ISystem` - main system that executes every frame in Update. Used for main game logic and data processing
* `IFixedSystem` - system that executes in FixedUpdate with fixed time step
* `ILateSystem` - system that executes in LateUpdate, after all Updates. Useful for logic that should run after main updates
* `ICleanupSystem` - system that executes after ILateUpdateSystem. Designed for cleanup operations, resetting states, and handling end-of-frame tasks

Beware that ScriptableObject-based systems do still exist in 2024 version, but they are deprecated and will be removed in the future.

#### 🔖 SystemsGroup

The type that contains the systems. Consider them as a "feature" to group the systems by their common purpose.

```c#
var newWorld = World.Create();

var newSystem = new HealthSystem();
var newInitializer = new HealthInitializer();

var systemsGroup = newWorld.CreateSystemsGroup();
systemsGroup.AddSystem(newSystem);
systemsGroup.AddInitializer(newInitializer);

//it is a bad practice to turn systems off and on, but sometimes it is very necessary for debugging
systemsGroup.DisableSystem(newSystem);
systemsGroup.EnableSystem(newSystem);

systemsGroup.RemoveSystem(newSystem);
systemsGroup.RemoveInitializer(newInitializer);

newWorld.AddSystemsGroup(order: 0, systemsGroup);
newWorld.RemoveSystemsGroup(systemsGroup);
```

#### 🔖 World

A type that contains entities, components stashes, systems and root filter.
```c#
var newWorld = World.Create();
//a variable that specifies whether the world should be updated automatically by the game engine.
//if set to false, then you can update the world manually.
//and can also be used for game pauses by changing the value of this variable.
newWorld.UpdateByUnity = true;

var newEntity = newWorld.CreateEntity();
newWorld.RemoveEntity(newEntity);

var systemsGroup = newWorld.CreateSystemsGroup();
systemsGroup.AddSystem(new HealthSystem());

newWorld.AddSystemsGroup(order: 0, systemsGroup);
newWorld.RemoveSystemsGroup(systemsGroup);

var filter = newWorld.Filter.With<HealthComponent>();

var healthCache = newWorld.GetStash<HealthComponent>();
var reflectionHealthCache = newWorld.GetReflectionStash(typeof(HealthComponent));

//manually world updates
newWorld.Update(Time.deltaTime);
newWorld.FixedUpdate(Time.fixedDeltaTime);
newWorld.LateUpdate(Time.deltaTime);
newWorld.CleanupUpdate(Time.deltaTime);

//apply all entity changes, filters will be updated.
//automatically invoked between systems
newWorld.Commit();
```

#### 🔖 Filter

A type that allows filtering entities constrained by conditions With and/or Without.  
You can chain them in any order and quantity.  
Call `Build()` to finalize the filter for further use.
```c#
var filter = this.World.Filter.With<HealthComponent>()
                              .With<BooComponent>()
                              .Without<DummyComponent>()
                              .Build();

var firstEntityOrException = filter.First();
var firstEntityOrNull = filter.FirstOrDefault();

bool filterIsEmpty = filter.IsEmpty();
bool filterIsNotEmpty = filter.IsNotEmpty();
int filterLengthCalculatedOnCall = filter.GetLengthSlow();
```

#### 🔖 Stash

A type that stores components data.

```c#
var healthStash = this.World.GetStash<HealthComponent>();
var entity = this.World.CreateEntity();

ref var addedHealthComponent  = ref healthStash.Add(entity);
ref var gottenHealthComponent = ref healthStash.Get(entity);

bool removed = healthStash.Remove(entity);

healthStash.Set(entity, new HealthComponent {healthPoints = 100});

bool hasHealthComponent = healthStash.Has(entity);

//delete all HealthComponent from the world (affects all entities)
healthStash.RemoveAll();

bool healthStashIsEmpty = healthStash.IsEmpty();
bool healthStashIsNotEmpty = healthStash.IsNotEmpty();

var newEntity = this.World.CreateEntity();
//transfers a component from one entity to another
healthStash.Migrate(from: entity, to: newEntity);

//not a generic variation of stash, so we can only do a limited set of operations
var reflectionHealthCache = newWorld.GetReflectionStash(typeof(HealthComponent));

//set default(HealthComponent) to entity
reflectionHealthCache.Set(entity);

bool removed = reflectionHealthCache.Remove(entity);

bool hasHealthComponent = reflectionHealthCache.Has(entity);
```

---


### 📖 Advanced

#### 🧩 Filter Extensions

Filter extensions are a way to reuse queries or their parts.
Let's look at an example:  

Create a struct and implement the IFilterExtension interface.

```c#  
public struct SomeExtension : IFilterExtension {
    public FilterBuilder Extend(FilterBuilder rootFilter) => rootFilter.With<Translation>().With<Rotation>();
}
```

The next step is to call the Extend method in any order when requesting a filter.  
The Extend method continues query.

```c#  
private Filter filter;

public void OnAwake() {
    this.filter = this.World.Filter.With<TestA>()
                                   .Extend<SomeExtension>()
                                   .With<TestC>()
                                   .Build();
}
```

#### 🧹 Filter Disposing
`Filter.Dispose` allows you to completely remove the filter from the world, as if it never existed there.

> [!IMPORTANT]
> `Filter.Dispose` removes all filter instances across all systems where it was used, not just the instance on which `Dispose` was called.

#### 🧹 Component Disposing

> [!IMPORTANT]  
> Make sure you don't have the `MORPEH_DISABLE_COMPONENT_DISPOSE` define enabled.  

Sometimes it becomes necessary to clear component values.
For this, it is enough that a component implements `IDisposable`. For example:

```c#  
public struct PlayerView : IComponent, IDisposable {
    public GameObject value;
    
    public void Dispose() {
        Object.Destroy(value);
    }
}
```

An initializer or a system needs to mark the stash as disposable. For example:

```c# 
public class PlayerViewDisposeInitializer : IInitializer {
    public void OnAwake() {
        this.World.GetStash<PlayerView>().AsDisposable();
    }
    
    public void Dispose() {
    }
}
```

or

```c# 
public class PlayerViewSystem : ISystem {
    public void OnAwake() {
        this.World.GetStash<PlayerView>().AsDisposable();
    }
    
    public void OnUpdate(float deltaTime) {
        ...
    }
    
    public void Dispose() {
    }
}
```

Now, when the component is removed from an entity, the `Dispose()` method will be called on the `PlayerView` component.  

#### 📏 Stash size

If you know the expected number of components in a stash, you have the option to set a base size to prevent resizing and avoid unnecessary allocations.

```c#
ComponentId<T>.StashSize = 1024;
```

This value is not tied to a specific ``World``, so it needs to be set before starting ECS, so that all newly created stashes of this type in any ``World`` have the specified capacity.

---

## 🔌 Plugins

* [**Morpeh Helpers**](https://github.com/SH42913/morpeh.helpers)
* [**Morpeh.Events**](https://github.com/codewriter-packages/Morpeh.Events)
* [**Morpeh.SystemStateProcessor**](https://github.com/codewriter-packages/Morpeh.SystemStateProcessor)
* [**Morpeh.Queries**](https://github.com/actionk/Morpeh.Queries)
* [**Morpeh.SourceGenerator**](https://github.com/kandreyc/Scellecs.Morpeh.SourceGenerator)
* [**PlayerLoopAPI Runner Morpeh plugin**](https://github.com/skelitheprogrammer/PlayerLoopCustomizationAPI.Runner.Morpeh-Plugin)

---

## 📚 Examples

* [**Tanks**](https://github.com/scellecs/morpeh.examples.tanks) by *SH42913*
* [**Ping Pong**](https://github.com/scellecs/morpeh.examples.pong) by *SH42913*
* [**Flappy Bird**](https://github.com/R1nge/MorpehECS_FlappyBird) by *R1nge*

---

## 🔥 Games

* **One State RP - Life Simulator** by *Chillbase*  
  [Android](https://play.google.com/store/apps/details?id=com.Chillgaming.oneState) [iOS](https://apps.apple.com/us/app/one-state-rp-online/id1597760047)


* **Zombie City** by *GreenButtonGames*  
  [Android](https://play.google.com/store/apps/details?id=com.greenbuttongames.zombiecity) [iOS](https://apps.apple.com/us/app/zombie-city-master/id1543420906)


* **Fish Idle** by *GreenButtonGames*  
  [Android](https://play.google.com/store/apps/details?id=com.greenbuttongames.FishIdle) [iOS](https://apps.apple.com/us/app/fish-idle-hooked-tycoon/id1534396279)


* **Stickman of Wars: RPG Shooters** by *Multicast Games*  
  [Android](https://play.google.com/store/apps/details?id=com.multicastgames.sow3) [iOS](https://apps.apple.com/us/app/stickman-of-wars-rpg-shooters/id1620422798)


* **Cowravaneer** by *FESUNENKO GAMES*  
  [Android](https://play.google.com/store/apps/details?id=com.FesunenkoGames.Cowravaneer)

---

## 📘 License

📄 [MIT License](LICENSE.md)

---

## 💬 Contacts

✉️ Telegram: [olegmrzv](https://t.me/olegmrzv)  
📧 E-Mail: [benjminmoore@gmail.com](mailto:benjminmoore@gmail.com)  
👥 Telegram Community RU: [Morpeh ECS Development](https://t.me/morpeh_development_chat)

