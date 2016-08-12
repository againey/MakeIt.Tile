/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;

namespace Experilous.MakeIt.Generate
{
	public struct InternalSlotConnection
	{
		public OutputSlot source;
		public OutputSlot target;

		public InternalSlotConnection(OutputSlot source, OutputSlot target)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (target == null) throw new ArgumentNullException("target");
			this.source = source;
			this.target = target;
		}
	}
}
