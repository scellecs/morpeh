namespace SourceGenerators.Helpers {
    using System;

    public class IndentSource {
        public int value;
        public int step;

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