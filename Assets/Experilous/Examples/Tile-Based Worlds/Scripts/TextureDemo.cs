/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using Experilous.Randomization;
using Experilous.Topological;
using System;

namespace Experilous.Examples.Topological
{
	public class TextureDemo : MonoBehaviour
	{
		public Toggle squareGridToggle;
		public Toggle hexGrid1Toggle;
		public Toggle hexGrid2Toggle;
		public Toggle jumbledGridToggle;

		public Toggle expandToggle;
		public Toggle shrinkToggle;
		public Toggle drawWireframeToggle;

		public MeshFilter largeTextureMeshPrefab;
		public MeshFilter smallTextureMeshPrefab;

		public Material wireframeMaterial;

		public Vector2 uvScale = new Vector2(1f, 1f);

		private Topology _topology;
		private IVertexAttribute<Vector3> _vertexPositions;

		protected void Start()
		{
			Camera.onPostRender += (Camera camera) =>
			{
				if (_topology != null && drawWireframeToggle.isOn)
				{
					DrawWireframe();
				}
			};
		}

		protected void DrawWireframe()
		{
			wireframeMaterial.SetPass(0);

			GL.PushMatrix();
			GL.MultMatrix(transform.localToWorldMatrix);
			GL.Begin(GL.LINES);
			foreach (var edge in _topology.faceEdges)
			{
				if (edge.nearFace.isInternal && (edge.farFace.isExternal || edge.isFirstTwin))
				{
					GL.Vertex(_vertexPositions[edge.prev]);
					GL.Vertex(_vertexPositions[edge]);
				}
			}
			GL.End();
			GL.PopMatrix();
		}

		private void CreateSquareGrid(out Topology topology, out IVertexAttribute<Vector3> vertexPositions)
		{
			Vector3[] vertexPositionsArray;
			var surface = RectangularQuadGrid.Create(
				new PlanarDescriptor(Vector3.right, Vector3.up),
				new Index2D(10, 10));
			topology = surface.CreateManifold(out vertexPositionsArray);
			vertexPositions = PositionalVertexAttribute.Create(surface, vertexPositionsArray);
		}

		private void CreateHexGrid1(out Topology topology, out IVertexAttribute<Vector3> vertexPositions)
		{
			Vector3[] vertexPositionsArray;
			var surface = RectangularHexGrid.Create(
				new PlanarDescriptor(Vector3.right, Vector3.up),
				new HexGridDescriptor(HexGridAxisStyles.Straight, HexGridAxisStyles.Staggered, HexGridAxisRelations.Obtuse, true),
				new Index2D(10, 11));
			topology = surface.CreateManifold(out vertexPositionsArray);
			vertexPositions = PositionalVertexAttribute.Create(surface, vertexPositionsArray);
		}

		private void CreateHexGrid2(out Topology topology, out IVertexAttribute<Vector3> vertexPositions)
		{
			Vector3[] vertexPositionsArray;
			var surface = RectangularHexGrid.Create(
				new PlanarDescriptor(Vector3.right, Vector3.up),
				new HexGridDescriptor(HexGridAxisStyles.Staggered, HexGridAxisStyles.Straight, HexGridAxisRelations.Obtuse, true),
				new Index2D(11, 10));
			topology = surface.CreateManifold(out vertexPositionsArray);
			vertexPositions = PositionalVertexAttribute.Create(surface, vertexPositionsArray);
		}

