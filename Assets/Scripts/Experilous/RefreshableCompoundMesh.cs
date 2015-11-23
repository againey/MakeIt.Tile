using System;
using UnityEngine;

namespace Experilous
{
	[ExecuteInEditMode]
	public abstract class RefreshableCompoundMesh : CompoundMesh, IRefreshable
	{
		public event AwaitingRefreshEventHandler AwaitingRefresh;
		public event RefreshedEventHandler Refreshed;

		private bool _awaitingRefresh = false;

		public bool isAwaitingRefresh
		{
			get
			{
				return _awaitingRefresh;
			}
		}

		private void OnAwake()
		{
			Refresh();
		}

		private void OnValidate()
		{
			Refresh();
		}

		public bool Refresh()
		{
			if (_awaitingRefresh == false)
			{
				_awaitingRefresh = true;
				RaiseAwaitingRefresh();
				return true;
			}
			return false;
		}

		public bool RefreshImmediately()
		{
			bool wasAwaitingRefresh = _awaitingRefresh;
			RefreshContent();
			_awaitingRefresh = false;
			RaiseRefreshed(wasAwaitingRefresh);
			return wasAwaitingRefresh;
		}

		public bool RefreshImmediatelyIfAwaiting()
		{
			if (_awaitingRefresh == true)
			{
				RefreshContent();
				_awaitingRefresh = false;
				RaiseRefreshed(true);
				return true;
			}
			return false;
		}

		public void PropagateRefresh(IRefreshable source, bool wasAwaitingRefresh)
		{
			Refresh();
		}

		protected void Update()
		{
			if (_awaitingRefresh == true)
			{
				RefreshContent();
				_awaitingRefresh = false;
				RaiseRefreshed(true);
			}
		}

		protected abstract void RefreshContent();

		private void RaiseAwaitingRefresh()
		{
			var handler = AwaitingRefresh;
			if (handler != null)
			{
				handler(this);
			}
		}

		private void RaiseRefreshed(bool wasAwaitingRefresh)
		{
			var handler = Refreshed;
			if (handler != null)
			{
				handler(this, wasAwaitingRefresh);
			}
		}
	}
}
