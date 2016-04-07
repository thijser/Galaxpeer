﻿
using System.Collections.Generic;

namespace Galaxpeer
{
	public class ConcurrentDictionary<TKey, TValue>
	{
		public delegate void Functor (TKey key, TValue value);
		public delegate bool Filter (TKey key, TValue value);

		private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

		public void Set (TKey key, TValue value)
		{
			lock (dictionary) {
				dictionary[key] = value;
			}
		}

		public bool ContainsKey (TKey key)
		{
			lock (dictionary) {
				return dictionary.ContainsKey (key);
			}
		}

		public TValue Get (TKey key)
		{
			TValue value;
			lock (dictionary) {
				dictionary.TryGetValue(key, out value);
			}
			return value;
		}

		public bool Remove (TKey key)
		{
			lock (dictionary) {
				return dictionary.Remove (key);
			}
		}

		public void ForEach (Functor func)
		{
			lock (dictionary) {
				foreach (var item in dictionary) {
					func (item.Key, item.Value);
				}
			}
		}

		public void RemoveWhere (Filter filter)
		{
			List<TKey> toRemove = new List<TKey> ();
			lock (dictionary) {
				foreach (var item in dictionary) {
					if (filter (item.Key, item.Value)) {
						toRemove.Add (item.Key);
					}
				}

				foreach (var item in toRemove) {
					dictionary.Remove (item);
				}
			}
		}
	}
}