using SimpleSearch.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WindowPlacement;

namespace SimpleSearch
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      readonly GrepperViewModel _vm;
      private DispatcherTimer singleClickTimer;

      public MainWindow()
      {
         InitializeComponent();

         _vm = new GrepperViewModel();
         DataContext = _vm;

         singleClickTimer = new DispatcherTimer();
         singleClickTimer.Interval = TimeSpan.FromSeconds(1);
         singleClickTimer.Tick += actualSingleClickHandler;

      }

      private void _doubleClickOnFile(object sender, MouseButtonEventArgs e)
      {
         singleClickTimer.Stop();
         e.Handled = true;

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

      private void _fileListControl_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         singleClickTimer.Tag = e;
         singleClickTimer.Start();
      }

      /// <summary>
      /// Determine the index of a DataGridRow
      /// </summary>
      /// <param name="row"></param>
      /// <returns></returns>
      private int FindRowIndex(DataGridRow row)
      {
         DataGrid dataGrid = ItemsControl.ItemsControlFromItemContainer(row) as DataGrid;

         int index = dataGrid.ItemContainerGenerator.IndexFromContainer(row);

         return index;
      }

      /// <summary>
      /// Find the value that is bound to a DataGridCell
      /// </summary>
      /// <param name="row"></param>
      /// <param name="cell"></param>
      /// <returns></returns>
      private object ExtractBoundValue(DataGridRow row, DataGridCell cell)
      {
         // find the property that this cell's column is bound to
         string boundPropertyName = FindBoundProperty(cell.Column);

         // find the object that is realted to this row
         object data = row.Item;

         // extract the property value
         PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(data);
         PropertyDescriptor property = properties[boundPropertyName];
         object value = property.GetValue(data);

         return value;
      }

      /// <summary>
      /// Find the name of the property which is bound to the given column
      /// </summary>
      /// <param name="col"></param>
      /// <returns></returns>
      private string FindBoundProperty(DataGridColumn col)
      {
         DataGridBoundColumn boundColumn = col as DataGridBoundColumn;

         // find the property that this column is bound to
         Binding binding = boundColumn.Binding as Binding;
         string boundPropertyName = binding.Path.Path;

         return boundPropertyName;
      }

      private void actualSingleClickHandler(object sender, EventArgs e)
      {
         singleClickTimer.Stop();
         MouseButtonEventArgs mbArgs = (MouseButtonEventArgs)singleClickTimer.Tag;
         DependencyObject dep = (DependencyObject)mbArgs.OriginalSource;

         // This code traverses the visual tree to find the DataGridCell or ColumnHeader
         while ((dep != null) && !(dep is DataGridCell) && !(dep is DataGridColumnHeader))
         {
            dep = VisualTreeHelper.GetParent(dep);
         }

         if (dep == null)
            return;

         if (dep is DataGridColumnHeader)
         {
            DataGridColumnHeader columnHeader = dep as DataGridColumnHeader;

            // find the property that this cell's column is bound to
            string boundPropertyName = FindBoundProperty(columnHeader.Column);

            int columnIndex = columnHeader.Column.DisplayIndex;

            //MessageBox.Show(string.Format(
            //    "Header clicked [{0}] = {1}",
            //    columnIndex, boundPropertyName));
         }

         if (dep is DataGridCell)
         {
            DataGridCell cell = dep as DataGridCell;

            // navigate further up the tree
            while ((dep != null) && !(dep is DataGridRow))
            {
               dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
               return;

            DataGridRow row = dep as DataGridRow;

            //object value = ExtractBoundValue(row, cell);
            //int columnIndex = cell.Column.DisplayIndex;
            //int rowIndex = FindRowIndex(row);

            var searchResultWindow = new SearchResultWindow(new SearchResultViewModel((SearchResult)row.Item, _vm.SearchString, _vm.CaseSensitiveSearch));
            searchResultWindow.Show();
         }
      }
   }
}
