using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Threading
{
	public class MainSynchronizationContext
	{
		#region Singleton

		private static Lazy<MainSynchronizationContext> _instance = new Lazy<MainSynchronizationContext>(() => new MainSynchronizationContext());
		private static MainSynchronizationContext Instance
		{
			get { return _instance.Value; }
		}

		#endregion

		private StaSynchronizationContext _syncContext;

		private MainSynchronizationContext()
		{
			this._syncContext = new StaSynchronizationContext("MainSyncContext");
		}

		public static bool IsInWorker
		{
			get { return Instance._syncContext.IsInWorker; }
		}

		public static void Post(SendOrPostCallback d, object state)
		{
			Instance._syncContext.Post(d, state);
		}

		public static void Post(SendOrPostCallback d)
		{
			Instance._syncContext.Post(d, null);
		}

		public static void Send(SendOrPostCallback d, object state)
		{
			Instance._syncContext.Send(d, state);
		}

		public static void Send(SendOrPostCallback d)
		{
			Instance._syncContext.Send(d, null);
		}

		public static void Release()
		{
			Instance._syncContext.Dispose();
		}
	}
}
