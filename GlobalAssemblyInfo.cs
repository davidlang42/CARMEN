using System.Reflection;

// AssemblyInformationalVersion is used for Application.ProductVersion
[assembly: AssemblyInformationalVersion("0.3"
#if DEBUG
    + "-debug"
#endif
    )]
// AssemblyVersion must be deterministic in debug mode so that user settings persist
[assembly: AssemblyVersion("0.3"
#if !DEBUG
    + ".*"
#endif
    )]
[assembly: AssemblyProduct("CARMEN")]
[assembly: AssemblyDescription("Casting And Role Management Equality Network")]
[assembly: AssemblyCompany("David Lang")]
[assembly: AssemblyCopyright("David Lang")]