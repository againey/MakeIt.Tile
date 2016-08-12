/******************************************************************************\
* Copyright Andy Gainey                                                        *
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
