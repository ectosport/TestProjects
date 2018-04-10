using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimpleSearch
{
   internal class DirectoryHistoryViewModel
   {
      private readonly List<string> directoryHistory;

      public ICommand RemoveSelectedDirectoriesCommand { get; }

      public ICommand SelectDirectoryCommand { get; }

      public ICommand CloseWindowCommand { get; }

      public ObservableCollection<SelectableDirectoryItem> SelectableDirectoryItems { get; }

      public SelectableDirectoryItem SelectedDirectory { get; set; }

      public event EventHandler RequestClose;

      public DirectoryHistoryViewModel(List<string> directoryHistory)
      {
         this.RemoveSelectedDirectoriesCommand = new CommandHandler(RemoveSelectedDirectories, () => true);
         this.SelectDirectoryCommand = new CommandHandler(SelectDirectory, () => true);
         this.CloseWindowCommand = new CommandHandler((o) => this.RequestClose?.Invoke(this, new WindowCloseEventArgs(isCancelled: true)), () => true);
         this.directoryHistory = directoryHistory;

         var selectableDirectoryItems = new ObservableCollection<SelectableDirectoryItem>();
         directoryHistory.ForEach(directory =>
         {
            selectableDirectoryItems.Add(new SelectableDirectoryItem() { DirectoryPath = directory, Selected = false });
         });
         this.SelectableDirectoryItems = selectableDirectoryItems;
         
      }

      private void RemoveSelectedDirectories(object parameter)
      {
         var selectedDirectoryItems = this.SelectableDirectoryItems.Where(directory => directory.Selected).ToList();

         foreach (var item in selectedDirectoryItems)
         {
            this.directoryHistory.Remove(item.DirectoryPath);
            this.SelectableDirectoryItems.Remove(item);
         }
      }

      private void SelectDirectory(object parameter)
      {
         var isCancelled = (this.SelectedDirectory == null);
         this.RequestClose?.Invoke(this, new WindowCloseEventArgs(isCancelled));
      }
   }

   internal class SelectableDirectoryItem
   {
      public string DirectoryPath { get; set; }
      public bool Selected { get; set; }
   }
}
