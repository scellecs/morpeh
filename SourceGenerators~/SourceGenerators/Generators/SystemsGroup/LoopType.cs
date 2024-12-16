namespace SourceGenerators.Generators.SystemsGroup {
    public enum LoopType {
        EarlyNetworkUpdate = 0,
        FixedUpdate        = 1,
        UpdateEverySec     = 2,
        NetworkUpdate      = 3,
        Update             = 4,
        LateUpdate         = 5,
        CleanupUpdate      = 6,
        LateNetworkUpdate  = 7,
    }
}