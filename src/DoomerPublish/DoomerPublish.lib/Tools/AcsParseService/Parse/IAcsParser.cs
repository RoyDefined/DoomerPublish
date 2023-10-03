using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomerPublish.Tools;

internal interface IAcsParser
{
	public void Parse(AcsFile acsFile);
}
