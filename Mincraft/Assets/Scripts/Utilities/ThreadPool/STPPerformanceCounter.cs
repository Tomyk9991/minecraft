using System;
using System.Diagnostics;
using System.Threading;

namespace Amib.Threading
{
    public interface ISTPPerformanceCountersReader
    {
        long InUseThreads { get; }
        long ActiveThreads { get; }
        long WorkItemsQueued { get; }
        long WorkItemsProcessed { get; }
    }
}

namespace Amib.Threading.Internal
{
    internal interface ISTPInstancePerformanceCounters : IDisposable
    {
        void Close();
        void SampleThreads(long activeThreads, long inUseThreads);
        void SampleWorkItems(long workItemsQueued, long workItemsProcessed);
        void SampleWorkItemsWaitTime(TimeSpan workItemWaitTime);
        void SampleWorkItemsProcessTime(TimeSpan workItemProcessTime);
    }
#if !(_WINDOWS_CE) && !(_SILVERLIGHT) && !(WINDOWS_PHONE)

    internal enum STPPerformanceCounterType
	{
		// Fields
		ActiveThreads				= 0,
		InUseThreads				= 1,
		OverheadThreads				= 2,
		OverheadThreadsPercent		= 3,
		OverheadThreadsPercentBase	= 4,

		WorkItems					= 5,
		WorkItemsInQueue			= 6,
		WorkItemsProcessed			= 7,

		WorkItemsQueuedPerSecond	= 8,
		WorkItemsProcessedPerSecond	= 9,

		AvgWorkItemWaitTime			= 10,
		AvgWorkItemWaitTimeBase		= 11,

		AvgWorkItemProcessTime		= 12,
		AvgWorkItemProcessTimeBase	= 13,

		WorkItemsGroups				= 14,

		LastCounter					= 14,
	}
 

	/// <summary>
	/// Summary description for STPPerformanceCounter.
	/// </summary>
	internal class STPPerformanceCounter
	{
		protected string _counterHelp;
		protected string _counterName;

		// Methods
		public STPPerformanceCounter(
			string counterName, 
			string counterHelp)
		{
			_counterName = counterName;
			_counterHelp = counterHelp;
		}
 
		// Properties
		public string Name
		{
			get
			{
				return _counterName;
			}
		}
	}

	internal class STPPerformanceCounters
	{
		// Fields
		internal STPPerformanceCounter[] _stpPerformanceCounters;
		private static readonly STPPerformanceCounters _instance;
		internal const string _stpCategoryHelp = "SmartThreadPool performance counters";
		internal const string _stpCategoryName = "SmartThreadPool";

		// Methods
		static STPPerformanceCounters()
		{
			_instance = new STPPerformanceCounters();
		}
 
		private STPPerformanceCounters()
		{
			STPPerformanceCounter[] stpPerformanceCounters = new STPPerformanceCounter[] 
			{ 
			};

			_stpPerformanceCounters = stpPerformanceCounters;
		}

 
		// Properties
		public static STPPerformanceCounters Instance
		{
			get
			{
				return _instance;
			}
		}
 	}

	internal class STPInstancePerformanceCounter : IDisposable
	{
		// Fields
        private bool _isDisposed;

		// Methods
		protected STPInstancePerformanceCounter()
		{
            _isDisposed = false;
		}

		public STPInstancePerformanceCounter(
			string instance, 
			STPPerformanceCounterType spcType) : this()
		{
			STPPerformanceCounters counters = STPPerformanceCounters.Instance;
		}


		public void Close()
		{
		}
 
		public void Dispose()
		{
            Dispose(true);
		}

