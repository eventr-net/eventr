namespace EventR
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using EventR.Abstractions;

    /// <summary>
    /// Provides a pump that supports running asynchronous methods on the current thread.
    /// </summary>
    /// <remarks>
    /// This class is almost 1:1 copy from
    /// https://github.com/danielmarbach/AsyncTransactions/blob/master/AsyncTransactions/AsyncPump.cs
    /// </remarks>
    [CompilerGenerated]
    public static class AsyncPump
    {
        /// <summary>Runs the specified asynchronous function.</summary>
        /// <param name="func">The asynchronous function to execute.</param>
        public static void Run(Func<Task> func)
        {
            Expect.NotNull(func, "func");

            var prevCtx = SynchronizationContext.Current;
            try
            {
                // Establish the new context
                var syncCtx = new SingleThreadSyncContext();
                SynchronizationContext.SetSynchronizationContext(syncCtx);

                // Invoke the function and alert the context to when it completes
                var t = func();
                if (t == null)
                {
                    throw new InvalidOperationException("No task provided.");
                }

                t.ContinueWith(delegate { syncCtx.Complete(); }, TaskScheduler.Default);

                // Pump continuations and propagate any exceptions
                syncCtx.RunOnCurrentThread();
                t.GetAwaiter().GetResult();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prevCtx);
            }
        }

        /// <summary>
        /// Provides a SynchronizationContext that's single-threaded.
        /// </summary>
        private sealed class SingleThreadSyncContext : SynchronizationContext
        {
            private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> queue;
            private readonly Thread thread;

            public SingleThreadSyncContext()
                : this(new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>(), Thread.CurrentThread)
            {
            }

            private SingleThreadSyncContext(
                BlockingCollection<KeyValuePair<SendOrPostCallback, object>> queue,
                Thread currentThread)
            {
                this.queue = queue;
                thread = currentThread;
            }

            /// <summary>
            /// Dispatches an asynchronous message to the synchronization context.
            /// </summary>
            /// <param name="d">The System.Threading.SendOrPostCallback delegate to call.</param>
            /// <param name="state">The object passed to the delegate.</param>
            public override void Post(SendOrPostCallback d, object state)
            {
                queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
            }

            /// <summary>
            /// Not supported.
            /// </summary>
            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("Synchronously sending is not supported.");
            }

            public override SynchronizationContext CreateCopy()
            {
                return new SingleThreadSyncContext(queue, thread);
            }

            /// <summary>
            /// Runs an loop to process all queued work items.
            /// </summary>
            public void RunOnCurrentThread()
            {
                foreach (var workItem in queue.GetConsumingEnumerable())
                {
                    workItem.Key(workItem.Value);
                }
            }

            /// <summary>
            /// Notifies the context that no more work will arrive.
            /// </summary>
            public void Complete()
            {
                queue.CompleteAdding();
            }
        }
    }
}
