namespace StaticGenerators;

public static class Utilities {
    private static string _pathCache;
    
    public static string GetSolutionPath() {
        if (!string.IsNullOrEmpty(_pathCache)) {
            return _pathCache;
        }

        var current = Directory.GetCurrentDirectory();
            
        while (current != null) {
            var path = Path.Combine(current, "Scellecs.Morpeh.sln");
            
            if (File.Exists(path)) {
                _pathCache = current;
                break;
            }
                
            current = Directory.GetParent(current)?.FullName;
        }

        return _pathCache;
    }
    
    public static string GetPathInSolution(string path) {
        var solutionPath = GetSolutionPath();
        
        if (string.IsNullOrEmpty(solutionPath)) {
            throw new Exception("Solution path not found. What are you doing?");
        }
        
        return Path.Combine(solutionPath, path);
    }
}