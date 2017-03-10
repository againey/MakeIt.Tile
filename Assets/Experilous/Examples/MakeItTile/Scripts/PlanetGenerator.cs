#if false
using UnityEngine;
using System;
using System.Collections.Generic;
using Experilous.MakeItRandom;
using Experilous.Numerics;
using Experilous.MakeItTile;
using Experilous.MakeItColorful;
using Experilous.Topologies;

namespace Experilous.Examples.MakeItTile
{
#if UNITY_EDITOR
	[ExecuteInEditMode]
	[RequireComponent(typeof(GeneratorExecutive))]
	[RequireComponent(typeof(SphereGenerator))]
	[RequireComponent(typeof(RandomEngineGenerator))]
	public class PlanetGenerator : MonoBehaviour, IGenerator
	{
		public int tectonicPlateCount = 20;
		[Range(-1f, 2f)]
		public float tectonicPlateSizeUniformity = 0.5f;

		public float maximumDriftRate = 0.1f;
		public float maximumSpinRate = 0.1f;

		public float oceanicProbability = 0.7f;

		private SphereGenerator _sphereGenerator;
		private RandomEngineGenerator _randomEngineGenerator;

		public IEnumerable<IGenerator> dependencies
		{
			get
			{
				yield return _sphereGenerator;
				yield return _randomEngineGenerator;
			}
		}

		private struct TectonicPlateMotion
		{
			public Vector3 driftAxis;
			public float driftRate;
			public Vector3 spinAxis;
			public float spinRate;

			public TectonicPlateMotion(Vector3 driftAxis, float driftRate, Vector3 spinAxis, float spinRate)
			{
				this.driftAxis = driftAxis;
				this.driftRate = driftRate;
				this.spinAxis = spinAxis;
				this.spinRate = spinRate;
			}
		}

		private enum TectonicPlateCategory
		{
			Oceanic,
			Continental,
		}

		public struct TectonicPlateStress
		{
			public float pressure;
			public float shear;

			public TectonicPlateStress(float pressure, float shear)
			{
				this.pressure = pressure;
				this.shear = shear;
			}
		}

		private List<int> _tectonicPlateFirstFaceIndices;
		private List<TectonicPlateMotion> _tectonicPlateMotions;
		private List<TectonicPlateCategory> _tectonicPlateCategories;
		private List<Color> _tectonicPlateColors;

		private IFaceAttribute<int> _faceTectonicPlateIndices;
		private IFaceAttribute<int> _faceNextTectonicPlateFaceIndices;
		private IFaceAttribute<Vector3> _faceTectonicPlateMotions;
		private IEdgeAttribute<Vector3> _edgeTectonicPlateMotions;
		private IEdgeAttribute<TectonicPlateStress> _edgeTectonicPlateStresses;

		private IFaceAttribute<float> _faceElevations;
		private IFaceAttribute<float> _faceTemperatures;
		private IFaceAttribute<Vector3> _faceWindVelocities;
		private IFaceAttribute<float> _facePrecipitations;

		public IList<Color> tectonicPlateColors { get { return _tectonicPlateColors; } }
		public List<int> tectonicPlateFirstFaceIndices { get { return _tectonicPlateFirstFaceIndices; } }

		public IFaceAttribute<int> faceTectonicPlateIndices { get { return _faceTectonicPlateIndices; } }
		public IFaceAttribute<int> faceNextTectonicPlateFaceIndices { get { return _faceNextTectonicPlateFaceIndices; } }
		public IFaceAttribute<Vector3> faceTectonicPlateMotions { get { return _faceTectonicPlateMotions; } }
		public IEdgeAttribute<Vector3> edgeTectonicPlateMotions { get { return _edgeTectonicPlateMotions; } }
		public IEdgeAttribute<TectonicPlateStress> edgeTectonicPlateStresses { get { return _edgeTectonicPlateStresses; } }

		protected void OnEnable()
		{
			_sphereGenerator = GetComponent<SphereGenerator>();
			_randomEngineGenerator = GetComponent<RandomEngineGenerator>();
		}

		public void Generate()
		{
			var surface = _sphereGenerator.surface;
			var topology = _sphereGenerator.topology;
			var vertexPositions = _sphereGenerator.vertexPositions;
			var faceCentroids = _sphereGenerator.faceCentroids;
			var edgeMidpoints = _sphereGenerator.edgeMidpoints;
			var random = _randomEngineGenerator.random;

			BuildTectonicPlates(topology, random, faceCentroids);
			CalculateTectonicPlateStresses(surface, topology, vertexPositions, faceCentroids, edgeMidpoints);
		}

		private class TectonicPlateFaceQueue : Containers.IQueue<FaceVisitor<int>.QueueItem>
		{
			private IRandom _random;
			private float _exponent;
			private List<FaceVisitor<int>.QueueItem>[] _queueItems;
			private float[] _weights;
			private int _count;

