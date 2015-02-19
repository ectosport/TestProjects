using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CircularBuffer;

namespace ConsoleApplication1
{
    class Program 
    {
        const int kCapacity = 10;
        static Task<int[]> task;
        static void Main(string[] args)
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                TestCircularBufferDiscardOldest();
                TestCircularBufferDiscardLossless();
                sw.Stop();
                Console.WriteLine("Performance (ms): " + sw.ElapsedMilliseconds);
                TestPerformance();
                Console.ReadLine();
            }
            catch (TestFailureException tfe)
            {
                Console.WriteLine("Test failed at:\n" + tfe.StackTrace);                
            }
        }

        static bool TestCircularBufferDiscardLossless()
        {
            ICircularBuffer<int> cb = new CircularBuffer2<int>(kCapacity);

            TestBiggerThanCapacityLossless(cb);
            TestAddSomeReadSomeOverflowLossless(cb);
            TestOneByOneOverflowLossless(cb);
            TestThresholdNotification(cb);
            TestRetrieveTimeoutLossless(cb);
            TestIterator(cb, 0);
            TestAddingMultipleAtCapacityLossless(cb);

            return true;
        }

        static bool TestCircularBufferDiscardOldest()
        {
            ICircularBuffer<int> cb = new CircularBuffer<int>(kCapacity, CircularBuffer<int>.DataIntegrity.DiscardOldest);

            TestBiggerThanCapacityDiscard(cb);
            TestAddSomeReadSomeOverflowDiscard(cb);
            TestOneByOneDiscard(cb);
            TestIterator(cb, 1);

            return true;
        }

        static bool TestPerformance()
        {
            Stopwatch sw = new Stopwatch();
            ICircularBuffer<int> cb = new CircularBuffer<int>(kCapacity, CircularBuffer<int>.DataIntegrity.DiscardOldest);
            
            // add 100000 items
            sw.Start();
            for (int i = 0; i < 100000; ++i)
            {
                cb.Add(i);
            }
            sw.Stop();
            Console.WriteLine("Time to add 100000 to circular buffer (ms): " + sw.ElapsedMilliseconds);

            ConcurrentQueue<int> list = new ConcurrentQueue<int>();
            sw.Reset();
            sw.Start();
            for (int i = 0; i < 100000; ++i)
            {
                list.Enqueue(i);
            }
            sw.Stop();
            Console.WriteLine("Time to add 100000 to ConcurrentQueue (ms): " + sw.ElapsedMilliseconds);

            return true;
        }

        static bool TestAddingMultipleAtCapacityLossless(ICircularBuffer<int> cb)
        {
            cb.Clear();
            cb.Add(-1);
            int[] stuff = new int[kCapacity-1];
            for (int i = 0; i < stuff.Length; ++i)
            {
                stuff[i] = i;
            }
            cb.Add(stuff);
            if (cb.Retrieve() != -1) throw new TestFailureException();
            cb.Add(stuff.Length);

            for (int i = 0; i < stuff.Length + 1; ++i)
            {
                if (cb.Retrieve() != i) throw new TestFailureException();
            }

            return true;
        }

        static void TestIterator(ICircularBuffer<int> cb, int amtToOverflow)
        {
            cb.Clear();

            // add amount bigger than kCapacity.
            int numPoints = kCapacity + amtToOverflow;
            
            for (int i = 0; i < numPoints; ++i)
            {
                cb.Add(i);
            }

            foreach (var item in cb)
            {
                Console.WriteLine("item " + item); //+ " " + cb.Retrieve());
            }

            foreach (var item in cb)
            {
                // there should be no items left
                throw new TestFailureException();
            }

            for (int i = 0; i < numPoints; ++i)
            {
                cb.Add(i);
            }

            try
            {
                foreach (var item in cb)
                {
                    Console.WriteLine("item " + item + " " + cb.Retrieve());
                }
            }
            catch (InvalidOperationException)
            {
                // The above foreach should have read out item 1 and item 2, then
                // threw an exception because cb.Retrieve() modified the collection.
                // what's left in cb should be 3 through 10.
                int iteration = 2 + amtToOverflow;
                foreach (var item in cb)
                {
                    if (item != iteration++)
                    {
                        throw new TestFailureException();
                    }
                }

                return;
            }

            // if we reach here, that means we didn't get an InvalidOperationException when
            // we should have.
            throw new TestFailureException();
        }

        static bool TestBiggerThanCapacityLossless(ICircularBuffer<int> cb)
        {
            cb.Clear();

            // add amount bigger than kCapacity.
            int numPoints = kCapacity + 1;
            int[] stuff = new int[numPoints];
            for (int i = 0; i < numPoints; ++i)
            {
                stuff[i] = i;
            }

            try
            {
                cb.Add(stuff);
            }
            catch (OverflowException)
            {
                return true;
            }
            
            throw new TestFailureException();
        }

        static bool TestAddSomeReadSomeOverflowLossless(ICircularBuffer<int> cb)
        {
            cb.Clear();

            // add some, read some, overflow it, read everything
            int numPoints = kCapacity + 1;
            int[] stuff = new int[numPoints];
            for (int i = 0; i < numPoints; ++i)
            {
                stuff[i] = i;
            }

            cb.Add(stuff, kCapacity - 2); // add 0 through 7
            task = cb.RetrieveMultipleAsync(4); // read out 0 through 3.  4 items still unread
            int j = 0;
            foreach (var item in task.Result)
            {
                if (item != stuff[j++]) throw new TestFailureException();
            }
            for (int i = 0; i < numPoints; ++i)
            {
                stuff[i] = i + 8;
            }

            try
            {
                cb.Add(stuff, 9); // unread = 4, adding 6 or more items would overflow.
            }
            catch (OverflowException)
            {
                return true;
            }

            throw new TestFailureException();
        }

        static bool TestOneByOneOverflowLossless(ICircularBuffer<int> cb)
        {
            // add one by one, retrieve one by one.
            cb.Clear();
            for (int i = 0; i < kCapacity; ++i)
            {
                cb.Add(i);
            }
            for (int i = 0; i < kCapacity; ++i)
            {
                if (cb.Retrieve() != i) throw new TestFailureException();
            }
            for (int i = 0; i < kCapacity; ++i)
            {
                cb.Add(i);
            }
            try
            {
                cb.Add(0);
            }
            catch (OverflowException)
            {
                return true;
            }

            throw new TestFailureException();
        }

        static bool TestBiggerThanCapacityDiscard(ICircularBuffer<int> cb)
        {
            cb.Clear();

            // add amount bigger than kCapacity.
            const int numPoints = kCapacity + 1;
            int[] stuff = new int[numPoints];
            for (int i = 0; i < numPoints; ++i)
            {
                stuff[i] = i;
            }
            cb.Add(stuff);
            
            task = cb.RetrieveMultipleAsync();
            int j = numPoints - kCapacity;
            foreach (var item in task.Result)
            {
                if (item != stuff[j++]) throw new TestFailureException();
            }

            return true;
        }

        static bool TestAddSomeReadSomeOverflowDiscard(ICircularBuffer<int> cb)
        {
            cb.Clear();

            // add some, read some, overflow it, read everything
            int numPoints = kCapacity + 1;
            int[] stuff = new int[numPoints];
            for (int i = 0; i < numPoints; ++i)
            {
                stuff[i] = i;
            }
            
            cb.Add(stuff, kCapacity - 2); // add 0 through 7
            task = cb.RetrieveMultipleAsync(4); // read out 0 through 3.  4 items still unread
            int j = 0;
            foreach (var item in task.Result)
            {
                if (item != stuff[j++]) throw new TestFailureException();
            }
            for (int i = 0; i < numPoints; ++i)
            {
                stuff[i] = i + 8;
            }
            cb.Add(stuff, 9); // add 8 through 16, writes over 4, 5, 6 which was unread.  7 still there.
            task = cb.RetrieveMultipleAsync(); // read everything out, should be 7 through 16.
            j = 7;
            foreach (var item in task.Result)
            {
                if (item != j++) throw new TestFailureException();
            }

            return true;
        }

        static bool TestOneByOneDiscard(ICircularBuffer<int> cb)
        {
            // add one by one, retrieve one by one.
            cb.Clear();
            cb.DiscardedItemEvent += cb_DiscardedItemEvent;
            for (int i = 0; i < kCapacity + 20; ++i)
            {
                cb.Add(i);               
            }
            for (int i = 0; i < kCapacity; ++i)
            {
                if (cb.Retrieve() != i + 20) throw new TestFailureException();
            }
            cb.DiscardedItemEvent -= cb_DiscardedItemEvent;

            return true;
        }

        static void cb_DiscardedItemEvent(object sender, DiscardedItemEventArgs<int> e)
        {
            Console.WriteLine("Saw discarded item: " + e.DiscardedItem);
        }

        
        static bool TestThresholdNotification(ICircularBuffer<int> cb)
        {
            MyObserver observer = new MyObserver();
            cb.Clear();
            cb.Observer = observer;
            cb.Observer.ThresholdForUnreadNotification = cb.Capacity - 2;
            var solution = new int[cb.Observer.ThresholdForUnreadNotification];
            observer.CompareData = solution;

            for (int i = 0; i < cb.Observer.ThresholdForUnreadNotification; ++i)
            {
                solution[i] = i;
                cb.Add(i);
            }

            observer.DoneEvent.WaitOne();
            if (!observer.Success) throw new TestFailureException();
            cb.Observer = null;

            return true;
        }

        static void TestRetrieveTimeoutLossless(ICircularBuffer<int> cb)
        {
            cb.Clear();
            cb.Add(1);
            var task = cb.RetrieveAsync();
            if (task.Result != 1) throw new TestFailureException();

            try
            {
                task = cb.RetrieveAsync(100);
                int result = task.Result;
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    if (!(ex is TimeoutException)) throw new TestFailureException();
                }                
            }            
        }
    }

    class MyObserver : IObserveCircularBuffer<int>
    {
        public int[] CompareData { get; set; }
        public ManualResetEvent DoneEvent = new ManualResetEvent(false);
        public bool Success { get; set; }

        int IObserveCircularBuffer<int>.ThresholdForUnreadNotification
        {
            get;
            set;
        }

        void IObserveCircularBuffer<int>.NotifyUnreadThreshold(ICircularBuffer<int> cb, int numberUnread)
        {
            Success = true;
            int i = 0;
            var task1 = cb.RetrieveMultipleAsync();
            foreach (var item in task1.Result)
            {
                if (item != CompareData[i++])
                {
                    Success = false;
                    break;
                }
            }

            if (i != CompareData.Count()) Success = false;

            DoneEvent.Set();
        }
    }

    class TestFailureException : Exception
    {
        public TestFailureException() 
        {
            
        }
    }
}
