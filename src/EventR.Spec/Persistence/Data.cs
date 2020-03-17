namespace EventR.Spec.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using EventR.Abstractions;
    using KellermanSoftware.CompareNetObjects;

    public static class Data
    {
        public static Commit[] CreateValidCommits(int count)
        {
            var result = new Commit[count];
            var ver = 0;
            for (var i = 0; i < count; i++)
            {
                if (i == 0) // first
                {
                    result[i] = new Commit
                    {
                        Id = TimeGuid.NewId(),
                        StreamId = Guid.NewGuid().ToString("N").Substring(24),
                        Version = ++ver,
                        ItemsCount = 1,
                        SerializerId = "text",
                        Payload = Payload1,
                        PayloadLayout = null,
                    };
                    continue;
                }

                var isOdd = i % 2 == 1;
                result[i] = new Commit
                {
                    Id = TimeGuid.NewId(),
                    StreamId = Guid.NewGuid().ToString("N").Substring(24),
                    Version = ++ver,
                    ItemsCount = isOdd ? (short)2 : (short)1,
                    SerializerId = "text",
                    Payload = isOdd ? Payload2 : Payload1,
                    PayloadLayout = isOdd ? Layout2 : Layout1,
                };
            }

            return result;
        }

        public static IEnumerable<object[]> ValidCommits()
        {
            var result = CreateValidCommits(3);
            return ZipWithIndex(result);
        }

        public static IEnumerable<object[]> InvalidCommits()
        {
            var result = CreateValidCommits(13);
            result[0].Id = Guid.Empty;
            result[1].StreamId = null;
            result[2].StreamId = string.Empty;
            result[3].StreamId = "sijd ppwp";
            result[4].Version = 0;
            result[5].Version = -1;
            result[6].ItemsCount = 0;
            result[7].ItemsCount = -1;
            result[8].SerializerId = null;
            result[9].SerializerId = string.Empty;
            result[10].SerializerId = "4848 884";
            result[11].Payload = null;
            result[12].Payload = Array.Empty<byte>();
            return ZipWithIndex(result);
        }

        public static async Task<string> PrepareCommits(IPersistence persistence, int n)
        {
            Expect.NotNull(persistence, nameof(persistence));
            Expect.Range(n, 1, 100000, nameof(n));

            var streamId = Guid.NewGuid().ToString("N").Substring(24);
            using (var sess = persistence.OpenSession())
            {
                foreach (var commit in CreateValidCommits(n))
                {
                    await sess.SaveAsync(commit).ConfigureAwait(false);
                }
            }

            return streamId;
        }

        public static async Task<CommitsLoad> GetCommits(string streamId, IPersistence persistence)
        {
            using (var sess = persistence.OpenSession())
            {
                return await sess.LoadCommitsAsync(streamId).ConfigureAwait(false);
            }
        }

        public static async Task<Tuple<bool, string>> HasBeenSaved(Commit expected, IPersistence persistence, CompareLogic comparer)
        {
            var load = await GetCommits(expected.StreamId, persistence).ConfigureAwait(false);
            var commits = load?.Commits?.Where(x => x.Version == expected.Version).ToArray();
            if (commits == null || commits.Length == 0)
            {
                 return new Tuple<bool, string>(false, $"no stream {expected.StreamId} found");
            }

            if (commits.Length != 1)
            {
                return new Tuple<bool, string>(false, $"stream {expected.StreamId} contains more than one commit with version {expected.Version}");
            }

            var actual = commits.First();
            var result = comparer.Compare(expected, actual);
            if (!result.AreEqual)
            {
                var error = string.Format(
                    "commit (stream '{0}', version {1}) is found, but has different data properties: {2}",
                    expected.StreamId,
                    expected.Version,
                    result.DifferencesString);
                return new Tuple<bool, string>(false, error);
            }

            return new Tuple<bool, string>(true, string.Empty);
        }

        private static IEnumerable<object[]> ZipWithIndex<T>(T[] source)
        {
            for (var i = 0; i < source.Length; i++)
            {
                yield return new object[] { source[i], i };
            }
        }

        public static readonly byte[] Payload1 = Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consectetur adipiscing " +
            "elit. Etiam tempus scelerisque cursus. Suspendisse vehicula risus at tincidunt egestas. In vel turpis facilisis, " +
            "feugiat ante accumsan, imperdiet justo. Donec eu eleifend leo. Phasellus risus sapien, blandit et ex at, placerat " +
            "ultrices augue. Vestibulum ut rutrum neque. In ac mollis massa, nec ultrices dui.");

        public static readonly byte[] Payload2 = CreatePayload2();

        public static readonly PayloadLayout Layout1 = CreateLayout1();

        public static readonly PayloadLayout Layout2 = CreateLayout2();

        private static byte[] CreatePayload2()
        {
            var c2 = Encoding.UTF8.GetBytes("One ring to rule them all, one ring to find them, " +
                "One ring to bring them all and in the darkness bind them.");
            var payload = new byte[Payload1.Length + c2.Length];
            Array.Copy(Payload1, 0, payload, 0, Payload1.Length);
            Array.Copy(c2, 0, payload, Payload1.Length, c2.Length);
            return payload;
        }

        private static PayloadLayout CreateLayout1()
        {
            var pl = new PayloadLayout(1);
            pl.Add(0, Payload1.Length, "System.String");
            return pl;
        }

        private static PayloadLayout CreateLayout2()
        {
            var pl = new PayloadLayout(2);
            pl.Add(0, Payload1.Length, "System.String");
            pl.Add(Payload1.Length, Payload2.Length, "System.String");
            return pl;
        }
    }
}
