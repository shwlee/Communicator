using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Threading
{
	public class StaSynchronizationContext : SynchronizationContext, IDisposable
	{
		private BlockingCollection<Action> _workingCollection;
		private Thread _workerThread;
		private bool _isDisposed;

		private static uint _copyCount;

		public StaSynchronizationContext()
			: this("Noname")
		{
		}

		public StaSynchronizationContext(string contextName)
		{
			this._workingCollection = new BlockingCollection<Action>();
			this._workerThread = new Thread(DoWork)
			{
				Name = contextName
			};

			this._workerThread.Start();
		}

		public bool IsInWorker
		{
			get { return Thread.CurrentThread.ManagedThreadId == this._workerThread.ManagedThreadId; }
		}

		private void DoWork()
		{
			SynchronizationContext.SetSynchronizationContext(this);

			while (_isDisposed == false)
			{
				try
				{
					var item = this._workingCollection.Take();
					item();
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Worker exception : " + ex);
				}
			}

			Console.WriteLine("Worker Stop");
		}

		public override void Post(SendOrPostCallback d, object state)
		{
			try
			{
				this._workingCollection.TryAdd(() => d(state));
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Post exception : " + ex);
			}
		}

		public override void Send(SendOrPostCallback d, object state)
		{
			try
			{
				var future = new TaskCompletionSource<bool>();
				this._workingCollection.TryAdd(() => WaitForWorkDone(d, state, future));
				future.Task.Wait();
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Send exception : " + ex);
			}
		}

		private void WaitForWorkDone(SendOrPostCallback d, object state, TaskCompletionSource<bool> tcs)
		{
			try
			{
				d(state);
				tcs.SetResult(true);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("DoWork exception by Send : " + ex);
				tcs.SetException(ex);
			}
		}

		~StaSynchronizationContext()
		{
			Console.WriteLine("SyncContext Finalizer");
			DoDispose(false);
		}

		public void Dispose()
		{
			Console.WriteLine("SyncContext Dispose");
			DoDispose(true);
			GC.SuppressFinalize(this);
		}

		public virtual void DoDispose(bool disposing)
		{
			try
			{
				if (this._isDisposed)
				{
					return;
				}

				if (disposing == false)
				{
					return;
				}

				this.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine("SynchronizationContext dispose exception : " + ex);
				throw;
			}
			finally
			{
				Console.WriteLine("Disposed");
			}
		}

		public void Close()
		{
			this._isDisposed = true;

			this._workerThread.Abort();
			this._workerThread = null;

			this._workingCollection.Dispose();
			this._workingCollection = null;
		}
	}
}
