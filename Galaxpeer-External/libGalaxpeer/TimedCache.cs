
using System;
using System.Threading;

namespace Galaxpeer
{
	public class TimedCache<TKey, TValue>
	{
		struct ValuePair
		{
			public Timer timer;
			public TValue value;
		}

		private ConcurrentDictionary<TKey, ValuePair> dictionary = new ConcurrentDictionary<TKey, ValuePair> ();

		public delegate void TimeoutHandler (TKey key, TValue value);
		public event TimeoutHandler OnTimeout;
		public event TimeoutHandler OnRemove;

		public int CacheTimeout;

		public void Set (TKey key, TValue value)
		{
			ValuePair pair;
			pair.timer = new Timer(TimerHandler, key, CacheTimeout, Timeout.Infinite);
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

		public bool Remove (TKey key)
		{
			ValuePair pair = dictionary.Get(key);
			if (pair.timer != null) {
				pair.timer.Dispose ();
			}
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
				pair.timer.Change(CacheTimeout, Timeout.Infinite);
			}
			catch (Exception) {}
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
	}
}
