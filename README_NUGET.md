# Morpeh [![License](https://img.shields.io/github/license/scellecs/morpeh?color=3750c1&style=flat-square)](LICENSE.md) [![Unity](https://img.shields.io/badge/Unity-2020.3+-2296F3.svg?color=3750c1&style=flat-square)](https://unity.com/) [![Version](https://img.shields.io/github/package-json/v/scellecs/morpeh?color=3750c1&style=flat-square)](package.json)
🎲 **ECS Framework for Unity Game Engine and .Net Platform**  

* Simple Syntax.  
* Plug & Play Installation.  
* No code generation.  
* Structure-Based and Cache-Friendly.  

## 📖 Table of Contents
* [Introduction](#-introduction)
  * [Base concept of ECS pattern](#-base-concept-of-ecs-pattern)
  * [Advanced](#-advanced)
    * [Component Disposing](#-component-disposing)
* [Examples](#-examples)
* [Games](#-games)
* [License](#-license)
* [Contacts](#-contacts)

## 📖 Introduction
### 📘 Base concept of ECS pattern

#### 🔖 Entity
Container of components.  
Has a set of methods for add, get, set, remove components.  
It is reference type. Each entity is unique and not pooled. Only entity IDs are reused.

```c#
var entity = this.World.CreateEntity();

ref var addedHealthComponent  = ref entity.AddComponent<HealthComponent>();
ref var gottenHealthComponent = ref entity.GetComponent<HealthComponent>();

//if you remove last component on entity it will be destroyd on next world.Commit()
bool removed = entity.RemoveComponent<HealthComponent>();
entity.SetComponent(new HealthComponent {healthPoints = 100});

bool hasHealthComponent = entity.Has<HealthComponent>();

var newEntity = this.World.CreateEntity();
//after migration entity has no components, so it will be destroyd on next world.Commit()
entity.MigrateTo(newEntity);
//get string with entity ID
var debugString = entity.ToString();
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

All systems are represented by interfaces, but for convenience, there are ScriptableObject classes that make it easier to work with the inspector and `Installer`.  
Such classes are the default tool, but you can write pure classes that implement the interface, but then you need to use the `SystemsGroup` API instead of the `Installer`.

```c#
public class HealthSystem : ISystem {
    public World World { get; set; }

    private Filter filter;

    public void OnAwake() {
        this.filter = this.World.Filter.With<HealthComponent>().Build();
    }

    public void OnUpdate(float deltaTime) {
        foreach (var entity in this.filter) {
            ref var healthComponent = ref entity.GetComponent<HealthComponent>();
            healthComponent.healthPoints += 1;
        }
    }

    public void Dispose() {
    }
}
```

All systems types:
* `IInitializer` & `Initializer` - have only OnAwake and Dispose methods, convenient for executing startup logic
* `ISystem` & `UpdateSystem`
* `IFixedSystem` & `FixedUpdateSystem`
* `ILateSystem` & `LateUpdateSystem`
* `ICleanupSystem` & `CleanupSystem`

#### 🔖 SystemsGroup

The type that contains the systems.  
There is an `Installer` wrapper to work in the inspector, but if you want to control everything from code, you can use the systems group directly.

```c#
var newWorld = World.Create();

var newSystem = new HealthSystem();
var newInitializer = new HealthInitializer();

var systemsGroup = newWorld.CreateSystemsGroup();
systemsGroup.AddSystem(newSystem);
systemsGroup.AddInitializer(newInitializer);

//it is bad practice to turn systems off and on, but sometimes it is very necessary for debugging
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
A type that contains entities constrained by conditions With and/or Without.  
You can chain them in any order and quantity.  
After compose all constrains you should call Build() method.
```c#
var filter = this.World.Filter.With<HealthComponent>()
                              .With<BooComponent>()
                              .Without<DummyComponent>()
                              .Build();

var firstEntityOrException = filter.First();
var firstEntityOrNull = filter.FirstOrDefault();

bool filterIsEmpty = filter.IsEmpty();
int filterLengthCalculatedOnCall = filter.GetLengthSlow();

```

#### 🔖 Stash
A type that contains components.  
You can get components and do other operations directly from the stash, because entity methods look up the stash each time on call.  
However, such code is harder to read.
```c#
var healthStash = this.World.GetStash<HealthComponent>();
var entity = this.World.CreateEntity();

ref var addedHealthComponent  = ref healthStash.Add(entity);
ref var gottenHealthComponent = ref healthStash.Get(entity);

bool removed = healthStash.Remove(entity);

healthStash.Set(entity, new HealthComponent {healthPoints = 100});

bool hasHealthComponent = healthStash.Has(entity);

//delete all components that type from the world
healthStash.RemoveAll();

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

#### 🧹 Component Disposing

Sometimes it becomes necessary to clear component values.
For this, it is enough that the component implements `IDisposable`. For example:

```c#  
public struct PlayerView : IComponent, IDisposable {
    public GameObject value;
    
    public void Dispose() {
        Object.Destroy(value);
    }
}
```

The initializer or system needs to mark the stash as disposable. For example:

```c# 
public class PlayerViewDisposeInitializer : Initializer {
    public override void OnAwake() {
        this.World.GetStash<PlayerView>().AsDisposable();
    }
}
```

or

```c# 
public class PlayerViewSystem : UpdateSystem {
    public override void OnAwake() {
        this.World.GetStash<PlayerView>().AsDisposable();
    }
    
    public override void OnUpdate(float deltaTime) {
        ...
    }
}
```

Now, when the component is removed from the entity, the `Dispose()` method will be called on the `PlayerView` component.  

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

