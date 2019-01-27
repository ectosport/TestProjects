using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using SimpleSearch.Properties;
using Microsoft.Win32;

namespace SimpleSearch
{
    internal enum BrowseDirection
    {
        Back, Forward
    }

    internal class GrepperViewModel : INotifyPropertyChanged
    {
        #region Fields

        private SearchSingleDirTask _task;
        private bool _cancelRequested;
        private Stopwatch st = new Stopwatch();
        private List<string> browseHistory = new List<string>();
        private string _fileExtensions;
        private List<SearchResult> _searchResults;
        private bool _isSearching = false;
        private string _searchString;
        private string _statusText;

        #endregion

        #region Properties

        public bool SearchFilenamesOnly { get; set; }

        public bool CaseSensitiveSearch { get; set; }

        private string _rootDirectory;
        public string RootDirectory
        {
            get
            {
                return _rootDirectory;
            }
            set
            {
                _rootDirectory = value;

                NotifyPropertyChanged("RootDirectory");
                NotifyPropertyChanged("SearchButtonEnabled");
            }
        }

        public string FileExtensions
        {
            get
            {
                return _fileExtensions;
            }
            set
            {
                _fileExtensions = value;
                NotifyPropertyChanged("FileExtensions");
            }
        }

        public List<SearchResult> SearchResults
        {
            get
            {
                return _searchResults;
            }
            set
            {
                _searchResults = value;
                NotifyPropertyChanged("SearchResults");
            }
        }

        private bool IsSearching
        {
            get
            {
                return _isSearching;
            }
            set
            {
                _isSearching = value;
                NotifyPropertyChanged("SearchButtonText");
            }
        }
        public string SearchButtonText => IsSearching ? "Cancel" : "Search";

        public string SearchString
        {
            get
            {
                return _searchString;
            }
            set
            {
                _searchString = value;
                NotifyPropertyChanged("SearchString");
            }
        }

        public string StatusText
        {
            get
            {
                return _statusText;
            }
            set
            {
                _statusText = value;
                NotifyPropertyChanged("StatusText");
            }
        }

        private bool SearchButtonEnabled => (!_cancelRequested && Directory.Exists(_rootDirectory) && !String.IsNullOrEmpty(_searchString));

        public ICommand SearchCommand { get; }

        public ICommand BrowseCommand { get; }

        public ICommand BrowseHistoryCommand { get; }

        public ICommand EditDirectoryHistoryCommand { get; }

        public string BrowseHistoryTooltip => string.Join(Environment.NewLine, browseHistory.ToArray());

        #endregion

        public GrepperViewModel()
        {
            if (String.IsNullOrEmpty(Settings.Default.BrowseHistory) == false)
            {
                browseHistory = Settings.Default.BrowseHistory.Split('?').ToList();
                NotifyPropertyChanged("BrowseHistoryTooltip");
            }
            FileExtensions = Settings.Default.Extensions;
            RootDirectory = Settings.Default.Path;
            SearchFilenamesOnly = Settings.Default.SearchFilenamesOnly;
            CaseSensitiveSearch = Settings.Default.CaseSensitiveSearch;
            SearchString = Settings.Default.SearchString;

            this.SearchCommand = new CommandHandler((o) => this.Search(), () => this.SearchButtonEnabled);
            this.BrowseCommand = new CommandHandler(browseForDirectory, () => true);
            this.BrowseHistoryCommand = new CommandHandler(this.navigateHistory, () => true);
            this.EditDirectoryHistoryCommand = new CommandHandler((o) => this.EditDirectoryHistory(o), () => true);
        }

        private void browseForDirectory(object parameter)
        {
            string strPath = Environment.CurrentDirectory;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Navigate to folder to search...";
            dialog.CheckFileExists = false;
            dialog.FileName = "Choose Current Folder";
            dialog.Filter = "Folder|Folder";
            dialog.InitialDirectory = this.RootDirectory;

            bool? b = dialog.ShowDialog();
            if (b == true)
            {
                this.RootDirectory = dialog.FileName.Substring(0, dialog.FileName.LastIndexOf(@"\")); ;
            }
        }

        private void EditDirectoryHistory(object parameter)
        {
            var dhvm = new DirectoryHistoryViewModel(browseHistory);
            var editDialog = new EditBrowseHistory(dhvm);
            bool? ok = editDialog.ShowDialog();
            if (ok == true)
            {
                string dir = dhvm.SelectedDirectory.DirectoryPath;
                if (string.IsNullOrEmpty(dir) == false)
                {
                    RootDirectory = dir;
                }
            }
        }

        private void navigateHistory(object parameter)
        {
            BrowseDirection direction = (BrowseDirection)parameter;

            int currentIndex = browseHistory.IndexOf(RootDirectory);

            if (direction == BrowseDirection.Forward && (currentIndex + 1) < browseHistory.Count)
            {
                RootDirectory = browseHistory[currentIndex + 1];
            }
            else if (direction == BrowseDirection.Back && (currentIndex - 1) >= 0)
            {
                RootDirectory = browseHistory[currentIndex - 1];
            }
        }

        public void SaveSettings()
        {
            // Settings are stored in a place similar to this:
            // C:\Users\<USERNAME>\AppData\Local\Microsoft\SimpleSearch.exe_Url_tbivpf0tacg32fgv0kknmcjlzzhn4nc2
            Settings.Default.Extensions = FileExtensions;
            Settings.Default.Path = RootDirectory;
            Settings.Default.SearchFilenamesOnly = SearchFilenamesOnly;
            Settings.Default.CaseSensitiveSearch = CaseSensitiveSearch;
            Settings.Default.SearchString = SearchString;
            Settings.Default.BrowseHistory = string.Join("?", browseHistory.ToArray());
            Settings.Default.Save();
        }

        async public void Search()
        {
            if (IsSearching)
            {
                StatusText = "Cancelling...";
                _task.CancelSearch();
                _cancelRequested = true;
                CommandManager.InvalidateRequerySuggested();
            }
            else
            {
                st.Reset();
                st.Start();
                SaveSettings();

                StatusText = "Searching...";
                IsSearching = true;
                _cancelRequested = false;
                CommandManager.InvalidateRequerySuggested();

                var trimmed = _rootDirectory.Trim('\\');

                if (browseHistory.Contains(trimmed) == false && Directory.Exists(trimmed))
                {
                    browseHistory.Add(trimmed);
                    browseHistory.Sort();
                    NotifyPropertyChanged("BrowseHistoryTooltip");
                }

                _task = new SearchSingleDirTask(RootDirectory, FileExtensions, SearchString, SearchFilenamesOnly, CaseSensitiveSearch);
                try
                {
                    await _task.PerformSearch();
                }
                catch (Exception ex)
                {
                    if (ex is AggregateException || ex is OperationCanceledException)
                    {

                    }
                    else throw;
                }

                SearchResults = _task.SearchResults;
                st.Stop();

                StatusText = (_cancelRequested ? "Cancelled!" : "Done!") + "  Found " + SearchResults.Count +
                    " files... Checked " + _task.FilesSearched + " files. Time elapsed: " + st.Elapsed.ToString();

                IsSearching = false;
                _cancelRequested = false;
                CommandManager.InvalidateRequerySuggested(); ;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
