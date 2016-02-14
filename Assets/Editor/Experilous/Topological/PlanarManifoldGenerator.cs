using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Experilous.Generation;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(TopologyCategory), "Planar Manifold")]
	public class PlanarManifoldGenerator : Generator
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

		public PlanarTileShapes planarTileShape;
		public Index2D size;
		public HorizontalAxisOptions quadGridHorizontalAxisOptions;
		public VerticalAxisOptions quadGridVerticalAxisOptions;
		public HexGridAxisStyleOptions hexGridAxisStyleOptions;
		public HexGridAxisOptions hexGridAxisOptions;
		public HorizontalAxisOptions hexGridHorizontalAxisOptions;
		public VerticalAxisOptions hexGridVerticalAxisOptions;
		public Vector3 horizontalAxis;
		public Vector3 verticalAxis;
		public float horizontalAxisLength;
		public float verticalAxisLength;
		public bool variableRowLength;
		public NormalOptions normalOptions;
		public Vector3 normal;
		public Vector3 originPosition;
		public WrapOptions wrapOptions;

		public OutputSlot surfaceDescriptor;
		public OutputSlot topologyDescriptor;
		public OutputSlot vertexPositionsDescriptor;

		[SerializeField] private bool _pregenerationFailed = false;

		[MenuItem("Assets/Create/Topology/Planar Manifold Generator")]
		public static void CreateDefaultGeneratorCollection()
		{
			var collection = TopologyGeneratorCollection.Create("New Planar Manifold");
			collection.Add(CreateInstance<PlanarManifoldGenerator>(collection, "Manifold"));
			collection.Add(CreateInstance<RandomEngineGenerator>(collection));
			collection.Add(CreateInstance<TopologyRandomizerGenerator>(collection));
			collection.Add(CreateInstance<FaceCentroidsGenerator>(collection));
			collection.Add(CreateInstance<FaceNormalsGenerator>(collection));
			collection.Add(CreateInstance<RectangularFaceGroupsGenerator>(collection));
			collection.Add(CreateInstance<MeshGenerator>(collection));
			collection.Add(CreateInstance<PrefabGenerator>(collection));
			collection.CreateAsset();
		}

		protected override void Initialize()
		{
			// Fields
			planarTileShape = PlanarTileShapes.Quadrilateral;
			size = new Index2D(64, 64);
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
			originPosition = Vector3.zero;
			wrapOptions = WrapOptions.NoWrap;

			// Outputs
			OutputSlot.CreateOrResetGrouped<PlanarSurface>(ref surfaceDescriptor, this, "Surface", "Descriptors");
			OutputSlot.CreateOrResetGrouped<Topology>(ref topologyDescriptor, this, "Topology", "Descriptors");
			OutputSlot.CreateOrResetGrouped<IVertexAttribute<Vector3>>(ref vertexPositionsDescriptor, this, "Vertex Positions", "Attributes");
		}

		public override IEnumerable<OutputSlot> outputs
		{
			get
			{
				yield return surfaceDescriptor;
				yield return topologyDescriptor;
				yield return vertexPositionsDescriptor;
			}
		}

		public override IEnumerable<InternalSlotConnection> internalConnections
		{
			get
			{
				yield return surfaceDescriptor.Uses(topologyDescriptor);
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
			catch (InvalidTopologyException)
			{
				_pregenerationFailed = true;
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
			catch (InvalidTopologyException)
			{
				_pregenerationFailed = true;
			}

			yield break;
		}

		public override bool canGenerate
		{
			get
			{
				return
					!_pregenerationFailed &&
					size.x > 0 &&
					size.y > 0 &&
					horizontalAxis != Vector3.zero &&
					verticalAxis != Vector3.zero &&
					Mathf.Abs(Vector3.Dot(horizontalAxis, verticalAxis)) < 0.99f; //Axes are not nearly parallel
			}
		}

		public override float estimatedGenerationTime
		{
			get
			{
				return 0.1f;
			}
		}

		private void CreateQuadGridManifold(bool generate = true)
		{
			var surface = surfaceDescriptor.GetAsset<RectangularQuadGrid>();
			if (surface == null) surface = surfaceDescriptor.SetAsset(CreateInstance<RectangularQuadGrid>(), false);

			Vector3 finalHorizontalAxis;
			switch (quadGridHorizontalAxisOptions)
			{
				case HorizontalAxisOptions.LeftToRight: finalHorizontalAxis = Vector3.right * horizontalAxisLength; break;
				case HorizontalAxisOptions.RightToLeft: finalHorizontalAxis = Vector3.left * horizontalAxisLength; break;
				case HorizontalAxisOptions.Custom: finalHorizontalAxis = horizontalAxis; break;
				default: throw new System.NotImplementedException();
			}

			Vector3 finalVerticalAxis;
			switch (quadGridVerticalAxisOptions)
			{
				case VerticalAxisOptions.BottomToTop: finalVerticalAxis = Vector3.up * verticalAxisLength; break;
				case VerticalAxisOptions.TopToBottom: finalVerticalAxis = Vector3.down * verticalAxisLength; break;
				case VerticalAxisOptions.NearToFar: finalVerticalAxis = Vector3.forward * verticalAxisLength; break;
				case VerticalAxisOptions.FarToNear: finalVerticalAxis = Vector3.back * verticalAxisLength; break;
				case VerticalAxisOptions.Custom: finalVerticalAxis = verticalAxis; break;
				default: throw new System.NotImplementedException();
			}

			Vector3 finalNormal;
			switch (normalOptions)
			{
				case NormalOptions.Automatic: finalNormal = Vector3.Cross(finalVerticalAxis, finalHorizontalAxis).normalized; break;
				case NormalOptions.Forward: finalNormal = Vector3.forward; break;
				case NormalOptions.Back: finalNormal = Vector3.back; break;
				case NormalOptions.Up: finalNormal = Vector3.up; break;
				case NormalOptions.Down: finalNormal = Vector3.down; break;
				case NormalOptions.Custom: finalNormal = normal.normalized; break;
				default: throw new System.NotImplementedException();
			}

			surface.Reset(
				new PlanarDescriptor(
					new PlanarAxis(finalHorizontalAxis, (wrapOptions & WrapOptions.WrapHorizontally) != 0),
					new PlanarAxis(finalVerticalAxis, (wrapOptions & WrapOptions.WrapVertically) != 0),
					originPosition,
					finalNormal),
				size);

			if (generate)
			{
				Vector3[] vertexPositionsArray;

				var topology = surface.CreateManifold(out vertexPositionsArray);

				surfaceDescriptor.Persist();
				topologyDescriptor.SetAsset(topology);
				vertexPositionsDescriptor.SetAsset(PositionalVertexAttribute.Create(surfaceDescriptor.GetAsset<Surface>(), vertexPositionsArray));
			}
		}

		private void CreateHexGridManifold(bool generate = true)
		{
			var surface = surfaceDescriptor.GetAsset<RectangularHexGrid>();
			if (surface == null) surface = surfaceDescriptor.SetAsset(CreateInstance<RectangularHexGrid>(), false);

			const float angledShort = 0.5f;
			const float angledLong = RectangularHexGrid.halfSqrtThree;

			Vector3 basicHorizontalAxis;
			Vector3 basicVerticalAxis;
			Vector3 finalHorizontalAxis;
			Vector3 finalVerticalAxis;
			HexGridAxisStyles finalHorizontalAxisStyle;
			HexGridAxisStyles finalVerticalAxisStyle;
			HexGridAxisRelations finalAxisRelation;
			bool finalVariableRowLength;

			switch (hexGridHorizontalAxisOptions)
			{
				case HorizontalAxisOptions.LeftToRight: basicHorizontalAxis = Vector3.right; break;
				case HorizontalAxisOptions.RightToLeft: basicHorizontalAxis = Vector3.left; break;
				case HorizontalAxisOptions.Custom: basicHorizontalAxis = horizontalAxis; break;
				default: throw new System.NotImplementedException();
			}

			switch (hexGridVerticalAxisOptions)
			{
				case VerticalAxisOptions.BottomToTop: basicVerticalAxis = Vector3.up; break;
				case VerticalAxisOptions.TopToBottom: basicVerticalAxis = Vector3.down; break;
				case VerticalAxisOptions.NearToFar: basicVerticalAxis = Vector3.forward; break;
				case VerticalAxisOptions.FarToNear: basicVerticalAxis = Vector3.back; break;
				case VerticalAxisOptions.Custom: basicVerticalAxis = verticalAxis; break;
				default: throw new System.NotImplementedException();
			}

			switch (hexGridAxisStyleOptions)
			{
				case HexGridAxisStyleOptions.ObliqueAcute:
					finalHorizontalAxisStyle = HexGridAxisStyles.Straight;
					finalVerticalAxisStyle = HexGridAxisStyles.Straight;
					finalAxisRelation = HexGridAxisRelations.Acute;
					finalVariableRowLength = false;

					if (hexGridAxisOptions != HexGridAxisOptions.Custom)
					{
						if (hexGridHorizontalAxisOptions == HorizontalAxisOptions.Custom) throw new System.NotSupportedException();
						if (hexGridVerticalAxisOptions == VerticalAxisOptions.Custom) throw new System.NotSupportedException();
					}

					switch (hexGridAxisOptions)
					{
						case HexGridAxisOptions.AngledVerticalAxis:
							finalHorizontalAxis = basicHorizontalAxis * horizontalAxisLength;
							finalVerticalAxis = basicVerticalAxis * (verticalAxisLength * angledLong) + basicHorizontalAxis * angledShort;
							break;
						case HexGridAxisOptions.AngledHorizontalAxis:
							finalHorizontalAxis = basicHorizontalAxis * (horizontalAxisLength * angledLong) + basicVerticalAxis * angledShort;
							finalVerticalAxis = basicVerticalAxis * verticalAxisLength;
							break;
						case HexGridAxisOptions.RightAngleAxes:
							finalHorizontalAxis = basicHorizontalAxis * horizontalAxisLength;
							finalVerticalAxis = basicVerticalAxis * verticalAxisLength;
							break;
						case HexGridAxisOptions.Custom:
							finalHorizontalAxis = basicHorizontalAxis;
							finalVerticalAxis = basicVerticalAxis;
							break;
						default: throw new System.NotImplementedException();
					}
					break;
				case HexGridAxisStyleOptions.ObliqueObtuse:
					finalHorizontalAxisStyle = HexGridAxisStyles.Straight;
					finalVerticalAxisStyle = HexGridAxisStyles.Straight;
					finalAxisRelation = HexGridAxisRelations.Obtuse;
					finalVariableRowLength = false;

					if (hexGridAxisOptions != HexGridAxisOptions.Custom)
					{
						if (hexGridHorizontalAxisOptions == HorizontalAxisOptions.Custom) throw new System.NotSupportedException();
						if (hexGridVerticalAxisOptions == VerticalAxisOptions.Custom) throw new System.NotSupportedException();
					}

					switch (hexGridAxisOptions)
					{
						case HexGridAxisOptions.AngledVerticalAxis:
							finalHorizontalAxis = basicHorizontalAxis * horizontalAxisLength;
							finalVerticalAxis = basicVerticalAxis * (verticalAxisLength * angledLong) - basicHorizontalAxis * angledShort;
							break;
						case HexGridAxisOptions.AngledHorizontalAxis:
							finalHorizontalAxis = basicHorizontalAxis * (horizontalAxisLength * angledLong) - basicVerticalAxis * angledShort;
							finalVerticalAxis = basicVerticalAxis * verticalAxisLength;
							break;
						case HexGridAxisOptions.RightAngleAxes:
							finalHorizontalAxis = basicHorizontalAxis * horizontalAxisLength;
							finalVerticalAxis = basicVerticalAxis * verticalAxisLength;
							break;
						case HexGridAxisOptions.Custom:
							finalHorizontalAxis = basicHorizontalAxis;
							finalVerticalAxis = basicVerticalAxis;
							break;
						default: throw new System.NotImplementedException();
					}
					break;
				case HexGridAxisStyleOptions.StaggeredVerticallyAcute:
				case HexGridAxisStyleOptions.StaggeredVerticallyObtuse:
					finalHorizontalAxisStyle = HexGridAxisStyles.Straight;
					finalVerticalAxisStyle = HexGridAxisStyles.Staggered;
					finalAxisRelation = hexGridAxisStyleOptions == HexGridAxisStyleOptions.StaggeredVerticallyAcute
						? HexGridAxisRelations.Acute : HexGridAxisRelations.Obtuse;
					finalVariableRowLength = variableRowLength;
					finalHorizontalAxis = basicHorizontalAxis;
					finalVerticalAxis = basicVerticalAxis;
					break;
				case HexGridAxisStyleOptions.StaggeredHorizontallyAcute:
				case HexGridAxisStyleOptions.StaggeredHorizontallyObtuse:
					finalHorizontalAxisStyle = HexGridAxisStyles.Staggered;
					finalVerticalAxisStyle = HexGridAxisStyles.Straight;
					finalAxisRelation = hexGridAxisStyleOptions == HexGridAxisStyleOptions.StaggeredHorizontallyAcute
						? HexGridAxisRelations.Acute : HexGridAxisRelations.Obtuse;
					finalVariableRowLength = variableRowLength;
					finalHorizontalAxis = basicHorizontalAxis;
					finalVerticalAxis = basicVerticalAxis;
					break;
				default: throw new System.NotImplementedException();
			}

			Vector3 finalNormal;
			switch (normalOptions)
			{
				case NormalOptions.Automatic: finalNormal = Vector3.Cross(finalVerticalAxis, finalHorizontalAxis).normalized; break;
				case NormalOptions.Forward: finalNormal = Vector3.forward; break;
				case NormalOptions.Back: finalNormal = Vector3.back; break;
				case NormalOptions.Up: finalNormal = Vector3.up; break;
				case NormalOptions.Down: finalNormal = Vector3.down; break;
				case NormalOptions.Custom: finalNormal = normal.normalized; break;
				default: throw new System.NotImplementedException();
			}

			if (size.x > 0 && size.y > 0)
			{
				surface.Reset(
					new PlanarDescriptor(
						new PlanarAxis(finalHorizontalAxis, (wrapOptions & WrapOptions.WrapHorizontally) != 0),
						new PlanarAxis(finalVerticalAxis, (wrapOptions & WrapOptions.WrapVertically) != 0),
						originPosition,
						finalNormal),
					new HexGridDescriptor(finalHorizontalAxisStyle, finalVerticalAxisStyle, finalAxisRelation, finalVariableRowLength),
					size);
			}

			if (generate)
			{
				Vector3[] vertexPositionsArray;

				var topology = surface.CreateManifold(out vertexPositionsArray);

				surfaceDescriptor.Persist();
				topologyDescriptor.SetAsset(topology);
				vertexPositionsDescriptor.SetAsset(PositionalVertexAttribute.Create(surfaceDescriptor.GetAsset<Surface>(), vertexPositionsArray));
			}
		}
	}
}
