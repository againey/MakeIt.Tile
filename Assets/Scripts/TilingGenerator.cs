using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Tiling;

[ExecuteInEditMode]
public abstract class TilingGenerator : MonoBehaviour
{
	private Topology _topology;
	private Topology.FaceAttribute<Vector3> _facePositions;
	private Topology.VertexAttribute<Vector3> _vertexPositions;

	private bool _invalidated = true;

	public Topology Topology { get { return _topology; } }
	public Topology.FaceAttribute<Vector3> FacePositions { get { return _facePositions; } }
	public Topology.VertexAttribute<Vector3> VertexPositions { get { return _vertexPositions; } }

	public void Invalidate()
	{
		_invalidated = true;
	}

	void Awake()
	{
		Invalidate();
	}

	void OnValidate()
	{
		Invalidate();
	}

	void Update()
	{
		if (_invalidated)
		{
			RebuildTiling(out _topology, out _facePositions, out _vertexPositions);
			_invalidated = false;
		}
	}

	protected abstract void RebuildTiling(out Topology topology, out Topology.FaceAttribute<Vector3> facePositions, out Topology.VertexAttribute<Vector3> vertexPositions);
}