		private void CreateJumbledGrid(out Topology topology, out IVertexAttribute<Vector3> vertexPositions)
		{
			Vector3[] vertexPositionsArray;
			var surface = RectangularHexGrid.Create(
				new PlanarDescriptor(Vector3.right, Vector3.up),
				new HexGridDescriptor(HexGridAxisStyles.Staggered, HexGridAxisStyles.Straight, HexGridAxisRelations.Obtuse, true),
				new Index2D(11, 10));
			topology = surface.CreateManifold(out vertexPositionsArray);
			vertexPositions = PositionalVertexAttribute.Create(surface, vertexPositionsArray);

			var regularityWeight = 0.5f;
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

			var localTopology = topology;
			var localVertexPositions = vertexPositions;

			Func<float> relaxIterationFunction = () =>
			{
				PlanarManifoldUtility.RelaxVertexPositionsForRegularity(localTopology, localVertexPositions, false, regularityRelaxedVertexPositions);
				PlanarManifoldUtility.RelaxVertexPositionsForEqualArea(localTopology, localVertexPositions, totalArea, false, equalAreaRelaxedVertexPositions, faceCentroids, vertexAreas);
				for (int i = 0; i < relaxedVertexPositions.Count; ++i)
				{
					relaxedVertexPositions[i] = regularityRelaxedVertexPositions[i] * regularityWeight + equalAreaRelaxedVertexPositions[i] * equalAreaWeight;
				}
				var relaxationAmount = PlanarManifoldUtility.CalculateRelaxationAmount(localVertexPositions, relaxedVertexPositions);
				for (int i = 0; i < localVertexPositions.Count; ++i)
				{
					localVertexPositions[i] = relaxedVertexPositions[i];
				}
				return relaxationAmount;
			};

			Func<bool> repairFunction = () =>
			{
				return PlanarManifoldUtility.ValidateAndRepair(localTopology, localVertexPositions, 0.5f, false);
			};

			Action relaxationLoopFunction = TopologyRandomizer.CreateRelaxationLoopFunction(20, 20, 0.95f, relaxIterationFunction, repairFunction);

			TopologyRandomizer.Randomize(
				topology, 1, 0.2f,
				3, 3, 5, 7, false,
				new RandomUtility(XorShift128Plus.Create(23478)),
				relaxationLoopFunction);
		}

		private void CreateGrid(out Topology topology, out IVertexAttribute<Vector3> vertexPositions, out IFaceAttribute<Vector3> facePositions)
		{
			if (squareGridToggle.isOn)
			{
				CreateSquareGrid(out topology, out vertexPositions);
			}
			else if (hexGrid1Toggle.isOn)
			{
				CreateHexGrid1(out topology, out vertexPositions);
			}
			else if (hexGrid2Toggle.isOn)
			{
				CreateHexGrid2(out topology, out vertexPositions);
			}
			else
			{
				CreateJumbledGrid(out topology, out vertexPositions);
			}

			_topology = topology;
			_vertexPositions = vertexPositions;

			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
		}

		private void CreateMesh(Topology.FacesIndexer faces, DynamicMesh.ITriangulation triangulation, MeshFilter meshPrefab)
		{
			var dynamicMesh = DynamicMesh.Create(
				faces,
				DynamicMesh.VertexAttributes.Position |
				DynamicMesh.VertexAttributes.Normal |
				DynamicMesh.VertexAttributes.UV,
				triangulation);

			int existingChildren = transform.childCount;
			for (int i = 0; i < existingChildren; ++i)
			{
				Destroy(transform.GetChild(i).gameObject);
			}

			foreach (var submesh in dynamicMesh.submeshes)
			{
				var meshObject = Instantiate(meshPrefab);
				meshObject.mesh = submesh;
				meshObject.transform.SetParent(transform, false);
			}
		}

		private void CenterCamera(IVertexAttribute<Vector3> vertexPositions)
		{
			var bounds = new Bounds(vertexPositions[0], Vector3.zero);
			foreach (var vertexPosition in vertexPositions)
			{
				bounds.Encapsulate(vertexPosition);
			}

			Camera.main.transform.position = new Vector3(bounds.center.x - Camera.main.orthographicSize / 3f, bounds.center.y, Camera.main.transform.position.z);
		}

		private AspectRatioPreservation GetAspectRatioPreservation()
		{
			if (expandToggle.isOn) return AspectRatioPreservation.Expand;
			else if (shrinkToggle.isOn) return AspectRatioPreservation.Shrink;
			else return AspectRatioPreservation.None;
		}

		public void OnPlanarGlobalUnnormalized()
		{
			Topology topology;
			IVertexAttribute<Vector3> vertexPositions;
			IFaceAttribute<Vector3> facePositions;

			CreateGrid(out topology, out vertexPositions, out facePositions);

			var edgeUVs = EdgeAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(topology.faceEdges, vertexPositions, Vector3.right * uvScale.x, Vector3.up * uvScale.y);

			var triangulation = new SeparatedFacesFanTriangulation(
				(Topology.FaceEdge edge, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.position = vertexPositions[edge];
					vertexAttributes.normal = Vector3.back;
					vertexAttributes.uv = edgeUVs[edge];
					vertexAttributes.Advance();
				});

			CreateMesh(topology.internalFaces, triangulation, largeTextureMeshPrefab);
			CenterCamera(vertexPositions);
		}

