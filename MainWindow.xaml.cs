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
            ParseTopCurrency();
            ParseExchanges();
        }
        //private void Chart1_Click(object sender, ChartPoint chartPoint)
        //{
        //    var index = (int)chartPoint.X;

        //    var clickedName = Labels[index];

        //    SelectedCurrencyInfoWindow window = new SelectedCurrencyInfoWindow();
   
        //    SelectedCurrencyInfoWindow.SelectedId = clickedName;
        //    window.Show();
        //}
       
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
                .Select(item => item.name.ToString())
                .ToArray();

            YFormatter = value => value.ToString("C");

            chart1.DataContext = this;
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
        private void currencyFind_MouseEnter(object sender, MouseEventArgs e)
        {
            currencyFind.Text = null;
        }
        private void currencyFind_MouseLeave(object sender, MouseEventArgs e)
        {
            currencyFind.Text = "Enter currency name...";
        }
    }

}
