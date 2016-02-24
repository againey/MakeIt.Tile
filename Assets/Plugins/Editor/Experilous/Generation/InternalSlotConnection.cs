/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using System;

namespace Experilous.Generation
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
