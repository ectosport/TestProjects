using System;
using System.Windows.Input;

namespace SimpleSearch
{
   internal class CommandHandler : ICommand
   {
      private Action<object> _execute;
      private Func<bool> _canExecute;

      public CommandHandler(Action<object> execute, Func<bool> canExecute)
      {
         _execute = execute;
         _canExecute = canExecute;
      }

      public event EventHandler CanExecuteChanged
      {
         add { CommandManager.RequerySuggested += value; }
         remove { CommandManager.RequerySuggested -= value; }
      }

      public void Execute(object parameter) => _execute(parameter);

      public bool CanExecute(object parameter) => _canExecute();
   }
}