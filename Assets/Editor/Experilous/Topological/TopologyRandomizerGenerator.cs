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
		[AutoSelect] public AssetInputSlot surfaceInputSlot;
		[AutoSelect] public AssetInputSlot topologyInputSlot;
		public AssetInputSlot vertexPositionsInputSlot;

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
			if (reset || surfaceInputSlot == null) surfaceInputSlot = AssetInputSlot.CreateRequiredMutating(this, typeof(Surface));
			if (reset || topologyInputSlot == null) topologyInputSlot = AssetInputSlot.CreateRequiredMutating(this, typeof(Topology));
			if (reset || vertexPositionsInputSlot == null) vertexPositionsInputSlot = AssetInputSlot.CreateOptionalMutating(this, typeof(IVertexAttribute<Vector3>));

			// Fields
			randomization.Initialize(this, reset);
		}

		public override IEnumerable<AssetInputSlot> inputs
		{
			get
			{
				yield return surfaceInputSlot;
				yield return topologyInputSlot;
				yield return vertexPositionsInputSlot;
				foreach (var input in randomization.inputs) yield return input;
			}
		}

		public override IEnumerator BeginGeneration()
		{
			var surface = surfaceInputSlot.GetAsset<Surface>();
			var topology = topologyInputSlot.GetAsset<Topology>();
			var vertexPositions = vertexPositionsInputSlot.source != null ? vertexPositionsInputSlot.GetAsset<IVertexAttribute<Vector3>>() : null;

			var random = new RandomUtility(randomization.GetRandomEngine());

			System.Func<float> relaxIterationFunction = null;
			System.Func<bool> repairFunction = null;
			System.Action relaxationLoopFunction = null;

			if (vertexPositions != null)
			{
				if (surface is PlanarSurface)
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
						var faceCentroids = PositionalFaceAttribute.Create(surface, topology.internalFaces.Count);
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
							PlanarManifoldUtility.RelaxVertexPositionsForEqualArea(topology, vertexPositions, totalArea, lockBoundaryPositions, relaxedVertexPositions, faceCentroids, vertexAreas);
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
						var faceCentroids = PositionalFaceAttribute.Create(surface, topology.internalFaces.Count);
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
							PlanarManifoldUtility.RelaxVertexPositionsForEqualArea(topology, vertexPositions, totalArea, lockBoundaryPositions, equalAreaRelaxedVertexPositions, faceCentroids, vertexAreas);
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
				else if (surface is SphericalSurface)
				{
					var sphericalSurface = (SphericalSurface)surface;
					if (relaxForRegularityWeight >= 1.0f)
					{
						var relaxedVertexPositions = new Vector3[topology.vertices.Count].AsVertexAttribute();
						relaxIterationFunction = () =>
						{
							SphericalManifoldUtility.RelaxVertexPositionsForRegularity(topology, vertexPositions, sphericalSurface.radius, lockBoundaryPositions, relaxedVertexPositions);
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
						var faceCentroids = PositionalFaceAttribute.Create(surface, topology.internalFaces.Count);
						var faceCentroidAngles = new float[topology.faceEdges.Count].AsEdgeAttribute();
						var vertexAreas = new float[topology.vertices.Count].AsVertexAttribute();
						relaxIterationFunction = () =>
						{
							SphericalManifoldUtility.RelaxVertexPositionsForEqualArea(topology, vertexPositions, sphericalSurface.radius, lockBoundaryPositions, relaxedVertexPositions, faceCentroids, faceCentroidAngles, vertexAreas);
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
						var faceCentroids = PositionalFaceAttribute.Create(surface, topology.internalFaces.Count);
						var faceCentroidAngles = new float[topology.faceEdges.Count].AsEdgeAttribute();
						var vertexAreas = new float[topology.vertices.Count].AsVertexAttribute();
						relaxIterationFunction = () =>
						{
							SphericalManifoldUtility.RelaxVertexPositionsForRegularity(topology, vertexPositions, sphericalSurface.radius, lockBoundaryPositions, regularityRelaxedVertexPositions);
							SphericalManifoldUtility.RelaxVertexPositionsForEqualArea(topology, vertexPositions, sphericalSurface.radius, lockBoundaryPositions, equalAreaRelaxedVertexPositions, faceCentroids, faceCentroidAngles, vertexAreas);
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
						return SphericalManifoldUtility.ValidateAndRepair(topology, vertexPositions, sphericalSurface.radius, repairRate, lockBoundaryPositions);
					};

					relaxationLoopFunction = TopologyRandomizer.CreateRelaxationLoopFunction(maxRelaxIterations, maxRepairIterations, relaxRelativePrecision, relaxIterationFunction, repairFunction);
				}
			}

			var waitHandle = collection.GenerateConcurrently(() =>
			{
				TopologyRandomizer.Randomize(topology, passCount, frequency,
					minVertexNeighbors, maxVertexNeighbors, minFaceNeighbors, maxFaceNeighbors,
					lockBoundaryPositions, random, relaxationLoopFunction);
			});
			while (waitHandle.WaitOne(10) == false)
			{
				yield return null;
			}

			EditorUtility.SetDirty(topology);

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
