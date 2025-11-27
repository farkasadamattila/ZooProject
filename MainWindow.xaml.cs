using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Windows.Threading;
using System.Linq;
using Microsoft.Win32;

namespace Zoo
{
    public partial class MainWindow : Window
    {
        private List<Animal> animals = new List<Animal>();
        private const string JsonPath = "animals.json";
        private readonly DispatcherTimer _clockTimer;
        private Animal? selectedAnimal = null;

        public MainWindow()
        {
            InitializeComponent();
            _clockTimer = new DispatcherTimer(DispatcherPriority.Background);
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += ClockTimer_Tick;
            _clockTimer.Start();

            LoadAnimals();
            this.Loaded += (s, e) => RefreshDisplay();
        }

        private void ClockTimer_Tick(object? sender, EventArgs e)
        {
            if (ClockTextBlock != null)
            {
                ClockTextBlock.Text = DateTime.Now.ToString("HH:mm:ss");
            }
        }

        private void LoadAnimals()
        {
            if (!File.Exists(JsonPath))
            {
                animals = new List<Animal>();
                return;
            }

            string json = File.ReadAllText(JsonPath);
            if (string.IsNullOrWhiteSpace(json))
            {
                animals = new List<Animal>();
                return;
            }

            animals = JsonSerializer.Deserialize<List<Animal>>(json) ?? new List<Animal>();
        }

        private void SaveAnimals()
        {
            string json = JsonSerializer.Serialize(animals, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(JsonPath, json);
        }

        private void AddAnimalButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(IdTextBox.Text, out int id) &&
                !string.IsNullOrEmpty(NameTextBox.Text) &&
                HabitatComboBox.SelectedItem is ComboBoxItem selectedHabitat &&
                HealthStatusComboBox.SelectedItem is ComboBoxItem selectedHealth &&
                ArrivalDatePicker.SelectedDate != null)
            {
                if (animals.Any(a => a.id == id))
                {
                    MessageBox.Show("An animal with this ID already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                animals.Add(new Animal
                {
                    id = id,
                    species = NameTextBox.Text,
                    habitat = selectedHabitat.Content.ToString(),
                    arrival_date = ArrivalDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    health_status = selectedHealth.Content.ToString(),
                    is_fed = IsFedCheckBox.IsChecked ?? true
                });
                SaveAnimals();

                MessageBox.Show("Animal added successfully.", "Success");
                
                IdTextBox.Text = "";
                NameTextBox.Text = "";
                HabitatComboBox.SelectedIndex = -1;
                ArrivalDatePicker.SelectedDate = null;
                HealthStatusComboBox.SelectedIndex = 0;
                IsFedCheckBox.IsChecked = true;

                RefreshDisplay();
            }
            else
            {
                MessageBox.Show("Please fill in all fields correctly.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowListButton_Click(object sender, RoutedEventArgs e)
        {
            var sortedAnimals = GetSortedAnimals();
            var listBox = new ListBox
            {
                ItemsSource = sortedAnimals,
                Margin = new Thickness(0, 10, 0, 0),
                ItemTemplate = (DataTemplate)FindResource("AnimalListTemplate")
            };
            listBox.SelectionChanged += DisplayListBox_SelectionChanged;
            DataDisplay.Content = listBox;
        }

        private void ShowLabelsButton_Click(object sender, RoutedEventArgs e)
        {
            var sortedAnimals = GetSortedAnimals();
            var panel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };
            foreach (var animal in sortedAnimals)
            {
                var border = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.LightGray,
                    BorderThickness = new Thickness(1),
                    Background = System.Windows.Media.Brushes.White,
                    CornerRadius = new System.Windows.CornerRadius(8),
                    Margin = new Thickness(0, 4, 0, 4),
                    Padding = new Thickness(12),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = animal
                };

                var label = new Label
                {
                    Content = $"ID: {animal.id} | Species: {animal.species} | Habitat: {animal.habitat} | Arrival: {animal.arrival_date} | Health: {animal.health_status} | Fed: {animal.is_fed}",
                    FontSize = 13,
                    FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                    Foreground = System.Windows.Media.Brushes.DarkSlateGray
                };

                border.Child = label;
                border.MouseLeftButtonDown += (s, e) =>
                {
                    if (s is Border clickedBorder && clickedBorder.Tag is Animal clickedAnimal)
                    {
                        LoadAnimalForModification(clickedAnimal);
                    }
                };
                panel.Children.Add(border);
            }
            DataDisplay.Content = panel;
        }

        private void DisplayListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is Animal animal)
            {
                LoadAnimalForModification(animal);
            }
        }

