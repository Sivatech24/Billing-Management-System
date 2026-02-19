using Billing_Management_System.Models;
using Billing_Management_System.Views;
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

namespace Billing_Management_System
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Show dashboard on startup
            // defer showing HomeView until component names are available
            // this.Loaded += (s, e) => { MainFrame.Content = new HomeView(); };
        }
        private void Home_Click(object sender, RoutedEventArgs e)
        {
            // Home view removed: clear the frame or navigate to a default existing view
            MainFrame.Content = null;
        }

        private void Bill_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BillingView());
        }

        private void Payments_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PaymentsView());
        }

        private void AddData_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AddDataView());
        }

        private void ViewData_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ViewDataView());
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            // Get the currently displayed BillingView from the frame and use its selected bill
            var billingPage = MainFrame.Content as BillingView;
            if (billingPage == null)
            {
                MessageBox.Show("Open 'Make Bill' and select a bill before printing.");
                return;
            }

            var selectedBill = billingPage.SelectedBill;
            if (selectedBill == null)
            {
                MessageBox.Show("Please select a bill from the list to print.");
                return;
            }

            MainFrame.Navigate(new PrintView(selectedBill.Id));
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new OptionsView());
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new HelpView());
        }
    }
}
