using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using AsyncHelpers.Helpers;

using Nito.AsyncEx;

namespace AsyncHelpers.Synchronization
{
    /// <summary>
    /// Directed acyclic graph node that supports Read/Write async locks on graph.
    /// Class in progress...
    /// </summary>
    public class RWAsyncDAGVertex
    {
        public async Task<IDisposable> GetWriteLockAsync(CancellationToken ct)
        {
            var linkedReadLocks = await GetLinkedReadLocksAsync(ct).ConfigureAwait(false);

            var readLock = await _writeLockGuard.LockAsync(ct).ConfigureAwait(false);

            await _readLockGuard.WaitAsync(ct);

            return new ActionDispose(() =>
            {
                readLock.Dispose();
                foreach (var readLock in linkedReadLocks)
                {
                    readLock.Dispose();
                }
            });

        }

        private async Task<IEnumerable<IDisposable>> GetLinkedReadLocksAsync(CancellationToken ct)
        {
            //Get all nods from graph
            //Execute Read lock on every item
            //Return read handles
            await Task.Yield();
            throw new NotImplementedException();
        }

        private async Task<IDisposable> GetReadLockInternalAsync(CancellationToken ct)
        {
            using var _ = await _writeLockGuard.LockAsync(ct).ConfigureAwait(false);
            _readLockGuard.AddCount(1);
            return new ActionDispose(_readLockGuard.Signal);
        }

        public async Task<IDisposable> GetReadLockAsync(CancellationToken ct)
        {
            var readHandle = await GetReadLockInternalAsync(ct).ConfigureAwait(false);

            var linkedReadLocks = await GetLinkedReadLocksAsync(ct).ConfigureAwait(false);

            return new ActionDispose(() =>
            {
                readHandle.Dispose();
                foreach (var readLock in linkedReadLocks)
                {
                    readLock.Dispose();
                }
            });
        }

        public void AddEdgesTo(params RWAsyncDAGVertex[] reachableNodes)
        {
            if (reachableNodes == null)
                throw new ArgumentNullException(nameof(reachableNodes));
            _reachableNodes.AddRange(reachableNodes);
        }

        /// <summary>
        /// Validates directed cycles.
        /// </summary>
        public void ValidateGraph()
        {
            var previousNodes = new HashSet<RWAsyncDAGVertex>();

            bool DeepFirstLoopSearch([NotNull] RWAsyncDAGVertex node)
            {
                previousNodes.Add(node);

                bool hasLoops = false;

                foreach (var child in _reachableNodes)
                {
                    if (previousNodes.Contains(child) || DeepFirstLoopSearch(child))
                    {
                        hasLoops = true;
                        break;
                    }
                }

                previousNodes.Remove(node);

                return hasLoops;
            }

            var hasLoops = DeepFirstLoopSearch(this);
            if (hasLoops)
                throw new InvalidOperationException("Graph contains loops");
        }

        private readonly List<RWAsyncDAGVertex> _reachableNodes = new();
        private readonly AsyncLock _writeLockGuard = new();
        private readonly AsyncCountdownEvent _readLockGuard = new(0);
    }
}
