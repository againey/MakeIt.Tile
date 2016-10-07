/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Experilous.MakeItGenerate;
using Experilous.Numerics;

namespace Experilous.MakeItTile
{
	[Generator(typeof(TopologyGeneratorCollection), "Topology/Planar Manifold")]
	public class PlanarManifoldGenerator : Generator, ISerializationCallbackReceiver
	{
		public enum PlanarTileShapes
		{
			Quadrilateral,
			Hexagonal,
		}

		[System.Flags]
		public enum WrapOptions
		{
			NoWrap = 0,
			WrapHorizontally = 1,
			WrapVertically = 2,
			WrapBoth = WrapHorizontally | WrapVertically,
		}

		public enum HorizontalAxisOptions
		{
			LeftToRight,
			RightToLeft,
			Custom,
		}

		public enum VerticalAxisOptions
		{
			BottomToTop,
			TopToBottom,
			NearToFar,
			FarToNear,
			Custom,
		}

		public enum HexGridAxisStyleOptions
		{
			ObliqueAcute,
			ObliqueObtuse,
			StaggeredVerticallyAcute,
			StaggeredVerticallyObtuse,
			StaggeredHorizontallyAcute,
			StaggeredHorizontallyObtuse,
		}

		public enum HexGridAxisOptions
		{
			AngledVerticalAxis,
			AngledHorizontalAxis,
			RightAngleAxes,
			Custom,
		}

		public enum NormalOptions
		{
			Automatic,
			Forward,
			Back,
			Up,
			Down,
			Custom,
		}

		public enum TileTypes
		{
			Quadrilateral,
			Hexagonal,
		}

		public enum QuadTileShapes
		{
			Square,
			Isometric,
			Diamond2x1,
			Custom,
		}

		public enum HexTileShapes
		{
			RegularPointUp,
			RegularSideUp,
			SquarePointUp,
			SquareSideUp,
			BlockHorizontal,
			BlockVertical,
			Brick,
			Custom,
		}

		[SerializeField] private string _version;

		public TileTypes tileType;
		public QuadTileShapes quadTileShape;
		public HexTileShapes hexTileShape;
		public IntVector2 size;

		public Vector2 axis0;
		public Vector2 axis1;
		public Vector2 midpoint;
		public Vector2 majorCorner;
		public Vector2 minorCorner;
		public HexGridAxisStyles hexGridAxisStyle;
		public bool swapAxes;

		public bool isAxis0Wrapped;
		public bool isAxis1Wrapped;

		public Vector3 origin;
		public Vector3 rotation;

		[SerializeField] private PlanarTileShapes planarTileShape;
		[SerializeField] private HorizontalAxisOptions quadGridHorizontalAxisOptions;
		[SerializeField] private VerticalAxisOptions quadGridVerticalAxisOptions;
		[SerializeField] private HexGridAxisStyleOptions hexGridAxisStyleOptions;
		[SerializeField] private HexGridAxisOptions hexGridAxisOptions;
		[SerializeField] private HorizontalAxisOptions hexGridHorizontalAxisOptions;
		[SerializeField] private VerticalAxisOptions hexGridVerticalAxisOptions;
		[SerializeField] private Vector3 horizontalAxis;
		[SerializeField] private Vector3 verticalAxis;
		[SerializeField] private float horizontalAxisLength;
		[SerializeField] private float verticalAxisLength;
		[SerializeField] private bool variableRowLength;
		[SerializeField] private NormalOptions normalOptions;
		[SerializeField] private Vector3 normal;
		[SerializeField] private WrapOptions wrapOptions;

		public OutputSlot surfaceOutputSlot;
		public OutputSlot topologyOutputSlot;
		public OutputSlot vertexPositionsOutputSlot;

		[SerializeField] private bool _pregenerationFailed = false;
		[SerializeField] private string _pregenerationFailedMessage;

