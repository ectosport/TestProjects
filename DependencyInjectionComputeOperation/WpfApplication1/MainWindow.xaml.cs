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

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IOperation computeEngine = null;
        public MainWindow()
        {
            InitializeComponent();
            Operation_Clicked(AddOperation, null);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            double jonathan = 0.0;

            if (computeEngine != null)
            {
                jonathan = computeEngine.Compute(double.Parse(number1.Text), double.Parse(number2.Text));
                answer.Text = jonathan.ToString();    
            }
        }

        private void Operation_Clicked(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton != null)
            {
                computeEngine = System.Reflection.Assembly.GetExecutingAssembly().CreateInstance("WpfApplication1." + radioButton.Name) as IOperation;
            }
        }
    }
}
