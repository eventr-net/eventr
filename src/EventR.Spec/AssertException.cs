namespace EventR.Spec
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public static class AssertException
    {
        public static async Task Is<T>(Func<Task> blockOfCode, Func<T, bool> check = null)
            where T : Exception
        {
            var exName = typeof(T).Name;
            try
            {
                await blockOfCode().ConfigureAwait(false);

                Assert.True(false, $"{exName} has not been thrown");
            }
            catch (T ex)
            {
                Check(ex, check);
            }
            catch (AggregateException aggEx)
            {
                var ex = TryExtract<T>(aggEx);
                if (ex == null)
                {
                    throw;
                }

                Check(ex, check, $"{exName} found within AggregateException, but does not meet expected criteria");
            }
        }

        private static T TryExtract<T>(AggregateException aggEx)
            where T : Exception
        {
            if (aggEx.InnerExceptions.Count == 0)
            {
                return null;
            }

            return aggEx.Flatten().InnerExceptions.FirstOrDefault(x => x is T) as T;
        }

        private static void Check<T>(T ex, Func<T, bool> check = null, string customMsg = null)
            where T : Exception
        {
            if (check != null && !check(ex))
            {
                var msg = customMsg ?? $"{typeof(T).Name} has been thrown, but does not meet expected criteria";
                Assert.True(false, msg);
            }
        }
    }
}
