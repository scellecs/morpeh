# Morpeh
<img align="right" width="160px" height="160px" src="Unity/Utils/Editor/Resources/logo.png">

Fast and Simple ECS Framework for Unity Game Engine.

## How to install

### Unity Package
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

### Git way
Add to your project manifiest by path `UnityProject/Packages/manifiest.json` next line:
```json
{
  "dependencies": {
     "com.xcrew.morpeh": "https://github.com/X-Crew/Morpeh.git",
     ...
  }
}
```

### Manual Installation 
- Go to [Releases](https://github.com/X-Crew/Morpeh/releases) and download latest package.
- Import Morpeh.

## Introduction
TODO

## License

[MIT License](LICENSE)
