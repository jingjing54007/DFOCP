﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "DFO login library" )]
[assembly: AssemblyDescription( "Library to assist starting Dungeon Fighter Online without a web browser" )]
[assembly: AssemblyConfiguration( "" )]
[assembly: AssemblyCompany( "Lord High Captain Studios" )]
[assembly: AssemblyProduct( "Browserless DFO Launcher" )]
[assembly: AssemblyCopyright( "Copyright © Greg Najda 2009" )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "63b2ba11-f76f-4345-af8b-eb2dda62731e" )]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion( "1.2." + Dfo.Login.VersionInfo.Revision + ".*" )]
[assembly: AssemblyFileVersion( "1.2." + Dfo.Login.VersionInfo.Revision + ".0" )]