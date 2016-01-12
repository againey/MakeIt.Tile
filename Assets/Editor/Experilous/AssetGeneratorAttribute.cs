using System;

namespace Experilous
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
	public class AssetGeneratorAttribute : Attribute
	{
		private Type _assetGeneratorBundleType;
		private Type _categoryType;
		private string _name;
		public Type after;

		public AssetGeneratorAttribute(Type assetGeneratorBundleType, Type categoryType, string name)
		{
			if (assetGeneratorBundleType == null)
				throw new ArgumentNullException(string.Format("The asset generator bundle type supplied for the AssetGenerator attribute with name \"{1}\" should not be but was null.", name), "assetGeneratorBundleType");
			if (!assetGeneratorBundleType.IsSubclassOf(typeof(AssetGeneratorBundle)))
				throw new ArgumentException(string.Format("The asset generator bundle type {0} supplied for the AssetGenerator attribute with name \"{1}\" should be but was not a subclass of AssetGeneratorBundle.", assetGeneratorBundleType.Name, name), "assetGeneratorBundleType");

			if (categoryType == null)
				throw new ArgumentNullException("The asset generator category type supplied for the AssetGenerator attribute should not be but was null.", "categoryType");
			if (GetCustomAttribute(categoryType, typeof(AssetGeneratorCategoryAttribute)) == null)
				throw new ArgumentException(string.Format("The asset generator category type {0} supplied for the AssetGenerator attribute should but did not have an AssetGeneratorCategory attribute.", categoryType.Name), "categoryType");

			_assetGeneratorBundleType = assetGeneratorBundleType;
			_categoryType = categoryType;
			_name = name;
		}

		public Type assetGeneratorBundleType { get { return _assetGeneratorBundleType; } }
		public Type categoryType { get { return _categoryType; } }
		public string name { get { return _name; } }
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class AssetGeneratorCreatorAttribute : Attribute
	{
	}
}
