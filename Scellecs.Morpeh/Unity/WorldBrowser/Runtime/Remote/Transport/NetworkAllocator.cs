#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;
using Unity.Collections;
using Scellecs.Morpeh.WorldBrowser.Utils;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;

namespace Scellecs.Morpeh.WorldBrowser.Remote {
    internal unsafe sealed class NetworkAllocator : IDisposable {
        private readonly object allocLock;
        private readonly DoubleRewindableAllocators* allocator;

        internal NetworkAllocator(object lockObject) {
            this.allocLock = lockObject;
            this.allocator = (DoubleRewindableAllocators*)UnsafeUtility.Malloc(sizeof(DoubleRewindableAllocators), JobsUtility.CacheLineSize, Allocator.Persistent);
            *this.allocator = new DoubleRewindableAllocators(Allocator.Persistent, 128 * 1024);
        }

        internal byte* Alloc(int length) {
            lock (this.allocLock) {
                return this.allocator->Allocator.Allocate<byte>(length);
            }
        }

        internal void Update() {
            lock (this.allocLock) {
                this.allocator->Update();
            }
        }

        public void Dispose() {
            this.allocator->Dispose();
            UnsafeUtility.Free(this.allocator, Allocator.Persistent);
        }
    }
}
#endif