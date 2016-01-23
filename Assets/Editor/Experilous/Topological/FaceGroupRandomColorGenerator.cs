using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorBundle), typeof(FaceAttributesCategory), "Group Random Colors")]
	public class FaceGroupRandomColorGenerator : AssetGenerator
	{
		public AssetDescriptor faceGroupCollection;
		public AssetDescriptor faceGroupIndices;

		public AssetDescriptor faceGroupColors;
		public AssetDescriptor faceColors;

		public static FaceGroupRandomColorGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<FaceGroupRandomColorGenerator>();
			generator.bundle = bundle;
			generator.name = name;
			generator.hideFlags = HideFlags.HideInHierarchy;
			return generator;
		}

		public override IEnumerable<AssetDescriptor> dependencies
		{
			get
			{
				if (faceGroupCollection != null) yield return faceGroupCollection;
				if (faceGroupIndices != null) yield return faceGroupIndices;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				if (faceGroupColors == null) faceGroupColors = AssetDescriptor.Create(this, typeof(IFaceGroupAttribute<Color>), "Random Face Group Colors", "Attributes");
				if (faceColors == null) faceColors = AssetDescriptor.Create(this, typeof(IFaceAttribute<Color>), "Random Face Colors", "Attributes");

				yield return faceGroupColors;
				yield return faceColors;
			}
		}

		public override void ResetDependency(AssetDescriptor dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			if (!ResetMemberDependency(dependency, ref faceGroupCollection, ref faceGroupIndices))
			{
				throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this face centroids generator.", dependency.name, dependency.GetType().Name), "dependency");
			}
		}

		public override void Generate()
		{
			var faceGroups = faceGroupCollection.GetAsset<FaceGroupCollection>().faceGroups;
			var faceGroupColorsArray = new Color[faceGroups.Length];

			var random = new Random(new NativeRandomEngine(2));

			for (int i = 0; i < faceGroups.Length; ++i)
			{
				faceGroupColorsArray[i] = new Color(random.ClosedFloatUnit(), random.ClosedFloatUnit(), random.ClosedFloatUnit());
			}

			faceGroupColors.SetAsset(ColorFaceGroupAttribute.CreateInstance(faceGroupColorsArray));
			faceColors.SetAsset(ColorFaceGroupLookupFaceAttribute.CreateInstance(faceGroupIndices.GetAsset<IntFaceAttribute>(), faceGroupColors.GetAsset<ColorFaceGroupAttribute>()));
		}

		public override bool CanGenerate()
		{
			return
				faceGroupCollection != null &&
				faceGroupIndices != null;
		}
	}
}
