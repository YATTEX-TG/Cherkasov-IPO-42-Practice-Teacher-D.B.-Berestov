using CherkasovLibrary;
using CherkasovLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace CherkasovApp
{
    public partial class SaleDialogWindow : Window
    {
        private AppDbContext _context;
        private Sale _sale;
        private int _partnerId;

        public SaleDialogWindow(AppDbContext context, int partnerId, Sale sale = null)
        {
            InitializeComponent();
            _context = context;
            _partnerId = partnerId;

            // Если продажа новая - создаем с правильными значениями
            if (sale == null)
            {
                _sale = new Sale
                {
                    PartnerId = partnerId,
                    SaleDate = DateTime.Now,
                    CreatedAt = DateTime.Now  // Явно устанавливаем CreatedAt
                };
            }
            else
            {
                _sale = sale;
            }

            LoadProducts();

            if (sale != null)
            {
                Title = "Редактирование продажи";
                LoadSaleData();
            }
            else
            {
                Title = "Добавление продажи";
            }

            // Подписка на изменение количества
            QuantityTextBox.TextChanged += QuantityTextBox_TextChanged;
        }

        private void LoadProducts()
        {
            try
            {
                var products = _context.Products.ToList();
                ProductComboBox.ItemsSource = products;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продуктов: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSaleData()
        {
            ProductComboBox.SelectedValue = _sale.ProductId;
            QuantityTextBox.Text = _sale.Quantity.ToString();
            SaleDatePicker.SelectedDate = _sale.SaleDate;
            UpdateTotalAmount();
        }

        private void QuantityTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateTotalAmount();
        }

        private void UpdateTotalAmount()
        {
            if (ProductComboBox.SelectedItem is Product selectedProduct &&
                int.TryParse(QuantityTextBox.Text, out int quantity) && quantity > 0)
            {
                try
                {
                    // Проверяем на переполнение
                    decimal total = selectedProduct.Price * quantity;

                    // Проверяем, не слишком ли большое число
                    if (total > 99999999.99m) // Максимум для decimal(10,2)
                    {
                        TotalAmountTextBox.Text = "Слишком большая сумма!";
                        _sale.TotalAmount = 0;
                        return;
                    }

                    TotalAmountTextBox.Text = total.ToString("C");
                    _sale.TotalAmount = total;
                }
                catch (OverflowException)
                {
                    TotalAmountTextBox.Text = "Переполнение!";
                    _sale.TotalAmount = 0;
                }
            }
            else
            {
                TotalAmountTextBox.Text = "0 ₽";
                _sale.TotalAmount = 0;
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (ProductComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите продукцию", "Предупреждение",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Введите корректное количество (положительное число)",
                                   "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    QuantityTextBox.Focus();
                    return;
                }

                if (SaleDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату продажи", "Предупреждение",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedProduct = (Product)ProductComboBox.SelectedItem;

                // Проверяем переполнение ДО сохранения
                decimal totalAmount;
                try
                {
                    totalAmount = selectedProduct.Price * quantity;

                    // Проверяем, что сумма не превышает лимит decimal(10,2)
                    if (totalAmount > 99999999.99m)
                    {
                        MessageBox.Show($"Сумма продажи ({totalAmount:C}) слишком большая!\n" +
                                       "Максимально допустимая сумма: 99,999,999.99 ₽",
                                       "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                catch (OverflowException)
                {
                    MessageBox.Show("Переполнение при расчете суммы! Уменьшите количество.",
                                   "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Заполнение данных
                _sale.ProductId = selectedProduct.Id;
                _sale.Quantity = quantity;
                _sale.SaleDate = SaleDatePicker.SelectedDate.Value;
                _sale.PartnerId = _partnerId;
                _sale.TotalAmount = totalAmount;

                if (_sale.Id == 0)
                {
                    _sale.CreatedAt = DateTime.Now;
                    _context.Sales.Add(_sale);
                }
                else
                {
                    _context.Entry(_sale).State = EntityState.Modified;
                }

                _context.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nДетали: {ex.InnerException.Message}";
                }

                MessageBox.Show($"Ошибка сохранения: {errorMessage}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}