		[MenuItem("Assets/Create/Topology/Planar Manifold Generator")]
		public static void CreateDefaultGeneratorCollection()
		{
			var collection = TopologyGeneratorCollection.Create("New Planar Manifold");

			var manifoldGenerator = collection.Add(CreateInstance<PlanarManifoldGenerator>(collection));
			collection.Add(CreateInstance<RandomnessGenerator>(collection));
			var faceCentroidsGenerator = collection.Add(CreateInstance<FaceCentroidsGenerator>(collection));
			var faceNormalsGenerator = collection.Add(CreateInstance<FaceNormalsGenerator>(collection));
			var vertexNormalsGenerator = collection.Add(CreateInstance<VertexNormalsGenerator>(collection));
			var rectangularFaceGroupsGenerator = collection.Add(CreateInstance<RectangularFaceGroupsGenerator>(collection));
			var meshGenerator = collection.Add(CreateInstance<MeshGenerator>(collection));
			var prefabGenerator = collection.Add(CreateInstance<PrefabGenerator>(collection));
			var faceSpatialPartitioningGenerator = collection.Add(CreateInstance<FaceSpatialPartitioningGenerator>(collection));

			faceCentroidsGenerator.topologyInputSlot.source = manifoldGenerator.topologyOutputSlot;
			faceCentroidsGenerator.surfaceInputSlot.source = manifoldGenerator.surfaceOutputSlot;
			faceCentroidsGenerator.vertexPositionsInputSlot.source = manifoldGenerator.vertexPositionsOutputSlot;

			faceNormalsGenerator.calculationMethod = FaceNormalsGenerator.CalculationMethod.FromSurfaceNormal;
			faceNormalsGenerator.topologyInputSlot.source = manifoldGenerator.topologyOutputSlot;
			faceNormalsGenerator.surfaceInputSlot.source = manifoldGenerator.surfaceOutputSlot;
			faceNormalsGenerator.facePositionsInputSlot.source = faceCentroidsGenerator.faceCentroidsOutputSlot;

			vertexNormalsGenerator.calculationMethod = VertexNormalsGenerator.CalculationMethod.FromSurfaceNormal;
			vertexNormalsGenerator.topologyInputSlot.source = manifoldGenerator.topologyOutputSlot;
			vertexNormalsGenerator.surfaceInputSlot.source = manifoldGenerator.surfaceOutputSlot;
			vertexNormalsGenerator.vertexPositionsInputSlot.source = manifoldGenerator.vertexPositionsOutputSlot;

			rectangularFaceGroupsGenerator.axisDivisions = new IntVector2(4, 4);
			rectangularFaceGroupsGenerator.topologyInputSlot.source = manifoldGenerator.topologyOutputSlot;
			rectangularFaceGroupsGenerator.surfaceInputSlot.source = manifoldGenerator.surfaceOutputSlot;
			rectangularFaceGroupsGenerator.facePositionsInputSlot.source = faceCentroidsGenerator.faceCentroidsOutputSlot;

			meshGenerator.sourceType = MeshGenerator.SourceType.FaceGroupCollection;
			meshGenerator.faceGroupCollectionInputSlot.source = rectangularFaceGroupsGenerator.faceGroupCollectionOutputSlot;
			meshGenerator.triangulation = MeshGenerator.Triangulation.Umbrella;
			meshGenerator.vertexAttributes = DynamicMesh.VertexAttributes.Position | DynamicMesh.VertexAttributes.Normal;
			meshGenerator.ringDepth = 1;
			meshGenerator.UpdateVertexAttributeInputSlots();
			meshGenerator.ringVertexPositionsInputSlots[0].source = manifoldGenerator.vertexPositionsOutputSlot;
			meshGenerator.centerVertexPositionsInputSlots[0].source = faceCentroidsGenerator.faceCentroidsOutputSlot;
			meshGenerator.ringVertexNormalsInputSlots[0].source = vertexNormalsGenerator.vertexNormalsOutputSlot;
			meshGenerator.centerVertexNormalsInputSlots[0].source = faceNormalsGenerator.faceNormalsOutputSlot;

			prefabGenerator.dynamicMeshInputSlot.source = meshGenerator.dynamicMeshOutputSlot;

			faceSpatialPartitioningGenerator.topologyInputSlot.source = manifoldGenerator.topologyOutputSlot;
			faceSpatialPartitioningGenerator.surfaceInputSlot.source = manifoldGenerator.surfaceOutputSlot;
			faceSpatialPartitioningGenerator.vertexPositionsInputSlot.source = manifoldGenerator.vertexPositionsOutputSlot;

			collection.CreateAsset();
		}