			public TectonicPlateFaceQueue(IRandom random, int tectonicPlateCount, float uniformity)
			{
				_random = random;
				_exponent = 1f - uniformity;
				_queueItems = new List<FaceVisitor<int>.QueueItem>[tectonicPlateCount];
				_weights = new float[tectonicPlateCount];
				for (int i = 0; i < tectonicPlateCount; ++i)
				{
					_queueItems[i] = new List<FaceVisitor<int>.QueueItem>();
					_weights[i] = 0f;
				}
			}

			public int Count
			{
				get
				{
					return _count;
				}
			}

			public bool isEmpty
			{
				get
				{
					return _count == 0;
				}
			}

			public void Clear()
			{
				foreach (var list in _queueItems)
				{
					list.Clear();
				}
			}

			private void RecalcuateWeight(int tectonicPlateIndex)
			{
				int itemCount = _queueItems[tectonicPlateIndex].Count;
				_weights[tectonicPlateIndex] = itemCount > 0 ? Mathf.Pow(itemCount, _exponent) : 0f;
			}

			public FaceVisitor<int>.QueueItem Pop()
			{
				if (_count == 0) throw new InvalidOperationException();
				int tectonicPlateIndex = _random.WeightedIndex(_weights);
				var tectonicPlateList = _queueItems[tectonicPlateIndex];
				int queueItemIndex = _random.Index(tectonicPlateList.Count);
				var poppedItem = tectonicPlateList[queueItemIndex];
				var lastIndex = tectonicPlateList.Count - 1;
				tectonicPlateList[queueItemIndex] = tectonicPlateList[lastIndex];
				tectonicPlateList.RemoveAt(lastIndex);
				--_count;
				RecalcuateWeight(tectonicPlateIndex);
				return poppedItem;
			}

			public void Push(FaceVisitor<int>.QueueItem item)
			{
				_queueItems[item.state].Add(item);
				++_count;
				RecalcuateWeight(item.state);
			}
		}

		private void BuildTectonicPlates(Topology topology, IRandom random, IFaceAttribute<Vector3> facePositions)
		{
			_tectonicPlateMotions = new List<TectonicPlateMotion>(tectonicPlateCount);
			_tectonicPlateCategories = new List<TectonicPlateCategory>(tectonicPlateCount);
			_tectonicPlateColors = new List<Color>(tectonicPlateCount);

			var categorySelector = random.MakeWeightedEnumGenerator((TectonicPlateCategory category) =>
			{
				switch (category)
				{
					case TectonicPlateCategory.Oceanic: return oceanicProbability;
				}
				return 1f - oceanicProbability;
			});

			_tectonicPlateFirstFaceIndices = new List<int>(tectonicPlateCount);
			var rootFaces = new List<FaceVisitor<int>.FaceStatePair>(tectonicPlateCount);
			for (int tectonicPlateIndex = 0; tectonicPlateIndex < tectonicPlateCount; ++tectonicPlateIndex)
			{
				int faceIndex;
				do
				{
					faceIndex = random.Index(topology.internalFaces.Count);
				} while (_tectonicPlateFirstFaceIndices.Contains(faceIndex));
				_tectonicPlateFirstFaceIndices.Add(faceIndex);
				var faceIndices = new List<int>();
				faceIndices.Add(faceIndex);
				rootFaces.Add(new FaceVisitor<int>.FaceStatePair(new Topology.Face(topology, faceIndex), tectonicPlateIndex));

				_tectonicPlateMotions.Add(new TectonicPlateMotion(
					random.UnitVector3(),
					random.SignedFloatCC() * maximumDriftRate,
					facePositions[faceIndex].normalized,
					random.SignedFloatCC() * maximumSpinRate));
				_tectonicPlateCategories.Add(categorySelector.Next());
				_tectonicPlateColors.Add(new ColorHSY(random.FloatCO(), random.RangeCC(0.5f, 1f), random.RangeCC(0.25f, 0.75f)));
			}

			_faceTectonicPlateIndices = new int[topology.faces.Count].AsFaceAttribute();
			FaceVisitor<int>.Visit(rootFaces, new TectonicPlateFaceQueue(random, tectonicPlateCount, tectonicPlateSizeUniformity), (FaceVisitor<int> visitor) =>
			{
				_faceTectonicPlateIndices[visitor.face] = visitor.state;
				visitor.VisitInternalNeighbors(visitor.state);
			});

			var tectonicPlateLastFaceIndices = new List<int>(_tectonicPlateFirstFaceIndices);
			_faceNextTectonicPlateFaceIndices = new int[topology.internalFaces.Count].AsFaceAttribute();
			TopologyVisitor.VisitFacesInBreadthFirstOrder(rootFaces, (FaceVisitor<int> visitor) =>
			{
				int lastFaceIndex = tectonicPlateLastFaceIndices[visitor.state];
				_faceNextTectonicPlateFaceIndices[lastFaceIndex] = visitor.face.index;
				tectonicPlateLastFaceIndices[visitor.state] = visitor.face.index;

				foreach (var edge in visitor.face.edges)
				{
					if (!edge.isExternal && _faceTectonicPlateIndices[edge.face] == visitor.state)
					{
						visitor.VisitNeighbor(edge.face, visitor.state);
					}
				}
			});

			for (int tectonicPlateIndex = 0; tectonicPlateIndex < tectonicPlateCount; ++tectonicPlateIndex)
			{
				int firstFaceIndex = _tectonicPlateFirstFaceIndices[tectonicPlateIndex];
				int lastFaceIndex = tectonicPlateLastFaceIndices[tectonicPlateIndex];
				_faceNextTectonicPlateFaceIndices[lastFaceIndex] = firstFaceIndex;
			}
		}

