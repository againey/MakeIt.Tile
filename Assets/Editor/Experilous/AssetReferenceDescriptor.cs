using System;

namespace Experilous
{
	public struct AssetReferenceDescriptor
	{
		public AssetDescriptor referencee;
		public AssetDescriptor referencer;

		public AssetReferenceDescriptor(AssetDescriptor referencee, AssetDescriptor referencer)
		{
			if (referencee == null) throw new ArgumentNullException("referencee");
			if (referencer == null) throw new ArgumentNullException("referencer");
			this.referencee = referencee;
			this.referencer = referencer;
		}
	}
}
