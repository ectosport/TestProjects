using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CircularBuffer
{
    public interface ICircularBuffer<T> : IEnumerable<T>
    {
        #region Methods
        void Add(T item);
        void Add(T[] items, int count = -1);
        void Clear();
        T Retrieve(int millisecondTimeout = -1);
        Task<T> RetrieveAsync(int millisecondTimeout = -1);
        Task<T[]> RetrieveMultipleAsync(int numberToRetrieve = -1, int millisecondTimeout = -1);
        #endregion
        
        #region Properties

        int Capacity { get; }
        int Count { get; }
        IObserveCircularBuffer<T> Observer { get; set; }

        #endregion

        #region Events
        event EventHandler<DiscardedItemEventArgs<T>> DiscardedItemEvent;
        #endregion
    }
}