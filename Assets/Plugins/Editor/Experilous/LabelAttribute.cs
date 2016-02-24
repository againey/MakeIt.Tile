/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using System;

namespace Experilous
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class LabelAttribute : Attribute
	{
		private string _text;

		public LabelAttribute(string text)
		{
			_text = text;
		}

		public string text { get { return _text; } }
	}
}
