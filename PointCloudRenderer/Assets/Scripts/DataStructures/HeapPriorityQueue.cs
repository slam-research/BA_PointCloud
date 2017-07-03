﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataStructures {
    public class HeapPriorityQueue<I, T> : PriorityQueue<I, T> where I : IComparable<I> {

        private Entry[] heapArray;
        private int count = 0;
        private int initialCapacity;
        protected object locker = new object();

        public HeapPriorityQueue() : this(20) {
        }

        public HeapPriorityQueue(int capacity) {
            lock (locker) {
                initialCapacity = capacity;
                heapArray = new Entry[capacity];
            }
        }

        public HeapPriorityQueue(HeapPriorityQueue<I, T> original) {
            lock (locker) {
                initialCapacity = original.initialCapacity;
                count = original.count;
                heapArray = new Entry[original.heapArray.Length];
                for (int i = 0; i < heapArray.Length; i++) {
                    heapArray[i] = original.heapArray[i];
                }
            }
        }

        private void IncreaseCapacity() {
            Entry[] newArray = new Entry[(int)(heapArray.Length * 1.5)];
            for (int i = 0; i < heapArray.Length; i++) {
                newArray[i] = heapArray[i];
            }
            heapArray = newArray;
        }

        /* Swaps the element at the given index with the lower elements to ensure the heap-property
         */
        private void HeapifyDown(int currentindex) {
            int i = currentindex;
            while (2 * i < count) {
                //heapArray[i] hat ein linkes Kind (heapArray[j])
                int j = 2 * i;
                if (j < count - 1) {
                    //heapArray[i] hat ein rechtes Kind (heapArray[j+1])
                    if (heapArray[j].priority.CompareTo(heapArray[j + 1].priority) < 0) {
                        j = j + 1;
                    }
                }
                //j ist der Index des größeren Kindes. Mit diesem muss getauscht werden, falls Heap-Bedingung verletzt (i < j)
                if (heapArray[i].priority.CompareTo(heapArray[j].priority) < 0) {
                    Swap(i, j);
                    i = j;
                } else {
                    i = count; // Finished
                }
            }
        }

        public override int Count {
            get {
                lock (locker) {
                    return count;
                }
            }
        }

        public override void Clear() {
            lock (locker) {
                count = 0;
                heapArray = new Entry[initialCapacity];
            }
        }

        //Complexity: O(logn)
        public override T Dequeue() {
            I p;
            return Dequeue(out p);
        }

        //Removes and returns the element with the highest priority from the queue. The priority is given through the parameter. Throws an InvalidOperationExcpetion if no element exists
        public override T Dequeue(out I priority) {
            lock (locker) {
                if (count == 0) throw new InvalidOperationException("Queue is empty!");
                priority = heapArray[0].priority;
                T max = heapArray[0].element;
                --count;
                heapArray[0] = heapArray[count];
                heapArray[count] = null;
                HeapifyDown(0);
                return max;
            }
        }

        public override I MaxPriority() {
            lock (locker) {
                if (count == 0) throw new InvalidOperationException("Queue is empty!");
                return heapArray[0].priority;
            }
        }

        //Complexity: O(logn)
        public override void Enqueue(T element, I priority) {
            lock (locker) {
                if (count == heapArray.Length) {
                    IncreaseCapacity();
                }
                heapArray[count] = new Entry(priority, element);
                ++count;
                HeapifyUp(count - 1);
            }
        }

        /* Swaps the element at the given index with the upper elements to ensure the heap-property
         */
        private void HeapifyUp(int currentindex) {
            int parentindex = (currentindex) / 2;
            while (currentindex > 0 && heapArray[currentindex].priority.CompareTo(heapArray[parentindex].priority) > 0) {
                Swap(currentindex, parentindex);
                currentindex = parentindex;
                parentindex = parentindex / 2;
            }
        }

        private void Swap(int i, int j) {
            Entry ei = heapArray[i];
            heapArray[i] = heapArray[j];
            heapArray[j] = ei;
        }

        //Complexity: O(n)
        public override IEnumerator<T> GetEnumerator() {
            lock (locker) {
                return new HeapPriorityQueueEnumerator(this);
            }
        }

        public override bool IsEmpty() {
            lock (locker) {
                return count == 0;
            }
        }

        //Complexity: O(1)
        public override T Peek() {
            lock (locker) {
                if (Count == 0) throw new InvalidOperationException("Queue is empty!");
                return heapArray[0].element;
            }
        }

        //Complexity: O(n)
        public override void Remove(T element) {
            lock (locker) {
                for (int i = 0; i < count; i++) {
                    if (heapArray[i].element.Equals(element)) {
                        RemoveByArrayIndex(i);
                        return;
                    }
                }
            }
        }

        //Complexity: O(n)
        public override void Remove(T element, I priority) {
            lock (locker) {
                if (count == 0) return;
                //Suche nach Priority -> ermöglicht überspringen von Teilbäumen
                Queue<int> indizesToCheck = new Queue<int>();
                indizesToCheck.Enqueue(0);
                while (indizesToCheck.Count != 0) {
                    int i = indizesToCheck.Dequeue();
                    Entry entry = heapArray[i];
                    if (entry.priority.Equals(priority) && entry.element.Equals(element)) {
                        RemoveByArrayIndex(i);
                        return;
                    } else {
                        //Wenn die Kinder kleiner als der gewünschte Index sind, brauchen sie nicht mehr angesehen zu werden
                        if (i * 2 < count && heapArray[i * 2].priority.CompareTo(priority) >= 0) {
                            indizesToCheck.Enqueue(i * 2);
                        }
                        if (i * 2 + 1 < count && heapArray[i * 2 + 1].priority.CompareTo(priority) >= 0) {
                            indizesToCheck.Enqueue(i * 2 + 1);
                        }
                    }
                }
            }
        }

        private void RemoveByArrayIndex(int i) {
            int lastIdx = count - 1;
            Swap(lastIdx, i);
            count--;
            if (i != lastIdx) {
                //Wenn Root oder Parent größer ist (wies sein soll) -> checken nach unten
                if (i == 0 || heapArray[i].priority.CompareTo(heapArray[i / 2].priority) < 0) {
                    HeapifyDown(i);
                } else {
                    HeapifyUp(i);
                }
            }
        }

        private class Entry {
            public I priority;
            public T element;

            public Entry(I p, T e) {
                priority = p;
                element = e;
            }
        }

        private class HeapPriorityQueueEnumerator : IEnumerator<T> {

            private HeapPriorityQueue<I, T> original;
            private HeapPriorityQueue<I, T> copy;
            private T current;
            private bool valid = false;

            public HeapPriorityQueueEnumerator(HeapPriorityQueue<I,T> original) {
                this.original = original;
                Reset();
            }

            public T Current {
                get {
                    if (!valid) {
                        throw new InvalidOperationException("No current element!");
                    } else {
                        return current;
                    }
                }
            }

            object IEnumerator.Current {
                get {
                    return Current;
                }
            }

            public void Dispose() {
                valid = false;
                copy.Clear();
                copy = null;
                original = null;
                current = default(T);
            }

            public bool MoveNext() {
                bool hasNext = !copy.IsEmpty();
                if (hasNext) {
                    current = copy.Dequeue();
                    valid = true;
                } else {
                    valid = false;
                }
                return hasNext;
            }

            public void Reset() {
                valid = false;
                copy = new HeapPriorityQueue<I, T>(original);
            }
        }
    }
}
