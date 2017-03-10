/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/
#if false
using System;
using System.Runtime.Serialization;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// Exception type to be thrown whenever a requested topology creation or modification would result in an invalid topology arrangement.
	/// </summary>
	public class InvalidTopologyException : ApplicationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidTopologyException"/> class.
		/// </summary>
		public InvalidTopologyException()
			: base() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidTopologyException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">The exception message explaining more specifically what went wrong.</param>
		public InvalidTopologyException(string message)
			: base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidTopologyException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The exception message explaining more specifically what went wrong.</param>
		/// <param name="innerException">The inner exception that is the cause of this exception.</param>
		public InvalidTopologyException(string message, Exception innerException)
			: base(message, innerException) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidTopologyException"/> class with serialized data.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
		public InvalidTopologyException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }
	}
}
#endif