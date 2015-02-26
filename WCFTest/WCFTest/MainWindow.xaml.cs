using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
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

namespace WCFTest
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      private ServiceHost host;
      public MainWindow()
      {
         InitializeWCF();
         InitializeComponent();

         Closed += MainWindow_Closed;
      }

      void MainWindow_Closed(object sender, EventArgs e)
      {
         host.Close();
      }

      private void InitializeWCF()
      {
         // experimented with using a service instance (because I needed to set a property)
         // http://stackoverflow.com/questions/14206267/how-do-i-pass-parameters-to-a-servicehost
         Uri baseServiceAddress = new Uri("http://localhost:1972/Computer");
         FirstComputer instance = new FirstComputer {Multipler = 2};
         host = new ServiceHost(instance, baseServiceAddress);
         
         // Enable MetaData publishing.
         ServiceMetadataBehavior serviceMetaDataBehaviour = new ServiceMetadataBehavior();
         serviceMetaDataBehaviour.HttpGetEnabled = true;
         serviceMetaDataBehaviour.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
         host.Description.Behaviors.Add(serviceMetaDataBehaviour);
         // Open the ServiceHost to start listening for messages. No endpoint are explicitly defined, runtime creates default endpoint.
         host.Open();
      }
   }
}
