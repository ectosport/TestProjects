using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;

namespace DirectorySizes
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private readonly DirectorySizesViewModel _viewModel;

        public Window1()
        {
            InitializeComponent();
            _viewModel = new DirectorySizesViewModel();
            DataContext = _viewModel;
            
#if false
            GridView view = new GridView();
            GridViewColumn c1 = new GridViewColumn();
            c1.Header = "Subdirectory";
            c1.Width = 350;
            view.Columns.Add(c1);

            GridViewColumn c2 = new GridViewColumn();
            c2.Header = "Size";
            c2.Width = 150;
            view.Columns.Add(c2);

            _dirListView.View = view;

            ListViewItem item = new ListViewItem();
            item.
            _dirListView.Items.Add(item); 
#endif
        }

        //private void subDirDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    var item = _dirListView.SelectedItem as dirData;

        //    if (item != null && item.isDir == true)
        //    {
        //        DirectoryInfo dirInfo = new DirectoryInfo(_viewModel.TopLevelDir);
                
        //        if (item.dirName == "..")
        //        {
        //            _viewModel.TopLevelDir = dirInfo.Parent.FullName;
        //        }
        //        else
        //        {
        //            _viewModel.TopLevelDir = dirInfo.FullName + (dirInfo.FullName.EndsWith("\\") ? "" : "\\") + item.dirName;
        //        }

        //        _viewModel.RefreshCommand.Execute(null);
        //    }
        //}

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var datagrid = sender as DataGrid;
            var item = datagrid.SelectedItem as dirData;

            if (item != null && item.isDir == true)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(_viewModel.TopLevelDir);

                if (item.dirName == "..")
                {
                    _viewModel.TopLevelDir = dirInfo.Parent.FullName;
                }
                else
                {
                    _viewModel.TopLevelDir = dirInfo.FullName + (dirInfo.FullName.EndsWith("\\") ? "" : "\\") + item.dirName;
                }

                _viewModel.RefreshCommand.Execute(null);
            }
        }
    }   
}
