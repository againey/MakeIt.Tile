﻿using System;

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
