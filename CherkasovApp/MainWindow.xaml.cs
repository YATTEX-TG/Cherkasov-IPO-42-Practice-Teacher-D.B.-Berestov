using CherkasovApp.ViewModels;
using CherkasovLibrary;
using CherkasovLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Globalization;
using System.Windows.Controls;

namespace CherkasovApp
{
    public partial class MainWindow : Window
    {
        private AppDbContext _context;
        private List<PartnerViewModel> _partners;

        public MainWindow()
        {
            InitializeComponent();

            // Показываем приветствие
            MessageBox.Show("Добро пожаловать в программу учета партнеров!",
                           "Приветствие",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _context = new AppDbContext();

                var partners = _context.Partners
                    .Include(p => p.PartnerType)
                    .Include(p => p.Sales)
                        .ThenInclude(s => s.Product)
                    .ToList();

                _partners = partners.Select(p => new PartnerViewModel(p)).ToList();
                PartnersListBox.ItemsSource = _partners;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PartnersListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (PartnersListBox.SelectedItem is PartnerViewModel selectedPartner)
            {
                LoadPartnerSales(selectedPartner.Id);
            }
        }

        private void LoadPartnerSales(int partnerId)
        {
            try
            {
                var sales = _context.Sales
                    .Include(s => s.Product)
                    .Where(s => s.PartnerId == partnerId)
                    .OrderByDescending(s => s.SaleDate)
                    .ToList();

                SalesDataGrid.ItemsSource = sales.Select(s => new SaleViewModel(s));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продаж: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== Обработчики меню =====









        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Все поддерживаемые файлы|*.txt;*.csv;*.json|Текстовые файлы (*.txt)|*.txt|CSV файлы (*.csv)|*.csv|JSON файлы (*.json)|*.json",
                Title = "Открыть файл с данными"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string extension = System.IO.Path.GetExtension(dialog.FileName).ToLower();

                    switch (extension)
                    {
                        case ".txt":
                            OpenTxtFile(dialog.FileName);
                            break;
                        case ".csv":
                            OpenCsvFile(dialog.FileName);
                            break;
                        case ".json":
                            OpenJsonFile(dialog.FileName);
                            break;
                        default:
                            MessageBox.Show("Неподдерживаемый формат файла", "Ошибка",
                                           MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии файла: {ex.Message}",
                                   "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
















        private void ImportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Поддерживаемые файлы|*.txt;*.csv;*.json|Текстовые файлы (*.txt)|*.txt|CSV файлы (*.csv)|*.csv|JSON файлы (*.json)|*.json",
                Title = "Выберите файл для импорта"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string extension = System.IO.Path.GetExtension(dialog.FileName).ToLower();
                    string fileName = dialog.FileName;

                    // Спрашиваем, что делать с данными
                    var importOption = MessageBox.Show(
                        "Как импортировать данные?\n\n" +
                        "Нажмите Да - добавить к существующим данным\n" +
                        "Нажмите Нет - заменить существующие данные\n" +
                        "Нажмите Отмена - отменить импорт",
                        "Выберите режим импорта",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    if (importOption == MessageBoxResult.Cancel)
                        return;

                    bool addToExisting = (importOption == MessageBoxResult.Yes);

                    switch (extension)
                    {
                        case ".txt":
                            ImportFromTxt(fileName, addToExisting);
                            break;
                        case ".csv":
                            ImportFromCsv(fileName, addToExisting);
                            break;
                        case ".json":
                            ImportFromJson(fileName, addToExisting);
                            break;
                        default:
                            MessageBox.Show("Неподдерживаемый формат файла", "Ошибка",
                                           MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при импорте: {ex.Message}",
                                   "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }





        private void ImportFromTxt(string filename, bool addToExisting)
        {
            try
            {
                // Читаем файл
                var lines = File.ReadAllLines(filename, Encoding.UTF8);

                // Простой парсинг TXT (можно доработать под ваш формат)
                int importedCount = 0;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("===") || line.StartsWith("---"))
                        continue;

                    // Пример парсинга: ищем строки с "Партнер:"
                    if (line.Contains("Партнер:"))
                    {
                        // Здесь логика парсинга вашего TXT формата
                        importedCount++;
                    }
                }

                _context.SaveChanges();
                LoadData(); // Обновляем отображение

                MessageBox.Show($"Импорт завершен!\n\nИмпортировано: {importedCount} записей",
                               "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте TXT: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportFromCsv(string filename, bool addToExisting)
        {
            try
            {
                var lines = File.ReadAllLines(filename, Encoding.UTF8);

                if (lines.Length < 2) // Нет заголовка или данных
                {
                    MessageBox.Show("Файл CSV не содержит данных", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Заголовки
                var headers = lines[0].Split(';');
                int importedCount = 0;

                // Начинаем с 1 строки (пропускаем заголовок)
                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;

                    var values = lines[i].Split(';');

                    // Здесь логика импорта CSV
                    // Пример: если CSV содержит данные партнеров
                    if (values.Length >= 5 && headers.Contains("Партнер"))
                    {
                        // Ищем тип партнера
                        string partnerType = "ООО"; // Значение по умолчанию
                        string partnerName = values[0].Replace("ООО", "").Replace("ЗАО", "").Trim();

                        // Создаем нового партнера
                        var partner = new Partner
                        {
                            Name = partnerName,
                            TypeId = 1, // По умолчанию ООО
                            DirectorFullname = values.Length > 1 ? values[1] : null,
                            Phone = values.Length > 2 ? values[2] : null,
                            Email = values.Length > 3 ? values[3] : null,
                            Rating = values.Length > 4 && int.TryParse(values[4], out int rating) ? rating : (int?)null,
                            CreatedAt = DateTime.Now
                        };

                        _context.Partners.Add(partner);
                        importedCount++;
                    }
                }

                _context.SaveChanges();
                LoadData();

                MessageBox.Show($"Импорт CSV завершен!\n\nИмпортировано: {importedCount} записей",
                               "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте CSV: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportFromJson(string filename, bool addToExisting)
        {
            try
            {
                string jsonContent = File.ReadAllText(filename, Encoding.UTF8);

                // Показываем первые 500 символов для отладки
                string preview = jsonContent.Length > 500 ? jsonContent.Substring(0, 500) + "..." : jsonContent;
                MessageBox.Show($"Содержимое JSON (первые 500 символов):\n\n{preview}",
                               "Отладка", MessageBoxButton.OK, MessageBoxImage.Information);

                // Пробуем разные варианты десериализации
                try
                {
                    // Вариант 1: как массив объектов
                    var importedArray = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonContent);

                    if (importedArray != null && importedArray.Count > 0)
                    {
                        ImportFromJsonArray(importedArray, addToExisting);
                        return;
                    }
                }
                catch { /* Не массив */ }

                try
                {
                    // Вариант 2: как один объект
                    var importedObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonContent);

                    if (importedObject != null && importedObject.Count > 0)
                    {
                        var list = new List<Dictionary<string, object>> { importedObject };
                        ImportFromJsonArray(list, addToExisting);
                        return;
                    }
                }
                catch { /* Не объект */ }

                MessageBox.Show("Не удалось распознать формат JSON файла", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте JSON: {ex.Message}\n\n{ex.StackTrace}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportFromJsonArray(List<Dictionary<string, object>> items, bool addToExisting)
        {
            int importedCount = 0;
            var fieldNames = new System.Text.StringBuilder();

            foreach (var item in items)
            {
                try
                {
                    // Собираем информацию о полях для отладки
                    foreach (var key in item.Keys)
                    {
                        if (!fieldNames.ToString().Contains(key))
                            fieldNames.AppendLine($"Поле: {key} = {item[key]} (тип: {item[key]?.GetType()})");
                    }

                    // Пытаемся найти название партнера
                    string partnerName = null;

                    // Проверяем возможные названия полей
                    var possibleNameFields = new[] { "Partner", "partner", "Партнер", "партнер", "Name", "name", "Название", "Наименование" };
                    foreach (var field in possibleNameFields)
                    {
                        if (item.ContainsKey(field))
                        {
                            partnerName = item[field]?.ToString();
                            if (!string.IsNullOrWhiteSpace(partnerName))
                                break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(partnerName))
                    {
                        System.Diagnostics.Debug.WriteLine("Пропуск: не найдено название партнера");
                        continue;
                    }

                    // Создаем нового партнера
                    var partner = new Partner
                    {
                        Name = partnerName,
                        TypeId = 1, // По умолчанию ООО
                        CreatedAt = DateTime.Now
                    };

                    // Пытаемся найти тип партнера
                    string typeName = "ООО";
                    var possibleTypeFields = new[] { "Тип", "тип", "Type", "type", "PartnerType" };
                    foreach (var field in possibleTypeFields)
                    {
                        if (item.ContainsKey(field))
                        {
                            typeName = item[field]?.ToString();
                            break;
                        }
                    }

                    // Ищем тип в базе
                    var partnerType = _context.PartnerTypes.FirstOrDefault(t => t.Name == typeName);
                    if (partnerType != null)
                    {
                        partner.TypeId = partnerType.Id;
                    }

                    // Директор
                    var possibleDirectorFields = new[] { "Director", "director", "Директор", "директор", "DirectorFullname" };
                    foreach (var field in possibleDirectorFields)
                    {
                        if (item.ContainsKey(field))
                        {
                            partner.DirectorFullname = item[field]?.ToString();
                            break;
                        }
                    }

                    // Телефон
                    var possiblePhoneFields = new[] { "Phone", "phone", "Телефон", "телефон" };
                    foreach (var field in possiblePhoneFields)
                    {
                        if (item.ContainsKey(field))
                        {
                            partner.Phone = item[field]?.ToString();
                            break;
                        }
                    }

                    // Email
                    var possibleEmailFields = new[] { "Email", "email", "Почта", "почта" };
                    foreach (var field in possibleEmailFields)
                    {
                        if (item.ContainsKey(field))
                        {
                            partner.Email = item[field]?.ToString();
                            break;
                        }
                    }

                    // Рейтинг
                    var possibleRatingFields = new[] { "Rating", "rating", "Рейтинг", "рейтинг" };
                    foreach (var field in possibleRatingFields)
                    {
                        if (item.ContainsKey(field))
                        {
                            if (int.TryParse(item[field]?.ToString(), out int rating))
                            {
                                partner.Rating = rating;
                            }
                            break;
                        }
                    }

                    _context.Partners.Add(partner);
                    importedCount++;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка при обработке элемента: {ex.Message}");
                }
            }

            // Показываем отладочную информацию о найденных полях
            MessageBox.Show($"Найденные поля в JSON:\n{fieldNames.ToString()}\n\nНажмите ОК для продолжения импорта.",
                           "Отладка", MessageBoxButton.OK, MessageBoxImage.Information);

            if (importedCount > 0)
            {
                _context.SaveChanges();
                LoadData();
                MessageBox.Show($"Импорт JSON завершен!\n\nИмпортировано: {importedCount} записей",
                               "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Не удалось импортировать ни одной записи.\n\n" +
                               "Проверьте, что JSON файл содержит данные в ожидаемом формате.",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




















        private void OpenTxtFile(string filename)
        {
            // Создаем новое окно для просмотра TXT
            var viewerWindow = new Window
            {
                Title = $"Просмотр файла - {System.IO.Path.GetFileName(filename)}",
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            // Создаем основную сетку
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Для текста
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Для кнопок

            // Создаем текстовое поле
            var textBox = new System.Windows.Controls.TextBox
            {
                IsReadOnly = true,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 12,
                Text = File.ReadAllText(filename, Encoding.UTF8),
                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                Margin = new Thickness(5)
            };
            Grid.SetRow(textBox, 0);

            // Создаем панель с кнопками
            var buttonPanel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Margin = new Thickness(5)
            };

            // Кнопка импорта
            //var importButton = new Button
            //{
            //    Content = "Импортировать в базу",
            //    Width = 150,
            //    Height = 30,
            //    Margin = new Thickness(5),
            //    Padding = new Thickness(5),
            //    ToolTip = "Импортировать данные из файла в базу данных"
            //};

            //importButton.Click += (s, args) =>
            //{
            //    // Здесь будет логика импорта
            //    var result = MessageBox.Show(
            //        $"Импортировать данные из файла?\n\n{filename}\n\nЭто может заменить существующие данные в базе.",
            //        "Подтверждение импорта",
            //        MessageBoxButton.YesNo,
            //        MessageBoxImage.Question);

            //    if (result == MessageBoxResult.Yes)
            //    {
            //        MessageBox.Show("Функция импорта в разработке", "Информация",
            //                       MessageBoxButton.OK, MessageBoxImage.Information);
            //    }
            //};

            // Кнопка закрытия
            var closeButton = new Button
            {
                Content = "Закрыть",
                Width = 80,
                Height = 30,
                Margin = new Thickness(5),
                Padding = new Thickness(5),
                ToolTip = "Закрыть окно"
            };

            closeButton.Click += (s, args) => viewerWindow.Close();

            // Добавляем кнопки на панель
            //buttonPanel.Children.Add(importButton);
            //buttonPanel.Children.Add(closeButton);
            //Grid.SetRow(buttonPanel, 1);

            // Добавляем элементы на сетку
            grid.Children.Add(textBox);
            grid.Children.Add(buttonPanel);

            viewerWindow.Content = grid;
            viewerWindow.ShowDialog();
        }

        private void OpenCsvFile(string filename)
        {
            try
            {
                // Читаем CSV файл
                var lines = File.ReadAllLines(filename, Encoding.UTF8);

                if (lines.Length == 0)
                {
                    MessageBox.Show("Файл пуст", "Информация",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Создаем окно для отображения CSV
                var viewerWindow = new Window
                {
                    Title = $"Просмотр CSV - {System.IO.Path.GetFileName(filename)}",
                    Width = 900,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                // Создаем основную сетку
                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Парсим CSV
                var headers = lines[0].Split(';');
                var data = new System.Collections.ObjectModel.ObservableCollection<dynamic>();

                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;

                    var values = lines[i].Split(';');
                    var expando = new System.Dynamic.ExpandoObject();
                    var dict = expando as System.Collections.Generic.IDictionary<string, object>;

                    for (int j = 0; j < headers.Length && j < values.Length; j++)
                    {
                        dict[headers[j]] = values[j];
                    }

                    data.Add(expando);
                }

                // Создаем DataGrid
                var dataGrid = new System.Windows.Controls.DataGrid
                {
                    AutoGenerateColumns = true,
                    IsReadOnly = true,
                    Margin = new Thickness(5),
                    VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                    ItemsSource = data
                };
                Grid.SetRow(dataGrid, 0);

                // Создаем панель с кнопками
                var buttonPanel = new StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    Margin = new Thickness(5)
                };

                // Кнопка импорта
                //var importButton = new Button
                //{
                //    Content = "Импортировать в базу",
                //    Width = 150,
                //    Height = 30,
                //    Margin = new Thickness(5),
                //    Padding = new Thickness(5),
                //    ToolTip = "Импортировать данные из файла в базу данных"
                //};

                //importButton.Click += (s, args) =>
                //{
                //    var result = MessageBox.Show(
                //        $"Импортировать данные из файла?\n\n{filename}\n\nЭто может заменить существующие данные в базе.",
                //        "Подтверждение импорта",
                //        MessageBoxButton.YesNo,
                //        MessageBoxImage.Question);

                //    if (result == MessageBoxResult.Yes)
                //    {
                //        MessageBox.Show("Функция импорта в разработке", "Информация",
                //                       MessageBoxButton.OK, MessageBoxImage.Information);
                //    }
                //};

                // Кнопка закрытия
                var closeButton = new Button
                {
                    Content = "Закрыть",
                    Width = 80,
                    Height = 30,
                    Margin = new Thickness(5),
                    Padding = new Thickness(5),
                    ToolTip = "Закрыть окно"
                };

                closeButton.Click += (s, args) => viewerWindow.Close();

                // Добавляем кнопки на панель
                //buttonPanel.Children.Add(importButton);
                //buttonPanel.Children.Add(closeButton);
                //Grid.SetRow(buttonPanel, 1);

                // Добавляем элементы на сетку
                grid.Children.Add(dataGrid);
                grid.Children.Add(buttonPanel);

                viewerWindow.Content = grid;
                viewerWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии CSV файла: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenJsonFile(string filename)
        {
            try
            {
                // Читаем JSON файл
                string jsonContent = File.ReadAllText(filename, Encoding.UTF8);

                // Пытаемся распарсить для форматирования
                try
                {
                    var parsedJson = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonContent);
                    jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(parsedJson,
                        Newtonsoft.Json.Formatting.Indented);
                }
                catch { /* Если не получается распарсить, оставляем как есть */ }

                // Создаем окно для просмотра JSON
                var viewerWindow = new Window
                {
                    Title = $"Просмотр JSON - {System.IO.Path.GetFileName(filename)}",
                    Width = 800,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                // Создаем основную сетку
                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Создаем текстовое поле
                var textBox = new System.Windows.Controls.TextBox
                {
                    IsReadOnly = true,
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    FontSize = 12,
                    Text = jsonContent,
                    VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                    Margin = new Thickness(5)
                };
                Grid.SetRow(textBox, 0);

                // Создаем панель с кнопками
                var buttonPanel = new StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    Margin = new Thickness(5)
                };

                // Кнопка импорта
                //var importButton = new Button
                //{
                //    Content = "Импортировать в базу",
                //    Width = 150,
                //    Height = 30,
                //    Margin = new Thickness(5),
                //    Padding = new Thickness(5),
                //    ToolTip = "Импортировать данные из файла в базу данных"
                //};

                //importButton.Click += (s, args) =>
                //{
                //    var result = MessageBox.Show(
                //        $"Импортировать данные из файла?\n\n{filename}\n\nЭто может заменить существующие данные в базе.",
                //        "Подтверждение импорта",
                //        MessageBoxButton.YesNo,
                //        MessageBoxImage.Question);

                //    if (result == MessageBoxResult.Yes)
                //    {
                //        MessageBox.Show("Функция импорта в разработке", "Информация",
                //                       MessageBoxButton.OK, MessageBoxImage.Information);
                //    }
                //};

                // Кнопка закрытия
                var closeButton = new Button
                {
                    Content = "Закрыть",
                    Width = 80,
                    Height = 30,
                    Margin = new Thickness(5),
                    Padding = new Thickness(5),
                    ToolTip = "Закрыть окно"
                };

                closeButton.Click += (s, args) => viewerWindow.Close();

                // Добавляем кнопки на панель
                //buttonPanel.Children.Add(importButton);
                //buttonPanel.Children.Add(closeButton);
                //Grid.SetRow(buttonPanel, 1);

                // Добавляем элементы на сетку
                grid.Children.Add(textBox);
                grid.Children.Add(buttonPanel);

                viewerWindow.Content = grid;
                viewerWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии JSON файла: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }












        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _context.SaveChanges();
                MessageBox.Show("Данные сохранены", "Информация",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAsTxtMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveDataToFile("txt", "Текстовые файлы (*.txt)|*.txt", SaveAsTxt);
        }

        private void SaveAsExcelMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveDataToFile("csv", "CSV файлы (*.csv)|*.csv", SaveAsCsv);
        }

        private void SaveAsJsonMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveDataToFile("json", "JSON файлы (*.json)|*.json", SaveAsJson);
        }

        private void SaveDataToFile(string extension, string filter, Action<string> saveAction)
        {
            var dialog = new SaveFileDialog
            {
                Filter = filter,
                DefaultExt = extension,
                FileName = $"partners_export_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    saveAction(dialog.FileName);
                    MessageBox.Show($"Данные успешно сохранены в файл:\n{dialog.FileName}",
                                   "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения файла: {ex.Message}",
                                   "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveAsTxt(string filename)
        {
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                writer.WriteLine("=== СПИСОК ПАРТНЕРОВ ===");
                writer.WriteLine($"Дата экспорта: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                writer.WriteLine(new string('-', 80));

                foreach (var partner in _partners)
                {
                    writer.WriteLine($"Партнер: {partner.DisplayName}");
                    writer.WriteLine($"Директор: {partner.Director}");
                    writer.WriteLine($"Телефон: {partner.Phone}");
                    writer.WriteLine($"Рейтинг: {(partner.Rating.HasValue ? partner.Rating.Value.ToString() : "Не указан")}");
                    writer.WriteLine($"Скидка: {partner.Discount}%");

                    // Продажи партнера - загружаем отдельно
                    var sales = _context.Sales
                        .Include("Product")
                        .Where(s => s.PartnerId == partner.Id)
                        .ToList();

                    if (sales.Any())
                    {
                        writer.WriteLine("Продажи:");
                        foreach (var sale in sales)
                        {
                            string productName = sale.Product != null ? sale.Product.Name : "Неизвестно";
                            writer.WriteLine($"  - {productName}: {sale.Quantity} шт. на {sale.TotalAmount:C} (дата: {sale.SaleDate:dd.MM.yyyy})");
                        }
                    }

                    writer.WriteLine(new string('-', 80));
                }
            }
        }

        private void SaveAsCsv(string filename)
        {
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                // Заголовки
                writer.WriteLine("Партнер;Директор;Телефон;Рейтинг;Скидка;Продукт;Количество;Дата;Сумма");

                foreach (var partner in _partners)
                {
                    var sales = _context.Sales
                        .Include("Product")
                        .Where(s => s.PartnerId == partner.Id)
                        .ToList();

                    if (sales.Any())
                    {
                        foreach (var sale in sales)
                        {
                            string productName = sale.Product != null ? sale.Product.Name : "Неизвестно";
                            string rating = partner.Rating.HasValue ? partner.Rating.Value.ToString() : "";
                            writer.WriteLine($"{partner.DisplayName};{partner.Director};{partner.Phone};{rating};{partner.Discount}%;{productName};{sale.Quantity};{sale.SaleDate:dd.MM.yyyy};{sale.TotalAmount:F2}");
                        }
                    }
                    else
                    {
                        string rating = partner.Rating.HasValue ? partner.Rating.Value.ToString() : "";
                        writer.WriteLine($"{partner.DisplayName};{partner.Director};{partner.Phone};{rating};{partner.Discount}%;;;;");
                    }
                }
            }
        }

        private void SaveAsJson(string filename)
        {
            var exportData = new List<object>();

            foreach (var partner in _partners)
            {
                var sales = _context.Sales
                    .Include("Product")
                    .Where(s => s.PartnerId == partner.Id)
                    .Select(s => new
                    {
                        Product = s.Product != null ? s.Product.Name : "Неизвестно",
                        Quantity = s.Quantity,
                        Date = s.SaleDate.ToString("dd.MM.yyyy"),
                        Amount = s.TotalAmount
                    }).ToList();

                var partnerData = new
                {
                    Partner = partner.DisplayName,
                    Director = partner.Director,
                    Phone = partner.Phone,
                    Rating = partner.Rating,
                    Discount = partner.Discount,
                    Sales = sales
                };

                exportData.Add(partnerData);
            }

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(exportData, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filename, json, Encoding.UTF8);
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // ===== Обработчики для партнеров =====

        private void AddPartnerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PartnerDialogWindow(_context);
            if (dialog.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void EditPartnerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PartnersListBox.SelectedItem is PartnerViewModel selectedPartner)
            {
                var dialog = new PartnerDialogWindow(_context, selectedPartner.Partner);
                if (dialog.ShowDialog() == true)
                {
                    LoadData();
                    LoadPartnerSales(selectedPartner.Id);
                }
            }
            else
            {
                MessageBox.Show("Выберите партнера для редактирования", "Предупреждение",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeletePartnerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PartnersListBox.SelectedItem is PartnerViewModel selectedPartner)
            {
                var result = MessageBox.Show(
                    $"Удалить партнера {selectedPartner.DisplayName}?\n" +
                    "Все связанные продажи также будут удалены.",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.Partners.Remove(selectedPartner.Partner);
                        _context.SaveChanges();
                        LoadData();
                        SalesDataGrid.ItemsSource = null;

                        MessageBox.Show("Партнер успешно удален", "Информация",
                                       MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}",
                                       "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите партнера для удаления", "Предупреждение",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ===== Обработчики для продаж =====

        private void AddSaleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PartnersListBox.SelectedItem is PartnerViewModel selectedPartner)
            {
                var dialog = new SaleDialogWindow(_context, selectedPartner.Id);
                if (dialog.ShowDialog() == true)
                {
                    LoadPartnerSales(selectedPartner.Id);
                    LoadData(); // Обновляем список партнеров (скидка могла измениться)
                }
            }
            else
            {
                MessageBox.Show("Выберите партнера для добавления продажи", "Предупреждение",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void EditSaleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, выбран ли элемент в DataGrid
            if (SalesDataGrid.SelectedItem != null)
            {
                // Получаем выбранную продажу
                if (SalesDataGrid.SelectedItem is SaleViewModel selectedSale)
                {
                    // Проверяем, выбран ли партнер
                    if (PartnersListBox.SelectedItem is PartnerViewModel selectedPartner)
                    {
                        // Ищем продажу в базе данных
                        var sale = _context.Sales.Find(selectedSale.Id);
                        if (sale != null)
                        {
                            var dialog = new SaleDialogWindow(_context, selectedPartner.Id, sale);
                            if (dialog.ShowDialog() == true)
                            {
                                LoadPartnerSales(selectedPartner.Id);
                                LoadData(); // Обновляем список партнеров (скидка могла измениться)

                                MessageBox.Show("Продажа успешно обновлена", "Информация",
                                               MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Продажа не найдена в базе данных", "Ошибка",
                                           MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Сначала выберите партнера", "Предупреждение",
                                       MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите конкретную продажу для редактирования (кликните на строку в таблице)",
                               "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteSaleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, выбран ли элемент в DataGrid
            if (SalesDataGrid.SelectedItem != null)
            {
                // Получаем выбранную продажу
                if (SalesDataGrid.SelectedItem is SaleViewModel selectedSale)
                {
                    // Проверяем, выбран ли партнер
                    if (PartnersListBox.SelectedItem is PartnerViewModel selectedPartner)
                    {
                        var result = MessageBox.Show(
                            $"Удалить продажу?\n\n" +
                            $"Продукт: {selectedSale.ProductName}\n" +
                            $"Количество: {selectedSale.Quantity} шт.\n" +
                            $"Дата: {selectedSale.SaleDate:dd.MM.yyyy}\n" +
                            $"Сумма: {selectedSale.TotalAmount:C}",
                            "Подтверждение удаления",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                // Ищем продажу в базе данных
                                var sale = _context.Sales.Find(selectedSale.Id);
                                if (sale != null)
                                {
                                    _context.Sales.Remove(sale);
                                    _context.SaveChanges();

                                    // Обновляем отображение
                                    LoadPartnerSales(selectedPartner.Id);
                                    LoadData(); // Обновляем список партнеров (скидка могла измениться)

                                    MessageBox.Show("Продажа успешно удалена", "Информация",
                                                   MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                else
                                {
                                    MessageBox.Show("Продажа не найдена в базе данных", "Ошибка",
                                                   MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка удаления: {ex.Message}",
                                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Сначала выберите партнера", "Предупреждение",
                                       MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите конкретную продажу для удаления (кликните на строку в таблице)",
                               "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ===== Статистика =====

        private void StatisticsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var totalPartners = _context.Partners.Count();
                var totalSales = _context.Sales.Count();
                var totalProducts = _context.Products.Count();
                var totalRevenue = _context.Sales.Sum(s => (decimal?)s.TotalAmount) ?? 0;

                var topPartner = _context.Partners
                    .Include(p => p.Sales)
                    .OrderByDescending(p => p.Sales.Sum(s => (int?)s.Quantity) ?? 0)
                    .FirstOrDefault();

                var topProduct = _context.Products
                    .Select(p => new
                    {
                        Product = p,
                        TotalQuantity = _context.Sales.Where(s => s.ProductId == p.Id).Sum(s => (int?)s.Quantity) ?? 0
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .FirstOrDefault();

                var stats = $"=== СТАТИСТИКА ===\n\n" +
                           $"Всего партнеров: {totalPartners}\n" +
                           $"Всего продаж: {totalSales}\n" +
                           $"Всего продуктов: {totalProducts}\n" +
                           $"Общая выручка: {totalRevenue:C}\n\n" +
                           $"Лучший партнер: {(topPartner?.Name ?? "Нет данных")}\n" +
                           $"Самый популярный продукт: {(topProduct?.Product?.Name ?? "Нет данных")} (продано: {topProduct?.TotalQuantity} шт.)\n\n" +
                           $"Средняя сумма продажи: {(totalSales > 0 ? (totalRevenue / totalSales).ToString("C") : "0")}";

                MessageBox.Show(stats, "Статистика", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подсчете статистики: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== Обработчики окна =====

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из программы?",
                                        "Подтверждение выхода",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                MessageBox.Show("До свидания! Спасибо за работу с программой.",
                               "Завершение работы",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
            }
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Программа для учета партнеров\n" +
                "Разработал: Cherkasov\n" +
                "Версия: 2.0\n\n" +
                "Новые возможности:\n" +
                "- Управление продажами\n" +
                "- Экспорт данных в TXT, CSV, JSON\n" +
                "- Статистика\n" +
                "- Контекстное меню (ПКМ)\n" +
                "- Автоматический расчет скидок",
                "О программе",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }



        private void RulesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string rules =
                "ПРАВИЛА РАСЧЕТА СКИДКИ\n" +
                "══════════════════════\n\n" +
                "Скидка зависит от общего количества\n" +
                "проданной продукции партнером:\n\n" +
                "▪ до 10 000 шт. → скидка 0%\n" +
                "▪ от 10 000 до 50 000 шт. → скидка 5%\n" +
                "▪ от 50 000 до 300 000 шт. → скидка 10%\n" +
                "▪ более 300 000 шт. → скидка 15%\n\n" +
                "Скидка пересчитывается автоматически\n" +
                "при добавлении или изменении продаж.";

            MessageBox.Show(rules, "Правила расчета скидки",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
        }




        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}