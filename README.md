<p align="center">
    <img src="Unity/Utils/Editor/Resources/logo.png" width="260" height="260" alt="Morpeh">
</p>

# Morpeh
🎲 **ECS Framework for Unity Game Engine.**  
   
Adapted for the rapid development mobile games like:  
&nbsp;&nbsp;&nbsp;&nbsp;📕 Hyper Casual  
&nbsp;&nbsp;&nbsp;&nbsp;📗 Idlers  
&nbsp;&nbsp;&nbsp;&nbsp;📘 Arcades  
&nbsp;&nbsp;&nbsp;&nbsp;📚 Other genres  
  
Features:  
* Simple Syntax.  
* Plug & Play Installation.  
* No code generation.  
* Structure-Based and Cache-Friendly.  
* Built-in Events and Reactive Variables.  
* Single-threaded.  

## 📖 Table of Contents

* [How To Install](#-how-to-install)
* [Introduction](#-introduction)
  * [Base concept of ECS pattern](#-base-concept-of-ecs-pattern)
  * [Getting Start](#-getting-start)
  * [Advanced](#-advanced)
* [License](#-license)
* [Contacts](#-contacts)


## 📖 How To Install

> 💡 **IMPORTANT**  
> Check if you have Git LFS installed.  
> Minimal Unity Version is 2019.4

<details>
    <summary>Open Package Manager and add Morpeh URL.  </summary>
    
![installation_step1.png](Gifs~/installation_step1.png)  
![installation_step2.png](Gifs~/installation_step2.png)  
</details>
  
&nbsp;&nbsp;&nbsp;&nbsp;⭐ Master: https://github.com/X-Crew/Morpeh.git  
&nbsp;&nbsp;&nbsp;&nbsp;🚧 Dev:  https://github.com/X-Crew/Morpeh.git#develop  
&nbsp;&nbsp;&nbsp;&nbsp;🏷️ Tag:  https://github.com/X-Crew/Morpeh.git#2021.1.0  

<details>
    <summary>You can update Morpeh by Discover Window. Select Help/Morpeh Discover menu.   </summary>
    
![update_morpeh.png](Gifs~/update_morpeh.png)  
</details>


## 📖 Introduction
### 📘 Base concept of ECS pattern

#### 🔖 Entity
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

#### 🔖 World
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

### 📘 Getting Started
> 💡 **IMPORTANT**  
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
> 💡 Don't care about attributes.  
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
 
> 💡 Icon U means UpdateSystem. Also you can create FixedUpdateSystem and LateUpdateSystem.  
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
> 💡 You can chain filters by two operators `With<>` and `Without<>`.  
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
> 💡 Don't forget about `ref` operator.  
> Components are struct and if you want to change them directly, then you must use reference operator.

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

### 📖 Advanced

#### 📘 Event system. Singletons and Globals Assets.  
There is an execution order in the ECS pattern, so we cannot use standard delegates or events, they will break it.  

ECS uses the concept of a deferred call, or the events are data.  
In the simplest cases, events are used as entities with empty components called tags or markers.  
That is, in order to notify someone about the event, you need to create an entity and add an empty component to it.  
Another system creates a filter for this component, and if there are entities, we believe that the event is published.  

In general, this is a working approach, but there are several problems.  
Firstly, these tags overwhelm the project with their types, if you wrote a message bus, then you understand what I mean.  
Each event in the game has its own unique type and there is not enough imagination to give everyone a name.  
Secondly, it’s uncomfortable working with these events from MonoBehaviours, UI, Visual Scripting Frameworks (Playmaker, Bolt, etc.)  

As a solution to this problem, global assets were created.  

🔖 **Singleton** is a simple ScriptableObject that is associated with one specific entity.  
It is usually used to add dynamic components to one entity without using filters.  

🔖 **GlobalEvent** is a Singleton, which has the functionality of publishing events to the world by adding a tag to its entity.  
It has 4 main methods for working with it:  
1) Publish (arg) - publish within the frame, and all downstream systems will see this.  
2) NextFrame (arg) - publish in the next frame, all systems will see this.  
3) IsPublished - did anyone publish this event  
4) BatchedChanges - a data stack where publication arguments are added.  

🔖 **GlobalVariable** is a GlobalEvent that stores the start value and the last value after the changes.  
It also has the functionality of saving and loading data from PlayerPrefs.  

You can create globals by context menu `Create/ECS/Globals/` in Project Window.  
You can declare globals in any systems, components and scripts and set it by Inspector Window, for example:  
```c#  
public sealed class HealthSystem : UpdateSystem {
    public GlobalEvent myEvent;
    ...
}
```

And check their publication with:  
```c#  
public sealed class HealthSystem : UpdateSystem {
    public GlobalEvent myEvent;
    ...
    public override void OnUpdate(float deltaTime) {
        if (myEvent.IsPublished) {
            Debug.Log("Event is published");
        }
    }
}
```

And there is also a variation with checking for null:  
```c#  
public sealed class HealthSystem : UpdateSystem {
    public GlobalEvent myEvent;
    ...
    public override void OnUpdate(float deltaTime) {
        if (myEvent) {
            Debug.Log("Event is not null and is published");
        }
    }
}
```

---

## 📘 License

📄 [MIT License](LICENSE)

## 💬 Contacts

✉️ Telegram: [benjminmoore](https://t.me/benjminmoore)  
📧 E-Mail: [benjminmoore@gmail.com](mailto:benjminmoore@gmail.com)
