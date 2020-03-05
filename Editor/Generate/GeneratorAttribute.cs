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
using System.Text.RegularExpressions;

namespace MakeIt.Generate
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
