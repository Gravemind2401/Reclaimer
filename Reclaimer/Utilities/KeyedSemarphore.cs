using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reclaimer.Utilities
{
    public class KeyedSemaphore
    {
        private readonly Dictionary<object, SemaphoreSlim> lookup = new Dictionary<object, SemaphoreSlim>();
        private readonly Dictionary<object, int> counter = new Dictionary<object, int>();

        public async Task<IDisposable> WaitAsync(object key)
        {
            await GetSemaphore(key).WaitAsync().ConfigureAwait(false);
            return new LockToken(key, Release);
        }

        private SemaphoreSlim GetSemaphore(object key)
        {
            lock (lookup)
            {
                if (lookup.ContainsKey(key))
                {
                    counter[key]++;
                    return lookup[key];
                }

                var semaphore = new SemaphoreSlim(1, 1);
                lookup.Add(key, semaphore);
                counter.Add(key, 1);
                return semaphore;
            }
        }

        public void Release(object key)
        {
            SemaphoreSlim semaphore;

            lock (lookup)
            {
                if (!lookup.ContainsKey(key))
                    return;

                semaphore = lookup[key];

                counter[key]--;
                if (counter[key] == 0)
                {
                    counter.Remove(key);
                    lookup.Remove(key);
                }
            }

            semaphore.Release();
        }

        private sealed class LockToken : IDisposable
        {
            private readonly object key;
            private readonly Action<object> disposeAction;

            public LockToken(object key, Action<object> disposeAction)
            {
                this.key = key;
                this.disposeAction = disposeAction;
            }

            public void Dispose() => disposeAction(key);
        }
    }
}
