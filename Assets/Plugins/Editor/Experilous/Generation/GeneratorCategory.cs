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
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
	public class GeneratorCategoryAttribute : Attribute
	{
		private Type _generatorExecutiveType;
		private string _name;
		public Type after;

		public GeneratorCategoryAttribute(Type generatorExecutiveType, string name)
		{
			if (generatorExecutiveType == null)
				throw new ArgumentNullException(string.Format("The generator exective type supplied for the GeneratorCategory attribute with name \"{1}\" should not be but was null.", name), "generatorExecutiveType");
			if (!typeof(GeneratorExecutive).IsAssignableFrom(generatorExecutiveType))
				throw new ArgumentException(string.Format("The generator executive type {0} supplied for the GeneratorCategory attribute with name \"{1}\" should be but was not a subclass of GeneratorExecutive.", this.generatorExecutiveType.Name, name), "generatorExecutiveType");

			_generatorExecutiveType = generatorExecutiveType;
			_name = name;
		}

		public Type generatorExecutiveType { get { return _generatorExecutiveType; } }
		public string name { get { return _name; } }
	}
}
