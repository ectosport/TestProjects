using System;
using CircularBuffer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CircularBufferUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        const int kCapacity = 10;
        readonly CircularBuffer<int> cbLossless = new CircularBuffer<int>(kCapacity);
        readonly CircularBuffer<int> cbDiscard = new CircularBuffer<int>(kCapacity, CircularBuffer<int>.DataIntegrity.DiscardOldest);
            
        [TestMethod]
        public void TestAddingMultipleAtCapacityLossless()
        {
            cbLossless.Clear();
            cbLossless.Add(-1);
            int[] stuff = new int[kCapacity - 1];
            for (int i = 0; i < stuff.Length; ++i)
            {
                stuff[i] = i;
            }
            cbLossless.Add(stuff);
            Assert.AreEqual(cbLossless.Retrieve(), -1, "First item added couldn't be retrieved");
            
            cbLossless.Add(stuff.Length);

            for (int i = 0; i < stuff.Length + 1; ++i)
            {
                Assert.AreEqual(cbLossless.Retrieve(), i, "Item " + i + " could not be retrieved");
            }
        }

        [TestMethod]
        public void TestIteratorLossless()
        {
            cbLossless.Clear();

            for (int i = 0; i < kCapacity; ++i)
            {
                cbLossless.Add(i);
            }

            foreach (var item in cbLossless)
            {
                Console.WriteLine("item " + item);
            }

            foreach (var item in cbLossless)
            {
                Assert.Fail("There should be no items left in buffer.");
            }

            for (int i = 0; i < kCapacity; ++i)
            {
                cbLossless.Add(i);
            }

            try
            {
                foreach (var item in cbLossless)
                {
                    Console.WriteLine("item " + item + " " + cbLossless.Retrieve());
                }
            }
            catch (InvalidOperationException)
            {
                // The above foreach should have read out item 1 and item 2, then
                // throw an exception because cbLossless.Retrieve() modified the collection.
                // what's left in cbLossless should be 3 through 10.
                int iteration = 2;
                foreach (var item in cbLossless)
                {
                    Assert.AreEqual(item, iteration++, "Collection contents does not match expectation.");
                }

                return;
            }

            Assert.Fail("Should have received InvalidOperationException because collection was modified during iterating.");
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void Lossless_TestBiggerThanCapacity()
        {
            cbLossless.Clear();

            // add amount bigger than kCapacity.
            int numPoints = kCapacity + 1;
            int[] stuff = new int[numPoints];
            for (int i = 0; i < numPoints; ++i)
            {
                stuff[i] = i;
            }

            cbLossless.Add(stuff);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void Lossless_TestAddSomeReadSomeOverflow()
        {
            cbLossless.Clear();

            // add some, read some, overflow it, read everything
            int numPoints = kCapacity + 1;
            int[] stuff = new int[numPoints];
            for (int i = 0; i < numPoints; ++i)
            {
                stuff[i] = i;
            }

            cbLossless.Add(stuff, kCapacity - 2); // add 0 through 7
            var task = cbLossless.RetrieveMultipleAsync(4);
            int j = 0;
            foreach (var item in task.Result)
            {
                Assert.AreEqual(item, stuff[j++]);
            }

            for (int i = 0; i < numPoints; ++i)
            {
                stuff[i] = i + 8;
            }

            cbLossless.Add(stuff, 9); // unread = 4, adding 6 or more items would overflow.
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void TestOneByOneOverflowLossless()
        {
            // add one by one, retrieve one by one.
            cbLossless.Clear();
            for (int i = 0; i < kCapacity; ++i)
            {
                cbLossless.Add(i);
            }
            for (int i = 0; i < kCapacity; ++i)
            {
                Assert.AreEqual(cbLossless.Retrieve(), i);
            }
            for (int i = 0; i < kCapacity; ++i)
            {
                cbLossless.Add(i);
            }
            
            cbLossless.Add(0);
        }

        [TestMethod]
        public void Discard_TestBiggerThanCapacity()
        {
            cbDiscard.Clear();

            // add amount bigger than kCapacity.
            const int numPoints = kCapacity + 1;
            int[] stuff = new int[numPoints];
            for (int i = 0; i < numPoints; ++i)
            {
                stuff[i] = i;
            }
            cbDiscard.Add(stuff);

            var task = cbDiscard.RetrieveMultipleAsync();
            int j = numPoints - kCapacity;
            foreach (var item in task.Result)
            {
                Assert.AreEqual(item, stuff[j++]);
            }
        }

        [TestMethod]
        public void Discard_TestAddSomeReadSomeOverflow()
        {
            cbDiscard.Clear();

            // add some, read some, overflow it, read everything
            int numPoints = kCapacity + 1;
            int[] stuff = new int[numPoints];
            for (int i = 0; i < numPoints; ++i)
            {
                stuff[i] = i;
            }

            cbDiscard.Add(stuff, kCapacity - 2); // add 0 through 7
            var task = cbDiscard.RetrieveMultipleAsync(4);
            int j = 0;
            foreach (var item in task.Result)
            {
                Assert.AreEqual(item, stuff[j++]);
            }
            for (int i = 0; i < numPoints; ++i)
            {
                stuff[i] = i + 8;
            }
            cbDiscard.Add(stuff, 9); // add 8 through 16, writes over 4, 5, 6 which was unread.  7 still there.
            task = cbDiscard.RetrieveMultipleAsync(); // read everything out, should be 7 through 16.
            j = 7;
            foreach (var item in task.Result)
            {
                Assert.AreEqual(item, j++);
            }
        }
    } 
}
