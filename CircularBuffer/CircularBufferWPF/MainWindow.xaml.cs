//#define ___useTimer___

using System;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CircularBuffer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IObserveCircularBuffer<int>
    {
        private CircularBuffer<int> cb = new CircularBuffer<int>(10);
        private readonly Random randnum = new Random();
        private Timer readTimer = new Timer();
        private readonly Timer writeTimer = new Timer();
        private int currentNumber = 0;
        private bool isRunning = true;
        
        public MainWindow()
        {
            InitializeComponent();

            this.ThresholdForUnreadNotification = 7;
            cb.Observer = this;

            writeTimer.Elapsed += writeTimer_Elapsed;
            writeTimer.Interval = randnum.Next(100, 1000);
            writeTimer.Start();

#if ___useTimer___
            readTimer.Elapsed += readTimer_Elapsed;
            readTimer.Interval = randnum.Next(100, 1000);
            readTimer.Start();
#endif

#if ___useTask___
            Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            //int thingRead = cb.Retrieve(10);
                            int thingRead = await cb.RetrieveAsync();
                            Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    RetrievedNumbersBox.Items.Add(thingRead);
                                    RetrievedNumbersBox.ScrollIntoView(thingRead);
                                }));
                        }
                        catch (TimeoutException)
                        {
                            Dispatcher.BeginInvoke((Action)(() => RetrievedNumbersBox.Items.Add("Timed out, nothing to read...")));
                        }
                    }
                });
#endif
        }

        void writeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            int thingToAdd = (sender == null) ? randnum.Next() : currentNumber++;

            try
            {
                if (randnum.Next(0, 2) == 1)
                {
                    cb.Add(thingToAdd);

                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        AddedNumbersBox.Items.Add(thingToAdd);
                        AddedNumbersBox.ScrollIntoView(thingToAdd);
                        AvailableToReadTextBlock.Text = "Number in buffer: " + cb.Count.ToString();
                    }));
                }
                else
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        AddLots();
                    }));
                }

                writeTimer.Interval = randnum.Next(10, 1000);
            }
            catch (OverflowException)
            {
                Dispatcher.BeginInvoke((Action)(() => AvailableToReadTextBlock.Text = "OVERFLOWED!!!!  Clearing buffer..."));
                cb.Clear();
            }
        }

#if ___useTimer___
        async void readTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                int thingRead = await cb.RetrieveAsync();

                Dispatcher.BeginInvoke((Action)(() =>
                    {
                        RetrievedNumbersBox.Items.Add(thingRead);
                        RetrievedNumbersBox.ScrollIntoView(thingRead);
                    }));

                readTimer.Interval = randnum.Next(100, 1000);
            }
            catch (TimeoutException)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    string text = "Timed out waiting...";
                    RetrievedNumbersBox.Items.Add(text);                    
                }));
            }
        }
#endif

        async private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int[] items = await cb.RetrieveMultipleAsync(3, 1000);
                foreach (var item in items)
                {
                    RetrievedNumbersBox.Items.Add(item);
                    RetrievedNumbersBox.ScrollIntoView(item);
                }
            }
            catch (TimeoutException)
            {
                RetrievedNumbersBox.Items.Add("Timed out before all items were available");
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (randnum.Next(0, 2) == 1)
                writeTimer_Elapsed(null, null);
            else
                AddLots();

            RenderTargetBitmap rtb = RenderFrameworkElement(this);
            MemoryStream stream = new MemoryStream();
            SaveRenderTargetToStream(rtb, stream);
            SaveStreamToFile(@"e:\vbstyle.png", stream);
        }

        public RenderTargetBitmap RenderFrameworkElement(FrameworkElement elementToRender)
        {
            var windowWidth = (int)elementToRender.ActualWidth;
            var windowHeight = (int)elementToRender.ActualHeight;
            int pixelsPerInch = 96;
            var renderer = new RenderTargetBitmap(windowWidth, windowHeight, pixelsPerInch, pixelsPerInch, PixelFormats.Default);
            renderer.Render(elementToRender);
            return renderer;
        }

        public void SaveRenderTargetToStream(RenderTargetBitmap renderer, MemoryStream stream)
        {
            var encoder = new PngBitmapEncoder();
            var pixelImage = BitmapFrame.Create(renderer);
            encoder.Frames.Add(pixelImage);
            encoder.Save(stream);
            stream.Flush();
        }

        public void SaveStreamToFile(string path, MemoryStream fileContents)
        {
            fileContents.Seek(offset: 0, loc: SeekOrigin.Begin);
            var fileStream = new FileStream(path, FileMode.Create);
            fileContents.WriteTo(fileStream);
            fileStream.Flush();
            fileStream.Close();
        }

        // this is called from UI thread
        private void AddLots()
        {
            int numToAdd = randnum.Next(1, 9);
            AddedNumbersBox.Items.Add("Attempting to add " + numToAdd + " items");

            int[] thingsToAdd = new int[numToAdd];
            for (int i = 0; i < numToAdd; ++i)
            {
                thingsToAdd[i] = randnum.Next();
            }

            try
            {
                cb.Add(thingsToAdd);
                
                for (int j=0;j<numToAdd;++j)
                {
                    AddedNumbersBox.Items.Add(thingsToAdd[j]);
                }
                AddedNumbersBox.ScrollIntoView(thingsToAdd[numToAdd-1]);
                AvailableToReadTextBlock.Text = "Number in buffer: " + cb.Count.ToString();
                
                writeTimer.Interval = randnum.Next(100, 1000);
            }
            catch (OverflowException)
            {
                AddedNumbersBox.Items.Add("OVERFLOWED!!!!  Clearing buffer...");
                cb.Clear();
            }
        }

        public int ThresholdForUnreadNotification
        {
            get;
            set;
        }

        async public void NotifyUnreadThreshold(ICircularBuffer<int> cb, int numberUnread)
        {
            int[] items = await cb.RetrieveMultipleAsync(numberUnread);
            Dispatcher.BeginInvoke((Action)(() =>
                {
                    RetrievedNumbersBox.Items.Add("Retrieving due to threshold: " + numberUnread);
                    foreach (var item in items)
                    {
                        RetrievedNumbersBox.Items.Add(item);
                        RetrievedNumbersBox.ScrollIntoView(item);
                    }
                }));
        }

        private void ToggleTimer_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (isRunning)
            {
                b.Content = "Start";
                writeTimer.Stop();
            }
            else
            {
                b.Content = "Stop";
                writeTimer.Start();
            }

            isRunning = !isRunning;
        }
    }
}
