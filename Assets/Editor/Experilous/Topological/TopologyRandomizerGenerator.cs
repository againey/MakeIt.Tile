using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorBundle), typeof(TopologyCategory), "Topology Randomizer")]
	public class TopologyRandomizerGenerator : AssetGenerator
	{
		public enum SurfaceType
		{
			Planar,
			Spherical,
		}

		public SurfaceType surfaceType = SurfaceType.Planar;

		public int passCount = 1;
		public float frequency = 0.1f;

		public int randomSeed = 0;

		public int minVertexNeighbors = 3;
		public int maxVertexNeighbors = 5;
		public int minFaceNeighbors = 3;
		public int maxFaceNeighbors = 9;

		public bool lockBoundaryVertices = true;

		public float relaxForRegularityWeight = 0.5f;

		public int maxRelaxIterations = 20;
		public float relaxRelativePrecision = 0.95f;
		public int maxRepairIterations = 20;
		public float repairRate = 0.5f;

		public AssetDescriptor topology;
		public AssetDescriptor vertexPositions;
		public AssetDescriptor surfaceDescriptor;
		public AssetDescriptor edgeWrap;

		public static TopologyRandomizerGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<TopologyRandomizerGenerator>();
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
				if (surfaceDescriptor != null) yield return surfaceDescriptor;
				if (edgeWrap != null) yield return edgeWrap;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				yield break;
			}
		}

		public override void ResetDependency(AssetDescriptor dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			if (!ResetMemberDependency(dependency, ref topology, ref vertexPositions, ref surfaceDescriptor, ref edgeWrap))
			{
				throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this vertex normals generator.", dependency.name, dependency.GetType().Name), "dependency");
			}
		}

		public override void Generate()
		{
			var topologyAsset = topology.GetAsset<Topology>();
			var vertexPositionsAsset = vertexPositions != null ? vertexPositions.GetAsset<IVertexAttribute<Vector3>>() : null;
			var surfaceDescriptorAsset = surfaceDescriptor != null ? surfaceDescriptor.GetAsset<PlanarSurfaceDescriptor>() : null;
			var edgeWrapAsset = edgeWrap != null ? edgeWrap.GetAsset<IEdgeAttribute<EdgeWrap>>() : null;

			var random = new Random(new NativeRandomEngine(randomSeed));

			System.Func<float> relaxIterationFunction = null;
			System.Func<bool> repairFunction = null;
			System.Action relaxationLoopFunction = null;

			if (vertexPositionsAsset != null)
			{
				if (surfaceType == SurfaceType.Planar)
				{
					if (surfaceDescriptorAsset == null || edgeWrapAsset == null)
					{
						if (relaxForRegularityWeight == 1.0f)
						{
							var relaxedVertexPositions = new Vector3[topologyAsset.vertices.Count].AsVertexAttribute();
							relaxIterationFunction = () =>
							{
								PlanarManifoldUtility.RelaxVertexPositionsForRegularity(topologyAsset, vertexPositionsAsset, lockBoundaryVertices, relaxedVertexPositions);
								var relaxationAmount = PlanarManifoldUtility.CalculateRelaxationAmount(vertexPositionsAsset, relaxedVertexPositions);
								for (int i = 0; i < vertexPositionsAsset.Count; ++i)
								{
									vertexPositionsAsset[i] = relaxedVertexPositions[i];
								}
								return relaxationAmount;
							};

							repairFunction = () =>
							{
								return PlanarManifoldUtility.ValidateAndRepair(topologyAsset, vertexPositionsAsset, repairRate, lockBoundaryVertices);
							};

							relaxationLoopFunction = TopologyRandomizer.CreateRelaxationLoopFunction(maxRelaxIterations, maxRepairIterations, relaxRelativePrecision, relaxIterationFunction, repairFunction);
						}
					}
					else
					{
						var wrappedVertexPositions = Vector3OffsetWrappedVertexAttribute.CreateInstance(topologyAsset, (EdgeWrapDataEdgeAttribute)edgeWrapAsset, (Vector3VertexAttribute)vertexPositionsAsset, surfaceDescriptorAsset);

						if (relaxForRegularityWeight == 1.0f)
						{
							var relaxedVertexPositions = new Vector3[topologyAsset.vertices.Count].AsVertexAttribute();
							relaxIterationFunction = () =>
							{
								PlanarManifoldUtility.RelaxVertexPositionsForRegularity(topologyAsset, wrappedVertexPositions, lockBoundaryVertices, relaxedVertexPositions);
								var relaxationAmount = PlanarManifoldUtility.CalculateRelaxationAmount(vertexPositionsAsset, relaxedVertexPositions);
								for (int i = 0; i < vertexPositionsAsset.Count; ++i)
								{
									vertexPositionsAsset[i] = relaxedVertexPositions[i];
								}
								return relaxationAmount;
							};

							repairFunction = () =>
							{
								return PlanarManifoldUtility.ValidateAndRepair(topologyAsset, vertexPositionsAsset, wrappedVertexPositions, repairRate, lockBoundaryVertices);
							};

							relaxationLoopFunction = TopologyRandomizer.CreateRelaxationLoopFunction(maxRelaxIterations, maxRepairIterations, relaxRelativePrecision, relaxIterationFunction, repairFunction);
						}
					}

				}
				else if (surfaceType == SurfaceType.Spherical)
				{
					if (relaxForRegularityWeight == 1.0f)
					{
						var relaxedVertexPositions = new Vector3[topologyAsset.vertices.Count].AsVertexAttribute();
						relaxIterationFunction = () =>
						{
							SphericalManifoldUtility.RelaxVertexPositionsForRegularity(topologyAsset, vertexPositionsAsset, 1f, false, relaxedVertexPositions);
							var relaxationAmount = SphericalManifoldUtility.CalculateRelaxationAmount(vertexPositionsAsset, relaxedVertexPositions);
							for (int i = 0; i < vertexPositionsAsset.Count; ++i)
							{
								vertexPositionsAsset[i] = relaxedVertexPositions[i];
							}
							return relaxationAmount;
						};
					}
					else if (relaxForRegularityWeight == 0.0f)
					{
						var relaxedVertexPositions = new Vector3[topologyAsset.vertices.Count].AsVertexAttribute();
						var faceCentroids = new Vector3[topologyAsset.internalFaces.Count].AsFaceAttribute();
						var faceCentroidAngles = new float[topologyAsset.faceEdges.Count].AsEdgeAttribute();
						var vertexAreas = new float[topologyAsset.vertices.Count].AsVertexAttribute();
						relaxIterationFunction = () =>
						{
							SphericalManifoldUtility.RelaxForEqualArea(topologyAsset, vertexPositionsAsset, 1f, false, relaxedVertexPositions, faceCentroids, faceCentroidAngles, vertexAreas);
							var relaxationAmount = SphericalManifoldUtility.CalculateRelaxationAmount(vertexPositionsAsset, relaxedVertexPositions);
							for (int i = 0; i < vertexPositionsAsset.Count; ++i)
							{
								vertexPositionsAsset[i] = relaxedVertexPositions[i];
							}
							return relaxationAmount;
						};
					}
					else
					{
						var regularityWeight = Mathf.Clamp01(relaxForRegularityWeight);
						var equalAreaWeight = 1f - regularityWeight;

						var regularityRelaxedVertexPositions = new Vector3[topologyAsset.vertices.Count].AsVertexAttribute();
						var equalAreaRelaxedVertexPositions = new Vector3[topologyAsset.vertices.Count].AsVertexAttribute();
						var relaxedVertexPositions = regularityRelaxedVertexPositions;
						var faceCentroids = new Vector3[topologyAsset.internalFaces.Count].AsFaceAttribute();
						var faceCentroidAngles = new float[topologyAsset.faceEdges.Count].AsEdgeAttribute();
						var vertexAreas = new float[topologyAsset.vertices.Count].AsVertexAttribute();
						relaxIterationFunction = () =>
						{
							SphericalManifoldUtility.RelaxVertexPositionsForRegularity(topologyAsset, vertexPositionsAsset, 1f, false, regularityRelaxedVertexPositions);
							SphericalManifoldUtility.RelaxForEqualArea(topologyAsset, vertexPositionsAsset, 1f, false, equalAreaRelaxedVertexPositions, faceCentroids, faceCentroidAngles, vertexAreas);
							for (int i = 0; i < relaxedVertexPositions.Count; ++i)
							{
								relaxedVertexPositions[i] = regularityRelaxedVertexPositions[i] * regularityWeight + equalAreaRelaxedVertexPositions[i] * equalAreaWeight;
							}
							var relaxationAmount = SphericalManifoldUtility.CalculateRelaxationAmount(vertexPositionsAsset, relaxedVertexPositions);
							for (int i = 0; i < vertexPositionsAsset.Count; ++i)
							{
								vertexPositionsAsset[i] = relaxedVertexPositions[i];
							}
							return relaxationAmount;
						};
					}

					repairFunction = () =>
					{
						return SphericalManifoldUtility.ValidateAndRepair(topologyAsset, vertexPositionsAsset, 1f, repairRate, false);
					};

					relaxationLoopFunction = TopologyRandomizer.CreateRelaxationLoopFunction(maxRelaxIterations, maxRepairIterations, relaxRelativePrecision, relaxIterationFunction, repairFunction);
				}
			}

			if (edgeWrapAsset == null)
			{
				TopologyRandomizer.Randomize(topologyAsset, passCount, frequency,
					minVertexNeighbors, maxVertexNeighbors, minFaceNeighbors, maxFaceNeighbors,
					lockBoundaryVertices, random, relaxationLoopFunction);
				EditorUtility.SetDirty(topologyAsset);
			}
			else
			{
				TopologyRandomizer.Randomize(topologyAsset, edgeWrapAsset, passCount, frequency,
					minVertexNeighbors, maxVertexNeighbors, minFaceNeighbors, maxFaceNeighbors,
					lockBoundaryVertices, random, relaxationLoopFunction);
				EditorUtility.SetDirty(topologyAsset);
				EditorUtility.SetDirty((Object)edgeWrapAsset);
			}

			if (vertexPositionsAsset != null)
			{
				EditorUtility.SetDirty((Object)vertexPositionsAsset);
			}
		}

		public override bool CanGenerate()
		{
			return (
				topology != null &&
				(surfaceDescriptor == null) == (edgeWrap == null));
		}
	}
}
