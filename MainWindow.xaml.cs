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
        static public SeriesCollection SeriesCollection2 { get; set; }
        static public string[] Labels2 { get; set; }
        static public Func<double, string> YFormatter { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ParseTopCurrency();
            ParseExchanges();
            ParseExchangesPercentTotalVolume();

        }
        private void Chart1_Click(object sender, ChartPoint chartPoint)
        {
            var index = (int)chartPoint.X;

            var clickedName = Labels[index];
            var clickedId = clickedName.Split('-')[0].Trim();
            ParseSelectedExchange(clickedId);
        }

        private async void ParseTopCurrency()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://api.coincap.io");
            HttpRequestMessage request = new HttpRequestMessage();

            request = new HttpRequestMessage(HttpMethod.Get, "/v2/assets");

            DataTable dtCurrency = new DataTable();
            dtCurrency.Columns.Add("Rank");
            dtCurrency.Columns.Add("Id");
            dtCurrency.Columns.Add("Name");
            dtCurrency.Columns.Add("Price");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

           
            var data = JsonConvert.DeserializeObject<Assets>(json);
            var sortedData = data.data.OrderByDescending(item => item.priceUsd);
            foreach (var item in sortedData)
            {
                dtCurrency.Rows.Add(item.rank,item.id,item.name, item.priceUsd.ToString("N8"));
            }

            datagrid1.ItemsSource = dtCurrency.DefaultView;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://api.coincap.io");
            HttpRequestMessage request = new HttpRequestMessage();

            request = new HttpRequestMessage(HttpMethod.Get, "/v2/assets");

            DataTable dtCurrency = new DataTable();
            dtCurrency.Columns.Add("Rank");
            dtCurrency.Columns.Add("Id");
            dtCurrency.Columns.Add("Name");
            dtCurrency.Columns.Add("Price");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();


            var data = JsonConvert.DeserializeObject<Assets>(json);
            var sortedData = data.data.Where(x=>x.name.Contains(currencyFind.Text));
            foreach (var item in sortedData)
            {
                dtCurrency.Rows.Add(item.rank, item.id, item.name, item.priceUsd.ToString("N8"));
            }
            datagrid1.ItemsSource = dtCurrency.DefaultView;
        }
        private async void ParseExchanges()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://api.coincap.io");
            var request = new HttpRequestMessage(HttpMethod.Get, "/v2/exchanges");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<Exchanges>(json);
            var sortedData = data.data.OrderByDescending(item => item.rank);

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

            chart1.DataContext = this;
        }
        private async void ParseSelectedExchange(string SelectedID)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://api.coincap.io");
            HttpRequestMessage request = new HttpRequestMessage();
            request = new HttpRequestMessage(HttpMethod.Get, $"/v2/exchanges/{SelectedID}");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();


            var data = JsonConvert.DeserializeObject<ExchangeResponse>(json);
            data.data.dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(data.timestamp).DateTime;

            detailInfo.DataContext = data.data;
        }

        private async void ParseExchangesPercentTotalVolume()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://api.coincap.io");
            var request = new HttpRequestMessage(HttpMethod.Get, "/v2/exchanges");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<Exchanges>(json);
            var sortedData = data.data.OrderByDescending(item => item.percentTotalVolume);

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
                var values = new ChartValues<decimal> { dataItem.percentTotalVolume.Value };
                var color = GetRandomColor();
                var pieSeries = new PieSeries
                {
                    Title = dataItem.name,
                    Values = values,
                    Fill = new SolidColorBrush(color),
                    Stroke = new SolidColorBrush(Colors.White)
                };
                SeriesCollection2.Add(pieSeries);
            }

            // Add "Other" category
            decimal otherTotalVolume = otherData.Sum(item => item.percentTotalVolume.Value);
            var otherValues = new ChartValues<decimal> { otherTotalVolume };
            var otherColor = Brushes.LightGray.Color;
            var otherPieSeries = new PieSeries
            {
                Title = "Other",
                Values = otherValues,
                Fill = new SolidColorBrush(otherColor),
                Stroke = new SolidColorBrush(Colors.White)
            };
            SeriesCollection2.Add(otherPieSeries);

            Labels2 = top10Data
                .Select(item => item.name.ToString())
                .ToArray();

            chart2.DataContext = this;
        }
        private Color GetRandomColor()
        {
            Random random = new Random();
            byte[] bytes = new byte[3];
            random.NextBytes(bytes);
            return Color.FromRgb(bytes[0], bytes[1], bytes[2]);
        }
        private void datagrid1_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (datagrid1.SelectedItem is DataRowView selectedRow)
            {
                string name = selectedRow["Id"].ToString();

                SelectedCurrencyInfoWindow window = new SelectedCurrencyInfoWindow();

                SelectedCurrencyInfoWindow.SelectedId = name;
                window.Show();

            }
        }
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Window1 window = new Window1();
            window.Show();
        }
      
    }

}