		protected override void Initialize()
		{
			// Fields
			planarTileShape = PlanarTileShapes.Quadrilateral;
			size = new IntVector2(64, 64);
			quadGridHorizontalAxisOptions = HorizontalAxisOptions.LeftToRight;
			quadGridVerticalAxisOptions = VerticalAxisOptions.BottomToTop;
			hexGridAxisStyleOptions = HexGridAxisStyleOptions.ObliqueAcute;
			hexGridAxisOptions = HexGridAxisOptions.AngledVerticalAxis;
			hexGridHorizontalAxisOptions = HorizontalAxisOptions.LeftToRight;
			hexGridVerticalAxisOptions = VerticalAxisOptions.BottomToTop;
			horizontalAxis = Vector3.right;
			verticalAxis = Vector3.up;
			horizontalAxisLength = 1f;
			verticalAxisLength = 1f;
			variableRowLength = false;
			normalOptions = NormalOptions.Back;
			normal = Vector3.back;
			origin = Vector3.zero;
			wrapOptions = WrapOptions.NoWrap;

			// Outputs
			OutputSlot.CreateOrReset<QuadrilateralSurface>(ref surfaceOutputSlot, this, "Surface");
			OutputSlot.CreateOrReset<Topology>(ref topologyOutputSlot, this, "Topology");
			OutputSlot.CreateOrResetGrouped<IVertexAttribute<Vector3>>(ref vertexPositionsOutputSlot, this, "Vertex Positions", "Attributes");
		}

		public void OnAfterDeserialize()
		{
			OutputSlot.ResetAssetTypeIfNull<QuadrilateralSurface>(surfaceOutputSlot);
			OutputSlot.ResetAssetTypeIfNull<Topology>(topologyOutputSlot);
			OutputSlot.ResetAssetTypeIfNull<IVertexAttribute<Vector3>>(vertexPositionsOutputSlot);

			switch (tileType)
			{
				case TileTypes.Quadrilateral: SetQuadTileShape(quadTileShape); break;
				case TileTypes.Hexagonal: SetHexTileShape(hexTileShape); break;
				default: throw new System.NotImplementedException();
			}
		}

		public void OnBeforeSerialize()
		{
		}

		public override IEnumerable<OutputSlot> outputs
		{
			get
			{
				yield return surfaceOutputSlot;
				yield return topologyOutputSlot;
				yield return vertexPositionsOutputSlot;
			}
		}

		public override IEnumerable<InternalSlotConnection> internalConnections
		{
			get
			{
				yield return surfaceOutputSlot.Uses(topologyOutputSlot);
			}
		}

		protected override void OnUpdate()
		{
			try
			{
				switch (planarTileShape)
				{
					case PlanarTileShapes.Quadrilateral:
						CreateQuadGridManifold(false);
						break;
					case PlanarTileShapes.Hexagonal:
						CreateHexGridManifold(false);
						break;
					default:
						throw new System.NotImplementedException();
				}

				_pregenerationFailed = false;
			}
			catch (InvalidTopologyException ex)
			{
				_pregenerationFailed = true;
				_pregenerationFailedMessage = ex.Message;
			}
		}

		public override IEnumerator BeginGeneration()
		{
			try
			{
				switch (planarTileShape)
				{
					case PlanarTileShapes.Quadrilateral:
						CreateQuadGridManifold();
						break;
					case PlanarTileShapes.Hexagonal:
						CreateHexGridManifold();
						break;
					default:
						throw new System.NotImplementedException();
				}
			}
			catch (InvalidTopologyException ex)
			{
				_pregenerationFailed = true;
				_pregenerationFailedMessage = ex.Message;
			}

			yield break;
		}

