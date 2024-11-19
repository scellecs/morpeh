#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
namespace Scellecs.Morpeh.Utils.Editor {
    internal sealed class Trie {
        internal struct Node {
            internal int[] children; //TODO To Dictionary?
            internal List<int> endOfWordsIndices;

            internal static Node Create() {
                var node = new Node() {
                    children = new int[ALPHABET_SIZE],
                    endOfWordsIndices = new List<int>(),
                };

                Array.Fill(node.children, NULL);
                return node;
            }
        }

        private const int ALPHABET_SIZE = 128;
        private const int NULL = -1;

        internal readonly List<Node> nodes;

        internal Trie() {
            this.nodes = new List<Node> { Node.Create() };
        }

        internal void Insert(ReadOnlySpan<char> word, int index) {
            var currentNodeIndex = 0;

            foreach (var ch in word) {
                var charIndex = ch;
                var nextNodeIndex = this.nodes[currentNodeIndex].children[charIndex];

                if (nextNodeIndex == NULL) {
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
                var charIndex = ch;
                if (charIndex > ALPHABET_SIZE) {
                    return result;
                }

                var nextNodeIndex = this.nodes[currentNodeIndex].children[charIndex];
                if (nextNodeIndex == NULL) {
                    return result;
                }
                currentNodeIndex = nextNodeIndex;
            }

            CollectIndices(currentNodeIndex, result);
            return result;
        }

        private void CollectIndices(int nodeIndex, List<int> result) {
            var node = this.nodes[nodeIndex];
            if (node.endOfWordsIndices.Any()) {
                result.AddRange(node.endOfWordsIndices);
            }

            for (int i = 0; i < node.children.Length; i++) {
                var childIndex = node.children[i];
                if (childIndex != NULL) {
                    CollectIndices(childIndex, result);
                }
            }
        }
    }
}
#endif