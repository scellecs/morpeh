#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;
using System.Collections.Generic;
using System.Linq;
namespace Scellecs.Morpeh.WorldBrowser {
    internal sealed class Trie {
        internal struct Node {
            internal Dictionary<int, int> children;
            internal List<int> endOfWordsIndices;

            internal static Node Create() {
                return new Node() {
                    children = new Dictionary<int, int>(),
                    endOfWordsIndices = new List<int>()
                };
            }
        }

        private readonly List<Node> nodes;

        internal Trie() {
            this.nodes = new List<Node> { Node.Create() };
        }

        internal void Insert(ReadOnlySpan<char> word, int index) {
            var currentNodeIndex = 0;

            foreach (var ch in word) {
                var charIndex = char.ToLower(ch);
                if (!this.nodes[currentNodeIndex].children.TryGetValue(charIndex, out var nextNodeIndex)) {
                    nextNodeIndex = this.nodes.Count;
                    this.nodes.Add(Node.Create());
                    this.nodes[currentNodeIndex].children[charIndex] = nextNodeIndex;
                }

                currentNodeIndex = nextNodeIndex;
            }

            this.nodes[currentNodeIndex].endOfWordsIndices.Add(index);
        }

        internal List<int> GetWordIndicesWithPrefix(ReadOnlySpan<char> prefix, List<int> result) {
            result.Clear();
            var currentNodeIndex = 0;

            foreach (var ch in prefix) {
                var charIndex = char.ToLower(ch);
                if (!this.nodes[currentNodeIndex].children.TryGetValue(charIndex, out var nextNodeIndex)) {
                    return result;
                }

                currentNodeIndex = nextNodeIndex;
            }

            this.CollectIndices(currentNodeIndex, result);
            return result;
        }

        private void CollectIndices(int nodeIndex, List<int> result) {
            var node = this.nodes[nodeIndex];
            if (node.endOfWordsIndices.Any()) {
                result.AddRange(node.endOfWordsIndices);
            }
            foreach (var childIndex in node.children.Values) {
                this.CollectIndices(childIndex, result);
            }
        }
    }
}
#endif