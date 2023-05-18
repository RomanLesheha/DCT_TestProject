using DCT_TestProject.Models;
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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }
        private async void ParseBothCurrencyRating()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://api.coincap.io");
            HttpRequestMessage request = new HttpRequestMessage();
            HttpRequestMessage request1 = new HttpRequestMessage();

            request = new HttpRequestMessage(HttpMethod.Get, "/v2/assets");
            request1 = new HttpRequestMessage(HttpMethod.Get, "/v2/rates");

            DataTable dtCurrency = new DataTable();
            dtCurrency.Columns.Add("Text");
            dtCurrency.Columns.Add("Value");
            dtCurrency.Rows.Add("--SELECT--", 0);

            var response = await client.SendAsync(request);
            var response1 = await client.SendAsync(request1);
            response.EnsureSuccessStatusCode();
            response1.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var json1 = await response1.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<Assets>(json);
            foreach (var item in data.data)
            {
                dtCurrency.Rows.Add(item.name, item.priceUsd);
            }
            cmbFromCurrency.ItemsSource = dtCurrency.DefaultView;
            cmbFromCurrency.DisplayMemberPath = "Text";
            cmbFromCurrency.SelectedValuePath = "Value";
            cmbFromCurrency.SelectedIndex = 0;

            var data1 = JsonConvert.DeserializeObject<Rates>(json1);
            DataTable dtCurrency1 = new DataTable();
            dtCurrency1.Columns.Add("Text");
            dtCurrency1.Columns.Add("Value");
            dtCurrency1.Rows.Add("--SELECT--", 0);
            foreach (var item in data1.data)
            {   
                dtCurrency1.Rows.Add(item.id, item.rateUsd);
            }
            cmbToCurrency.ItemsSource = dtCurrency1.DefaultView;
            cmbToCurrency.DisplayMemberPath = "Text";
            cmbToCurrency.SelectedValuePath = "Value";
            cmbToCurrency.SelectedIndex = 0;
        }
        private async void ParseCurrencyRating(bool CryptoCurrency)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://api.coincap.io");
            HttpRequestMessage request = new HttpRequestMessage();

            if (CryptoCurrency)
                request = new HttpRequestMessage(HttpMethod.Get, "/v2/assets");
            else          
                request = new HttpRequestMessage(HttpMethod.Get, "/v2/rates");
           
            DataTable dtCurrency = new DataTable();
            dtCurrency.Columns.Add("Text");
            dtCurrency.Columns.Add("Value");
            dtCurrency.Rows.Add("--SELECT--", 0);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            if (CryptoCurrency)
            {
                var data = JsonConvert.DeserializeObject<Assets>(json);

                foreach (var item in data.data)
                {
                    dtCurrency.Rows.Add(item.name, item.priceUsd);
                }
            }
            else
            {
                var data = JsonConvert.DeserializeObject<Rates>(json);
                foreach (var item in data.data)
                {
                    dtCurrency.Rows.Add(item.id, item.rateUsd);
                }
            }
           

            cmbFromCurrency.ItemsSource = dtCurrency.DefaultView;
            cmbFromCurrency.DisplayMemberPath = "Text";
            cmbFromCurrency.SelectedValuePath = "Value";
            cmbFromCurrency.SelectedIndex = 0;

            cmbToCurrency.ItemsSource = dtCurrency.DefaultView;
            cmbToCurrency.DisplayMemberPath = "Text";
            cmbToCurrency.SelectedValuePath = "Value";
            cmbToCurrency.SelectedIndex = 0;
        }
        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            double ConvertedValue = (double.Parse(cmbFromCurrency.SelectedValue.ToString()) * double.Parse(CountTxt.Text)) / double.Parse(cmbToCurrency.SelectedValue.ToString());

            resultlbl.Content = ConvertedValue.ToString("N5");
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CryptoCurrChecked(object sender, RoutedEventArgs e)
        {
            cmbToCurrency.ItemsSource = null;
            cmbFromCurrency.ItemsSource = null;
            ParseCurrencyRating(true);
        }

        private void CurrChecked(object sender, RoutedEventArgs e)
        {
            cmbToCurrency.ItemsSource = null;
            cmbFromCurrency.ItemsSource = null;
            ParseCurrencyRating(false);
        }

        private void CryptoToCurrChecked(object sender, RoutedEventArgs e)
        {
            cmbToCurrency.ItemsSource = null;
            cmbFromCurrency.ItemsSource = null;
            ParseBothCurrencyRating();

        }
    }
}
