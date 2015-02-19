using System.Linq;
using System.Threading;
using CircularBuffer;

namespace CircularBufferUnitTests
{
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
}