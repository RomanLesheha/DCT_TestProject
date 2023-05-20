using DCT_TestProject.Models;
using LiveCharts;
using LiveCharts.Wpf;
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
using System.Windows.Shapes;

namespace DCT_TestProject
{
    /// <summary>
    /// Interaction logic for SelectedCurrencyInfoWindow.xaml
    /// </summary>
    public partial class SelectedCurrencyInfoWindow : Window
    {
        static public SeriesCollection Chart2SeriesCollection { get; set; }
        static public string[] Labels { get; set; }
        static public SeriesCollection SeriesCollection { get; set; }
        static public string[] Chart2Labels { get; set; }
        static public Func<double, string> YFormatter { get; set; }

        static public string SelectedId { get; set; }

        public SelectedCurrencyInfoWindow()
        {
            InitializeComponent();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await ParseSelectedCurrencyInterval(SelectedId);
            await ParseSelectedCurrencyMarkets(SelectedId,10);
            await ParseSelectedCurrency(SelectedId);
            chart2.Series = Chart2SeriesCollection;
            chart2.AxisX[0].Labels = Chart2Labels;
        }
        private async Task ParseSelectedCurrency(string SelectedID)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://api.coincap.io");
            HttpRequestMessage request = new HttpRequestMessage();
            request = new HttpRequestMessage(HttpMethod.Get, $"/v2/assets/{SelectedID}");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();


            var data = JsonConvert.DeserializeObject<AssetsResponse>(json);
            data.data.dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(data.timestamp).DateTime;

            detailInfo.DataContext = data.data;
        }
        private async Task ParseSelectedCurrencyMarkets(string SelectedId , int countOfMarkets )
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://api.coincap.io");
            HttpRequestMessage request = new HttpRequestMessage();

            request = new HttpRequestMessage(HttpMethod.Get, $"/v2/assets/{SelectedId}/markets");

            DataTable dtCurrency = new DataTable();
            dtCurrency.Columns.Add("MarketName");
            dtCurrency.Columns.Add("Price");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();


            var data = JsonConvert.DeserializeObject<Markets>(json);
            var sortedData = data.data.Take(countOfMarkets);
            foreach (var item in sortedData)
            {
                dtCurrency.Rows.Add($"{item.exchangeId}", item.priceUsd);
            }
            SeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Price",
                        Values = new ChartValues<double>(dtCurrency.Rows.OfType<DataRow>().Select(row => Convert.ToDouble(row["Price"])))
                    }
                };
           
            Labels = dtCurrency.Rows.OfType<DataRow>().Select(row => row["MarketName"].ToString()).ToArray();

            YFormatter = value => value.ToString("N8");

            chartMarkets.DataContext = this;
        }
        private async Task ParseSelectedCurrencyInterval(string currencyId)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://api.coincap.io");
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v2/assets/{currencyId}/history?interval=d1");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            List<decimal> priceUsd = new List<decimal>();
            List<DateTimeOffset> time = new List<DateTimeOffset>();
            var data = JsonConvert.DeserializeObject<SelectedAsset>(json);

            foreach (var item in data.data)
            {
                priceUsd.Add(item.priceUsd);
                time.Add(DateTimeOffset.FromUnixTimeMilliseconds(item.time).DateTime);
            }
            Chart2SeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Price (USD)",
                        Values = new ChartValues<decimal>(priceUsd)
                    }
                };

            Chart2Labels = time.Select(dt => dt.ToString()).ToArray();

            YFormatter = value => value.ToString("N8");

            chart2.DataContext = this;

        }
        private async void ShowMoreClick(object sender, RoutedEventArgs e)
        {
            await ParseSelectedCurrencyMarkets(SelectedId, 100);

            chartMarkets.Series = SeriesCollection;
            chartMarkets.AxisX[0].Labels = Labels;
        }

        private async void ShowLessClick(object sender, RoutedEventArgs e)
        {
            await ParseSelectedCurrencyMarkets(SelectedId, 10);

            chartMarkets.Series = SeriesCollection;
            chartMarkets.AxisX[0].Labels = Labels;
        }

    }
}