		public override bool canGenerate
		{
			get
			{
				if (_pregenerationFailed || size.x <= 0 || size.y <= 0) return false;

				switch (tileType)
				{
					case TileTypes.Quadrilateral:
						return
							axis0 != Vector2.zero &&
							axis1 != Vector2.zero &&
							Mathf.Abs(Vector2.Dot(axis0.normalized, axis1.normalized)) < 0.99f; //Axes are not nearly parallel
					case TileTypes.Hexagonal:
						return
							midpoint != Vector2.zero &&
							majorCorner != Vector2.zero &&
							minorCorner != Vector2.zero &&
							Mathf.Abs(Vector2.Dot(midpoint.normalized, majorCorner.normalized)) < 0.99f && //Axes are not nearly parallel
							Mathf.Abs(Vector2.Dot(midpoint.normalized, minorCorner.normalized)) < 0.99f && //Axes are not nearly parallel
							Mathf.Abs(Vector2.Dot(majorCorner.normalized, minorCorner.normalized)) < 0.99f; //Axes are not nearly parallel
					default: throw new System.NotImplementedException();
				}
			}
		}

		public override string canGenerateMessage
		{
			get
			{
				if (_pregenerationFailed)
				{
					return _pregenerationFailedMessage;
				}
				else if (size.x <= 0 || size.y <= 0)
				{
					return "Size dimensions must be greater than zero.";
				}
				else
				{
					switch (tileType)
					{
						case TileTypes.Quadrilateral:
							if (axis0 == Vector2.zero || axis1 == Vector2.zero)
								return "Axes must not be zero-length vectors.";
							else if (Mathf.Abs(Vector2.Dot(axis0.normalized, axis1.normalized)) >= 0.99f)
								return "Axes cannot be parallel to each other.";
							break;
						case TileTypes.Hexagonal:
							if (midpoint == Vector2.zero || majorCorner == Vector2.zero || minorCorner == Vector2.zero)
								return "Axes must not be zero-length vectors.";
							else if (
								Mathf.Abs(Vector2.Dot(midpoint.normalized, majorCorner.normalized)) >= 0.99f ||
								Mathf.Abs(Vector2.Dot(midpoint.normalized, minorCorner.normalized)) >= 0.99f ||
								Mathf.Abs(Vector2.Dot(majorCorner.normalized, minorCorner.normalized)) >= 0.99f)
								return "Axes cannot be parallel to each other.";
							break;
						default: throw new System.NotImplementedException();
					}
					return base.canGenerateMessage;
				}
			}
		}

		public override float estimatedGenerationTime
		{
			get
			{
				return 0.1f;
			}
		}

		public RectangularQuadGrid ResetSurface(RectangularQuadGrid surface, Vector3 origin, Quaternion orientation, IntVector2 size)
		{
			return surface.Reset(axis0, axis1, origin, orientation, isAxis0Wrapped, isAxis1Wrapped, size);
		}

		public RectangularHexGrid ResetSurface(RectangularHexGrid surface, Vector3 origin, Quaternion orientation, IntVector2 size)
		{
			return surface.Reset(new HexGridDescriptor(midpoint, majorCorner, minorCorner, !swapAxes, hexGridAxisStyle), origin, orientation, isAxis0Wrapped, isAxis1Wrapped, size);
		}

		private void CreateQuadGridManifold(bool generate = true)
		{
			var surface = surfaceOutputSlot.GetAsset<RectangularQuadGrid>();
			if (surface == null || !generate && ReferenceEquals(surface, surfaceOutputSlot.persistedAsset))
			{
				surface = surfaceOutputSlot.SetAsset(CreateInstance<RectangularQuadGrid>(), false);
			}

			ResetSurface(surface, origin, Quaternion.Euler(rotation), size);

			if (generate)
			{
				Vector3[] vertexPositionsArray;

				var topology = surface.CreateManifold(out vertexPositionsArray);
				topologyOutputSlot.SetAsset(topology);

				surface.topology = topologyOutputSlot.GetAsset<Topology>();
				surfaceOutputSlot.Persist();

				vertexPositionsOutputSlot.SetAsset(PositionalVertexAttribute.Create(surfaceOutputSlot.GetAsset<Surface>(), vertexPositionsArray));
			}
		}

