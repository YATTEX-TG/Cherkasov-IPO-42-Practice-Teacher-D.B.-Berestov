using CherkasovLibrary.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CherkasovApp.ViewModels
{
    /// <summary>
    /// Модель представления для отображения партнера в интерфейсе
    /// </summary>
    public class PartnerViewModel : INotifyPropertyChanged
    {
        private Partner _partner;

        public PartnerViewModel(Partner partner)
        {
            _partner = partner;
        }

        public int Id => _partner.Id;
        public string DisplayName => $"{_partner.PartnerType?.Name} / {_partner.Name}";
        public string Director => _partner.DirectorFullname ?? "Не указан";
        public string Phone => _partner.Phone ?? "Не указан";
        public int? Rating => _partner.Rating;
        public int Discount => _partner.Discount;


        public string Address => _partner.Address ?? "Не указан";

        public Partner Partner => _partner;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}