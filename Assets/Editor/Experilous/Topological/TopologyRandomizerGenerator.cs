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

		public bool lockBoundaryPositions = true;

		public float relaxForRegularityWeight = 0.5f;

		public int maxRelaxIterations = 20;
		public float relaxRelativePrecision = 0.95f;
		public int maxRepairIterations = 20;
		public float repairRate = 0.5f;

		public AssetDescriptor topologyDescriptor;
		public AssetDescriptor positionalAttributeAdapterDescriptor;
		public AssetDescriptor vertexPositionsDescriptor;
		public AssetDescriptor edgeWrapDescriptor;

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
				if (topologyDescriptor != null) yield return topologyDescriptor;
				if (positionalAttributeAdapterDescriptor != null) yield return positionalAttributeAdapterDescriptor;
				if (vertexPositionsDescriptor != null) yield return vertexPositionsDescriptor;
				if (edgeWrapDescriptor != null) yield return edgeWrapDescriptor;
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
			if (!ResetMemberDependency(dependency, ref topologyDescriptor, ref positionalAttributeAdapterDescriptor, ref vertexPositionsDescriptor, ref edgeWrapDescriptor))
			{
				throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this vertex normals generator.", dependency.name, dependency.GetType().Name), "dependency");
			}
		}

		public override void Generate()
		{
			var topology = topologyDescriptor.GetAsset<Topology>();
			var positionalAttributeAdapter = positionalAttributeAdapterDescriptor != null
				? positionalAttributeAdapterDescriptor.GetAsset<PositionalAttributeAdapter>()
				: PositionalAttributeAdapter.Create();
			var vertexPositions = vertexPositionsDescriptor != null ? vertexPositionsDescriptor.GetAsset<IVertexAttribute<Vector3>>() : null;
			var edgeWrap = edgeWrapDescriptor != null ? edgeWrapDescriptor.GetAsset<IEdgeAttribute<EdgeWrap>>() : null;

			var random = new Random(new NativeRandomEngine(randomSeed));

			System.Func<float> relaxIterationFunction = null;
			System.Func<bool> repairFunction = null;
			System.Action relaxationLoopFunction = null;

			if (vertexPositions != null)
			{
				if (surfaceType == SurfaceType.Planar)
				{
					if (relaxForRegularityWeight >= 1.0f)
					{
						var relaxedVertexPositions = new Vector3[topology.vertices.Count].AsVertexAttribute();
						relaxIterationFunction = () =>
						{
							PlanarManifoldUtility.RelaxVertexPositionsForRegularity(topology, vertexPositions, lockBoundaryPositions, relaxedVertexPositions);
							var relaxationAmount = PlanarManifoldUtility.CalculateRelaxationAmount(vertexPositions, relaxedVertexPositions);
							for (int i = 0; i < vertexPositions.Count; ++i)
							{
								vertexPositions[i] = relaxedVertexPositions[i];
							}
							return relaxationAmount;
						};
					}
					else if (relaxForRegularityWeight <= 0.0f)
					{
						var relaxedVertexPositions = new Vector3[topology.vertices.Count].AsVertexAttribute();
						var faceCentroids = positionalAttributeAdapter.Adapt(new Vector3[topology.internalFaces.Count].AsFaceAttribute());
						var vertexAreas = new float[topology.vertices.Count].AsVertexAttribute();

						FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions, faceCentroids);
						VertexAttributeUtility.CalculateVertexAreasFromVertexPositionsAndFaceCentroids(topology.vertices, vertexPositions, faceCentroids, vertexAreas);

						var totalArea = 0f;
						foreach (var vertexArea in vertexAreas)
						{
							totalArea += vertexArea;
						}

						relaxIterationFunction = () =>
						{
							PlanarManifoldUtility.RelaxForEqualArea(topology, vertexPositions, totalArea, lockBoundaryPositions, relaxedVertexPositions, faceCentroids, vertexAreas);
							var relaxationAmount = PlanarManifoldUtility.CalculateRelaxationAmount(vertexPositions, relaxedVertexPositions);
							for (int i = 0; i < vertexPositions.Count; ++i)
							{
								vertexPositions[i] = relaxedVertexPositions[i];
							}
							return relaxationAmount;
						};
					}
					else
					{
						var regularityWeight = Mathf.Clamp01(relaxForRegularityWeight);
						var equalAreaWeight = 1f - regularityWeight;

						var regularityRelaxedVertexPositions = new Vector3[topology.vertices.Count].AsVertexAttribute();
						var equalAreaRelaxedVertexPositions = new Vector3[topology.vertices.Count].AsVertexAttribute();
						var relaxedVertexPositions = regularityRelaxedVertexPositions;
						var faceCentroids = positionalAttributeAdapter.Adapt(new Vector3[topology.internalFaces.Count].AsFaceAttribute());
						var vertexAreas = new float[topology.vertices.Count].AsVertexAttribute();

						FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions, faceCentroids);
						VertexAttributeUtility.CalculateVertexAreasFromVertexPositionsAndFaceCentroids(topology.vertices, vertexPositions, faceCentroids, vertexAreas);

						var totalArea = 0f;
						foreach (var vertexArea in vertexAreas)
						{
							totalArea += vertexArea;
						}

						relaxIterationFunction = () =>
						{
							PlanarManifoldUtility.RelaxVertexPositionsForRegularity(topology, vertexPositions, lockBoundaryPositions, regularityRelaxedVertexPositions);
							PlanarManifoldUtility.RelaxForEqualArea(topology, vertexPositions, totalArea, lockBoundaryPositions, equalAreaRelaxedVertexPositions, faceCentroids, vertexAreas);
							for (int i = 0; i < relaxedVertexPositions.Count; ++i)
							{
								relaxedVertexPositions[i] = regularityRelaxedVertexPositions[i] * regularityWeight + equalAreaRelaxedVertexPositions[i] * equalAreaWeight;
							}
							var relaxationAmount = PlanarManifoldUtility.CalculateRelaxationAmount(vertexPositions, relaxedVertexPositions);
							for (int i = 0; i < vertexPositions.Count; ++i)
							{
								vertexPositions[i] = relaxedVertexPositions[i];
							}
							return relaxationAmount;
						};
					}

					repairFunction = () =>
					{
						return PlanarManifoldUtility.ValidateAndRepair(topology, vertexPositions, repairRate, lockBoundaryPositions);
					};

					relaxationLoopFunction = TopologyRandomizer.CreateRelaxationLoopFunction(maxRelaxIterations, maxRepairIterations, relaxRelativePrecision, relaxIterationFunction, repairFunction);
				}
				else if (surfaceType == SurfaceType.Spherical)
				{
					if (relaxForRegularityWeight >= 1.0f)
					{
						var relaxedVertexPositions = new Vector3[topology.vertices.Count].AsVertexAttribute();
						relaxIterationFunction = () =>
						{
							SphericalManifoldUtility.RelaxVertexPositionsForRegularity(topology, vertexPositions, 1f, lockBoundaryPositions, relaxedVertexPositions);
							var relaxationAmount = SphericalManifoldUtility.CalculateRelaxationAmount(vertexPositions, relaxedVertexPositions);
							for (int i = 0; i < vertexPositions.Count; ++i)
							{
								vertexPositions[i] = relaxedVertexPositions[i];
							}
							return relaxationAmount;
						};
					}
					else if (relaxForRegularityWeight <= 0.0f)
					{
						var relaxedVertexPositions = new Vector3[topology.vertices.Count].AsVertexAttribute();
						var faceCentroids = positionalAttributeAdapter.Adapt(new Vector3[topology.internalFaces.Count].AsFaceAttribute());
						var faceCentroidAngles = new float[topology.faceEdges.Count].AsEdgeAttribute();
						var vertexAreas = new float[topology.vertices.Count].AsVertexAttribute();
						relaxIterationFunction = () =>
						{
							SphericalManifoldUtility.RelaxForEqualArea(topology, vertexPositions, 1f, lockBoundaryPositions, relaxedVertexPositions, faceCentroids, faceCentroidAngles, vertexAreas);
							var relaxationAmount = SphericalManifoldUtility.CalculateRelaxationAmount(vertexPositions, relaxedVertexPositions);
							for (int i = 0; i < vertexPositions.Count; ++i)
							{
								vertexPositions[i] = relaxedVertexPositions[i];
							}
							return relaxationAmount;
						};
					}
					else
					{
						var regularityWeight = Mathf.Clamp01(relaxForRegularityWeight);
						var equalAreaWeight = 1f - regularityWeight;

						var regularityRelaxedVertexPositions = new Vector3[topology.vertices.Count].AsVertexAttribute();
						var equalAreaRelaxedVertexPositions = new Vector3[topology.vertices.Count].AsVertexAttribute();
						var relaxedVertexPositions = regularityRelaxedVertexPositions;
						var faceCentroids = positionalAttributeAdapter.Adapt(new Vector3[topology.internalFaces.Count].AsFaceAttribute());
						var faceCentroidAngles = new float[topology.faceEdges.Count].AsEdgeAttribute();
						var vertexAreas = new float[topology.vertices.Count].AsVertexAttribute();
						relaxIterationFunction = () =>
						{
							SphericalManifoldUtility.RelaxVertexPositionsForRegularity(topology, vertexPositions, 1f, lockBoundaryPositions, regularityRelaxedVertexPositions);
							SphericalManifoldUtility.RelaxForEqualArea(topology, vertexPositions, 1f, lockBoundaryPositions, equalAreaRelaxedVertexPositions, faceCentroids, faceCentroidAngles, vertexAreas);
							for (int i = 0; i < relaxedVertexPositions.Count; ++i)
							{
								relaxedVertexPositions[i] = regularityRelaxedVertexPositions[i] * regularityWeight + equalAreaRelaxedVertexPositions[i] * equalAreaWeight;
							}
							var relaxationAmount = SphericalManifoldUtility.CalculateRelaxationAmount(vertexPositions, relaxedVertexPositions);
							for (int i = 0; i < vertexPositions.Count; ++i)
							{
								vertexPositions[i] = relaxedVertexPositions[i];
							}
							return relaxationAmount;
						};
					}

					repairFunction = () =>
					{
						return SphericalManifoldUtility.ValidateAndRepair(topology, vertexPositions, 1f, repairRate, lockBoundaryPositions);
					};

					relaxationLoopFunction = TopologyRandomizer.CreateRelaxationLoopFunction(maxRelaxIterations, maxRepairIterations, relaxRelativePrecision, relaxIterationFunction, repairFunction);
				}
			}

			if (edgeWrap == null)
			{
				TopologyRandomizer.Randomize(topology, passCount, frequency,
					minVertexNeighbors, maxVertexNeighbors, minFaceNeighbors, maxFaceNeighbors,
					lockBoundaryPositions, random, relaxationLoopFunction);
				EditorUtility.SetDirty(topology);
			}
			else
			{
				TopologyRandomizer.Randomize(topology, edgeWrap, passCount, frequency,
					minVertexNeighbors, maxVertexNeighbors, minFaceNeighbors, maxFaceNeighbors,
					lockBoundaryPositions, random, relaxationLoopFunction);
				EditorUtility.SetDirty(topology);
				EditorUtility.SetDirty((Object)edgeWrap);
			}

			if (vertexPositions != null)
			{
				EditorUtility.SetDirty((Object)vertexPositions);
			}
		}

		public override bool CanGenerate()
		{
			return (
				topologyDescriptor != null);
		}
	}
}
