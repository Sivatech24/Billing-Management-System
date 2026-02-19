using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace Billing_Management_System.Views
{
    /// <summary>
    /// Interaction logic for OptionsView.xaml
    /// </summary>
    public partial class OptionsView : Page
    {
        public OptionsView()
        {
            InitializeComponent();
        }

        private void ViewData_Click(object sender, RoutedEventArgs e)
        {
            var main = Application.Current.MainWindow as MainWindow;
            if (main != null)
            {
                main.MainFrame.Navigate(new ViewDataView());
            }
        }

        private void EditBill_Click(object sender, RoutedEventArgs e)
        {
            var main = Application.Current.MainWindow as MainWindow;
            if (main != null)
            {
                main.MainFrame.Navigate(new AddDataView());
            }
        }
    }
}
