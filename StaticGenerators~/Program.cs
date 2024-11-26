// PinnedArray<Entity> -> PinnedArrayEntity

using StaticGenerators;

new PinnedArrayGenerator(config => {
    config.ClassName          = "IntPinnedArray";
    config.DataType           = "int";
    config.Namespace          = "Scellecs.Morpeh.Collections";
    config.ExtraUsings        = null;
    config.GenerateEnumerator = true;
}).Run("Scellecs.Morpeh/Core/Collections/Unsafe/IntPinnedArray.cs");

/*
new PinnedArrayGenerator(config => {
    config.ClassName          = "PinnedArrayEntity";
    config.DataType           = "Entity";
    config.Namespace          = "Scellecs.Morpeh.Collections";
    config.ExtraUsings        = null;
    config.GenerateEnumerator = false;
}).Run("Scellecs.Morpeh/Core/Archetypes/PinnedArrayEntity.cs");
*/