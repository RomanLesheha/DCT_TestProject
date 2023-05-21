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
using DCT_TestProject.Interfaces;

namespace DCT_TestProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static public SeriesCollection SeriesCollection { get; set; }
        static public string[] Labels { get; set; }
        static public SeriesCollection SeriesCollection2 { get; set; }
        static public string[] Labels2 { get; set; }
        static public Func<double, string> YFormatter { get; set; }

        private readonly IApiParser _apiParser;

        public MainWindow()
        {
            InitializeComponent();
            _apiParser = new CoinCapApiParser();
            ParseTopCurrency();
            ParseExchanges();
            ParseExchangesPercentTotalVolume();

        }
      
        private async void ParseTopCurrency()
        {
            var data = await _apiParser.ParseAsync<Assets>("/v2/assets");
            var sortedData = data.data.OrderByDescending(item => item.priceUsd);

            DataTable dtCurrency = CreateCurrencyDataTable();

            foreach (var item in sortedData)
            {
                dtCurrency.Rows.Add(item.rank,item.id,item.name, item.priceUsd.ToString("N8"));
            }

            CurrencyDataGrid.ItemsSource = dtCurrency.DefaultView;
        }

        private async void ParseExchanges()
        {
            var exchanges = await _apiParser.ParseAsync<Exchanges>("/v2/exchanges");
            var sortedData = exchanges.data.OrderByDescending(item => item.rank);

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Volume (USD)",
                    Values = new ChartValues<decimal>(sortedData
                        .Where(item => item.volumeUsd != null)
                        .Select(item => item.volumeUsd.Value)),
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9D97DD"))
                }
            };

            Labels = sortedData
                .Where(item => item.volumeUsd != null)
                .Select(item => $"{item.exchangeId} - {item.name}")
                .ToArray();

            YFormatter = value => value.ToString("C");

            ExchangeVolumeChart.DataContext = this;
        }
        private async void ParseSelectedExchange(string SelectedID)
        {
            var data = await _apiParser.ParseAsync<ExchangeResponse>($"/v2/exchanges/{SelectedID}");

            data.data.dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(data.timestamp).DateTime;

            detailInfo.DataContext = data.data;
        }
        private async void ParseSelectedCurrency(string SelectedID)
        {
            var data = await _apiParser.ParseAsync<Assets>("/v2/assets");
            var sortedData = data.data.Where(x => x.name.Contains(SelectedID));

            DataTable dtCurrency = CreateCurrencyDataTable();

            foreach (var item in sortedData)
            {
                dtCurrency.Rows.Add(item.rank, item.id, item.name, item.priceUsd.ToString("N8"));
            }
            CurrencyDataGrid.ItemsSource = dtCurrency.DefaultView;
        }

        private async void ParseExchangesPercentTotalVolume()
        {
            var exchanges = await _apiParser.ParseAsync<Exchanges>("/v2/exchanges");
            var sortedData = exchanges.data.OrderByDescending(item => item.percentTotalVolume);

            var top10Data = sortedData
                .Where(item => item.percentTotalVolume != null)
                .Take(10)
                .ToList();

            var otherData = sortedData
                .Where(item => item.percentTotalVolume != null)
                .Skip(10)
                .ToList();

            SeriesCollection2 = new SeriesCollection();

            // Add top 10 exchanges
            for (int i = 0; i < top10Data.Count; i++)
            {
                var dataItem = top10Data[i];
                var pieSeries = new PieSeries
                {
                    Title = dataItem.name,
                    Values = new ChartValues<decimal> { dataItem.percentTotalVolume.Value },
                    Fill = new SolidColorBrush(GetRandomColor()),
                    Stroke = new SolidColorBrush(Colors.White)
                };
                SeriesCollection2.Add(pieSeries);
            }

            // Add "Other" category
            decimal otherTotalVolume = otherData.Sum(item => item.percentTotalVolume.Value);
            var otherPieSeries = new PieSeries
            {
                Title = "Other",
                Values = new ChartValues<decimal> { otherTotalVolume },
                Fill = new SolidColorBrush(Brushes.LightGray.Color),
                Stroke = new SolidColorBrush(Colors.White)
            };
            SeriesCollection2.Add(otherPieSeries);

            Labels2 = top10Data
                .Select(item => item.name.ToString())
                .ToArray();

            chart2.DataContext = this;
        }
        private void ExchangeVolumeChart_Click(object sender, ChartPoint chartPoint)
        {
            var index = (int)chartPoint.X;

            var clickedName = Labels[index];
            var clickedId = clickedName.Split('-')[0].Trim();
            ParseSelectedExchange(clickedId);
        }
        private void CurrencyDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (CurrencyDataGrid.SelectedItem is DataRowView selectedRow)
            {
                string name = selectedRow["Id"].ToString();

                SelectedCurrencyInfoWindow window = new SelectedCurrencyInfoWindow();

                SelectedCurrencyInfoWindow.SelectedId = name;
                window.Show();

            }
        }
        private void FindCurrencyBTN_Click(object sender, RoutedEventArgs e)
        {
            ParseSelectedCurrency(currencyFind.Text);
        }

        private void FindExchangeBTN_Click(object sender, RoutedEventArgs e)
        {
            ParseSelectedExchange(ExchangeFind.Text);
        }

        private void CurrencyConverterBTN_Click(object sender, RoutedEventArgs e)
        {
            Window1 window = new Window1();
            window.Show();
        }
        private Color GetRandomColor()
        {
            Random random = new Random();
            byte[] bytes = new byte[3];
            random.NextBytes(bytes);
            return Color.FromRgb(bytes[0], bytes[1], bytes[2]);
        }
        private DataTable CreateCurrencyDataTable()
        {
            DataTable dtCurrency = new DataTable();
            dtCurrency.Columns.Add("Rank");
            dtCurrency.Columns.Add("Id");
            dtCurrency.Columns.Add("Name");
            dtCurrency.Columns.Add("Price");
            return dtCurrency;
        }
    }

}
