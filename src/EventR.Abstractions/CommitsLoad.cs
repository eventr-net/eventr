namespace EventR.Abstractions
{
    using System;

    public sealed class CommitsLoad
    {
        public CommitsLoad(Commit[] commits, int version)
        {
            Commits = commits;
            Version = version;
        }

        public static readonly CommitsLoad Empty = new CommitsLoad(Array.Empty<Commit>(), 0);

        public Commit[] Commits { get; }

        public int Version { get; }

        public bool IsEmpty => Commits.Length == 0 || Version == 0;
    }
}
