using EurotorgTestTask.Commands;
using EurotorgTestTask.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EurotorgTestTask
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private static readonly HttpClient HttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        private ObservableCollection<Rate> _currencyRates = new ObservableCollection<Rate>();
        private DateTime _endDate = DateTime.Now;
        private string _errorMessage;
        private string _jsonFilePath = Directory.GetCurrentDirectory() + "\\CurrencyRates.json";
        private DateTime _startDate = DateTime.Now.AddDays(-7);

        public MainViewModel()
        {
            GetRatesCommand = new AsyncRelayCommand(GetCurrencyRatesAsync);
            SaveToJsonCommand = new RelayCommand(SaveRatesToJson);
            OpenFromJsonCommand = new RelayCommand(OpenRatesFromJson);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Rate> CurrencyRates
        {
            get => _currencyRates;
            set
            {
                _currencyRates = value;
                OnPropertyChanged(nameof(CurrencyRates));
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public ICommand GetRatesCommand { get; }

        public string JsonFilePath
        {
            get => _jsonFilePath;
            set
            {
                _jsonFilePath = value;
                OnPropertyChanged(nameof(JsonFilePath));
            }
        }
        public ICommand OpenFromJsonCommand { get; }
        public ICommand SaveToJsonCommand { get; }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        public async Task GetCurrencyRatesAsync()
        {
            ErrorMessage = null;
            try
            {
                var currencies = (await GetFirst10ActiveCurrenciesAsync()).ToArray();
                var rates = await GetRatesForPeriodAsync(currencies.Select(c => c.CurId));
                var resultRates = rates.Select(r => new Rate
                {
                    Abbreviation = currencies.Single(c => c.CurId == r.CurId).CurAbbreviation,
                    Date = r.Date,
                    Name = currencies.Single(c => c.CurId == r.CurId).CurName,
                    OfficialRate = r.CurOfficialRate
                });

                CurrencyRates.Clear();
                foreach (var rate in resultRates)
                {
                    CurrencyRates.Add(rate);
                }
            }
            catch
            {
                ErrorMessage = "Не удалось загрузить курсы валют из удалённого источника.";
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static async Task<IEnumerable<Currency>> GetFirst10ActiveCurrenciesAsync()
        {
            const string apiUrl = "https://api.nbrb.by/exrates/currencies";
            var responseString = await HttpClient.GetStringAsync(apiUrl);
            return JsonSerializer.Deserialize<IEnumerable<Currency>>(responseString)
                .Where(c => c.CurDateEnd.Year == 2050)
                .Take(10);
        }

        private async Task<IEnumerable<RateShort>> GetRatesForPeriodAsync(IEnumerable<int> curIds)
        {
            const string apiUrlBase = "https://api.nbrb.by/exrates/rates/dynamics/";
            var rates = new List<RateShort>();
            foreach (var curId in curIds)
            {
                var responseString = await HttpClient.GetStringAsync(apiUrlBase + curId + $"?startdate={StartDate:yyyy-MM-dd}&enddate={EndDate:yyyy-MM-dd}");
                var curIdRates = JsonSerializer.Deserialize<IEnumerable<RateShort>>(responseString);
                if (curIdRates != null)
                    rates.AddRange(curIdRates);
            }
            return rates;
        }

        private void OpenRatesFromJson()
        {
            ErrorMessage = null;
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json"
                };

                if (openFileDialog.ShowDialog() != true) return;
                JsonFilePath = openFileDialog.FileName;
                var jsonString = File.ReadAllText(JsonFilePath);
                var rates = JsonSerializer.Deserialize<IEnumerable<Rate>>(jsonString);

                CurrencyRates.Clear();
                foreach (var rate in rates)
                {
                    CurrencyRates.Add(rate);
                    rate.PropertyChanged += (s, e) => WriteRatesToJson();
                }
            }
            catch
            {
                ErrorMessage = "Не удалось загрузить из файла.";
            }
        }

        private void SaveRatesToJson()
        {
            ErrorMessage = null;
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json",
                    Title = "Сохранить JSON файл"
                };

                if (saveFileDialog.ShowDialog() != true) return;
                JsonFilePath = saveFileDialog.FileName;
                WriteRatesToJson();
            }
            catch
            {
                ErrorMessage = "Не удалось сохранить в файл.";
            }
        }

        private void WriteRatesToJson()
        {
            var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            var jsonString = JsonSerializer.Serialize(CurrencyRates, options);
            File.WriteAllText(JsonFilePath, jsonString);
        }
    }
}