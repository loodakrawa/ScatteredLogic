using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("ScatteredLogic")]
[assembly: AssemblyDescription("A simple Entity Component System with a Zero Waste policy")]
[assembly: AssemblyCopyright("Copyright © Luka \"loodakrawa\" Sverko")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyVersion("0.0.0.0")]
[assembly: AssemblyFileVersion("0.0.0.0")]
[assembly: AssemblyInformationalVersion("0.0.0.0")]

[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
