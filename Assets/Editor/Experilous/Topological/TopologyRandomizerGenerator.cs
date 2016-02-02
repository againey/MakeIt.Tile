using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Experilous.Randomization;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(TopologyCategory), "Topology Randomizer")]
	public class TopologyRandomizerGenerator : AssetGenerator
	{
		public enum SurfaceType
		{
			Planar,
			Spherical,
		}

		[AutoSelect] public AssetInputSlot topologyInputSlot;
		public AssetInputSlot vertexPositionsInputSlot;
		[AutoSelect] public AssetInputSlot edgeWrapInputSlot;
		[AutoSelect] [Label("Attribute Adapter")] public AssetInputSlot positionalAttributeAdapterInputSlot;

		public SurfaceType surfaceType = SurfaceType.Planar;

		public int passCount = 1;
		[Range(0f, 1f)] public float frequency = 0.1f;

		public AssetGeneratorRandomization randomization;

		[Range(2, 20)] public int minVertexNeighbors = 3;
		[Range(2, 20)] public int maxVertexNeighbors = 5;
		[Range(3, 20)] public int minFaceNeighbors = 3;
		[Range(3, 20)] public int maxFaceNeighbors = 7;

		[Label("Lock Boundaries")] public bool lockBoundaryPositions = true;

		[Range(0f, 1f)] public float relaxForRegularityWeight = 0.5f;

		[Range(0f, 1f)] public int maxRelaxIterations = 20;
		[Range(0f, 1f)] public float relaxRelativePrecision = 0.95f;
		public int maxRepairIterations = 20;
		public float repairRate = 0.5f;

		protected override void Initialize(bool reset = true)
		{
			// Inputs
			if (reset || topologyInputSlot == null) topologyInputSlot = AssetInputSlot.CreateRequiredMutating(this, typeof(Topology));
			if (reset || vertexPositionsInputSlot == null) vertexPositionsInputSlot = AssetInputSlot.CreateOptionalMutating(this, typeof(IVertexAttribute<Vector3>));
			if (reset || edgeWrapInputSlot == null) edgeWrapInputSlot = AssetInputSlot.CreateOptionalMutating(this, typeof(IEdgeAttribute<EdgeWrap>));
			if (reset || positionalAttributeAdapterInputSlot == null) positionalAttributeAdapterInputSlot = AssetInputSlot.CreateOptional(this, typeof(PositionalAttributeAdapter));

			// Fields
			randomization.Initialize(this, reset);
		}

		public override IEnumerable<AssetInputSlot> inputs
		{
			get
			{
				yield return topologyInputSlot;
				yield return vertexPositionsInputSlot;
				yield return edgeWrapInputSlot;
				yield return positionalAttributeAdapterInputSlot;
				foreach (var input in randomization.inputs) yield return input;
			}
		}

		public override IEnumerator BeginGeneration()
		{
			var topology = topologyInputSlot.GetAsset<Topology>();
			var vertexPositions = vertexPositionsInputSlot.source != null ? vertexPositionsInputSlot.GetAsset<IVertexAttribute<Vector3>>() : null;
			var edgeWrap = edgeWrapInputSlot.source != null ? edgeWrapInputSlot.GetAsset<IEdgeAttribute<EdgeWrap>>() : null;
			var positionalAttributeAdapter = positionalAttributeAdapterInputSlot.source != null
				? positionalAttributeAdapterInputSlot.GetAsset<PositionalAttributeAdapter>()
				: PositionalAttributeAdapter.Create();

			var random = new RandomUtility(randomization.GetRandomEngine());

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

			yield break;
		}

		public override float estimatedGenerationTime
		{
			get
			{
				return passCount * maxRelaxIterations * 0.1f;
			}
		}
	}
}
