/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Experilous.MakeItGenerate;
using Experilous.Core;

namespace Experilous.MakeItTile
{
	[Generator(typeof(TopologyGeneratorCollection), "Topology/Randomizer")]
	public class TopologyRandomizerGenerator : Generator, ISerializationCallbackReceiver
	{
		[AutoSelect] public InputSlot surfaceInputSlot;
		[AutoSelect] public InputSlot topologyInputSlot;
		public InputSlot vertexPositionsInputSlot;

		public int passCount;
		[Range(0f, 1f)] public float frequency;

		public RandomnessDescriptor randomness;

		[Range(2, 20)] public int minVertexNeighbors;
		[Range(2, 20)] public int maxVertexNeighbors;
		[Range(3, 20)] public int minFaceNeighbors;
		[Range(3, 20)] public int maxFaceNeighbors;

		[Label("Lock Boundaries")] public bool lockBoundaryPositions;

		[Range(0f, 1f)] public float relaxForRegularityWeight;

		[Range(0f, 1f)] public int maxRelaxIterations;
		[Range(0f, 1f)] public float relaxRelativePrecision;
		public int maxRepairIterations;
		public float repairRate;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequiredMutating<Surface>(ref surfaceInputSlot, this);
			InputSlot.CreateOrResetRequiredMutating<Topology>(ref topologyInputSlot, this);
			InputSlot.CreateOrResetOptionalMutating<IVertexAttribute<Vector3>>(ref vertexPositionsInputSlot, this);

			// Fields
			passCount = 1;
			frequency = 0.1f;

			randomness.Initialize(this);

			minVertexNeighbors = 3;
			maxVertexNeighbors = 5;
			minFaceNeighbors = 3;
			maxFaceNeighbors = 7;

			lockBoundaryPositions = true;

			relaxForRegularityWeight = 0.5f;

			maxRelaxIterations = 20;
			relaxRelativePrecision = 0.95f;
			maxRepairIterations = 20;
			repairRate = 0.5f;
		}

		public void OnAfterDeserialize()
		{
			InputSlot.ResetAssetTypeIfNull<Surface>(surfaceInputSlot);
			InputSlot.ResetAssetTypeIfNull<Topology>(topologyInputSlot);
			InputSlot.ResetAssetTypeIfNull<IVertexAttribute<Vector3>>(vertexPositionsInputSlot);
			randomness.ResetIfBroken(this);
		}

		public void OnBeforeSerialize()
		{
		}

		protected override void OnUpdate()
		{
			randomness.Update();
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return surfaceInputSlot;
				yield return topologyInputSlot;
				yield return vertexPositionsInputSlot;
				yield return randomness.randomSeedInputSlot;
			}
		}

		public override IEnumerator BeginGeneration()
		{
			var surface = surfaceInputSlot.GetAsset<Surface>();
			var topology = topologyInputSlot.GetAsset<Topology>();
			var vertexPositions = vertexPositionsInputSlot.source != null ? vertexPositionsInputSlot.GetAsset<IVertexAttribute<Vector3>>() : null;

			var random = randomness.GetRandom();

			System.Func<float> relaxIterationFunction = null;
			System.Func<bool> repairFunction = null;
			System.Action relaxationLoopFunction = null;

			if (vertexPositions != null)
			{
				if (surface is QuadrilateralSurface)
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
						return PlanarManifoldUtility.ValidateAndRepair(topology, ((QuadrilateralSurface)surface).normal, vertexPositions, repairRate, lockBoundaryPositions);
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

			yield return executive.GenerateConcurrently(() =>
			{
				TopologyRandomizer.Randomize(topology, passCount, frequency,
					minVertexNeighbors, maxVertexNeighbors, minFaceNeighbors, maxFaceNeighbors,
					lockBoundaryPositions, random, relaxationLoopFunction);
			});

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
