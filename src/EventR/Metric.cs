namespace EventR
{
    using App.Metrics;
    using App.Metrics.Counter;
    using App.Metrics.Histogram;
    using App.Metrics.Timer;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    public static class Metric
    {
        private static readonly IDisposable VoidTimer = new VoidTimerContext();

        #region Common

        private static CounterOptions SuccessOptions => new CounterOptions
        {
            Name = Names.SuccessCounter,
            MeasurementUnit = Unit.Calls,
            Context = Names.Context,
        };

        internal static void Success(IMetrics metrics, string operation)
        {
            metrics?.Measure.Counter.Increment(SuccessOptions, Tag(Names.Operation, operation));
        }

        private static CounterOptions ErrorOptions => new CounterOptions
        {
            Name = Names.ErrorCounter,
            MeasurementUnit = Unit.Errors,
            Context = Names.Context,
        };

        internal static void Error(IMetrics metrics, string operation)
        {
            metrics?.Measure.Counter.Increment(ErrorOptions, Tag(Names.Operation, operation));
        }

        #endregion
        #region Serializer

        private static HistogramOptions CommitSizeOptions => new HistogramOptions
        {
            Name = Names.CommitSizeHistogram,
            MeasurementUnit = Unit.Bytes,
            Context = Names.Context,
        };

        internal static void CommitSize(IMetrics metrics, int size)
        {
            metrics?.Measure.Histogram.Update(CommitSizeOptions, size);
        }

        private static TimerOptions DeserializeOptions => new TimerOptions
        {
            Name = Names.DeserializeTimer,
            DurationUnit = TimeUnit.Milliseconds,
            RateUnit = TimeUnit.Milliseconds,
            Context = Names.Context,
        };

        internal static IDisposable MeasureDeserialize(IMetrics metrics)
        {
            return metrics?.Measure.Timer.Time(DeserializeOptions) ?? VoidTimer;
        }

        private static TimerOptions SerializeOptions => new TimerOptions
        {
            Name = Names.SerializeTimer,
            DurationUnit = TimeUnit.Milliseconds,
            RateUnit = TimeUnit.Milliseconds,
            Context = Names.Context,
        };

        internal static IDisposable MeasureSerialize(IMetrics metrics)
        {
            return metrics?.Measure.Timer.Time(SerializeOptions) ?? VoidTimer;
        }

        private static CounterOptions SerializerOptions => new CounterOptions
        {
            Name = Names.SerializerCounter,
            MeasurementUnit = Unit.Calls,
            Context = Names.Context,
        };

        internal static void Serializer(IMetrics metrics, string serializerId)
        {
            metrics?.Measure.Counter.Increment(SerializerOptions, Tag(Names.SerializerId, serializerId));
        }

        #endregion
        #region Persistence

        private static CounterOptions OpenPersistenceSesssionOptions => new CounterOptions
        {
            Name = Names.OpenPersistenceSesssionCounter,
            MeasurementUnit = Unit.Calls,
            Context = Names.Context,
        };

        internal static void OpenPersistenceSesssion(IMetrics metrics)
        {
            metrics?.Measure.Counter.Increment(OpenPersistenceSesssionOptions);
        }

        private static TimerOptions LoadOptions => new TimerOptions
        {
            Name = Names.LoadTimer,
            MeasurementUnit = Unit.Requests,
            DurationUnit = TimeUnit.Milliseconds,
            RateUnit = TimeUnit.Milliseconds,
            Context = Names.Context,
        };

        internal static IDisposable MeasureLoad(IMetrics metrics)
        {
            return metrics?.Measure.Timer.Time(LoadOptions) ?? VoidTimer;
        }

        private static HistogramOptions CommitsPerLoadOptions => new HistogramOptions
        {
            Name = Names.CommitsPerLoadHistogram,
            MeasurementUnit = Unit.Items,
            Context = Names.Context,
        };

        internal static void CommitsPerLoad(IMetrics metrics, int count)
        {
            metrics?.Measure.Histogram.Update(CommitsPerLoadOptions, count);
        }

        private static CounterOptions StreamTooLongOptions => new CounterOptions
        {
            Name = Names.StreamTooLongCounter,
            MeasurementUnit = Unit.Errors,
            Context = Names.Context,
        };

        internal static void StreamTooLong(IMetrics metrics)
        {
            metrics?.Measure.Counter.Increment(StreamTooLongOptions);
        }

        private static TimerOptions SaveOptions => new TimerOptions
        {
            Name = Names.SaveTimer,
            MeasurementUnit = Unit.Requests,
            DurationUnit = TimeUnit.Milliseconds,
            RateUnit = TimeUnit.Milliseconds,
            Context = Names.Context,
        };

        internal static IDisposable MeasureSave(IMetrics metrics)
        {
            return metrics?.Measure.Timer.Time(SaveOptions) ?? VoidTimer;
        }

        private static TimerOptions DeleteOptions => new TimerOptions
        {
            Name = Names.DeleteTimer,
            MeasurementUnit = Unit.Requests,
            DurationUnit = TimeUnit.Milliseconds,
            RateUnit = TimeUnit.Milliseconds,
            Context = Names.Context,
        };

        internal static IDisposable MeasureDelete(IMetrics metrics)
        {
            return metrics?.Measure.Timer.Time(DeleteOptions) ?? VoidTimer;
        }

        private static CounterOptions VersionConflictOptions => new CounterOptions
        {
            Name = Names.VersionConflictCounter,
            MeasurementUnit = Unit.Errors,
            Context = Names.Context,
        };

        internal static void VersionConflict(IMetrics metrics)
        {
            if (metrics != null)
            {
                metrics.Measure.Counter.Increment(VersionConflictOptions);
                Error(metrics, Names.SaveOp);
            }
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MetricTags Tag(string key, string value)
        {
            return new MetricTags(key, value);
        }

        private class VoidTimerContext : IDisposable
        {
            public void Dispose()
            { }
        }

        public static class Names
        {
            public const string Context = "EventR";

            public const string SuccessCounter = "success";
            public const string ErrorCounter = "error";
            public const string Operation = "operation";
            public const string SaveOp = "save";
            public const string LoadOp = "load";
            public const string DeleteOp = "delete";

            public const string OpenPersistenceSesssionCounter = "open persistence session";

            public const string SerializerCounter = "serializer";
            public const string DeserializeTimer = "deserialize";
            public const string SerializeTimer = "serialize";
            public const string SerializerId = "id";

            public const string LoadTimer = "load";
            public const string SaveTimer = "save";
            public const string DeleteTimer = "delete";

            public const string CommitsPerLoadHistogram = "commits";
            public const string CommitSizeHistogram = "commit size";

            public const string StreamTooLongCounter = "stream too long";
            public const string VersionConflictCounter = "version conflict";
        }
    }
}