		public void OnPlanarGlobalNormalized()
		{
			Topology topology;
			IVertexAttribute<Vector3> vertexPositions;
			IFaceAttribute<Vector3> facePositions;

			CreateGrid(out topology, out vertexPositions, out facePositions);

			var edgeUVs = EdgeAttributeUtility.CalculateGlobalPlanarNormalizedUVsFromVertexPositions(topology.faceEdges, vertexPositions, Vector3.right * uvScale.x, Vector3.up * uvScale.y);

			var triangulation = new SeparatedFacesFanTriangulation(
				(Topology.FaceEdge edge, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.position = vertexPositions[edge];
					vertexAttributes.normal = Vector3.back;
					vertexAttributes.uv = edgeUVs[edge];
					vertexAttributes.Advance();
				});

			CreateMesh(topology.internalFaces, triangulation, largeTextureMeshPrefab);
			CenterCamera(vertexPositions);
		}

		public void OnPlanarLocalUniformScale()
		{
			Topology topology;
			IVertexAttribute<Vector3> vertexPositions;
			IFaceAttribute<Vector3> facePositions;

			CreateGrid(out topology, out vertexPositions, out facePositions);

			var edgeUVs = EdgeAttributeUtility.CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(topology.faceEdges, topology.internalFaces, vertexPositions, Vector3.right * uvScale.x, Vector3.up * uvScale.y, GetAspectRatioPreservation());

			var triangulation = new SeparatedFacesFanTriangulation(
				(Topology.FaceEdge edge, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.position = vertexPositions[edge];
					vertexAttributes.normal = Vector3.back;
					vertexAttributes.uv = edgeUVs[edge];
					vertexAttributes.Advance();
				});

			CreateMesh(topology.internalFaces, triangulation, smallTextureMeshPrefab);
			CenterCamera(vertexPositions);
		}

		public void OnPlanarLocalUniformScaleCentered()
		{
			Topology topology;
			IVertexAttribute<Vector3> vertexPositions;
			IFaceAttribute<Vector3> facePositions;

			CreateGrid(out topology, out vertexPositions, out facePositions);

			var edgeUVs = EdgeAttributeUtility.CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(topology.faceEdges, topology.internalFaces, vertexPositions, facePositions, Vector3.right * uvScale.x, Vector3.up * uvScale.y, GetAspectRatioPreservation());

			var triangulation = new SeparatedFacesFanTriangulation(
				(Topology.FaceEdge edge, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.position = vertexPositions[edge];
					vertexAttributes.normal = Vector3.back;
					vertexAttributes.uv = edgeUVs[edge];
					vertexAttributes.Advance();
				});

			CreateMesh(topology.internalFaces, triangulation, smallTextureMeshPrefab);
			CenterCamera(vertexPositions);
		}

		public void OnPlanarLocalVariableScale()
		{
			Topology topology;
			IVertexAttribute<Vector3> vertexPositions;
			IFaceAttribute<Vector3> facePositions;

			CreateGrid(out topology, out vertexPositions, out facePositions);

			var edgeUVs = EdgeAttributeUtility.CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(topology.faceEdges, topology.internalFaces, vertexPositions, Vector3.right * uvScale.x, Vector3.up * uvScale.y, GetAspectRatioPreservation());

			var triangulation = new SeparatedFacesFanTriangulation(
				(Topology.FaceEdge edge, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.position = vertexPositions[edge];
					vertexAttributes.normal = Vector3.back;
					vertexAttributes.uv = edgeUVs[edge];
					vertexAttributes.Advance();
				});

			CreateMesh(topology.internalFaces, triangulation, smallTextureMeshPrefab);
			CenterCamera(vertexPositions);
		}

		public void OnPlanarLocalVariableScaleCentered()
		{
			Topology topology;
			IVertexAttribute<Vector3> vertexPositions;
			IFaceAttribute<Vector3> facePositions;

			CreateGrid(out topology, out vertexPositions, out facePositions);

			var edgeUVs = EdgeAttributeUtility.CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(topology.faceEdges, topology.internalFaces, vertexPositions, facePositions, Vector3.right * uvScale.x, Vector3.up * uvScale.y, GetAspectRatioPreservation());

			var triangulation = new SeparatedFacesFanTriangulation(
				(Topology.FaceEdge edge, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.position = vertexPositions[edge];
					vertexAttributes.normal = Vector3.back;
					vertexAttributes.uv = edgeUVs[edge];
					vertexAttributes.Advance();
				});

			CreateMesh(topology.internalFaces, triangulation, smallTextureMeshPrefab);
			CenterCamera(vertexPositions);
		}
	}
}
