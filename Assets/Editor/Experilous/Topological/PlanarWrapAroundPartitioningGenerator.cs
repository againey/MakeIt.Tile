using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
#if false
	[AssetGenerator(typeof(TopologyGeneratorBundle), typeof(UtilitiesCategory), "Planar Wrap-Around Partitioning")]
	public class PlanarWrapAroundPartitioningGenerator : AssetGenerator
	{
		public TopologyGeneratedAsset topology;
		public Vector3VertexAttributeGeneratedAsset vertexPositions;

		public PlanarWrapAroundPartitioningGeneratedAsset partitioning;

		public static SphericalPartitioningGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<SphericalPartitioningGenerator>();
			generator.bundle = bundle;
			generator.name = name;
			generator.hideFlags = HideFlags.HideInHierarchy;
			return generator;
		}

		public override IEnumerable<GeneratedAsset> dependencies
		{
			get
			{
				if (topology != null) yield return topology;
				if (vertexPositions != null) yield return vertexPositions;
			}
		}

		public override IEnumerable<GeneratedAsset> outputs
		{
			get
			{
				if (partitioning == null) partitioning = PlanarWrapAroundPartitioningGeneratedAsset.CreateDefaultInstance(this, "Planar Wrap-Around Partitioning");
				yield return partitioning;
			}
		}

		public override void ResetDependency(GeneratedAsset dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			if (!ResetMemberDependency(dependency, ref topology, ref vertexPositions))
			{
				throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this planar wrap-around partitioning generator.", dependency.name, dependency.GetType().Name), "dependency");
			}
		}

		public override void Generate(string location, string name)
		{
			var instance = PlanarWrapAroundPartitioning.CreateInstance(topology.generatedInstance, vertexPositions.generatedInstance.array, new Vector2(0f, 0f));
			partitioning.SetGeneratedInstance(location, name, instance);
		}

		public override bool CanGenerate()
		{
			return
				topology != null &&
				vertexPositions != null;
		}
	}
#endif
}
