using System;

namespace Experilous
{
	public delegate void AwaitingRefreshEventHandler(IRefreshable refreshable);
	public delegate void RefreshedEventHandler(IRefreshable refreshable, bool wasAwaitingRefresh);

	public interface IRefreshable
	{
		event AwaitingRefreshEventHandler AwaitingRefresh;
		event RefreshedEventHandler Refreshed;

		bool Refresh();
		bool RefreshImmediately();
		bool RefreshImmediatelyIfAwaiting();
		bool isAwaitingRefresh { get; }

		void PropagateRefresh(IRefreshable source, bool wasAwaitingRefresh);
	}
}
