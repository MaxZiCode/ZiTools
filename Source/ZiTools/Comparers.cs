using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZiTools
{
	class CorpseTimeComparer : IComparer<string>
	{
		readonly Dictionary<string, int> CorpsesTimeRemainDict;

		public CorpseTimeComparer(Dictionary<string, int> dict)
		{
			this.CorpsesTimeRemainDict = dict ?? throw new ArgumentNullException(nameof(dict));
		}

		int IComparer<string>.Compare(string x, string y)
		{
			if (CorpsesTimeRemainDict[x] > 0 && CorpsesTimeRemainDict[x] < CorpsesTimeRemainDict[y] || CorpsesTimeRemainDict[y] == 0 && CorpsesTimeRemainDict[x] > 0)
				return -1;
			if (CorpsesTimeRemainDict[x] > CorpsesTimeRemainDict[y] || CorpsesTimeRemainDict[x] == 0 && CorpsesTimeRemainDict[y] > 0)
				return 1;
			else
				return 0;
		}
	}

	class LabelsComparer : IComparer<string>
	{
		readonly Dictionary<string, string> labelsDict;

		public LabelsComparer(Dictionary<string, string> dict)
		{
			this.labelsDict = dict ?? throw new ArgumentNullException(nameof(dict));
		}

		int IComparer<string>.Compare(string x, string y)
		{
			return labelsDict[x].CompareTo(labelsDict[y]);
		}
	}
}
