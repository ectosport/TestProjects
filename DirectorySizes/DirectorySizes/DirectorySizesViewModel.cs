using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.IO;
using System.Windows.Input;
using System.Windows.Data;
using System.Threading;
using System.Windows;

namespace DirectorySizes
{
    public class DirectorySizesViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<dirData> DirectoryCollection { get; private set; }

        private string _processingStatus;
        public string ProcessingStatus
        {
            get { return _processingStatus; }
            set { _processingStatus = value; OnPropertyChanged("ProcessingStatus"); }
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                OnPropertyChanged("IsRunning");
                if (_isRunning)
                    Mouse.OverrideCursor = Cursors.Wait;
                else
                {
                    Mouse.OverrideCursor = null;
                    ProcessingStatus = "";
                }
            }
        }

        private string _topLevelDir;
        public string TopLevelDir
        {
            get { return _topLevelDir; }
            set { _topLevelDir = value; OnPropertyChanged("TopLevelDir"); }
        }
        
        private BackgroundWorker worker = new BackgroundWorker();
        
        private delegate void UpdateUIDelegate(DirProgressData dpd);
        private void UpdateDirBeingProcessed(DirProgressData dpd)
        {
            ProcessingStatus = "Processing: " + dpd.Name;
        }
        private void UpdateDirAndSize(DirProgressData dpd)
        {
            DirectoryCollection.Add(new dirData(dpd.Name, dpd.Size, dpd.IsDir));                
        }
        
        public DirectorySizesViewModel()
        {
            DirectoryCollection = new ObservableCollection<dirData>();
            
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            
            BrowseCommand = new BrowseOperationCommand(this);
            RefreshCommand = new RefreshOperationCommand(this);
            CancelCommand = new CancelOperationCommand(this);
            TopLevelDir = "c:\\";            
        }

        // Need to use progress changed event because background worker thread cannot do updates
        // to properties bound to UI elements.
        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DirProgressData dpd = e.UserState as DirProgressData;
            if (dpd != null)
            {
                dpd.HandleUpdate(dpd);
            }
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsRunning = false;
            System.Diagnostics.Trace.WriteLine("Worker Completed");
            CommandManager.InvalidateRequerySuggested();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            DirectoryInfo dirInfo = e.Argument as DirectoryInfo;

            foreach (DirectoryInfo subDir in dirInfo.GetDirectories())
            {
                long dirSize = getDirSize(subDir);                

                double mbSize = dirSize / (1024.0 * 1024.0);
                worker.ReportProgress(0, new DirProgressData()
                {
                    HandleUpdate = UpdateDirAndSize,
                    IsDir = true,
                    Name = subDir.Name,
                    Size = mbSize
                });                

                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    System.Diagnostics.Trace.WriteLine("Cancelling DoWork");
                    return;
                }
            }            

            // Add files at the end, when no more directories exist
            foreach (FileInfo fileInfo in dirInfo.GetFiles())
            {
                worker.ReportProgress(0, new DirProgressData()
                {
                    HandleUpdate = UpdateDirAndSize,
                    IsDir = false,
                    Name = fileInfo.Name,
                    Size = fileInfo.Length / (1024.0 * 1024.0)
                });
            }
        }

        private long getDirSize(DirectoryInfo topDirectory)
        {
            if (worker.CancellationPending)
            {
                throw new CancellationException();
            }
            worker.ReportProgress(0, new DirProgressData() { 
                HandleUpdate = UpdateDirBeingProcessed, Name = topDirectory.FullName });

            long dirSize = 0;
            try
            {
                topDirectory.GetFiles().ToList().ForEach(info => dirSize += info.Length);
            }
            catch (Exception e)
            {
                if (e is UnauthorizedAccessException || e is System.IO.PathTooLongException ||
                    e is FileNotFoundException)
                {
                    System.Diagnostics.Trace.WriteLine("*** Exception: " + e.ToString());
                }
                else
                {
                    throw;
                }
            }
            
            try
            {
                topDirectory.GetDirectories().ToList().ForEach(childDir => dirSize += getDirSize(childDir));
            }
            catch (UnauthorizedAccessException e)
            {
                System.Diagnostics.Trace.WriteLine("*** Exception: " + e.ToString());
            }
            catch (CancellationException)
            {
                return dirSize;
            }

            return dirSize;
        }

        private void _browse()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Title = "Please choose a folder.";
            dlg.CheckFileExists = false;
            dlg.FileName = "Pick a folder";
            dlg.Filter = "HackyFolderSelector|dumb.ass";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == false)
            {
                return;
            }

            string filename = dlg.FileName;
            string directory = filename.Substring(0, filename.LastIndexOf(@"\"));

            TopLevelDir = directory;
            _refresh();
        }

        private void _refresh()
        {
            if (IsRunning && worker.IsBusy)
            {
                if (!worker.CancellationPending)
                {
                    System.Diagnostics.Trace.WriteLine("Trying to cancel");
                    worker.CancelAsync();
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine("Cancel pending...");
                    Thread.Sleep(100);
                }

                // queue this same method back up while we wait for thread to finish up.
                Dispatcher.CurrentDispatcher.Invoke(new Action(() => _refresh()));                
                return;
            }

            if (!TopLevelDir.EndsWith("\\")) TopLevelDir += "\\";
            DirectoryInfo dirInfo = new DirectoryInfo(TopLevelDir);
            if (!dirInfo.Exists)
            {
                MessageBox.Show("Invalid path");
                return;
            }

            IsRunning = true;
            DirectoryCollection.Clear();            
            
            worker.RunWorkerAsync(dirInfo);
            if (dirInfo.Parent != null)
            {
                worker.ReportProgress(0, new DirProgressData()
                {
                    HandleUpdate = UpdateDirAndSize,
                    IsDir = true,
                    Name = "..",
                    Size = 0
                });
            }
        }

        private void _cancel()
        {
            worker.CancelAsync();
        }
        
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region abstractCommand
        private abstract class abstractCommand : ICommand
        {
            protected readonly DirectorySizesViewModel _view;

            public abstractCommand(DirectorySizesViewModel view)
            {
                _view = view;
            }

            public virtual bool CanExecute(object parameter)
            {
                return !_view.IsRunning;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public abstract void Execute(object parameter);
        }
        #endregion
        
        #region BrowseCommand

        public ICommand BrowseCommand { get; private set; }

        private class BrowseOperationCommand : abstractCommand
        {
            public BrowseOperationCommand(DirectorySizesViewModel view) : base(view) { }

            public override void Execute(object parameter)
            {
                _view._browse();
            }
        }

        #endregion

        #region RefreshCommand

        public ICommand RefreshCommand { get; private set; }

        private class RefreshOperationCommand : abstractCommand
        {
            public RefreshOperationCommand(DirectorySizesViewModel view) : base(view) { }

            public override void Execute(object parameter)
            {
                _view._refresh();
            }
        }

        #endregion

        #region CancelCommand

        public ICommand CancelCommand { get; private set; }

        private class CancelOperationCommand : abstractCommand
        {
            public CancelOperationCommand(DirectorySizesViewModel view) : base(view) {}

            public override bool CanExecute(object parameter)
            {                
                return _view.IsRunning;
            }

            public override void Execute(object parameter)
            {
                _view._cancel();
            }
        }

        #endregion

        private class CancellationException : Exception
        {

        }

        private class DirProgressData
        {
            public double Size { get; set; }
            public string Name { get; set; }
            public bool IsDir { get; set; }
            public UpdateUIDelegate HandleUpdate { get; set; }
        }
    }

    #region Converters
    public abstract class tOneWayConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public abstract object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture);
    }

    public class tInvertBoolConverter : tOneWayConverter
    {
        public override object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }
    }
    #endregion

}
