namespace EventR.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;

    public static class Expect
    {
        private static readonly char[] NewLineChars = { '\r', '\n' };

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNull(object arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotEmpty(string arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }

            if (string.IsNullOrEmpty(arg))
            {
                throw new ArgumentException($"argument '{argName}' must not be empty string", argName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Regex(Regex rx, string arg, string argName)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }

            if (!rx.IsMatch(arg))
            {
                throw new ArgumentException($"argument '{argName}' must conform to regular expression {rx}", argName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotEmpty<T>(ICollection<T> arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }

            if (arg.Count == 0)
            {
                throw new ArgumentException($"argument '{argName}' must not be empty {arg.GetType().Name}", argName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Positive(int arg, string argName)
        {
            if (arg < 1)
            {
                throw new ArgumentException($"argument '{argName}' must be greater than 0", argName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Positive(long arg, string argName)
        {
            if (arg < 1)
            {
                throw new ArgumentException($"argument '{argName}' must be greater than 0", argName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotDefault<T>(T arg, string argName)
        {
            if (arg.Equals(default(T)))
            {
                throw new ArgumentException(
                    $"argument '{argName}' must not equal default value for {arg.GetType().Name}", argName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Range<T>(T arg, T min, T max, string argName)
            where T : IComparable<T>
        {
            if (arg.CompareTo(min) == -1 || arg.CompareTo(max) == 1)
            {
                var errMsg = $"argument '{argName}' must within between {min} and {max} (both inclusive)";
                throw new ArgumentException(errMsg, argName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotInclude(char[] exclusions, string arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }

            if (exclusions == null || exclusions.Length == 0)
            {
                return;
            }

            if (arg.IndexOfAny(exclusions) >= 0)
            {
                throw new ArgumentException(argName, $"argument '{argName}' must not contains excluded characters");
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotIncludeNewLine(string arg, string argName)
        {
            NotInclude(NewLineChars, arg, argName);
        }

        [DebuggerStepThrough]
        public static void NotDisposed(bool isDisposed, [CallerMemberName] string callerMemberName = "")
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(
                    $"calling method '{callerMemberName}' of disposed object is not allowed");
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsNotSet<T>(T variable, string name)
            where T : class
        {
            if (variable != null)
            {
                throw new InvalidOperationException(
                    $"variable {name} ({typeof(T).Name}) must be set only once; it is already not null");
            }
        }
    }
}
