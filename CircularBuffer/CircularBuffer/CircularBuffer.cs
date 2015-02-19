using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CircularBuffer
{
    public class CircularBuffer<T> : ICircularBuffer<T>
    {
        #region Fields

        private T[] buffer;
        private int readPosition = 0;
        private int writePosition = 0;
        private int currentUnreadItems = 0;
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

        #region Properties

        public int Count { get { return currentUnreadItems; } }
        public int Capacity { get { return capacity; } }
        
        #endregion 

        #region Events
        public event EventHandler<DiscardedItemEventArgs<T>> DiscardedItemEvent;
        #endregion

        #region Methods

        public CircularBuffer(int capacity, DataIntegrity dataIntegrityMode = DataIntegrity.Lossless)
        {
            this.capacity = capacity;
            buffer = new T[capacity];
            this.dataIntegrityMode = dataIntegrityMode;
        }

        public void Add(T item)
        {
            lock (buffer)
            {
                if (currentUnreadItems >= this.capacity)
                {
                    if (this.dataIntegrityMode == DataIntegrity.Lossless)
                    {
                        throw new OverflowException();
                    }
                    else
                    {
                        OnRaiseDiscardedItemEvent(buffer[readPosition]);

                        // move read position to skip unread items that will be written over.
                        readPosition = (readPosition == (this.capacity - 1)) ? 0 : readPosition + 1;
                        --currentUnreadItems;
                    }
                }

                buffer[writePosition] = item;
                writePosition = (writePosition == (this.capacity - 1)) ? 0 : writePosition + 1;
                ++version;
                ++currentUnreadItems;               
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
                if ((currentUnreadItems + count) > this.capacity && (this.dataIntegrityMode == DataIntegrity.Lossless))
                {
                    throw new OverflowException();
                }
                // if we're in discard oldest and we want to add more than capacity, then only copy the last part of the buffer
                else if ((count > this.capacity) && (this.dataIntegrityMode == DataIntegrity.DiscardOldest))
                {
                    startingSourceLocation = count - Capacity;
                    count = this.capacity;
                    readPosition = writePosition = 0;
                    currentUnreadItems = 0;
                }                
                // if we're in discard oldest and unread + items to add is over capacity, then move read position over oldest items
                else if ((this.dataIntegrityMode == DataIntegrity.DiscardOldest) && ((currentUnreadItems + count) > this.capacity))
                {
                    int amountOverboard = currentUnreadItems + count - this.capacity;
                    readPosition += amountOverboard;
                    readPosition = readPosition % this.capacity;
                    currentUnreadItems -= amountOverboard;
                }

                if (count + writePosition <= this.capacity)
                {
                    // single copy works
                    Array.Copy(items, startingSourceLocation, buffer, writePosition, count);
                    writePosition += count;
                    if (writePosition == this.capacity) writePosition = 0;
                }
                else
                {
                    // requires two copies
                    int adjustedCount = this.capacity - writePosition;
                    Array.Copy(items, 0, buffer, writePosition, adjustedCount);                    
                    Array.Copy(items, adjustedCount, buffer, 0, count - adjustedCount);
                    writePosition = count - adjustedCount;
                }

                Debug.Assert(writePosition < this.capacity, "Write position is out of bounds!");
                ++version;
                currentUnreadItems += count;
                itemWaiting.Set();
                this.NotifyUnreadThreshold();
            }
        }
                
        public void Clear()
        {
            lock (buffer)
            {
                ++version;
                this.currentUnreadItems = 0;
                writePosition = 0;
                readPosition = 0;
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

            if (currentUnreadItems > 0)
            {
                if (incrementVersion) ++version;

                --currentUnreadItems;
                if (currentUnreadItems > 0) itemWaiting.Set();
                else if (currentUnreadItems == 0) itemWaiting.Reset();

                valueToReturn = buffer[readPosition];
                readPosition = (readPosition == (this.capacity - 1)) ? 0 : readPosition + 1;
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
                        if (numberToRetrieve <= 0) numberToRetrieve = currentUnreadItems;

                        if (currentUnreadItems >= numberToRetrieve)
                        {
                            T[] items = new T[numberToRetrieve];

                            if (numberToRetrieve + readPosition <= this.capacity)
                            {
                                // single copy works
                                Array.Copy(buffer, readPosition, items, 0, numberToRetrieve);
                                readPosition += numberToRetrieve;
                            }
                            else
                            {
                                // requires two copies
                                int adjustedCount = this.capacity - readPosition;
                                Array.Copy(buffer, readPosition, items, 0, adjustedCount);
                                Array.Copy(buffer, 0, items, adjustedCount, numberToRetrieve - adjustedCount);
                                readPosition = numberToRetrieve - adjustedCount;
                            }

                            // update unread items and set event accordingly
                            ++version;
                            currentUnreadItems -= numberToRetrieve;
                            if (currentUnreadItems > 0) itemWaiting.Set();
                            else if (currentUnreadItems == 0) itemWaiting.Reset();
                            
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
            if (Observer != null && currentUnreadItems >= Observer.ThresholdForUnreadNotification)
            {
                Task.Run(() => Observer.NotifyUnreadThreshold(this, currentUnreadItems));
            }
        }
        
        #endregion

        #region Nested classes
        
        public struct Enumerator : IEnumerator<T>
        {
            /// <summary>
            /// The collection to iterate through.
            /// </summary>
            private CircularBuffer<T> cb;

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
            internal Enumerator(CircularBuffer<T> buffer)
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
                if (this.cb.currentUnreadItems > 0)
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

        
    }
    
}
