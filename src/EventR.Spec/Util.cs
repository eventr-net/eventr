namespace EventR.Spec
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Transactions;

    public static class Util
    {
        public static void AppendToStringBuilder(StringBuilder sb, Exception ex, int ident = 0)
        {
            for (var i = 0; i < ident; i++)
            {
                sb.Append("    ");
            }

            sb.AppendFormat("{0}: {1}\r\n", ex.GetType().Name, ex.Message);

            if (ex.InnerException != null)
            {
                AppendToStringBuilder(sb, ex.InnerException, ident + 1);
            }
        }

        public static string ToInfoString(this Exception ex)
        {
            var sb = new StringBuilder();
            AppendToStringBuilder(sb, ex);
            return sb.ToString();
        }

        public static string ToInfoString(this AggregateException aggEx)
        {
            return aggEx.Flatten().InnerExceptions.ToInfoString("AggregateException:");
        }

        public static string ToInfoString(this IEnumerable<Exception> exceptions, string label = null)
        {
            var sb = new StringBuilder();
            if (string.IsNullOrEmpty(label))
            {
                sb.Append(label);
                sb.AppendLine();
            }

            foreach (var ex in exceptions)
            {
                AppendToStringBuilder(sb, ex);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static TransactionScope CreateTransactionScope(
            TransactionScopeOption scopeOption = TransactionScopeOption.Required,
            int timeoutSecs = 10)
        {
#if DEBUG
            var timeout = TimeSpan.FromSeconds(Debugger.IsAttached ? 120 : timeoutSecs);
#else
            var timeout = TimeSpan.FromSeconds(timeoutSecs);
#endif
            var txOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = timeout,
            };
            return new TransactionScope(scopeOption, txOptions, TransactionScopeAsyncFlowOption.Enabled);
        }
    }
}
