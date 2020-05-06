# Morpeh
<img align="right" width="260px" height="260px" src="Unity/Utils/Editor/Resources/logo.png">

ECS Framework for Unity Game Engine.  

* Simple Syntax.
* Simple Integration with Unity Engine.
* No code generation and any C# Reflection in Runtime.
* Structure-based and Cache-friendly.
* Reactive and Fast Filters.
* Built-in Events and Reactive Variables.
* Single-threaded.

## Table of Contents

* [Introduction](#introduction)
  * [Base concept of ECS pattern](#base-concept-of-ecs-pattern)
  * [Getting Start](#getting-start)
  * [Advanced](#advanced)
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

---

### Getting Started
> **IMPORTANT**  
> For a better user experience, we strongly recommend having Odin Inspector and FindReferences2 in the project.  
> All GIFs are hidden under spoilers.
  
<details>
    <summary>After installation import ScriptTemplates and Restart Unity.  </summary>
    
![import_script_templates.gif](Gifs~/import_script_templates.gif)
</details>

Let's create our first component and open it.  
<details>
    <summary>Right click in project window and select <code>Create/ECS/Component</code>.  </summary>

![create_component.gif](Gifs~/create_component.gif)
</details>


After it, you will see something like this.
```c#  
using Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[System.Serializable]
public struct HealthComponent : IComponent {
}
```
> Don't care about attributes.  
> Il2CppSetOption attribute can give you better performance.  

Add health points field to the component.  

```c#  
public struct HealthComponent : IComponent {
    public int healthPoints;
}
```

It is okay.  
 
Now let's create first system.  
<details>
    <summary>Right click in project window and select <code>Create/ECS/System</code>.  </summary>

![create_system.gif](Gifs~/create_system.gif)
</details>
 
> Icon U means UpdateSystem. Also you can create FixedUpdateSystem and LateUpdateSystem.  
> They are similar as MonoBehaviour's Update, FixedUpdate, LateUpdate.

System looks like this.
```c#  
using Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(HealthSystem))]
public sealed class HealthSystem : UpdateSystem {
    public override void OnAwake() {
    }

    public override void OnUpdate(float deltaTime) {
    }
}
```

We have to add a filter to find all the entities with `HealthComponent`.
```c#  
public sealed class HealthSystem : UpdateSystem {
    private Filter filter;
    
    public override void OnAwake() {
        this.filter = this.World.Filter.With<HealthComponent>();
    }

    public override void OnUpdate(float deltaTime) {
    }
}
```
> You can chain filters by two operators `With<>` and `Without<>`.  
> For example `this.World.Filter.With<FooComponent>().With<BarComponent>().Without<BeeComponent>();`
  
Now we can iterate all needed entities.
```c#  
public sealed class HealthSystem : UpdateSystem {
    private Filter filter;
    
    public override void OnAwake() {
        this.filter = this.World.Filter.With<HealthComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        foreach (var entity in this.filter) {
            ref var healthComponent = ref entity.GetComponent<HealthComponent>();
            Debug.Log(healthComponent.healthPoints);
        }
    }
}
```
> Don't forget about `ref` operator.  
> Components are struct and if you wanna change them directly, then you must use reference operator.

For high performance, you can do cached sampling.  
No need to do GetComponent from entity every time.  
But we will focus on a simplified version, because even in this version GetComponent is very fast.

```c#  
public sealed class HealthSystem : UpdateSystem {
    private Filter filter;
    
    public override void OnAwake() {
        this.filter = this.World.Filter.With<HealthComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        var healthBag = this.filter.Select<HealthComponent>();

        for (int i = 0, length = this.filter.Length; i < length; i++) {
            ref var healthComponent = ref healthBag.GetComponent(i);
            Debug.Log(healthComponent.healthPoints);
        }
    }
}
```

Let's create ScriptableObject for HealthSystem.  
This will allow the system to have its inspector and we can refer to it in the scene.  
<details>
    <summary>Right click in project window and select <code>Create/ECS/Systems/HealthSystem</code>.  </summary>

![create_system_scriptableobject.gif](Gifs~/create_system_scriptableobject.gif)
</details>

Next step: create `Installer` on the scene.  
This will help us choose which systems should work and in which order.  

<details>
    <summary>Right click in hierarchy window and select <code>ECS/Installer</code>.  </summary>

![create_installer.gif](Gifs~/create_installer.gif)
</details>

<details>
    <summary>Add system to the installer and run project.  </summary>
    
![add_system_to_installer.gif](Gifs~/add_system_to_installer.gif)
</details>

Nothing happened because we did not create our entities.  
I will show the creation of entities directly related to GameObject, because to create them from the code it is enough to write `world.CreateEntity()`.  
To do this, we need a provider that associates GameObject with an entity.  
  
Create a new provider.  

<details>
    <summary>Right click in project window and select <code>Create/ECS/Provider</code>.  </summary>

![create_provider.gif](Gifs~/create_provider.gif)
</details>

```c#  
using Morpeh;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class HealthProvider : MonoProvider<{YOUR_COMPONENT}> {
}
```

We need to specify a component for the provider.
```c#  
public sealed class HealthProvider : MonoProvider<HealthComponent> {
}
```

<details>
    <summary>Create new GameObject and add <code>HealthProvider</code>.  </summary>
    
![add_provider.gif](Gifs~/add_provider.gif)
</details>

Now press the play button, and you will see Debug.Log with healthPoints.  
Nice!  

### Advanced

Globals

---

## How To Install

### Git Installation
#### For Unity 2019+

Add to your project manifiest by path `UnityProject/Packages/manifiest.json` next line:
```json
{
  "dependencies": {
     "com.xcrew.morpeh": "https://github.com/X-Crew/Morpeh.git"
  }
}
```
If you need develop branch add this line instead:
```json
{
  "dependencies": {
     "com.xcrew.morpeh": "https://github.com/X-Crew/Morpeh.git#develop"
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