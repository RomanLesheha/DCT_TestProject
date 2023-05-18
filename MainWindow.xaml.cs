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


namespace DCT_TestProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

            datagrid1.ItemsSource = dtCurrency.DefaultView;


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
