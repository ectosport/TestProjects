using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GrepperWPF
{
   internal class DirectoryHistoryViewModel
   {
      private readonly List<string> directoryHistory;

      private readonly ICommand removeSelectedDirectoriesCommand;
      public ICommand RemoveSelectedDirectoriesCommand
      {
         get { return removeSelectedDirectoriesCommand; }
      }

      private readonly ICommand selectDirectoryCommand;
      public ICommand SelectDirectoryCommand
      {
         get { return selectDirectoryCommand; }
      }

      private readonly ICommand closeWindowCommand;
      public ICommand CloseWindowCommand
      {
         get { return closeWindowCommand; }
      }

      public ObservableCollection<SelectableDirectoryItem> SelectableDirectoryItems
      {
         get; set;
      }

      public SelectableDirectoryItem SelectedDirectory { get; set; }

      public event EventHandler RequestClose;      

      public DirectoryHistoryViewModel(List<string> directoryHistory)
      {
         this.removeSelectedDirectoriesCommand = new CommandHandler(RemoveSelectedDirectories, () => true);
         this.selectDirectoryCommand = new CommandHandler(SelectDirectory, () => true);
         this.closeWindowCommand = new CommandHandler((o) => this.RequestClose?.Invoke(this, new WindowCloseEventArgs(isCancelled: true)), () => true);
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
