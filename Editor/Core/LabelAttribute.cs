/******************************************************************************\
* Copyright Andy Gainey                                                        *
*                                                                              *
* Licensed under the Apache License, Version 2.0 (the "License");              *
* you may not use this file except in compliance with the License.             *
* You may obtain a copy of the License at                                      *
*                                                                              *
*     http://www.apache.org/licenses/LICENSE-2.0                               *
*                                                                              *
* Unless required by applicable law or agreed to in writing, software          *
* distributed under the License is distributed on an "AS IS" BASIS,            *
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.     *
* See the License for the specific language governing permissions and          *
* limitations under the License.                                               *
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
