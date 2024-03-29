﻿using UnityEngine;
using System.Collections;
using System;

namespace ColbyDoan
{
	public class Heap<T> where T : IHeapItem<T>
	{
		T[] items;
		int currentItemCount;

		public T TopItem => items[0];

		public Heap(int maxHeapSize)
		{
			items = new T[maxHeapSize];
		}

		public void Add(T item)
		{
			item.HeapIndex = currentItemCount;
			items[currentItemCount] = item;
			SortUp(item);
			currentItemCount++;
		}

		/// <summary>
        /// Returns and removes largest valued member
        /// </summary>
        /// <returns></returns>
		public T RemoveFirst()
		{
			T firstItem = items[0];
			currentItemCount--;
			items[0] = items[currentItemCount];
			items[0].HeapIndex = 0;
			SortDown(items[0]);
			return firstItem;
		}

		public void UpdateItem(T item)
		{
			SortUp(item);
		}

		public void Clear()
        {
			currentItemCount = 0;
        }

		public int Count
		{
			get
			{
				return currentItemCount;
			}
		}

		public bool Contains(T item)
		{
			return Equals(items[item.HeapIndex], item);
		}

		void SortDown(T item)
		{
			while (true)
			{
				int childIndexLeft = item.HeapIndex * 2 + 1;
				int childIndexRight = item.HeapIndex * 2 + 2;
				int swapIndex;

				// check for child
				if (childIndexLeft < currentItemCount)
				{
					// get index of highest valued child
					swapIndex = childIndexLeft;

					if (childIndexRight < currentItemCount)
					{
						if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
						{
							swapIndex = childIndexRight;
						}
					}

					// swap with child if child is larger
					if (item.CompareTo(items[swapIndex]) < 0)
					{
						Swap(item, items[swapIndex]);
					}
					else
					{
						// parent is larger, stop sort
						return;
					}

				}
				else
				{
					// bottom of heap, stop sort
					return;
				}

			}
		}

		void SortUp(T item)
		{
			int parentIndex = (item.HeapIndex - 1) / 2;

			while (true)
			{
				T parentItem = items[parentIndex];
				if (item.CompareTo(parentItem) > 0)
				{
					Swap(item, parentItem);
				}
				else
				{
					break;
				}

				parentIndex = (item.HeapIndex - 1) / 2;
			}
		}

		void Swap(T itemA, T itemB)
		{
			items[itemA.HeapIndex] = itemB;
			items[itemB.HeapIndex] = itemA;
			int itemAIndex = itemA.HeapIndex;
			itemA.HeapIndex = itemB.HeapIndex;
			itemB.HeapIndex = itemAIndex;
		}
	}

	public interface IHeapItem<T> : IComparable<T>
	{
		int HeapIndex
		{
			get;
			set;
		}
	}
}