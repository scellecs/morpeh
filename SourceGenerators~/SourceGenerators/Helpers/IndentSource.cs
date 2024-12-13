namespace SourceGenerators.Helpers {
    using System;

    public class IndentSource {
        [ThreadStatic] private static IndentSource? source;
        
        public static IndentSource GetThreadSingleton() {
            if (source == null) {
                source = new IndentSource();
            } else {
                source.value = 0;
            }

            return source;
        }
        
        public           int value;
        private readonly int step;

        public IndentSource(int value = 0, int step = 4) {
            this.value = value;
            this.step  = step;
        }

        public void Right() => this.value += this.step;
        public void Left()  => this.value -= this.step;
        
        public IndentScope Scope() {
            this.Right();
            return new IndentScope {source = this};
        }

        public struct IndentScope : IDisposable {
            public IndentSource source;

            public void Dispose() {
                this.source.Left();
            }
        }
    }
}