        private void SearchRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox == null || SearchComboBox == null || SearchFedCheckBox == null)
                return;

            SearchTextBox.Visibility = Visibility.Collapsed;
            SearchComboBox.Visibility = Visibility.Collapsed;
            SearchFedCheckBox.Visibility = Visibility.Collapsed;
            SearchComboBox.Items.Clear();

            if (SearchByIdRadio.IsChecked == true || SearchBySpeciesRadio.IsChecked == true)
            {
                SearchTextBox.Visibility = Visibility.Visible;
                SearchTextBox.Text = "";
            }
            else if (SearchByHabitatRadio.IsChecked == true)
            {
                SearchComboBox.Visibility = Visibility.Visible;
                SearchComboBox.Items.Add(new ComboBoxItem { Content = "All" });
                SearchComboBox.Items.Add(new ComboBoxItem { Content = "Savannah" });
                SearchComboBox.Items.Add(new ComboBoxItem { Content = "Rainforest" });
                SearchComboBox.Items.Add(new ComboBoxItem { Content = "Desert" });
                SearchComboBox.Items.Add(new ComboBoxItem { Content = "Aquatic" });
                SearchComboBox.Items.Add(new ComboBoxItem { Content = "Mountain" });
                SearchComboBox.Items.Add(new ComboBoxItem { Content = "Grassland" });
                SearchComboBox.Items.Add(new ComboBoxItem { Content = "Arctic" });
                SearchComboBox.SelectedIndex = 0;
            }
            else if (SearchByHealthRadio.IsChecked == true)
            {
                SearchComboBox.Visibility = Visibility.Visible;
                SearchComboBox.Items.Add(new ComboBoxItem { Content = "All" });
                SearchComboBox.Items.Add(new ComboBoxItem { Content = "Excellent" });
                SearchComboBox.Items.Add(new ComboBoxItem { Content = "Good" });
                SearchComboBox.Items.Add(new ComboBoxItem { Content = "Fair" });
                SearchComboBox.Items.Add(new ComboBoxItem { Content = "Poor" });
                SearchComboBox.SelectedIndex = 0;
            }
            else if (SearchByFedRadio.IsChecked == true)
            {
                SearchFedCheckBox.Visibility = Visibility.Visible;
                SearchFedCheckBox.IsChecked = false;
                PerformSearch();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PerformSearch();
        }

        private void SearchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PerformSearch();
        }

        private void SearchFedCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void PerformSearch()
        {
            if (SearchResultsListBox == null)
                return;

            List<Animal> results = new List<Animal>();

            if (SearchByIdRadio.IsChecked == true)
            {
                if (int.TryParse(SearchTextBox.Text, out int searchId))
                {
                    results = animals.Where(a => a.id == searchId).ToList();
                }
            }
            else if (SearchBySpeciesRadio.IsChecked == true)
            {
                string searchText = SearchTextBox.Text.ToLower();
                results = animals.Where(a => a.species.ToLower().Contains(searchText)).ToList();
            }
            else if (SearchByHabitatRadio.IsChecked == true)
            {
                if (SearchComboBox.SelectedItem is ComboBoxItem item)
                {
                    string habitat = item.Content.ToString();
                    if (habitat == "All")
                        results = animals.ToList();
                    else
                        results = animals.Where(a => a.habitat == habitat).ToList();
                }
            }
            else if (SearchByHealthRadio.IsChecked == true)
            {
                if (SearchComboBox.SelectedItem is ComboBoxItem item)
                {
                    string health = item.Content.ToString();
                    if (health == "All")
                        results = animals.ToList();
                    else
                        results = animals.Where(a => a.health_status == health).ToList();
                }
            }
            else if (SearchByFedRadio.IsChecked == true)
            {
                bool showFed = SearchFedCheckBox.IsChecked ?? false;
                results = animals.Where(a => a.is_fed == showFed).ToList();
            }

            SearchResultsListBox.ItemsSource = results;
        }

        private void SearchResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultsListBox.SelectedItem is Animal animal)
            {
                LoadAnimalForModification(animal);
            }
        }

        private void LoadAnimalForModification(Animal animal)
        {
            selectedAnimal = animal;

            ModifyIdTextBox.Text = animal.id.ToString();
            ModifyNameTextBox.Text = animal.species;
            
            foreach (ComboBoxItem item in ModifyHabitatComboBox.Items)
            {
                if (item.Content.ToString() == animal.habitat)
                {
                    ModifyHabitatComboBox.SelectedItem = item;
                    break;
                }
            }

            foreach (ComboBoxItem item in ModifyHealthStatusComboBox.Items)
            {
                if (item.Content.ToString() == animal.health_status)
                {
                    ModifyHealthStatusComboBox.SelectedItem = item;
                    break;
                }
            }

            if (DateTime.TryParse(animal.arrival_date, out DateTime arrivalDate))
            {
                ModifyArrivalDatePicker.SelectedDate = arrivalDate;
            }

            ModifyIsFedCheckBox.IsChecked = animal.is_fed;

            ModifyGrid.IsEnabled = true;
            ModifyButton.IsEnabled = true;
            DeleteButton.IsEnabled = true;
        }

        private void ModifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedAnimal == null)
            {
                MessageBox.Show("No animal selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(ModifyNameTextBox.Text) ||
                ModifyHabitatComboBox.SelectedItem == null ||
                ModifyHealthStatusComboBox.SelectedItem == null ||
                ModifyArrivalDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please fill in all fields correctly.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            selectedAnimal.species = ModifyNameTextBox.Text;
            selectedAnimal.habitat = ((ComboBoxItem)ModifyHabitatComboBox.SelectedItem).Content.ToString();
            selectedAnimal.health_status = ((ComboBoxItem)ModifyHealthStatusComboBox.SelectedItem).Content.ToString();
            selectedAnimal.arrival_date = ModifyArrivalDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
            selectedAnimal.is_fed = ModifyIsFedCheckBox.IsChecked ?? true;

            SaveAnimals();
            MessageBox.Show("Animal updated successfully.", "Success");

            RefreshDisplay();
            ClearModifyForm();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedAnimal == null)
            {
                MessageBox.Show("No animal selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete animal ID {selectedAnimal.id} ({selectedAnimal.species})?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                animals.Remove(selectedAnimal);
                SaveAnimals();
                MessageBox.Show("Animal deleted successfully.", "Success");

                RefreshDisplay();
                ClearModifyForm();
            }
        }

        private void ClearModifyForm()
        {
            selectedAnimal = null;
            ModifyIdTextBox.Text = "";
            ModifyNameTextBox.Text = "";
            ModifyHabitatComboBox.SelectedIndex = -1;
            ModifyHealthStatusComboBox.SelectedIndex = -1;
            ModifyArrivalDatePicker.SelectedDate = null;
            ModifyIsFedCheckBox.IsChecked = false;

            ModifyGrid.IsEnabled = false;
            ModifyButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
        }

        private void RefreshDisplay()
        {
            if (DataDisplay == null)
                return;

            PerformSearch();

            if (DataDisplay.Content is ListBox)
            {
                ShowListButton_Click(null, null);
            }
            else if (DataDisplay.Content is StackPanel)
            {
                ShowLabelsButton_Click(null, null);
            }
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshDisplay();
        }

        private List<Animal> GetSortedAnimals()
        {
            if (SortComboBox == null || SortComboBox.SelectedIndex == -1)
                return animals.ToList();

            return SortComboBox.SelectedIndex switch
            {
                0 => animals.OrderBy(a => a.id).ToList(),
                1 => animals.OrderBy(a => a.species).ToList(),
                2 => animals.OrderBy(a => a.habitat).ToList(),
                3 => animals.OrderBy(a => a.arrival_date).ToList(),
                _ => animals.ToList()
            };
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var sortedAnimals = GetSortedAnimals();

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = "json",
                FileName = $"zoo_export_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string json = JsonSerializer.Serialize(sortedAnimals, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(saveFileDialog.FileName, json);
                    MessageBox.Show($"Data exported successfully to:\n{saveFileDialog.FileName}", "Export Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}