using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorBundle), typeof(UtilitiesCategory), "Spherical Partitioning")]
	public class SphericalPartitioningGenerator : AssetGenerator
	{
		public AssetDescriptor topology;
		public AssetDescriptor vertexPositions;

		public AssetDescriptor partitioning;

		public static SphericalPartitioningGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<SphericalPartitioningGenerator>();
			generator.bundle = bundle;
			generator.name = name;
			generator.hideFlags = HideFlags.HideInHierarchy;
			return generator;
		}

		public override IEnumerable<AssetDescriptor> dependencies
		{
			get
			{
				if (topology != null) yield return topology;
				if (vertexPositions != null) yield return vertexPositions;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				if (partitioning == null) partitioning = AssetDescriptor.Create(this, typeof(SphericalPartitioning), "Spherical Partitioning");
				yield return partitioning;
			}
		}

		public override void ResetDependency(AssetDescriptor dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			if (!ResetMemberDependency(dependency, ref topology, ref vertexPositions))
			{
				throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this spherical partitioning generator.", dependency.name, dependency.GetType().Name), "dependency");
			}
		}

		public override void Generate()
		{
			var instance = SphericalPartitioning.CreateInstance(topology.GetAsset<Topology>(), vertexPositions.GetAsset<IVertexAttribute<Vector3>>());
			partitioning.SetAsset(instance);
		}

		public override bool CanGenerate()
		{
			return
				topology != null &&
				vertexPositions != null;
		}
	}
}
