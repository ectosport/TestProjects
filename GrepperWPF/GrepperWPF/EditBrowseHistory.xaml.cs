using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace SimpleSearch
{
   /// <summary>
   /// Interaction logic for EditBrowseHistory.xaml
   /// </summary>
   public partial class EditBrowseHistory : Window
   {
      internal EditBrowseHistory(DirectoryHistoryViewModel dhvm)
      {
         InitializeComponent();
         
         this.DataContext = dhvm;
         dhvm.RequestClose += this.RequestClose;       
      }

      private void RequestClose(object sender, EventArgs e)
      {
         WindowCloseEventArgs wcea = (WindowCloseEventArgs)e;
         this.DialogResult = !wcea.IsCancelled;
         this.Close();
      }

      protected override void OnClosed(EventArgs e)
      {
         base.OnClosed(e);
         DirectoryHistoryViewModel dhvm = (DirectoryHistoryViewModel)this.DataContext;
         dhvm.RequestClose -= this.RequestClose;
      }
   }
}
