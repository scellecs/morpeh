namespace SourceGenerators.Fiddle {
    public enum LoopType {
        Update             = 0,
        FixedUpdate        = 1,
        LateUpdate         = 2,
        CleanupUpdate      = 3,
        
        EarlyNetworkUpdate = 4,
        LateNetworkUpdate  = 5,
        NetworkUpdate      = 6,
        
        UpdateEverySec     = 7,
        
        OnTick             = 8,
    }
}