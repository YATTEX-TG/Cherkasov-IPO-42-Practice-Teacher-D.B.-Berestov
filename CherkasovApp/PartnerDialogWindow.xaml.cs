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
    public partial class PartnerDialogWindow : Window
    {
        private AppDbContext _context;
        private Partner _partner;

        public PartnerDialogWindow(AppDbContext context, Partner partner = null)
        {
            InitializeComponent();
            _context = context;
            _partner = partner ?? new Partner();

            LoadTypes();

            if (partner != null)
            {
                Title = "Редактирование партнера";
                LoadPartnerData();
            }
            else
            {
                Title = "Добавление партнера";
            }
        }

        private void LoadTypes()
        {
            try
            {
                var types = _context.PartnerTypes.ToList();
                TypeComboBox.ItemsSource = types;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки типов: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPartnerData()
        {
            NameTextBox.Text = _partner.Name;
            TypeComboBox.SelectedValue = _partner.TypeId;
            RatingTextBox.Text = _partner.Rating?.ToString();
            DiscountTextBox.Text = _partner.Discount.ToString() + "%";
            DirectorTextBox.Text = _partner.DirectorFullname;
            PhoneTextBox.Text = _partner.Phone;
            EmailTextBox.Text = _partner.Email;
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
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    MessageBox.Show("Введите наименование партнера", "Предупреждение",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    NameTextBox.Focus();
                    return;
                }

                if (TypeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите тип партнера", "Предупреждение",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    TypeComboBox.Focus();
                    return;
                }

                // Заполнение данных
                _partner.Name = NameTextBox.Text.Trim();
                _partner.TypeId = (int)TypeComboBox.SelectedValue;
                _partner.Address = AddressTextBox.Text?.Trim();

                if (!string.IsNullOrWhiteSpace(RatingTextBox.Text))
                {
                    if (int.TryParse(RatingTextBox.Text, out int rating) && rating >= 0)
                    {
                        _partner.Rating = rating;
                    }
                    else
                    {
                        MessageBox.Show("Рейтинг должен быть целым неотрицательным числом",
                                       "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        RatingTextBox.Focus();
                        return;
                    }
                }

                _partner.DirectorFullname = string.IsNullOrWhiteSpace(DirectorTextBox.Text) ? null : DirectorTextBox.Text.Trim();
                _partner.Phone = string.IsNullOrWhiteSpace(PhoneTextBox.Text) ? null : PhoneTextBox.Text.Trim();
                _partner.Email = string.IsNullOrWhiteSpace(EmailTextBox.Text) ? null : EmailTextBox.Text.Trim();

                if (_partner.Id == 0)
                {
                    _partner.CreatedAt = DateTime.Now;
                    _context.Partners.Add(_partner);
                }
                else
                {
                    _partner.UpdatedAt = DateTime.Now;
                    _context.Entry(_partner).State = EntityState.Modified;
                }

                _context.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}",
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