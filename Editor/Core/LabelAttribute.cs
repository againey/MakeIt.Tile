/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;

namespace MakeIt.Core
{
	/// <summary>
	/// An attribute used for overriding the automatic name of a field shown in the inspector.  Not for general use; works only with <see cref="MakeIt.Generate.Generator"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class LabelAttribute : Attribute
	{
		private string _text;

		/// <summary>
		/// Constructs a label attribute using the specified text as the label string.
		/// </summary>
		/// <param name="text">The label to use for the field to which this attribute is attached..</param>
		public LabelAttribute(string text)
		{
			_text = text;
		}

		/// <summary>
		/// The label to use for the field to which this attribute is attached.
		/// </summary>
		public string text { get { return _text; } }
	}
}
