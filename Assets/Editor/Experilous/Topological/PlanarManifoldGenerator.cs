using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(TopologyCategory), "Planar Manifold")]
	public class PlanarManifoldGenerator : AssetGenerator
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
			Custom,
		}

		public PlanarTileShapes planarTileShape = PlanarTileShapes.Quadrilateral;
		public Index2D size = new Index2D(64, 64);
		public HorizontalAxisOptions quadGridHorizontalAxisOptions = HorizontalAxisOptions.LeftToRight;
		public VerticalAxisOptions quadGridVerticalAxisOptions = VerticalAxisOptions.BottomToTop;
		public HexGridAxisStyleOptions hexGridAxisStyleOptions = HexGridAxisStyleOptions.ObliqueAcute;
		public HexGridAxisOptions hexGridAxisOptions = HexGridAxisOptions.AngledVerticalAxis;
		public HorizontalAxisOptions hexGridHorizontalAxisOptions = HorizontalAxisOptions.LeftToRight;
		public VerticalAxisOptions hexGridVerticalAxisOptions = VerticalAxisOptions.BottomToTop;
		public Vector3 horizontalAxis = Vector3.right;
		public Vector3 verticalAxis = Vector3.up;
		public float horizontalAxisLength = 1f;
		public float verticalAxisLength = 1f;
		public bool variableRowLength = false;
		public NormalOptions normalOptions = NormalOptions.Back;
		public Vector3 normal = Vector3.back;
		public Vector3 originPosition = Vector3.zero;
		public WrapOptions wrapOptions = WrapOptions.NoWrap;

		public AssetDescriptor surfaceDescriptor;
		public AssetDescriptor topologyDescriptor;
		public AssetDescriptor vertexPositionsDescriptor;

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

		protected override void Initialize(bool reset = true)
		{
			// Outputs
			if (reset || surfaceDescriptor == null) surfaceDescriptor = AssetDescriptor.CreateGrouped<PlanarSurface>(this, "Surface", "Descriptors");
			if (reset || topologyDescriptor == null) topologyDescriptor = AssetDescriptor.CreateGrouped<Topology>(this, "Topology", "Descriptors");
			if (reset || vertexPositionsDescriptor == null) vertexPositionsDescriptor = AssetDescriptor.CreateGrouped<IVertexAttribute<Vector3>>(this, "Vertex Positions", "Attributes");
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				yield return surfaceDescriptor;
				yield return topologyDescriptor;
				yield return vertexPositionsDescriptor;
			}
		}

		public override IEnumerable<AssetReferenceDescriptor> references
		{
			get
			{
				yield return topologyDescriptor.ReferencedBy(surfaceDescriptor);
			}
		}

		public override void Pregenerate()
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
				case VerticalAxisOptions.Custom: finalVerticalAxis = verticalAxis; break;
				default: throw new System.NotImplementedException();
			}

			Vector3 finalNormal;
			switch (normalOptions)
			{
				case NormalOptions.Automatic: finalNormal = Vector3.Cross(finalVerticalAxis, finalHorizontalAxis).normalized; break;
				case NormalOptions.Forward: finalNormal = Vector3.forward; break;
				case NormalOptions.Back: finalNormal = Vector3.back; break;
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

			Vector3 finalHorizontalAxis;
			Vector3 finalVerticalAxis;
			HexGridAxisStyles finalHorizontalAxisStyle;
			HexGridAxisStyles finalVerticalAxisStyle;
			HexGridAxisRelations finalAxisRelation;
			bool finalVariableRowLength;

			switch (hexGridAxisStyleOptions)
			{
				case HexGridAxisStyleOptions.ObliqueAcute:
					finalHorizontalAxisStyle = HexGridAxisStyles.Straight;
					finalVerticalAxisStyle = HexGridAxisStyles.Straight;
					finalAxisRelation = HexGridAxisRelations.Acute;
					finalVariableRowLength = false;

					switch (hexGridAxisOptions)
					{
						case HexGridAxisOptions.AngledVerticalAxis:
							switch (hexGridHorizontalAxisOptions)
							{
								case HorizontalAxisOptions.LeftToRight:
									finalHorizontalAxis = Vector3.right * horizontalAxisLength;
									finalVerticalAxis = hexGridVerticalAxisOptions == VerticalAxisOptions.BottomToTop
										? new Vector3(angledShort, angledLong, 0f) * verticalAxisLength
										: new Vector3(angledShort, -angledLong, 0f) * verticalAxisLength;
									break;
								case HorizontalAxisOptions.RightToLeft:
									finalHorizontalAxis = Vector3.left * horizontalAxisLength;
									finalVerticalAxis = hexGridVerticalAxisOptions == VerticalAxisOptions.BottomToTop
										? new Vector3(-angledShort, angledLong, 0f) * verticalAxisLength
										: new Vector3(-angledShort, -angledLong, 0f) * verticalAxisLength;
									break;
								case HorizontalAxisOptions.Custom: throw new System.NotSupportedException();
								default: throw new System.NotImplementedException();
							}
							break;
						case HexGridAxisOptions.AngledHorizontalAxis:
							switch (hexGridVerticalAxisOptions)
							{
								case VerticalAxisOptions.BottomToTop:
									finalHorizontalAxis = hexGridHorizontalAxisOptions == HorizontalAxisOptions.LeftToRight
										? new Vector3(angledLong, angledShort, 0f) * horizontalAxisLength
										: new Vector3(-angledLong, angledShort, 0f) * horizontalAxisLength;
									finalVerticalAxis = Vector3.up * verticalAxisLength;
									break;
								case VerticalAxisOptions.TopToBottom:
									finalHorizontalAxis = hexGridHorizontalAxisOptions == HorizontalAxisOptions.LeftToRight
										? new Vector3(angledLong, -angledShort, 0f) * horizontalAxisLength
										: new Vector3(-angledLong, -angledShort, 0f) * horizontalAxisLength;
									finalVerticalAxis = Vector3.down * verticalAxisLength;
									break;
								case VerticalAxisOptions.Custom: throw new System.NotSupportedException();
								default: throw new System.NotImplementedException();
							}
							break;
						case HexGridAxisOptions.RightAngleAxes:
							finalHorizontalAxis = hexGridHorizontalAxisOptions == HorizontalAxisOptions.LeftToRight
								? Vector3.right * horizontalAxisLength
								: Vector3.left * horizontalAxisLength;
							finalVerticalAxis = hexGridVerticalAxisOptions == VerticalAxisOptions.BottomToTop
								? Vector3.up * verticalAxisLength
								: Vector3.down * verticalAxisLength;
							break;
						case HexGridAxisOptions.Custom:
							finalHorizontalAxis = horizontalAxis;
							finalVerticalAxis = verticalAxis;
							break;
						default: throw new System.NotImplementedException();
					}
					break;
				case HexGridAxisStyleOptions.ObliqueObtuse:
					finalHorizontalAxisStyle = HexGridAxisStyles.Straight;
					finalVerticalAxisStyle = HexGridAxisStyles.Straight;
					finalAxisRelation = HexGridAxisRelations.Obtuse;
					finalVariableRowLength = false;

					switch (hexGridAxisOptions)
					{
						case HexGridAxisOptions.AngledVerticalAxis:
							switch (hexGridHorizontalAxisOptions)
							{
								case HorizontalAxisOptions.LeftToRight:
									finalHorizontalAxis = Vector3.right * horizontalAxisLength;
									switch (hexGridVerticalAxisOptions)
									{
										case VerticalAxisOptions.BottomToTop: finalVerticalAxis = new Vector3(-angledShort, angledLong, 0f) * verticalAxisLength; break;
										case VerticalAxisOptions.TopToBottom: finalVerticalAxis = new Vector3(-angledShort, -angledLong, 0f) * verticalAxisLength; break;
										case VerticalAxisOptions.Custom: throw new System.NotSupportedException();
										default: throw new System.NotImplementedException();
									}
									break;
								case HorizontalAxisOptions.RightToLeft:
									finalHorizontalAxis = Vector3.left * horizontalAxisLength;
									switch (hexGridVerticalAxisOptions)
									{
										case VerticalAxisOptions.BottomToTop: finalVerticalAxis = new Vector3(angledShort, angledLong, 0f) * verticalAxisLength; break;
										case VerticalAxisOptions.TopToBottom: finalVerticalAxis = new Vector3(angledShort, -angledLong, 0f) * verticalAxisLength; break;
										case VerticalAxisOptions.Custom: throw new System.NotSupportedException();
										default: throw new System.NotImplementedException();
									}
									break;
								case HorizontalAxisOptions.Custom: throw new System.NotSupportedException();
								default: throw new System.NotImplementedException();
							}
							break;
						case HexGridAxisOptions.AngledHorizontalAxis:
							switch (hexGridVerticalAxisOptions)
							{
								case VerticalAxisOptions.BottomToTop:
									switch (hexGridHorizontalAxisOptions)
									{
										case HorizontalAxisOptions.LeftToRight: finalHorizontalAxis = new Vector3(angledLong, -angledShort, 0f) * horizontalAxisLength; break;
										case HorizontalAxisOptions.RightToLeft: finalHorizontalAxis = new Vector3(-angledLong, -angledShort, 0f) * horizontalAxisLength; break;
										case HorizontalAxisOptions.Custom: throw new System.NotSupportedException();
										default: throw new System.NotImplementedException();
									}
									finalVerticalAxis = Vector3.up * verticalAxisLength;
									break;
								case VerticalAxisOptions.TopToBottom:
									switch (hexGridHorizontalAxisOptions)
									{
										case HorizontalAxisOptions.LeftToRight: finalHorizontalAxis = new Vector3(angledLong, angledShort, 0f) * horizontalAxisLength; break;
										case HorizontalAxisOptions.RightToLeft: finalHorizontalAxis = new Vector3(-angledLong, angledShort, 0f) * horizontalAxisLength; break;
										case HorizontalAxisOptions.Custom: throw new System.NotSupportedException();
										default: throw new System.NotImplementedException();
									}
									finalVerticalAxis = Vector3.down * verticalAxisLength;
									break;
								case VerticalAxisOptions.Custom: throw new System.NotSupportedException();
								default: throw new System.NotImplementedException();
							}
							break;
						case HexGridAxisOptions.RightAngleAxes:
							switch (hexGridHorizontalAxisOptions)
							{
								case HorizontalAxisOptions.LeftToRight: finalHorizontalAxis = Vector3.right * horizontalAxisLength; break;
								case HorizontalAxisOptions.RightToLeft: finalHorizontalAxis = Vector3.left * horizontalAxisLength; break;
								case HorizontalAxisOptions.Custom: throw new System.NotSupportedException();
								default: throw new System.NotImplementedException();
							}
							switch (hexGridVerticalAxisOptions)
							{
								case VerticalAxisOptions.BottomToTop: finalVerticalAxis = Vector3.up * verticalAxisLength; break;
								case VerticalAxisOptions.TopToBottom: finalVerticalAxis = Vector3.down * verticalAxisLength; break;
								case VerticalAxisOptions.Custom: throw new System.NotSupportedException();
								default: throw new System.NotImplementedException();
							}
							break;
						case HexGridAxisOptions.Custom:
							finalHorizontalAxis = horizontalAxis;
							finalVerticalAxis = verticalAxis;
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

					switch (hexGridHorizontalAxisOptions)
					{
						case HorizontalAxisOptions.LeftToRight: finalHorizontalAxis = Vector3.right * horizontalAxisLength; break;
						case HorizontalAxisOptions.RightToLeft: finalHorizontalAxis = Vector3.left * horizontalAxisLength; break;
						case HorizontalAxisOptions.Custom: finalHorizontalAxis = horizontalAxis; break;
						default: throw new System.NotImplementedException();
					}

					switch (hexGridVerticalAxisOptions)
					{
						case VerticalAxisOptions.BottomToTop: finalVerticalAxis = Vector3.up * verticalAxisLength; break;
						case VerticalAxisOptions.TopToBottom: finalVerticalAxis = Vector3.down * verticalAxisLength; break;
						case VerticalAxisOptions.Custom: finalVerticalAxis = verticalAxis; break;
						default: throw new System.NotImplementedException();
					}

					break;

				case HexGridAxisStyleOptions.StaggeredHorizontallyAcute:
				case HexGridAxisStyleOptions.StaggeredHorizontallyObtuse:
					finalHorizontalAxisStyle = HexGridAxisStyles.Staggered;
					finalVerticalAxisStyle = HexGridAxisStyles.Straight;
					finalAxisRelation = hexGridAxisStyleOptions == HexGridAxisStyleOptions.StaggeredHorizontallyAcute
						? HexGridAxisRelations.Acute : HexGridAxisRelations.Obtuse;
					finalVariableRowLength = variableRowLength;

					switch (hexGridHorizontalAxisOptions)
					{
						case HorizontalAxisOptions.LeftToRight: finalHorizontalAxis = Vector3.right * horizontalAxisLength; break;
						case HorizontalAxisOptions.RightToLeft: finalHorizontalAxis = Vector3.left * horizontalAxisLength; break;
						case HorizontalAxisOptions.Custom: finalHorizontalAxis = horizontalAxis; break;
						default: throw new System.NotImplementedException();
					}

					switch (hexGridVerticalAxisOptions)
					{
						case VerticalAxisOptions.BottomToTop: finalVerticalAxis = Vector3.up * verticalAxisLength; break;
						case VerticalAxisOptions.TopToBottom: finalVerticalAxis = Vector3.down * verticalAxisLength; break;
						case VerticalAxisOptions.Custom: finalVerticalAxis = horizontalAxis; break;
						default: throw new System.NotImplementedException();
					}

					break;

				default: throw new System.NotImplementedException();
			}

			Vector3 finalNormal;
			switch (normalOptions)
			{
				case NormalOptions.Automatic: finalNormal = Vector3.Cross(finalVerticalAxis, finalHorizontalAxis).normalized; break;
				case NormalOptions.Forward: finalNormal = Vector3.forward; break;
				case NormalOptions.Back: finalNormal = Vector3.back; break;
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
