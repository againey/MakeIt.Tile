/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.Topological
{
	public interface IFaceSpatialPartitioning
	{
		Topology.Face FindFace(Vector3 point);
		Topology.Face FindFace(Ray ray);
		Topology.Face FindFace(ScaledRay ray);
	}
}
