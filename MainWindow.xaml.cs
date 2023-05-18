using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
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
using LiveCharts;
using LiveCharts.Wpf;
using System.Runtime.Serialization;
using DCT_TestProject.Models;

namespace DCT_TestProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static public SeriesCollection SeriesCollection { get; set; }
        static public string[] Labels { get; set; }
        static public Func<double, string> YFormatter { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ParseTop15Currency();
        }
        private void Chart1_Click(object sender, ChartPoint chartPoint)
        {
            // Retrieve the index of the clicked data point
            var index = (int)chartPoint.X;

            // Retrieve the corresponding name from the Labels array
            var clickedName = Labels[index];
            // Display the name (you can use MessageBox or any other UI element)

            SelectedCurrencyInfoWindow window = new SelectedCurrencyInfoWindow();
   
            SelectedCurrencyInfoWindow.SelectedId = clickedName;
            window.Show();
        }
       
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Window1 window = new Window1();
            window.Show();
        }
       
        private async void ParseTop15Currency()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://api.coincap.io");
            HttpRequestMessage request = new HttpRequestMessage();

            request = new HttpRequestMessage(HttpMethod.Get, "/v2/assets");

            DataTable dtCurrency = new DataTable();
            dtCurrency.Columns.Add("Name");
            dtCurrency.Columns.Add("Price (USD)");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

           
            var data = JsonConvert.DeserializeObject<Assets>(json);
            var sortedData = data.data.OrderByDescending(item => item.priceUsd).Take(10);
            foreach (var item in sortedData)
            {
                dtCurrency.Rows.Add(item.id, item.priceUsd);
            }
            SeriesCollection = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Price (USD)",
                        Values = new ChartValues<double>(dtCurrency.Rows.OfType<DataRow>().Select(row => Convert.ToDouble(row["Price (USD)"])))
                    }
                };

            Labels = dtCurrency.Rows.OfType<DataRow>().Select(row => row["Name"].ToString()).ToArray();

            YFormatter = value => value.ToString("C");
            chart1.DataContext = this;
        }
    }

}
