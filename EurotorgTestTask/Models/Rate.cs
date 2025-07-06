using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EurotorgTestTask.Models
{
    public class Rate : INotifyPropertyChanged
    {
        private string _abbreviation;
        private decimal? _officialRate;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Abbreviation
        {
            get => _abbreviation;
            set
            {
                if (_abbreviation == value) return;
                _abbreviation = value;
                OnPropertyChanged();
            }
        }

        public DateTime Date { get; set; }
        public string Name { get; set; }

        public decimal? OfficialRate
        {
            get => _officialRate;
            set
            {
                if (_officialRate == value) return;
                _officialRate = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}