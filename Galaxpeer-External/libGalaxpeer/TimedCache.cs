
using System;
using System.Threading;

namespace Galaxpeer
{
	public class TimedCache<TKey, TValue>
	{
		struct ValuePair
		{
			public Timer timeout;
			public Timer interval;
			public TValue value;
		}

		private ConcurrentDictionary<TKey, ValuePair> dictionary = new ConcurrentDictionary<TKey, ValuePair> ();

		public delegate void TimeoutHandler (TKey key, TValue value);
		public event TimeoutHandler OnInterval;
		public event TimeoutHandler OnTimeout;
		public event TimeoutHandler OnRemove;

		public int CacheTimeout;
		public int CacheInterval;

		public void Set (TKey key, TValue value)
		{
			stop (key);
			ValuePair pair = new ValuePair();
			if (CacheTimeout > 0) {
				pair.timeout = new Timer (TimerHandler, key, CacheTimeout, Timeout.Infinite);
			}
			if (CacheInterval > 0) {
				pair.interval = new Timer (IntervalHandler, key, CacheInterval, CacheInterval);
			}
			pair.value = value;
			dictionary.Set (key, pair);
		}

		public TValue Get (TKey key)
		{
			return dictionary.Get (key).value;
		}

		public bool ContainsKey (TKey key)
		{
			return dictionary.ContainsKey (key);
		}

		private ValuePair stop (TKey key)
		{
			ValuePair pair = dictionary.Get(key);
			if (pair.timeout != null) {
				pair.timeout.Dispose ();
			}
			if (pair.interval != null) {
				pair.interval.Dispose ();
			}
			return pair;
		}

		public bool Remove (TKey key)
		{
			ValuePair pair = stop (key);
			bool success = dictionary.Remove (key);
			if (pair.value != null && OnRemove != null) {
				OnRemove (key, pair.value);
			}
			return success;
		}

		public void ForEach (ConcurrentDictionary<TKey, TValue>.Functor functor)
		{
			dictionary.ForEach ((TKey key, ValuePair pair) => {
				functor (key, pair.value);
			});
		}

		public void Acquire (Action action)
		{
			dictionary.Acquire (action);
		}

		public void Update (TKey key)
		{
			ValuePair pair = dictionary.Get (key);

			try {
				if (pair.timeout != null) {
					pair.timeout.Change(CacheTimeout, Timeout.Infinite);
				}
				if (pair.interval != null) {
					pair.interval.Change(CacheInterval, CacheInterval);
				}
			}
			catch (Exception) {}
		}

		public int Count
		{
			get {
				return dictionary.Count;
			}
		}

		private void TimerHandler (object obj)
		{
			TKey key = (TKey)obj;
			TValue value = Get (key);

			dictionary.Remove (key);

			if (value != null) {
				if (OnTimeout != null) {
					OnTimeout (key, value);
				}
				if (OnRemove != null) {
					OnRemove (key, value);
				}
			}
		}

		private void IntervalHandler (object obj)
		{
			if (OnInterval != null) {
				TKey key = (TKey)obj;
				TValue value = Get (key);
				if (value != null) {
					OnInterval (key, value);
				}
			}
		}
	}
}
