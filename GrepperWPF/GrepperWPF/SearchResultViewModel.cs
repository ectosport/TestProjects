using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimpleSearch
{
   internal class SearchResultViewModel : INotifyPropertyChanged
   {
      private readonly SearchResult searchResult;
      private string fileContents;
      private string searchString;
      private int selectionStart = 0, selectionLength = 0;
      private bool caseSensitiveSearch;
      private List<int> indicesOfFound = new List<int>();
      
      public SearchResultViewModel(SearchResult sr, string searchString, bool caseSensitiveSearch)
      {
         searchResult = sr;

         FileContents = File.ReadAllText(this.FullPath);
         
         this.SearchText = searchString;
         this.caseSensitiveSearch = caseSensitiveSearch;

         this.NextCommand = new CommandHandler((o) => GotoInstance(), () => indicesOfFound.Count > 1);
         this.PreviousCommand = new CommandHandler((o) => GotoInstance(next: false), () => indicesOfFound.Count > 1);
      }

      private void GotoInstance(bool next = true)
      {
         var currentIndex = indicesOfFound.IndexOf(SelectionStart);
         if (next)
         {
            if (currentIndex >= (indicesOfFound.Count - 1))
            {
               // wrap back to beginning
               currentIndex = -1;
            }

            ++currentIndex;
         }
         else
         {
            if (currentIndex == 0)
            {
               // can't go negative, so wrap to end
               currentIndex = indicesOfFound.Count;
            }

            --currentIndex;
         }

         if (currentIndex < indicesOfFound.Count)
         {
            SelectionStart = indicesOfFound[currentIndex];
         }
         else
         {
            SelectionLength = 0;
         }
      }

      private void SearchForAllInstances()
      {
         int foundIndex = 0;
         this.indicesOfFound.Clear();
         
         while ((foundIndex = FileContents.IndexOf(this.searchString, foundIndex + selectionLength, this.CaseSensitiveSearch ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) > 0)
         {
            this.indicesOfFound.Add(foundIndex);
         }
      }

      public ICommand PreviousCommand { get; }

      public ICommand NextCommand { get; }

      public string FileContents 
      {
         get
         {
            return fileContents;
         }
         set
         {
            fileContents = value;
            NotifyPropertyChanged("FileContents");
         }
      }

      public int SelectionLength
      {
         get { return selectionLength; }
         set
         {
            selectionLength = value;
            NotifyPropertyChanged("SelectionLength");
         }
      }

      public string SearchText
      {
         get { return this.searchString; }
         set
         {
            this.searchString = value;
            this.selectionLength = this.searchString.Length;
            this.SelectionStart = 0; // reset this when search text changes to start searching from beginning
            SearchForAllInstances();
            GotoInstance();
            NotifyPropertyChanged(nameof(SearchText));
            NotifyPropertyChanged("InstanceIndicator");
         }
      }

      public bool CaseSensitiveSearch
      {
         get
         {
            return this.caseSensitiveSearch;
         }

         set
         {
            this.caseSensitiveSearch = value;
            this.SearchText = this.searchString; // just setting it to the same thing to get the property setter to run and all the search code to execute
         }
      }
      
      public int SelectionStart
      {
         get { return selectionStart; }
         set
         {
            // SelectionLength is set twice. This is to workaround a bug where if user changes what
            // is selected (with the mouse), the SelectionLength will not get updated.
            selectionStart = value;
            SelectionLength = 0;
            NotifyPropertyChanged("SelectionStart");
            SelectionLength = searchString.Length;
            NotifyPropertyChanged("InstanceIndicator");
         }
      }

      public string InstanceIndicator
      {
         get { return string.Format("At result: {0} / {1}", indicesOfFound.IndexOf(SelectionStart) + 1, indicesOfFound.Count); }
      }

      public string FullPath
      {
         get { return searchResult.Path + "\\" + searchResult.Filename; }
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
