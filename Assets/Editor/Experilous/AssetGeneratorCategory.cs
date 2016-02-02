using System;
using System.Reflection;

namespace Experilous
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
	public class AssetGeneratorCategoryAttribute : Attribute
	{
		private Type _assetGeneratorCollectionType;
		private string _name;
		public Type after;

		public AssetGeneratorCategoryAttribute(Type assetGeneratorCollectionType, string name)
		{
			if (assetGeneratorCollectionType == null)
				throw new ArgumentNullException(string.Format("The asset generator collection type supplied for the AssetGeneratorCategory attribute with name \"{1}\" should not be but was null.", name), "assetGeneratorCollectionType");
			if (!typeof(AssetGeneratorCollection).IsAssignableFrom(assetGeneratorCollectionType))
				throw new ArgumentException(string.Format("The asset generator collection type {0} supplied for the AssetGeneratorCategory attribute with name \"{1}\" should be but was not a subclass of AssetGeneratorCollection.", assetGeneratorCollectionType.Name, name), "assetGeneratorCollectionType");

			_assetGeneratorCollectionType = assetGeneratorCollectionType;
			_name = name;
		}

		public Type assetGeneratorCollectionType { get { return _assetGeneratorCollectionType; } }
		public string name { get { return _name; } }
	}
}
