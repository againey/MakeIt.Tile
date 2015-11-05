using UnityEngine;
using System.Collections.Generic;

namespace Experilous
{
	[ExecuteInEditMode]
	public abstract class DynamicMesh : UniqueMesh
	{
		private bool _invalidated = false;

		public void InvalidateMesh()
		{
			_invalidated = true;
		}

		public void Update()
		{
			if (_invalidated)
			{
				RebuildMesh();
				_invalidated = false;
			}
		}


		protected abstract void RebuildMesh();
	}
}
