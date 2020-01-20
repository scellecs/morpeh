# Morpeh
<img align="right" width="300px" height="300px" src="Unity/Utils/Editor/Resources/logo.png">

ECS Framework for Unity Game Engine.  

* Simple Syntax.
* Simple Integration with Unity Engine.
* No code generation and any C# Reflection.
* Structure-based and Cache-friendly.
* Reactive and Fast Filters based on bitsets.
* Built-in Events and Reactive Variables.
* Single-threaded.

## Table of Contents

* [Introduction](#introduction)
  * [Code Example](#code-example)
  * [Performance](#performance)
* [How To Install](#how-to-install)
* [License](#license)
* [Contacts](#contacts)

## Introduction
TODO

## How To Install

### Unity Package Installation
- Add to your project manifiest by path `UnityProject/Packages/manifiest.json` these lines:
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