using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CircularBuffer
{
    public class CircularBuffer2<T> : ICircularBuffer<T>
    {
        #region Fields
        private Queue<T> buffer;
        private AutoResetEvent itemWaiting = new AutoResetEvent(false);
        private DataIntegrity dataIntegrityMode;
        private long version = 0;
        private IEnumerator<T> currentEnumerator = null;
        private IObserveCircularBuffer<T> observer = null;
        private readonly int capacity;
        #endregion

        #region Enums
        public enum DataIntegrity { Lossless, DiscardOldest };
        #endregion

        #region Events
        public event EventHandler<DiscardedItemEventArgs<T>> DiscardedItemEvent;
        #endregion

        #region Properties
        public int Capacity { get { return capacity; } }
        public int Count { get { return buffer.Count; } }
        public IObserveCircularBuffer<T> Observer
        {
            get
            {
                return observer;
            }
            set
            {
                observer = value;
            }
        }
        #endregion

        #region Methods

        public CircularBuffer2(int capacity, DataIntegrity dataIntegrityMode = DataIntegrity.Lossless)
        {
            this.capacity = capacity;
            buffer = new Queue<T>();
            
            this.dataIntegrityMode = dataIntegrityMode;
        }

        public void Add(T item)
        {
            lock (buffer)
            {
                if (Count >= this.capacity)
                {
                    if (this.dataIntegrityMode == DataIntegrity.Lossless)
                    {
                        throw new OverflowException();
                    }
                    else
                    {
                        OnRaiseDiscardedItemEvent(buffer.Dequeue());
                    }
                }

                buffer.Enqueue(item);
                ++version;
                itemWaiting.Set();
                this.NotifyUnreadThreshold();
            }
        }

        protected virtual void OnRaiseDiscardedItemEvent(T discardedItem)
        {
            EventHandler<DiscardedItemEventArgs<T>> handler = DiscardedItemEvent;
            if (handler != null)
            {
                DiscardedItemEventArgs<T> e = new DiscardedItemEventArgs<T>(discardedItem);
                handler(this, e);
            }
        }

        public void Add(T[] items, int count = -1)
        {
            lock (buffer)
            {
                int startingSourceLocation = 0;

                if (count < 0) count = items.Count();

                // if we're lossless, make sure the unread items plus the stuff we're adding isn't over capacity
                if ((buffer.Count + count) > this.capacity && (this.dataIntegrityMode == DataIntegrity.Lossless))
                {
                    throw new OverflowException();
                }
                // if we're in discard oldest and we want to add more than capacity, then only copy the last part of the buffer
                else if ((count > this.capacity) && (this.dataIntegrityMode == DataIntegrity.DiscardOldest))
                {
                    startingSourceLocation = count - this.capacity;
                    count = this.capacity;                    
                    buffer.Clear();                    
                }
                // if we're in discard oldest and unread + items to add is over capacity, then dequeue the items to be discarded
                else if ((this.dataIntegrityMode == DataIntegrity.DiscardOldest) && ((buffer.Count + count) > this.capacity))
                {
                    int amountOverboard = buffer.Count + count - this.capacity;
                    
                    for (int i = 0; i < amountOverboard; ++i)
                    {
                        buffer.Dequeue();
                    }
                }

                for (int i = 0; i < count; ++i)
                {
                    buffer.Enqueue(items[i + startingSourceLocation]);
                }
                
                ++version;
                itemWaiting.Set();
                this.NotifyUnreadThreshold();
            }
        }

        public void Clear()
        {
            lock (buffer)
            {
                ++version;
                buffer.Clear();
                itemWaiting.Reset();
            }
        }

        public T Retrieve(int millisecondTimeout = -1)
        {
            if (itemWaiting.WaitOne(millisecondTimeout) == false)
            {
                throw new TimeoutException();
            }

            lock (buffer)
            {
                return RetrieveInternal();
            }
        }

        // this version of Retrieve has the option of incrementing version (default) or not.
        // When using the enumerator, it does not mark the collection as modified so that 
        // further iterating doesn't throw exception.
        private T RetrieveInternal(bool incrementVersion = true)
        {
            T valueToReturn = default(T);

            if (Count > 0)
            {
                if (incrementVersion) ++version;

                valueToReturn = buffer.Dequeue();                

                if (Count > 0) itemWaiting.Set();
                else if (Count == 0) itemWaiting.Reset();                
            }

            return valueToReturn;
        }

        public Task<T> RetrieveAsync(int millisecondTimeout = -1)
        {
            return Task.Run(() =>
            {
                return Retrieve(millisecondTimeout);
            });
        }

        public Task<T[]> RetrieveMultipleAsync(int numberToRetrieve = -1, int millisecondTimeout = -1)
        {
            return Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                while (true)
                {
                    lock (buffer)
                    {
                        if (numberToRetrieve <= 0) numberToRetrieve = Count;

                        if (Count >= numberToRetrieve)
                        {
                            T[] items = new T[numberToRetrieve];

                            for (int i = 0; i < numberToRetrieve; ++i)
                            {
                                items[i] = buffer.Dequeue();
                            }                            

                            // update unread items and set event accordingly
                            ++version;
                            
                            if (Count > 0) itemWaiting.Set();
                            else if (Count == 0) itemWaiting.Reset();

                            return items;
                        }
                    }

                    if (millisecondTimeout == -1)
                    {
                        itemWaiting.WaitOne();
                    }
                    else
                    {
                        if (itemWaiting.WaitOne(Math.Max(0, millisecondTimeout - (int)sw.ElapsedMilliseconds)) == false)
                        {
                            throw new TimeoutException();
                        }
                    }
                }
            });
        }

        private void NotifyUnreadThreshold()
        {
            if (Observer != null && Count >= Observer.ThresholdForUnreadNotification)
            {
                Task.Run(() => Observer.NotifyUnreadThreshold(this, Count));
            }
        }

        #endregion

        #region Nested classes

        public struct Enumerator : IEnumerator<T>
        {
            /// <summary>
            /// The collection to iterate through.
            /// </summary>
            private CircularBuffer2<T> cb;

            /// <summary>
            /// The modification value of the collection when the enumerator was created.
            /// </summary>
            private readonly long version;
            /// <summary>
            /// The current value of the enumeration.
            /// </summary>
            private T current;
            /// <summary>
            /// The constructor.
            /// </summary>
            /// <param name="buffer">The collection to iterate through.</param>
            internal Enumerator(CircularBuffer2<T> buffer)
            {
                this.cb = buffer;
                this.current = default(T);
                this.version = buffer.version;
            }
            /// <summary>
            /// Gets the current value of the iteration. Throws an exception if enumerator has whether not started or finished.
            /// </summary>
            public T Current
            {
                get
                {
                    return this.current;
                }
            }
            /// <summary>
            /// The method corresponding to the IDisposable interface.
            /// </summary>
            public void Dispose()
            {
                this.cb.currentEnumerator = null;
            }
            /// <summary>
            /// Advances the enumerator to the next element in the collection. Returns true if the enumerator has succefully advanced; otherwise false.
            /// </summary>
            public bool MoveNext()
            {
                if (this.version != this.cb.version)
                {
                    throw new InvalidOperationException("Enumeration canceled. Collection was modified.");
                }
                if (this.cb.Count > 0)
                {
                    this.current = this.cb.RetrieveInternal(incrementVersion: false);
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }
        }
        #endregion

        #region IEnumerable<T> interface
        // this is actually a consuming enumerator because items will be removed as you
        // iterate over the collection.
        public IEnumerator<T> GetEnumerator()
        {
            if (currentEnumerator != null)
            {
                // Do not allow multiple iterators at a time
                throw new InvalidOperationException();
            }
            currentEnumerator = new Enumerator(this);
            return currentEnumerator;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        
    }
}
