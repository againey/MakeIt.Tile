using System;
using System.Reflection;

namespace Experilous
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
	public class AssetGeneratorCategoryAttribute : Attribute
	{
		private Type _assetGeneratorBundleType;
		private string _name;
		public Type after;

		public AssetGeneratorCategoryAttribute(Type assetGeneratorBundleType, string name)
		{
			if (assetGeneratorBundleType == null)
				throw new ArgumentNullException(string.Format("The asset generator bundle type supplied for the AssetGeneratorCategory attribute with name \"{1}\" should not be but was null.", name), "assetGeneratorBundleType");
			if (!assetGeneratorBundleType.IsSubclassOf(typeof(AssetGeneratorBundle)))
				throw new ArgumentException(string.Format("The asset generator bundle type {0} supplied for the AssetGeneratorCategory attribute with name \"{1}\" should be but was not a subclass of AssetGeneratorBundle.", assetGeneratorBundleType.Name, name), "assetGeneratorBundleType");

			_assetGeneratorBundleType = assetGeneratorBundleType;
			_name = name;
		}

		public Type assetGeneratorBundleType { get { return _assetGeneratorBundleType; } }
		public string name { get { return _name; } }
	}
}
