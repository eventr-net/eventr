namespace EventR.Spec
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public static class AssertException
    {
        public static async Task Is<T>(Func<Task> blockOfCode, Func<T, bool> check = null, int testIndex = -1)
            where T : Exception
        {
            var exName = typeof(T).Name;
            var testIndexSuffix = testIndex > -1 ? $" Test index: {testIndex}." : string.Empty;
            try
            {
                await blockOfCode().ConfigureAwait(false);

                Assert.True(false, $"{exName} has not been thrown.{testIndexSuffix}");
            }
            catch (T ex)
            {
                Check(ex, $"{exName} does not meet expected criteria.{testIndexSuffix}", check);
            }
            catch (AggregateException aggEx)
            {
                var ex = TryExtract<T>(aggEx);
                if (ex == null)
                {
                    throw;
                }

                Check(ex, $"{exName} found within AggregateException, but does not meet expected criteria.{testIndexSuffix}", check);
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

        private static void Check<T>(T ex, string message, Func<T, bool> check = null)
            where T : Exception
        {
            if (check != null && !check(ex))
            {
                Assert.True(false, message);
            }
        }
    }
}
