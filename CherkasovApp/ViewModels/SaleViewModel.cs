using CherkasovLibrary.Models;
using System;

namespace CherkasovApp.ViewModels
{
    /// <summary>
    /// Модель представления для отображения продажи в интерфейсе
    /// </summary>
    public class SaleViewModel
    {
        private Sale _sale;

        public SaleViewModel(Sale sale)
        {
            _sale = sale;
        }

        // Добавляем свойство Id
        public int Id => _sale.Id;

        public string ProductName => _sale.Product?.Name ?? "Неизвестно";
        public int Quantity => _sale.Quantity;
        public DateTime SaleDate => _sale.SaleDate;
        public decimal TotalAmount => _sale.TotalAmount;
    }
}