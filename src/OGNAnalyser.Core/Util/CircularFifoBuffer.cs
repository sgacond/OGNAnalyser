using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Core.Util
{
    public class CircularFifoBuffer<T> : IEnumerable<T>
    {
        T[] buffer;
        int head;
        int tail;
        object lockObj = new object();

        private int nextPosition(int position) => (position + 1) % Capacity;
        private int prevPosition(int position) => (position == 0) ? Capacity - 1 : position - 1;

        public int Length { get; private set; }
        public int Capacity { get; private set; }

        public bool Empty { get { return Length == 0; } }

        public bool Full { get { return Length == Capacity; } }
        
        public CircularFifoBuffer(int bufferSize)
        {
            buffer = new T[bufferSize];
            this.Capacity = bufferSize;
            head = bufferSize - 1;
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
            var i = head;
            do
            {
                yield return buffer[i];
                i = prevPosition(i);
            } while (i != prevPosition(tail));
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
