# Morpeh
<img align="right" width="260px" height="260px" src="Unity/Utils/Editor/Resources/logo.png">

ECS Framework for Unity Game Engine.  

* Simple Syntax.
* Simple Integration with Unity Engine.
* No code generation and any C# Reflection in Runtime.
* Structure-based and Cache-friendly.
* Reactive and Fast Filters based on bitsets.
* Built-in Events and Reactive Variables.
* Single-threaded.

## Table of Contents

* [Introduction](#introduction)
  * [Base concept of ECS pattern](#base-concept-of-ecs-pattern)
  * [Performance](#performance)
* [How To Install](#how-to-install)
* [License](#license)
* [Contacts](#contacts)

## Introduction
### Base concept of ECS pattern

#### Entity
Container of components.  
Has a set of methods for add, get, set, remove components.

```c#
var entity = this.World.CreateEntity();

ref var addedHealthComponent  = ref entity.AddComponent<HealthComponent>();
ref var gottenHealthComponent = ref entity.GetComponent<HealthComponent>();

bool removed = entity.RemoveComponent<HealthComponent>();
entity.SetComponent(new HealthComponent {healthPoints = 100});

bool hasHealthComponent = entity.Has<HealthComponent>();
```


#### Component
Components are types which include only data.  
In Morpeh components are value types for performance purposes.
```c#
[System.Serializable]
public struct HealthComponent : IComponent {
    public int healthPoints;
}
```

#### System

Types that process entities with a specific set of components.  
Entities are selected using a filter.

```c#
public class HealthSystem : ISystem {
    public World World { get; set; }

    private Filter filter;

    public void OnAwake() {
        this.filter = this.World.Filter.With<HealthComponent>();
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

#### World
A type that contains entities, components caches, systems and root filter.
```c#
var newWorld = World.Create();

var newEntity = newWorld.CreateEntity();
newWorld.RemoveEntity(newEntity);

var systemsGroup = newWorld.CreateSystemsGroup();
systemsGroup.AddSystem(new HealthSystem());

newWorld.AddSystemsGroup(order: 0, systemsGroup);
newWorld.RemoveSystemsGroup(systemsGroup);

var filter = newWorld.Filter.With<HealthComponent>();
```

### Simple Start

## How To Install

### Unity Package Installation
- Add to your project manifiest by path `UnityProject/Packages/manifiest.json` these lines:
```json
{
  "dependencies": {
  },
  "scopedRegistries": [
    {
      "name": "XCrew",
      "url": "http://xcrew.dev",
      "scopes": [
        "com.xcrew"
      ]
    }
  ]
}
```
- Open window Package Manager in Unity and install Morpeh

### Git Installation
Add to your project manifiest by path `UnityProject/Packages/manifiest.json` next line:
```json
{
  "dependencies": {
     "com.xcrew.morpeh": "https://github.com/X-Crew/Morpeh.git"
  }
}
```

### Manual Installation 
- Go to [Releases](https://github.com/X-Crew/Morpeh/releases) and download latest package.
- Import Morpeh.

## License

[MIT License](LICENSE)

## Contacts

Telegram: [benjminmoore](https://t.me/benjminmoore)  
E-Mail: [benjminmoore@gmail.com](mailto:benjminmoore@gmail.com)