// This file is used by Code Analysis to maintain SuppressMessage attributes that are applied to this project.
// Project-level suppressions either have no target or are given a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "General exceptions can be caught at the root namespace.")]
[assembly: SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "To be implemented at a later stage.")]
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Feature has no use in the main application.")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Feature generates more false positives than is worth checking for.")]
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "Feature generates false positives.")]
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Generates false positives.")]
[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Disagreed.")]
[assembly: SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Disagreed.")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Disagreed.")]
