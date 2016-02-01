using OGNAnalyser.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OGNAnalyser.Tests.Analyser
{
    public class CircularFifoBufferTests
    {
        [Fact]
        public void CapacityOk()
        {
            var buf = new CircularFifoBuffer<int>(10);
            buf.Enqueue(1);
            Assert.Equal(10, buf.Capacity);
        }

        [Fact]
        public void LengthOkUnderCapacity()
        {
            var buf = new CircularFifoBuffer<int>(10);
            buf.Enqueue(1);
            buf.Enqueue(2);
            buf.Enqueue(3);
            Assert.Equal(3, buf.Length);
            Assert.Equal(buf.Count(), buf.Length);
        }

        [Fact]
        public void LengthOkOverCapacity()
        {
            var buf = new CircularFifoBuffer<int>(10);
            buf.Enqueue(1);
            buf.Enqueue(2);
            buf.Enqueue(3);
            buf.Enqueue(4);
            buf.Enqueue(5);
            buf.Enqueue(6);
            buf.Enqueue(7);
            buf.Enqueue(8);
            buf.Enqueue(9);
            buf.Enqueue(10);
            buf.Enqueue(11);
            Assert.Equal(10, buf.Length);
            Assert.Equal(buf.Count(), buf.Length);
        }

        [Fact]
        public void OrderOkUnderCapacity()
        {
            var buf = new CircularFifoBuffer<int>(10);
            buf.Enqueue(1);
            buf.Enqueue(2);
            buf.Enqueue(3);
            Assert.Equal(3, buf.First());
            Assert.Equal(1, buf.Last());
        }

        [Fact]
        public void OrderOkOverCapacity()
        {
            var buf = new CircularFifoBuffer<int>(10);
            buf.Enqueue(1);
            buf.Enqueue(2);
            buf.Enqueue(3);
            buf.Enqueue(4);
            buf.Enqueue(5);
            buf.Enqueue(6);
            buf.Enqueue(7);
            buf.Enqueue(8);
            buf.Enqueue(9);
            buf.Enqueue(10);
            buf.Enqueue(11);
            buf.Enqueue(12);
            Assert.Equal(12, buf.First());
            Assert.Equal(3, buf.Last());
        }

    }
}
