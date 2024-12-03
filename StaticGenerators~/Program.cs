using StaticGenerators;

new PinnedArrayGenerator(config => {
    config.ClassName          = "IntPinnedArray";
    config.DataType           = "int";
    config.Namespace          = "Scellecs.Morpeh.Collections";
    config.GenerateEnumerator = true;
}).Run("Scellecs.Morpeh/Core/Collections/Generated/IntPinnedArray.cs");

new PinnedArrayGenerator(config => {
    config.ClassName = "EntityPinnedArray";
    config.DataType  = "Entity";
    config.Namespace = "Scellecs.Morpeh.Collections";
}).Run("Scellecs.Morpeh/Core/Collections/Generated/EntityPinnedArray.cs");

new PinnedArrayGenerator(config => {
    config.ClassName = "IntHashMapSlotPinnedArray";
    config.DataType  = "IntHashMapSlot";
    config.Namespace = "Scellecs.Morpeh.Collections";
}).Run("Scellecs.Morpeh/Core/Collections/Generated/IntHashMapSlotPinnedArray.cs");