        public virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Close();
                }
            }
            _isDisposed = true;
        }
 
	}



	internal class STPInstancePerformanceCounters : ISTPInstancePerformanceCounters
	{
        private bool _isDisposed;
		// Fields
		private STPInstancePerformanceCounter[] _pcs;
		private static readonly STPInstancePerformanceCounter _stpInstanceNullPerformanceCounter;

		// Methods
 
		public STPInstancePerformanceCounters(string instance)
		{
            _isDisposed = false;
			_pcs = new STPInstancePerformanceCounter[(int)STPPerformanceCounterType.LastCounter];

            // Call the STPPerformanceCounters.Instance so the static constructor will
            // intialize the STPPerformanceCounters singleton.
			STPPerformanceCounters.Instance.GetHashCode();

			for (int i = 0; i < _pcs.Length; i++)
			{
				if (instance != null)
				{
					_pcs[i] = new STPInstancePerformanceCounter(
						instance, 
						(STPPerformanceCounterType) i);
				}
				else
				{
					_pcs[i] = _stpInstanceNullPerformanceCounter;
				}
			}
		}
 

		public void Close()
		{
			if (null != _pcs)
			{
				for (int i = 0; i < _pcs.Length; i++)
				{
                    if (null != _pcs[i])
                    {
                        _pcs[i].Dispose();
                    }
				}
				_pcs = null;
			}
		}

		public void Dispose()
		{
            Dispose(true);
		}

        public virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Close();
                }
            }
            _isDisposed = true;
        }
 
		private STPInstancePerformanceCounter GetCounter(STPPerformanceCounterType spcType)
		{
			return _pcs[(int) spcType];
		}

		public void SampleThreads(long activeThreads, long inUseThreads)
		{
			
		}

		public void SampleWorkItems(long workItemsQueued, long workItemsProcessed)
		{

		}

		public void SampleWorkItemsWaitTime(TimeSpan workItemWaitTime)
		{

		}

		public void SampleWorkItemsProcessTime(TimeSpan workItemProcessTime)
		{

		}
    }
#endif

    internal class NullSTPInstancePerformanceCounters : ISTPInstancePerformanceCounters, ISTPPerformanceCountersReader
	{
		private static readonly NullSTPInstancePerformanceCounters _instance = new NullSTPInstancePerformanceCounters();

		public static NullSTPInstancePerformanceCounters Instance
		{
			get { return _instance; }
		}

 		public void Close() {}
		public void Dispose() {}
 
		public void SampleThreads(long activeThreads, long inUseThreads) {}
		public void SampleWorkItems(long workItemsQueued, long workItemsProcessed) {}
		public void SampleWorkItemsWaitTime(TimeSpan workItemWaitTime) {}
		public void SampleWorkItemsProcessTime(TimeSpan workItemProcessTime) {}
        public long InUseThreads
        {
            get { return 0; }
        }

        public long ActiveThreads
        {
            get { return 0; }
        }

        public long WorkItemsQueued
        {
            get { return 0; }
        }

        public long WorkItemsProcessed
        {
            get { return 0; }
        }
	}

    internal class LocalSTPInstancePerformanceCounters : ISTPInstancePerformanceCounters, ISTPPerformanceCountersReader
    {
        public void Close() { }
        public void Dispose() { }

        private long _activeThreads;
        private long _inUseThreads;
        private long _workItemsQueued;
        private long _workItemsProcessed;

        public long InUseThreads
        {
            get { return _inUseThreads; }
        }

        public long ActiveThreads
        {
            get { return _activeThreads; }
        }

        public long WorkItemsQueued
        {
            get { return _workItemsQueued; }
        }

        public long WorkItemsProcessed
        {
            get { return _workItemsProcessed; }
        }

        public void SampleThreads(long activeThreads, long inUseThreads)
        {
            _activeThreads = activeThreads;
            _inUseThreads = inUseThreads;
        }

        public void SampleWorkItems(long workItemsQueued, long workItemsProcessed)
        {
            _workItemsQueued = workItemsQueued;
            _workItemsProcessed = workItemsProcessed;
        }

        public void SampleWorkItemsWaitTime(TimeSpan workItemWaitTime)
        {
            // Not supported
        }

        public void SampleWorkItemsProcessTime(TimeSpan workItemProcessTime)
        {
            // Not supported
        }
    }
}
