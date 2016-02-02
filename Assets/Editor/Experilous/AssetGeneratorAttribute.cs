using System;

namespace Experilous
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
	public class AssetGeneratorAttribute : Attribute
	{
		private Type _assetGeneratorCollectionType;
		private Type _categoryType;
		private string _name;
		public Type after;

		public AssetGeneratorAttribute(Type assetGeneratorCollectionType, Type categoryType, string name)
		{
			if (assetGeneratorCollectionType == null)
				throw new ArgumentNullException(string.Format("The asset generator collection type supplied for the AssetGenerator attribute with name \"{1}\" should not be but was null.", name), "assetGeneratorCollectionType");
			if (!typeof(AssetGeneratorCollection).IsAssignableFrom(assetGeneratorCollectionType))
				throw new ArgumentException(string.Format("The asset generator collection type {0} supplied for the AssetGenerator attribute with name \"{1}\" should be but was not a subclass of AssetGeneratorCollection.", assetGeneratorCollectionType.Name, name), "assetGeneratorCollectionType");

			if (categoryType == null)
				throw new ArgumentNullException("The asset generator category type supplied for the AssetGenerator attribute should not be but was null.", "categoryType");
			if (GetCustomAttribute(categoryType, typeof(AssetGeneratorCategoryAttribute)) == null)
				throw new ArgumentException(string.Format("The asset generator category type {0} supplied for the AssetGenerator attribute should but did not have an AssetGeneratorCategory attribute.", categoryType.Name), "categoryType");

			_assetGeneratorCollectionType = assetGeneratorCollectionType;
			_categoryType = categoryType;
			_name = name;
		}

		public Type assetGeneratorCollectionType { get { return _assetGeneratorCollectionType; } }
		public Type categoryType { get { return _categoryType; } }
		public string name { get { return _name; } }
	}
}
