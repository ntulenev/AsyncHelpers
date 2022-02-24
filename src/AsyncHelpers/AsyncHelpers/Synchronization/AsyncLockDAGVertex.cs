using System.Diagnostics.CodeAnalysis;

using AsyncHelpers.Helpers;

using Nito.AsyncEx;

namespace AsyncHelpers.Synchronization
{
    /// <summary>
    /// Directed acyclic graph that supports async locks.
    /// </summary>
    public class AsyncLockDAGVertex
    {
        /// <summary>
        /// Gets async lock on current node.
        /// </summary>
        /// <param name="ct">Token to cancel.</param>
        /// <returns>Lock object.</returns>
        public async Task<IDisposable> GetLockAsync(CancellationToken ct)
        {
            var linkedReadLocks = await GetLinkedReadLocksAsync(ct).ConfigureAwait(false);

            var writeLock = await _writeLockGuard.LockAsync(ct).ConfigureAwait(false);

            await _readLockGuard.WaitAsync(ct);

            return CreateLockObject(writeLock, linkedReadLocks);
        }

        /// <summary>
        /// Add edges to reachable nodes.
        /// </summary>
        /// <param name="reachableNodes">Reachable nodes.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="reachableNodes"/> is null.</exception>
        /// <exception cref="ArgumentException">If <paramref name="reachableNodes"/> is empty.</exception>
        public void AddEdgesTo(params AsyncLockDAGVertex[] reachableNodes)
        {
            ArgumentNullException.ThrowIfNull(reachableNodes);

            if (!reachableNodes.Any())
            {
                throw new ArgumentException("Nodes collection is empty.", nameof(reachableNodes));
            }

            foreach (var node in reachableNodes)
            {
                if (node is null)
                {
                    throw new ArgumentException("Some of nodes is null.", nameof(reachableNodes));
                }

                if (!_reachableNodes.Add(node))
                {
                    throw new ArgumentException("Attempt to add node that already exists in edges.", nameof(reachableNodes));
                }
            }
        }

        /// <summary>
        /// Validates that graph has no loops and throws <see cref="InvalidOperationException"/> if any.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public void ValidateGraph()
        {
            var previousNodes = new HashSet<AsyncLockDAGVertex>();

            bool DeepFirstLoopSearch([NotNull] AsyncLockDAGVertex node)
            {
                previousNodes.Add(node);

                bool hasLoops = false;

                foreach (var child in node._reachableNodes)
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
            {
                throw new InvalidOperationException("Graph contains loops");
            }
        }

        private ActionDispose CreateLockObject(IDisposable mainLock, IEnumerable<IDisposable> dependendLocks)
        {
            return new ActionDispose(() =>
            {
                mainLock.Dispose();
                foreach (var dependentLock in dependendLocks)
                {
                    dependentLock.Dispose();
                }
            });
        }

        private async Task<IEnumerable<IDisposable>> GetLinkedReadLocksAsync(CancellationToken ct)
        {
            var navegatedNodes = new List<IDisposable>();

            async Task GetAllPathsAsync(AsyncLockDAGVertex root)
            {
                foreach (var edge in root._reachableNodes)
                {
                    navegatedNodes.Add(await edge.GetReadLockInternalAsync(ct).ConfigureAwait(false));
                    await GetAllPathsAsync(edge).ConfigureAwait(false);
                }
            }

            await GetAllPathsAsync(this).ConfigureAwait(false);

            return navegatedNodes;
        }

        private async Task<IDisposable> GetReadLockInternalAsync(CancellationToken ct)
        {
            using var _ = await _writeLockGuard.LockAsync(ct).ConfigureAwait(false);

            _readLockGuard.AddCount(1);

            return new ActionDispose(_readLockGuard.Signal);
        }

        private readonly HashSet<AsyncLockDAGVertex> _reachableNodes = new();
        private readonly AsyncLock _writeLockGuard = new();
        private readonly AsyncCountdownEvent _readLockGuard = new(0);
    }
}
