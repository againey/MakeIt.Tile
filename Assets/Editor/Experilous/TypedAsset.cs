namespace Experilous
{
	public struct TypedAsset
	{
		public UnityEngine.Object asset;
		public System.Type type;

		public TypedAsset(UnityEngine.Object asset)
		{
			this.asset = asset;
			type = asset.GetType();
		}

		public TypedAsset(UnityEngine.Object asset, System.Type type)
		{
			if (!type.IsInstanceOfType(asset)) throw new System.ArgumentException(string.Format("The asset of type {0} was not an instance of {1}.", asset.GetType().Name, type.Name), "asset");

			this.asset = asset;
			this.type = type;
		}
	}
}
