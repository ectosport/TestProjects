using System;
using System.Linq;
using System.Text;

namespace CircularBuffer
{
    public interface IObserveCircularBuffer<T>
    {
        int ThresholdForUnreadNotification { get; set;  }
        void NotifyUnreadThreshold(CircularBuffer.ICircularBuffer<T> cb, int numberUnread);
    }

    public class DiscardedItemEventArgs<T> : EventArgs
    {
        private T discardedItem;
        public T DiscardedItem
        {
            get { return discardedItem; }
        }

        public DiscardedItemEventArgs(T item)
        {
            discardedItem = item;
        }
    }
}
