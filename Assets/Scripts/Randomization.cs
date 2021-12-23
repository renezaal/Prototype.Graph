using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;



public static class Randomization {
	public static RandomList<T> AtRandom<T>(this IList<T> list) {
		return new RandomList<T>(list, true);
	}

	public static T GetRandom<T>(IEnumerable<(float chance, T value)> valueChances) {
		List<(float chanceCumulative, T value)> cumulativeChances = new List<(float chanceCumulative, T value)>();
		float cumulativeChance = 0;
		foreach(var valueChance in valueChances) {
			cumulativeChance += valueChance.chance;
			cumulativeChances.Add((cumulativeChance, valueChance.value));
		}

		return cumulativeChances
			.First(x => x.chanceCumulative >= Random.Range(0f, cumulativeChance))
			.value;
	}

	public static T GetAndRemoveRandom<T>(this IList<T> list) {
		T result = list[Random.Range(0, list.Count)];
		list.Remove(result);
		return result;
	}

	public class RandomList<T> : IList<T> {
		private IList<T> _backingList = new List<T>();

		public RandomList() {
		}
		public RandomList(IList<T> list, bool connected) => this._backingList=connected ? list : new List<T>(list);
		public RandomList(IEnumerable<T> source) => this._backingList=new List<T>(source);

		public IList<T> BackingList { get => _backingList; set => _backingList=value; }

		public int Count => this._backingList.Count;

		public bool IsReadOnly => this._backingList.IsReadOnly;

		public T this[int index] { get => this._backingList[index]; set => this._backingList[index]=value; }


		private static T RemoveOne(IList<T> list) {
			if(list == null || list.Count == 0) { return default; }
			int index = Random.Range(0, list.Count);
			T element = list[index];
			list.RemoveAt(index);
			return element;
		}

		public T RemoveOne() => RemoveOne(this._backingList);

		private static T GetOne(IList<T> list) {
			if(list == null || list.Count == 0) { return default; }
			return list[Random.Range(0, list.Count)];
		}

		public T GetOne() => GetOne(this._backingList);

		public List<T> RemoveMultiple(int count) {
			List<T> result = new List<T>();
			for(int i = 0; i < count; i++) {
				result.Add(this.RemoveOne());
			}
			return result;
		}

		public List<T> GetMultiple(int count) {
			List<T> result = new List<T>();
			for(int i = 0; i < count; i++) {
				result.Add(this.GetOne());
			}
			return result;
		}
		public List<T> GetMultipleDistinct(int count) {
			List<T> result = new List<T>();
			List<T> fromList = new List<T>(this._backingList);
			for(int i = 0; i < count; i++) {
				result.Add(RemoveOne(fromList));
			}
			return result;
		}

		public int IndexOf(T item) => this._backingList.IndexOf(item);
		public void Insert(int index, T item) => this._backingList.Insert(index, item);
		public void RemoveAt(int index) => this._backingList.RemoveAt(index);
		public void Add(T item) => this._backingList.Add(item);
		public void Clear() => this._backingList.Clear();
		public bool Contains(T item) => this._backingList.Contains(item);
		public void CopyTo(T[] array, int arrayIndex) => this._backingList.CopyTo(array, arrayIndex);
		public bool Remove(T item) => this._backingList.Remove(item);
		public IEnumerator<T> GetEnumerator() => this._backingList.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) this._backingList).GetEnumerator();
	}
}
