/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using System;
using System.Text.RegularExpressions;

namespace Experilous.Generation
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
	public class GeneratorAttribute : Attribute
	{
		private Type _generatorExecutiveType;
		private string _name;

		public GeneratorAttribute(Type generatorExecutiveType, string name)
		{
			if (generatorExecutiveType == null)
				throw new ArgumentNullException(string.Format("The generator executive type supplied for the Generator attribute with name \"{1}\" should not be but was null.", name), "generatorExecutiveType");
			if (!typeof(GeneratorExecutive).IsAssignableFrom(generatorExecutiveType))
				throw new ArgumentException(string.Format("The generator executive type {0} supplied for the Generator attribute with name \"{1}\" should be but was not a subclass of GeneratorExecutive.", generatorExecutiveType.Name, name), "generatorExecutiveType");
			if (name == null)
				throw new ArgumentNullException("The generator name should not be but was null.");

			Regex.Replace(name, "\\\\", "/");
			Regex.Replace(name, "/+", "/");
			name.Trim('/');

			if (name == "")
				throw new ArgumentNullException("The generator name should not be but was empty.");

			_generatorExecutiveType = generatorExecutiveType;
			_name = name;
		}

		public Type generatorExecutiveType { get { return _generatorExecutiveType; } }
		public string name { get { return _name; } }
		public string[] path { get { return _name.Split('/'); } }
	}
}
