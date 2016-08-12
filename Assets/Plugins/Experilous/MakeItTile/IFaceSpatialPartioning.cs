/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using Experilous.MakeIt.Utilities;

namespace Experilous.MakeIt.Tile
{
	public interface IFaceSpatialPartitioning
	{
		Topology.Face FindFace(Vector3 point);
		Topology.Face FindFace(Ray ray);
		Topology.Face FindFace(ScaledRay ray);
	}
}
