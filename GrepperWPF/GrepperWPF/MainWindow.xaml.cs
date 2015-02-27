using GrepperWPF.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowPlacement;

namespace GrepperWPF
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      readonly GrepperViewModel _vm;

      public MainWindow()
      {
         InitializeComponent();

         _vm = new GrepperViewModel();
         DataContext = _vm;
      }

      private void _doubleClickOnFile(object sender, MouseButtonEventArgs e)
      {
         var firstCell = _fileListControl.SelectedCells.First();
         if (firstCell != null)
         {
            SearchResult sr = firstCell.Item as SearchResult;
            if (sr != null)
            {
               if (firstCell.Column.DisplayIndex == 0)
               {
                  System.Diagnostics.Process.Start(sr.Path + "\\" + sr.Filename);
               }
               else
               {
                  System.Diagnostics.Process.Start(sr.Path);
               }

            }
         }
      }

      private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
      {
          _vm.SaveSettings();
      }

      protected override void OnSourceInitialized(EventArgs e)
      {
         base.OnSourceInitialized(e);
         this.SetPlacement(Settings.Default.MainWindowPlacement);
      }
   }
}
