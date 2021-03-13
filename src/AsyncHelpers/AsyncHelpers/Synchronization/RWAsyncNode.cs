﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AsyncHelpers.Helpers;

using Nito.AsyncEx;

namespace AsyncHelpers.Synchronization
{
    /// <summary>
    /// Class in progress....
    /// </summary>
    public class RWAsyncNode
    {
        public RWAsyncNode()
        {
        }

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

        public void AddLink(RWAsyncNode nodeToLink)
        {
            if (nodeToLink == null)
                throw new ArgumentNullException(nameof(nodeToLink));
            _linkedNodes.Add(nodeToLink);
            nodeToLink.AddLink(this);
        }

        public void ValidateGraph()
        {
            throw new NotImplementedException();
        }

        private readonly List<RWAsyncNode> _linkedNodes = new();
        private readonly AsyncLock _writeLockGuard = new();
        private readonly AsyncCountdownEvent _readLockGuard = new(0);
    }
}
