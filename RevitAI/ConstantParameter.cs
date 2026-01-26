using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAI
{
    public static class ConstantParameter
    {
		private static ExternalCommandData commandData;

		public static ExternalCommandData CommandData
        {
			get { return commandData; }
			set { commandData = value; }
		}

	}
}
