namespace EventR.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public sealed class PayloadLayout
    {
        public PayloadLayout()
        {
            data = new List<(string typeId, int ends)>(2);
        }

        public PayloadLayout(string serialized)
        {
            data = new List<(string typeId, int ends)>(2);

            if (string.IsNullOrEmpty(serialized))
            {
                return;
            }

            foreach (var entry in serialized.Split(new[] { Delim }, StringSplitOptions.RemoveEmptyEntries))
            {
                var sepAt = entry.IndexOf(Sep);
                if (sepAt > 0 && sepAt < entry.Length - 1) // separates two non-empty parts
                {
                    var typeId = entry.Substring(0, sepAt);
                    if (int.TryParse(entry.Substring(sepAt + 1), out int ends)
                        && ends > 0)
                    {
                        data.Add((typeId, ends));
                    }
                }
            }
        }

        private const char Delim = '|';
        private const char Sep = ':';
        private readonly List<(string typeId, int ends)> data;

        public override string ToString()
        {
            if (data.Count == 0)
            {
                return string.Empty;
            }

            var first = true;
            var sb = new StringBuilder();
            foreach (var (typeId, ends) in data)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(Delim);
                }

                sb.Append(typeId);
                sb.Append(Sep);
                sb.Append(ends);
            }

            return sb.ToString();
        }
    }
}
