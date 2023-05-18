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

        private void Chart_Click(object sender, ChartPoint chartPoint)
        {
            // Retrieve the index of the clicked data point
            var index = (int)chartPoint.X;

            // Retrieve the corresponding name from the Labels array
            var clickedName = Labels[index];

            // Display the name (you can use MessageBox or any other UI element)
            MessageBox.Show($"Clicked Name: {clickedName}");
        }

        public MainWindow()
        {
            InitializeComponent();
            Parse();
        }
       
       

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Window1 window = new Window1();
            window.Show();
            
        }
        private async void Parse()
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

           
            var data = JsonConvert.DeserializeObject<Asset>(json);
            var sortedData = data.data.OrderByDescending(item => item.priceUsd).Take(15);
            foreach (var item in sortedData)
            {
                dtCurrency.Rows.Add(item.name, item.priceUsd);

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
            DataContext = this;
        }


    }
    public class Asset
    {
        public string id { get; set; }
        public string rank { get; set; }
        public string symbol { get; set; }
        public string name { get; set; }
        public string supply { get; set; }
        public string maxSupply { get; set; }
        public string marketCapUsd { get; set; }
        public string volumeUsd24Hr { get; set; }
        public decimal priceUsd { get; set; }
        public string changePercent24Hr { get; set; }
        public string vwap24Hr { get; set; }

        public Asset[] data { get; set; }
    }
}
