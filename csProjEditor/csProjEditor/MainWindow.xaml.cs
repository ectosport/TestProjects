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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace csProjEditor
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window, INotifyPropertyChanged
   {
      public ObservableCollection<string> ProjectReferences { get; set; }
      public List<string> SelectedReferences { get; set; }
      public string CsProjLocation 
      {
         get { return csProjLocation; }
         set
         {
            csProjLocation = value; 
            NotifyPropertyChanged("CsProjLocation");
         }
      }

      private Dictionary<string, XElement> projectReferenceLookup = new Dictionary<string, XElement>();
      private XDocument csProjDocument;
      private string csProjLocation;

      public MainWindow()
      {
         InitializeComponent();
         DataContext = this;
         ProjectReferences = new ObservableCollection<string>();
      }

      private void readXMLButtonClicked(object sender, RoutedEventArgs e)
      {
         csProjDocument = XDocument.Load(this.CsProjLocation);
         ProjectReferences.Clear();
         projectReferenceLookup.Clear();

         var projRefElements = (from d in csProjDocument.Descendants()
            where d.Name.LocalName == "ProjectReference" || d.Name.LocalName == "Reference"
            select d);

         foreach (var projRef in projRefElements)
         {
            var includeAttr = projRef.Attributes("Include").First();
            if (includeAttr != null)
            {
               ProjectReferences.Add(includeAttr.Value);
               projectReferenceLookup.Add(includeAttr.Value, projRef);
            }
         }
      }

      private void deleteSelectedClicked(object sender, RoutedEventArgs e)
      {
         var itemsToDelete = ProjectReferencesListBox.SelectedItems;
         if (itemsToDelete.Count > 0)
         {
            foreach (string refName in itemsToDelete)
            {
               projectReferenceLookup[refName].Remove();
            }

            csProjDocument.Save(this.CsProjLocation);

            readXMLButtonClicked(null, null);
         }
      }

      private void browseDirClicked(object sender, RoutedEventArgs e)
      {
         string strPath;
         if (string.IsNullOrEmpty(csProjLocation))
         {
            strPath = Environment.CurrentDirectory;
         }
         else
         {
            strPath = Path.GetDirectoryName(csProjLocation);
         }

         OpenFileDialog dialog = new OpenFileDialog();
         dialog.Title = "Select csproj file...";
         dialog.CheckFileExists = true;
         dialog.Filter = "*.csproj|*.csproj";
         dialog.InitialDirectory = strPath;

         bool? b = dialog.ShowDialog();
         if (b == true)
         {
            this.CsProjLocation = dialog.FileName;
            readXMLButtonClicked(null, null);
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;

      private void NotifyPropertyChanged(string property)
      {
         if (PropertyChanged != null)
         {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
         }
      }
   }
}
