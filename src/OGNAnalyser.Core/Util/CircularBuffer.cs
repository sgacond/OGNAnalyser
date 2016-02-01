using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Core.Util
{
    public class CircularBuffer<T> : IEnumerable<T>
    {
        T[] buffer;
        int head;
        int tail;
        object lockObj = new object();

        private int nextPosition(int position) => (position + 1) % Capacity;

        public int Length { get; private set; }
        public int Capacity { get; private set; }

        public bool Empty { get { return Length == 0; } }

        public bool Full { get { return Length == Capacity; } }
        
        public CircularBuffer(int bufferSize)
        {
            buffer = new T[bufferSize];
            this.Capacity = bufferSize;
            head = bufferSize - 1;
        }

        public T Dequeue()
        {
            lock (lockObj)
            {
                if (Empty) throw new InvalidOperationException("Queue exhausted");

                T dequeued = buffer[tail];
                tail = nextPosition(tail);
                Length--;
                return dequeued;
            }
        }

        public void Enqueue(T toAdd)
        {
            lock (lockObj)
            {
                head = nextPosition(head);
                buffer[head] = toAdd;
                if (Full)
                    tail = nextPosition(tail);
                else
                    Length++;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            var i = tail;
            do
            {
                yield return buffer[i];
                i = nextPosition(i);
            } while (i != nextPosition(head));
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
