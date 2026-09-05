namespace SourceGenerators.Fiddle;

using System;
using Scellecs.Morpeh;
using Test.Namespace;

[EcsSystem(alwaysEnabled: true)]
[IncludeStash(typeof(TagComponent))]
[IncludeStash(typeof(GenericComponent<int>))]
[IncludeStash(typeof(GenericComponent<GenericComponent<int>>))]
[IncludeStash(typeof(GlobalNamespaceComponent))]
[IncludeStash(typeof(DisposableComponent))]
public partial class MinimalSystem {
    public void OnUpdate(float deltaTime) {
        throw new NotImplementedException();
    }
}