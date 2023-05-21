using DCT_TestProject.Interfaces;
using DCT_TestProject.Models;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Net.Http;
using System.Windows;


namespace DCT_TestProject
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private readonly IApiParser _apiParser;
        public Window1()
        {
            _apiParser = new CoinCapApiParser();
            InitializeComponent();
        }
        private async void ParseBothCurrencyRating()
        {
            var data = await _apiParser.ParseAsync<Assets>("/v2/assets");
            var data1 = await _apiParser.ParseAsync<Rates>("/v2/rates");

            DataTable dtCurrency = new DataTable();
            dtCurrency.Columns.Add("Text");
            dtCurrency.Columns.Add("Value");
            dtCurrency.Rows.Add("--SELECT--", 0);

            foreach (var item in data.data)
            {
                dtCurrency.Rows.Add(item.name, item.priceUsd);
            }
            cmbFromCurrency.ItemsSource = dtCurrency.DefaultView;
            cmbFromCurrency.DisplayMemberPath = "Text";
            cmbFromCurrency.SelectedValuePath = "Value";
            cmbFromCurrency.SelectedIndex = 0;

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
        private void ParseCurrencyRating(bool CryptoCurrency , dynamic data)
        {
            DataTable dtCurrency = new DataTable();
            dtCurrency.Columns.Add("Text");
            dtCurrency.Columns.Add("Value");
            dtCurrency.Rows.Add("--SELECT--", 0);

            if (CryptoCurrency)
            {
                foreach (var item in data.data)
                {
                    dtCurrency.Rows.Add(item.name, item.priceUsd);
                }
            }
            else
            {
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
            resultlbl.Content = null;
            CountTxt.Text = null;
            cmbFromCurrency.SelectedIndex = 0;
            cmbToCurrency.SelectedIndex = 0;

        }

        private async void CryptoCurrChecked(object sender, RoutedEventArgs e)
        {
            cmbToCurrency.ItemsSource = null;
            cmbFromCurrency.ItemsSource = null;
            var data = await _apiParser.ParseAsync<Assets>("/v2/assets");
            ParseCurrencyRating(true ,data);
        }

        private async void CurrChecked(object sender, RoutedEventArgs e)
        {
            cmbToCurrency.ItemsSource = null;
            cmbFromCurrency.ItemsSource = null;
            var data = await _apiParser.ParseAsync<Rates>("/v2/rates");
            ParseCurrencyRating(false, data);
        }

        private void CryptoToCurrChecked(object sender, RoutedEventArgs e)
        {
            cmbToCurrency.ItemsSource = null;
            cmbFromCurrency.ItemsSource = null;
            ParseBothCurrencyRating();

        }
    }
}