		private void CreateHexGridManifold(bool generate = true)
		{
			var surface = surfaceOutputSlot.GetAsset<RectangularHexGrid>();
			if (surface == null || !generate && ReferenceEquals(surface, surfaceOutputSlot.persistedAsset))
			{
				surface = surfaceOutputSlot.SetAsset(CreateInstance<RectangularHexGrid>(), false);
			}

			ResetSurface(surface, origin, Quaternion.Euler(rotation), size);

			if (generate)
			{
				Vector3[] vertexPositionsArray;

				var topology = surface.CreateManifold(out vertexPositionsArray);
				topologyOutputSlot.SetAsset(topology);

				surface.topology = topologyOutputSlot.GetAsset<Topology>();
				surfaceOutputSlot.Persist();

				vertexPositionsOutputSlot.SetAsset(PositionalVertexAttribute.Create(surfaceOutputSlot.GetAsset<Surface>(), vertexPositionsArray));
			}
		}

		public void SetQuadTileShape(QuadTileShapes shape)
		{
			if (shape == quadTileShape) return;

			quadTileShape = shape;
			switch (quadTileShape)
			{
				case QuadTileShapes.Square:
					axis0 = Vector2.right;
					axis1 = Vector2.up;
					break;
				case QuadTileShapes.Isometric:
					axis0 = new Vector2(1f, -1f / Mathf.Sqrt(3f));
					axis1 = new Vector2(1f, +1f / Mathf.Sqrt(3f));
					break;
				case QuadTileShapes.Diamond2x1:
					axis0 = new Vector2(1f, -0.5f);
					axis1 = new Vector2(1f, +0.5f);
					break;
			}
		}

		public void SetHexTileShape(HexTileShapes shape)
		{
			if (shape == hexTileShape) return;

			hexTileShape = shape;
			switch (hexTileShape)
			{
				case HexTileShapes.RegularPointUp:
					midpoint = new Vector2(0.5f, 0f);
					majorCorner = new Vector2(0f, 1f / Mathf.Sqrt(3f));
					minorCorner = new Vector2(0.5f, 0.5f / Mathf.Sqrt(3f));
					break;
				case HexTileShapes.RegularSideUp:
					midpoint = new Vector2(0f, 0.5f);
					majorCorner = new Vector2(1f / Mathf.Sqrt(3f), 0f);
					minorCorner = new Vector2(0.5f / Mathf.Sqrt(3f), 0.5f);
					break;
				case HexTileShapes.SquarePointUp:
					midpoint = new Vector2(0.5f, 0f);
					majorCorner = new Vector2(0f, 0.5f);
					minorCorner = new Vector2(0.5f, 0.25f);
					break;
				case HexTileShapes.SquareSideUp:
					midpoint = new Vector2(0f, 0.5f);
					majorCorner = new Vector2(0.5f, 0f);
					minorCorner = new Vector2(0.25f, 0.5f);
					break;
				case HexTileShapes.BlockHorizontal:
					midpoint = new Vector2(0.5f, 0f);
					majorCorner = new Vector2(0f, 0.5f);
					minorCorner = new Vector2(0.5f, 0.5f);
					break;
				case HexTileShapes.BlockVertical:
					midpoint = new Vector2(0f, 0.5f);
					majorCorner = new Vector2(0.5f, 0f);
					minorCorner = new Vector2(0.5f, 0.5f);
					break;
				case HexTileShapes.Brick:
					midpoint = new Vector2(0.5f, 0f);
					majorCorner = new Vector2(0f, 0.125f);
					minorCorner = new Vector2(0.5f, 0.125f);
					break;
			}
		}
	}
}
