using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GrepperWPF
{
   internal class SearchResultViewModel : INotifyPropertyChanged
   {
      private readonly SearchResult searchResult;
      private string fileContents;
      private readonly string searchString;
      private int selectionStart = 0, selectionLength = 0;
      private ICommand previousCommand;
      private ICommand nextCommand;
      private List<int> indicesOfFound = new List<int>();
      
      public SearchResultViewModel(SearchResult sr, string searchString)
      {
         searchResult = sr;

         FileContents = File.ReadAllText(this.FullPath);
         
         this.searchString = searchString;
         selectionLength = searchString.Length;
         SearchForAllInstances();

         nextCommand = new CommandHandler((o) => GotoInstance(), () => indicesOfFound.Count > 1);
         previousCommand = new CommandHandler((o) => GotoInstance(next: false), () => indicesOfFound.Count > 1);

         GotoInstance();
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

         SelectionStart = indicesOfFound[currentIndex];
      }

      private void SearchForAllInstances()
      {
         int foundIndex = 0;
         
         while ((foundIndex = FileContents.IndexOf(this.searchString, foundIndex + selectionLength)) > 0)
         {
            this.indicesOfFound.Add(foundIndex);
         }
      }

      public ICommand PreviousCommand
      {
         get { return previousCommand; }
      }

      public ICommand NextCommand
      {
         get { return nextCommand; }
      }

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
