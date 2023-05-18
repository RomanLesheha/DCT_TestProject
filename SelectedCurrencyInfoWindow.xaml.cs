using DCT_TestProject.Models;
using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

            chart2.Series = Chart2SeriesCollection;
            chart2.AxisX[0].Labels = Chart2Labels;
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

            YFormatter = value => value.ToString("C");

            chart2.DataContext = this;

        }
    }
}
