namespace EventR
{
    using System.Security.Cryptography.X509Certificates;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using EventR.Abstractions;

    public static class SslCert
    {
        public static readonly Regex ThumbprintRgx = new Regex("^[0-9A-F]{40}$", RegexOptions.Compiled);
        private static readonly Regex NormRx = new Regex(@"\s|\W", RegexOptions.Compiled);

        public static X509Certificate2 FromStore(string thumbprint, StoreName name = StoreName.Root, StoreLocation location = StoreLocation.LocalMachine)
        {
            var tp = NormalizeThumbprint(thumbprint);
            Expect.Regex(ThumbprintRgx, tp, nameof(thumbprint));

            using (var store = new X509Store(name, location))
            {
                store.Open(OpenFlags.OpenExistingOnly);
                var eligible = store.Certificates.Find(X509FindType.FindByThumbprint, tp, validOnly: true);
                if (eligible.Count == 0)
                {
                    throw new ArgumentException("no valid SSL certificate found for given thumbprint", nameof(thumbprint));
                }

                return eligible[0];
            }
        }

        public static X509Certificate2 Embedded(string name, Assembly assembly = null)
        {
            var asm = assembly ?? Assembly.GetEntryAssembly();
            using (var stream = asm.GetManifestResourceStream(name))
            {
                if (stream == null)
                {
                    throw new ArgumentException($"no embedded resource found ({name} in assembly {asm.GetName().Name})", nameof(name));
                }

                using (var reader = new BinaryReader(stream))
                {
                    var bytes = reader.ReadBytes((int)stream.Length);
                    return new X509Certificate2(bytes);
                }
            }
        }

        // http://stackoverflow.com/questions/8448147/problems-with-x509store-certificates-find-findbythumbprint
        public static string NormalizeThumbprint(string thumbprint)
        {
            return string.IsNullOrEmpty(thumbprint)
                ? string.Empty
                : NormRx.Replace(thumbprint, string.Empty).ToUpperInvariant();
        }
    }
}
