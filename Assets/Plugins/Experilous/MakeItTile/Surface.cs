/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using Experilous.Numerics;

namespace Experilous.MakeItTile
{
	public interface ISurface
	{
		bool isFlat { get; }
		bool isCurved { get; }

		bool isConvex { get; }
		bool isConcave { get; }

		Vector3 OffsetVertToVertAttribute(Vector3 position, EdgeWrap edgeWrap);
		Vector3 OffsetVertToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap);
		Vector3 OffsetVertToFaceAttribute(Vector3 position, EdgeWrap edgeWrap);
		Vector3 OffsetEdgeToVertAttribute(Vector3 position, EdgeWrap edgeWrap);
		Vector3 OffsetEdgeToFaceAttribute(Vector3 position, EdgeWrap edgeWrap);
		Vector3 OffsetFaceToVertAttribute(Vector3 position, EdgeWrap edgeWrap);
		Vector3 OffsetFaceToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap);
		Vector3 OffsetFaceToFaceAttribute(Vector3 position, EdgeWrap edgeWrap);

		Vector3 ReverseOffsetVertToVertAttribute(Vector3 position, EdgeWrap edgeWrap);
		Vector3 ReverseOffsetVertToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap);
		Vector3 ReverseOffsetVertToFaceAttribute(Vector3 position, EdgeWrap edgeWrap);
		Vector3 ReverseOffsetEdgeToVertAttribute(Vector3 position, EdgeWrap edgeWrap);
		Vector3 ReverseOffsetEdgeToFaceAttribute(Vector3 position, EdgeWrap edgeWrap);
		Vector3 ReverseOffsetFaceToVertAttribute(Vector3 position, EdgeWrap edgeWrap);
		Vector3 ReverseOffsetFaceToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap);
		Vector3 ReverseOffsetFaceToFaceAttribute(Vector3 position, EdgeWrap edgeWrap);
	}

	public abstract class Surface : ScriptableObject, ISurface
	{
		public abstract bool isFlat { get; }
		public abstract bool isCurved { get; }

		public abstract bool isConvex { get; }
		public abstract bool isConcave { get; }

		public abstract Vector3 Intersect(Ray ray);
		public abstract Vector3 Intersect(ScaledRay ray);
		public abstract bool Intersect(Ray ray, out Vector3 intersection);
		public abstract bool Intersect(ScaledRay ray, out Vector3 intersection);

		public abstract Vector3 Project(Vector3 position);
		public abstract Vector3 GetNormal(Vector3 position);

		public virtual Vector3 OffsetVertToVertAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		public virtual Vector3 OffsetVertToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		public virtual Vector3 OffsetVertToFaceAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		public virtual Vector3 OffsetEdgeToVertAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		public virtual Vector3 OffsetEdgeToFaceAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		public virtual Vector3 OffsetFaceToVertAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		public virtual Vector3 OffsetFaceToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		public virtual Vector3 OffsetFaceToFaceAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }

		public virtual Vector3 ReverseOffsetVertToVertAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		public virtual Vector3 ReverseOffsetVertToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		public virtual Vector3 ReverseOffsetVertToFaceAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		public virtual Vector3 ReverseOffsetEdgeToVertAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		public virtual Vector3 ReverseOffsetEdgeToFaceAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		public virtual Vector3 ReverseOffsetFaceToVertAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		public virtual Vector3 ReverseOffsetFaceToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		public virtual Vector3 ReverseOffsetFaceToFaceAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
	}
}
