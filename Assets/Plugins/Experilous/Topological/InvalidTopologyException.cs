/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using System;
using System.Runtime.Serialization;

namespace Experilous.Topological
{
	public class InvalidTopologyException : ApplicationException
	{
		public InvalidTopologyException()
			: base() { }

		public InvalidTopologyException(string message)
			: base(message) { }

		public InvalidTopologyException(string message, Exception innerException)
			: base(message, innerException) { }

		public InvalidTopologyException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }
	}
}
