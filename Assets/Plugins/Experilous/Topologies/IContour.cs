/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.Topologies
{
	public enum ContourState
	{
		Normal,
		Breaking,
		Broken,
		Resuming,
	}

	public interface IContour
	{
		VoronoiDiagram voronoiDiagram { get; }
		float targetDistance { get; }

		ContourState state { get; }

		Vector3 position { get; }
		Vector3 sitePosition { get; }

		float distance { get; }

		VoronoiSiteType siteType { get; }
		int siteIndex { get; }

		int index { get; set; }

		bool MoveNext();
	}
}
