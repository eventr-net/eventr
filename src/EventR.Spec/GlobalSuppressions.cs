using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Microsoft.ApiDesignGuidelines.Analyzers",
    "CA2007:DoNotDirectlyAwaitATask",
    Justification = "lots of tests throwing exceptions early and not really awaited")]
