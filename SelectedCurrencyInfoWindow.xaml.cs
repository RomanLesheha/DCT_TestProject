using DCT_TestProject.Interfaces;
using DCT_TestProject.Models;
using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        public string SelectedInterval { get; set; }

        private readonly IApiParser _apiParser;
        public SelectedCurrencyInfoWindow()
        {
            InitializeComponent();
            _apiParser = new CoinCapApiParser();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await ParseSelectedCurrencyHistory(SelectedId);
            await ParseSelectedCurrencyMarkets(SelectedId,10);
            await ParseSelectedCurrency(SelectedId);
            chart2.Series = Chart2SeriesCollection;
            chart2.AxisX[0].Labels = Chart2Labels;
        }
        private async Task ParseSelectedCurrency(string SelectedID)
        {
            var data = await _apiParser.ParseAsync<AssetsResponse>($"/v2/assets/{SelectedID}");
           
            data.data.dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(data.timestamp).DateTime;

            detailInfo.DataContext = data.data;
        }
        private async Task ParseSelectedCurrencyMarkets(string SelectedId , int countOfMarkets )
        {
            var data = await _apiParser.ParseAsync<Markets>($"/v2/assets/{SelectedId}/markets");

            var sortedData = data.data.Take(countOfMarkets);

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Price",
                    Values = new ChartValues<decimal>(sortedData.Select(item => item.priceUsd)),
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9D97DD"))
                }
            };

            Labels = sortedData.Select(item => item.exchangeId).ToArray();
            YFormatter = value => value.ToString("N8");

            chartMarkets.DataContext = this;
        }
        private async Task ParseSelectedCurrencyHistory(string currencyId , string interval = "d1")
        {
            var data = await _apiParser.ParseAsync<SelectedAsset>($"/v2/assets/{currencyId}/history?interval={interval}");

            var priceUsd = data.data.Select(item => item.priceUsd).Take(250).ToList();
            var time = data.data.Select(item => DateTimeOffset.FromUnixTimeMilliseconds(item.time).DateTime).ToList();

            Chart2SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Price (USD)",
                    Values = new ChartValues<decimal>(priceUsd),
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9D97DD"))
                }
            };

            Chart2Labels = time.Select(dt => dt.ToString()).ToArray();

            YFormatter = value => value.ToString("N8");

            chart2.DataContext = this;

        }
        private async void cmbInterval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbInterval.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedTag = selectedItem.Tag.ToString();
                await ParseSelectedCurrencyHistory(SelectedId , selectedTag);
                chart2.Series = Chart2SeriesCollection;
                chart2.AxisX[0].Labels = Chart2Labels;
            }
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
