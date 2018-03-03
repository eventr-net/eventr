namespace EventR.InMemory
{
    using EventR.Abstractions;
    using System;
    using System.Transactions;

    /// <summary>
    /// Encapsulates operation on in-memory storage.
    /// It allows in-memory storage to take part in transactions.
    /// </summary>
    public class EnlistedOperation : IEnlistmentNotification
    {
        private readonly Commit commit;
        private readonly Action<Commit> operation;

        public EnlistedOperation(Commit commit, Action<Commit> operation)
        {
            Expect.NotNull(commit, "commit");
            Expect.NotNull(operation, "operation");
            this.commit = commit;
            this.operation = operation;
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            operation(commit);
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            enlistment.Done();
        }
    }
}
