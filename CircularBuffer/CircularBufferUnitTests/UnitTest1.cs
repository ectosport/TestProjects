using System;
using System.Collections.Generic;
using CircularBuffer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CircularBufferUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        const int kCapacity = 10;
        private Queue<int> compareData;
        private readonly List<ICircularBuffer<int>> cbLosslessImplementations = new List<ICircularBuffer<int>>();
        private readonly List<ICircularBuffer<int>> cbDiscardImplementations = new List<ICircularBuffer<int>>();

        public UnitTest1()
        {
            cbLosslessImplementations.Add(new CircularBuffer<int>(kCapacity));
            cbLosslessImplementations.Add(new CircularBuffer2<int>(kCapacity));

            cbDiscardImplementations.Add(new CircularBuffer<int>(kCapacity, CircularBuffer<int>.DataIntegrity.DiscardOldest));
            cbDiscardImplementations.Add(new CircularBuffer2<int>(kCapacity, CircularBuffer2<int>.DataIntegrity.DiscardOldest));
        }

        [TestMethod]
        public void Lossless_TestAddingMultipleAtCapacity()
        {
            foreach (var cbLossless in cbLosslessImplementations)
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
        }

        [TestMethod]
        public void Lossless_TestIterator()
        {
            foreach (var cbLossless in cbLosslessImplementations)
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

                    continue;
                }

                Assert.Fail("Should have received InvalidOperationException because collection was modified during iterating.");
            }
        }

        [TestMethod]
        public void Lossless_TestBiggerThanCapacity()
        {
            foreach (var cbLossless in cbLosslessImplementations)
            {
                cbLossless.Clear();

                // add amount bigger than kCapacity.
                const int numPoints = kCapacity + 1;
                var stuff = new int[numPoints];
                for (var i = 0; i < numPoints; ++i)
                {
                    stuff[i] = i;
                }

                try
                {
                    cbLossless.Add(stuff);
                }
                catch (OverflowException)
                {
                    // Expected
                    continue;
                }
                
                Assert.Fail("Should have received OverflowException.");
            }
        }

        [TestMethod]
        public void Lossless_TestAddSomeReadSomeOverflow()
        {
            foreach (var cbLossless in cbLosslessImplementations)
            {
                cbLossless.Clear();

                // add some, read some, overflow it, read everything
                const int numPoints = kCapacity + 1;
                var stuff = new int[numPoints];
                for (var i = 0; i < numPoints; ++i)
                {
                    stuff[i] = i;
                }

                cbLossless.Add(stuff, kCapacity - 2); // add 0 through 7
                var task = cbLossless.RetrieveMultipleAsync(4);
                var j = 0;
                foreach (var item in task.Result)
                {
                    Assert.AreEqual(item, stuff[j++]);
                }

                for (int i = 0; i < numPoints; ++i)
                {
                    stuff[i] = i + 8;
                }

                try
                {
                    cbLossless.Add(stuff, 9); // unread = 4, adding 6 or more items would overflow.
                }
                catch (OverflowException)
                {
                    // expected
                    continue;
                }

                Assert.Fail("Should have received OverflowException.");
            }
        }

        [TestMethod]
        public void Lossless_TestOneByOneOverflow()
        {
            foreach (var cbLossless in cbLosslessImplementations)
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

                try
                {
                    cbLossless.Add(0);
                }
                catch (OverflowException)
                {
                    // expected
                    continue;
                }

                Assert.Fail("Should have received OverflowException.");
            }
        }

        [TestMethod]
        public void Discard_TestBiggerThanCapacity()
        {
            foreach (var cbDiscard in cbDiscardImplementations)
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
        }

        [TestMethod]
        public void Discard_TestAddSomeReadSomeOverflow()
        {
            foreach (var cbDiscard in cbDiscardImplementations)
            {
                cbDiscard.Clear();

                // add some, read some, overflow it, read everything
                const int numPoints = kCapacity + 1;
                var stuff = new int[numPoints];
                for (var i = 0; i < numPoints; ++i)
                {
                    stuff[i] = i;
                }

                cbDiscard.Add(stuff, kCapacity - 2); // add 0 through 7
                var task = cbDiscard.RetrieveMultipleAsync(4);
                var j = 0;
                foreach (var item in task.Result)
                {
                    Assert.AreEqual(item, stuff[j++]);
                }
                for (var i = 0; i < numPoints; ++i)
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

        [TestMethod]
        public void Discard_TestOneByOneAddRetrieve()
        {
            foreach (var cbDiscard in cbDiscardImplementations)
            {
                // add one by one, retrieve one by one.
                cbDiscard.Clear();
                cbDiscard.DiscardedItemEvent += cb_DiscardedItemEvent;
                compareData = new Queue<int>();

                for (int i = 0; i < kCapacity + 20; ++i)
                {
                    cbDiscard.Add(i);
                    compareData.Enqueue(i);
                }
                for (int i = 0; i < kCapacity; ++i)
                {
                    Assert.AreEqual(cbDiscard.Retrieve(), i + 20);
                }
                cbDiscard.DiscardedItemEvent -= cb_DiscardedItemEvent;
            }
        }

        private void cb_DiscardedItemEvent(object sender, DiscardedItemEventArgs<int> e)
        {
            Console.WriteLine("Saw discarded item: " + e.DiscardedItem);
            Assert.AreEqual(e.DiscardedItem, compareData.Dequeue());
        }

        [TestMethod]
        public void Discard_TestThresholdNotification()
        {
            foreach (var cbDiscard in cbDiscardImplementations)
            {
                MyObserver observer = new MyObserver();
                cbDiscard.Clear();
                cbDiscard.Observer = observer;
                cbDiscard.Observer.ThresholdForUnreadNotification = cbDiscard.Capacity - 2;
                var solution = new int[cbDiscard.Observer.ThresholdForUnreadNotification];
                observer.CompareData = solution;

                for (int i = 0; i < cbDiscard.Observer.ThresholdForUnreadNotification; ++i)
                {
                    solution[i] = i;
                    cbDiscard.Add(i);
                }

                observer.DoneEvent.WaitOne();
                Assert.IsTrue(observer.Success);
                cbDiscard.Observer = null;
            }
        }
    }
}
