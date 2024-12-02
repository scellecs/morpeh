#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

#if MORPEH_UNITY && MORPEH_BENCHMARK_COLLECTIONS && UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace Scellecs.Morpeh.Benchmarks.Collections.Editor {
    [CustomEditor(typeof(BenchmarkResultsExporter))]
    internal sealed class BenchmarkResultsExporterEditor : UnityEditor.Editor {
        public override VisualElement CreateInspectorGUI() {
            var root = new VisualElement();
            var defaultInspector = new VisualElement();
            InspectorElement.FillDefaultInspector(defaultInspector, this.serializedObject, this);

            var exportButton = new Button();
            exportButton.text = "Export Benchmark Results";
            exportButton.style.height = 20;
            exportButton.clicked += () =>  {
                var exporter = (BenchmarkResultsExporter)this.target;
                exporter.ExportPerformanceTestsResult();
            };

            root.Add(defaultInspector);
            root.Add(exportButton);
            
            return root;
        }
    }
}
#endif