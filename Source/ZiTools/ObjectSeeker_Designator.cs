using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ZiTools
{
	public class ObjectSeeker_Designator : Designator
	{
		public ObjectSeeker_Designator()
		{
			this.defaultLabel = "ZiT_ObjectsSeekerLabel".Translate();
			this.defaultDesc = "ZiT_ObjectsSeekerDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Lupa(not Pupa)", true);
			this.activateSound = null;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 loc) => false;

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			ObjectSeeker_Window.DrawWindow();
		}
	}
}