		private static Vector3 CalculateAxialMotion(Vector3 axis, float rate, Vector3 position)
		{
			var direction = Vector3.Cross(axis, position);
			var sqrMagnitude = direction.sqrMagnitude;
			if (sqrMagnitude > 0f)
			{
				var positionAxisDistance = (position - Geometry.ProjectOnto(position, axis)).magnitude;
				return (direction / Mathf.Sqrt(sqrMagnitude)) * (rate * positionAxisDistance);
			}
			else
			{
				return Vector3.zero;
			}
		}

		private void CalculateTectonicPlateStresses(SphericalSurface surface, Topology topology, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> faceCentroids, IEdgeAttribute<Vector3> edgeMidpoints)
		{
			_faceTectonicPlateMotions = new Vector3[topology.internalFaces.Count].AsFaceAttribute();
			foreach (var face in topology.internalFaces)
			{
				var tectonicPlateMotion = _tectonicPlateMotions[_faceTectonicPlateIndices[face]];
				var position = faceCentroids[face];
				var drift = CalculateAxialMotion(tectonicPlateMotion.driftAxis, tectonicPlateMotion.driftRate, position);
				var spin = CalculateAxialMotion(tectonicPlateMotion.spinAxis, tectonicPlateMotion.spinRate, position);
				_faceTectonicPlateMotions[face] = drift + spin;
			}

			_edgeTectonicPlateMotions = new Vector3[topology.faceEdges.Count].AsEdgeAttribute();
			foreach (var edge in topology.faceEdges)
			{
				int nearTectonicPlateIndex = _faceTectonicPlateIndices[edge.nearFace];
				int farTectonicPlateIndex = _faceTectonicPlateIndices[edge.farFace];
				if (nearTectonicPlateIndex != farTectonicPlateIndex)
				{
					var tectonicPlateMotion = _tectonicPlateMotions[nearTectonicPlateIndex];
					var position = edgeMidpoints[edge];
					var drift = CalculateAxialMotion(tectonicPlateMotion.driftAxis, tectonicPlateMotion.driftRate, position);
					var spin = CalculateAxialMotion(tectonicPlateMotion.spinAxis, tectonicPlateMotion.spinRate, position);
					_edgeTectonicPlateMotions[edge] = drift + spin;
				}
			}

			_edgeTectonicPlateStresses = new TectonicPlateStress[topology.faceEdges.Count].AsEdgeAttribute();
			foreach (var edge in topology.faceEdges)
			{
				int nearTectonicPlateIndex = _faceTectonicPlateIndices[edge.nearFace];
				int farTectonicPlateIndex = _faceTectonicPlateIndices[edge.farFace];
				if (nearTectonicPlateIndex != farTectonicPlateIndex && edge.isFirstTwin)
				{
					var boundaryVector = vertexPositions[edge] - vertexPositions[edge.twin];
					var boundaryNormal = Vector3.Cross(boundaryVector, surface.GetNormal(edgeMidpoints[edge]));
					var firstMotion = _edgeTectonicPlateMotions[edge];
					var secondMotion = _edgeTectonicPlateMotions[edge.twin];
					var relativeMotion = secondMotion - firstMotion;

					var pressureVector = Geometry.ProjectOnto(relativeMotion, boundaryNormal);
					var pressure = pressureVector.magnitude;
					pressure = 2f / (1f + Mathf.Exp(-pressure * 33.33f)) - 1f;
					if (Vector3.Dot(pressureVector, boundaryNormal) < 0f) pressure = -pressure;

					var shearVector = Geometry.ProjectOnto(relativeMotion, boundaryVector);
					var shear = shearVector.magnitude;
					shear = 2f / (1f + Mathf.Exp(-shear * 33.33f)) - 1f;

					_edgeTectonicPlateStresses[edge] = new TectonicPlateStress(pressure, shear);
				}
			}
		}
	}
#endif
}
#endif