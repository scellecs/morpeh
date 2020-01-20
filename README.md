# Red
<img align="right" width="160px" height="160px" src="Assets/Plugins/Red/Editor/Resources/logo.png">

Toolchain based on Reactive Extensions for Unity Game Engine.
  
Extends the capabilities of [UniRx](https://github.com/neuecc/UniRx), allowing you to conveniently resolve dependencies and implement loose coupling between components or modules of your architecture.  

There are two ways to build architecture.

Simple way:
- **Container** is Service Locator, which can solve global dependencies and make it possible to abandon static classes, singletons.  
- **Contract** contains all reactive streams, and helps to make it easy to build MVP for UI and further interaction with business logic. 

Pure way:
- **DI** manual injections or use Zenject. 
- **IntegrationRoot** integrates all systems with reactive streams. 
- **Systems** ECS-like business logic. 

## How to install

### Simple Installation
- Add to your project manifiest by path UnityProject/Packages/manifiest.json `scopedRegistries` like this
```json
{
  "dependencies": {
  },
  "scopedRegistries": [
    {
      "name": "Unity",
      "url": "https://packages.unity.com",
      "scopes": [
        "com.unity"
      ]
    },
    {
      "name": "XCrew",
      "url": "http://xcrew.dev",
      "scopes": [
        "com"
      ]
    }
  ]
}
```
- Open window Package Manager in Unity and install Red
  

### Manual Installation 
- Go to [Releases](https://github.com/X-Crew/Red/releases) and download latest package.
- If your Unity version is lower than 2018.3, open Player Settings in Unity project and change Scripting Runtime Version to `.Net 4.x Equivalent` and install CSharpLatest.unitypackage.
- Import UniRx from [AssetStore](https://assetstore.unity.com/packages/tools/integration/unirx-reactive-extensions-for-unity-17276).
- Import Red.

> Note:  
> Sometimes the solution is not updated to latest C#.  
> Close your IDE and delete all .csproj and .sln from main folder.  
> Now open some .cs file. Solution will be recreated with the correct version.  


## Introduction

First of all, you should be well versed in Rx and specifically in [UniRx](https://github.com/neuecc/UniRx).  
Learn this in detail if you still do not know it.  

## Code of Conduct

[Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md)

## License

[MIT License](LICENSE)
