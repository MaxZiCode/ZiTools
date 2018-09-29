using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using Verse;

namespace ZiTools
{
	[StaticConstructorOnStartup]
    public static class StaticConstructor
    {
		public static readonly ObjectSeeker_Data OSD_Global = new ObjectSeeker_Data();
	}
}
