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
        private List<Animal> animals;
        private const string JsonPath = "animals.json";
        private DispatcherTimer clockTimer;
        private Animal selectedAnimal;
        private bool isAscending;

        public MainWindow()
        {
            InitializeComponent();
            animals = new List<Animal>();
            isAscending = true;
            
            InitializeClock();
            LoadAnimals();
            
            this.Loaded += (s, e) => UpdateSearchResults();
        }

        private void InitializeClock()
        {
            clockTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            clockTimer.Tick += (s, e) => 
            {
                if (ClockTextBlock != null)
                    ClockTextBlock.Text = DateTime.Now.ToString("HH:mm:ss");
            };
            clockTimer.Start();
        }

        private void LoadAnimals()
        {
            if (!File.Exists(JsonPath))
                return;

            string json = File.ReadAllText(JsonPath);
            if (!string.IsNullOrWhiteSpace(json))
                animals = JsonSerializer.Deserialize<List<Animal>>(json) ?? new List<Animal>();
        }

        private void SaveAnimals()
        {
            string json = JsonSerializer.Serialize(animals, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(JsonPath, json);
        }

        private void AddAnimalButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateAddAnimalInput(out int id, out string habitat, out string healthStatus))
            {
                MessageBox.Show("Please fill in all fields correctly.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (animals.Any(a => a.id == id))
            {
                MessageBox.Show("An animal with this ID already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var newAnimal = new Animal
            {
                id = id,
                species = NameTextBox.Text,
                habitat = habitat,
                arrival_date = ArrivalDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"),
                health_status = healthStatus,
                is_fed = IsFedCheckBox.IsChecked ?? true
            };

            animals.Add(newAnimal);
            SaveAnimals();
            MessageBox.Show("Animal added successfully.", "Success");
            ClearAddAnimalForm();
            UpdateSearchResults();
        }

        private bool ValidateAddAnimalInput(out int id, out string habitat, out string healthStatus)
        {
            id = 0;
            habitat = null;
            healthStatus = null;

            if (!int.TryParse(IdTextBox.Text, out id) || 
                string.IsNullOrEmpty(NameTextBox.Text) ||
                ArrivalDatePicker.SelectedDate == null)
                return false;

            if (HabitatComboBox.SelectedItem is ComboBoxItem habitatItem)
                habitat = habitatItem.Content.ToString();
            
            if (HealthStatusComboBox.SelectedItem is ComboBoxItem healthItem)
                healthStatus = healthItem.Content.ToString();

            return habitat != null && healthStatus != null;
        }

        private void ClearAddAnimalForm()
        {
            IdTextBox.Text = string.Empty;
            NameTextBox.Text = string.Empty;
            HabitatComboBox.SelectedIndex = -1;
            ArrivalDatePicker.SelectedDate = null;
            HealthStatusComboBox.SelectedIndex = 0;
            IsFedCheckBox.IsChecked = true;
        }

        private void SearchRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox == null || SearchComboBox == null || SearchFedCheckBox == null)
                return;

            HideAllSearchInputs();
            SearchComboBox.Items.Clear();

            if (SearchByIdRadio.IsChecked == true || SearchBySpeciesRadio.IsChecked == true)
            {
                SearchTextBox.Visibility = Visibility.Visible;
                SearchTextBox.Text = string.Empty;
            }
            else if (SearchByHabitatRadio.IsChecked == true)
            {
                SetupHabitatSearchComboBox();
            }
            else if (SearchByHealthRadio.IsChecked == true)
            {
                SetupHealthSearchComboBox();
            }
            else if (SearchByFedRadio.IsChecked == true)
            {
                SearchFedCheckBox.Visibility = Visibility.Visible;
                SearchFedCheckBox.IsChecked = false;
                UpdateSearchResults();
            }
        }

        private void HideAllSearchInputs()
        {
            SearchTextBox.Visibility = Visibility.Collapsed;
            SearchComboBox.Visibility = Visibility.Collapsed;
            SearchFedCheckBox.Visibility = Visibility.Collapsed;
        }

        private void SetupHabitatSearchComboBox()
        {
            SearchComboBox.Visibility = Visibility.Visible;
            var habitats = new[] { "All", "Savannah", "Rainforest", "Desert", "Aquatic", "Mountain", "Grassland", "Arctic" };
            foreach (var habitat in habitats)
                SearchComboBox.Items.Add(new ComboBoxItem { Content = habitat });
            SearchComboBox.SelectedIndex = 0;
        }

        private void SetupHealthSearchComboBox()
        {
            SearchComboBox.Visibility = Visibility.Visible;
            var healthStatuses = new[] { "All", "Excellent", "Good", "Fair", "Poor" };
            foreach (var status in healthStatuses)
                SearchComboBox.Items.Add(new ComboBoxItem { Content = status });
            SearchComboBox.SelectedIndex = 0;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateSearchResults();
        private void SearchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateSearchResults();
        private void SearchFedCheckBox_Changed(object sender, RoutedEventArgs e) => UpdateSearchResults();
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateSearchResults();

        private void SortOrderButton_Click(object sender, RoutedEventArgs e)
        {
            isAscending = !isAscending;
            SortOrderButton.Content = isAscending ? "⬆" : "⬇";
            SortOrderButton.ToolTip = isAscending ? "Ascending" : "Descending";
            UpdateSearchResults();
        }

        private void UpdateSearchResults()
        {
            if (SearchResultsListBox == null)
                return;

            var results = GetFilteredAnimals();
            var sortedResults = ApplySorting(results);
            SearchResultsListBox.ItemsSource = sortedResults;
        }

        private List<Animal> GetFilteredAnimals()
        {
            if (SearchByIdRadio.IsChecked == true)
                return FilterById();
            
            if (SearchBySpeciesRadio.IsChecked == true)
                return FilterBySpecies();
            
            if (SearchByHabitatRadio.IsChecked == true)
                return FilterByHabitat();
            
            if (SearchByHealthRadio.IsChecked == true)
                return FilterByHealth();
            
            if (SearchByFedRadio.IsChecked == true)
                return FilterByFedStatus();

            return animals.ToList();
        }

        private List<Animal> FilterById()
        {
            if (int.TryParse(SearchTextBox.Text, out int searchId))
                return animals.Where(a => a.id == searchId).ToList();
            return new List<Animal>();
        }

        private List<Animal> FilterBySpecies()
        {
            string searchText = SearchTextBox.Text.ToLower();
            return animals.Where(a => a.species.ToLower().Contains(searchText)).ToList();
        }

        private List<Animal> FilterByHabitat()
        {
            if (SearchComboBox.SelectedItem is ComboBoxItem item)
            {
                string habitat = item.Content.ToString();
                return habitat == "All" ? animals.ToList() : animals.Where(a => a.habitat == habitat).ToList();
            }
            return animals.ToList();
        }

        private List<Animal> FilterByHealth()
        {
            if (SearchComboBox.SelectedItem is ComboBoxItem item)
            {
                string health = item.Content.ToString();
                return health == "All" ? animals.ToList() : animals.Where(a => a.health_status == health).ToList();
            }
            return animals.ToList();
        }

        private List<Animal> FilterByFedStatus()
        {
            bool showFed = SearchFedCheckBox.IsChecked ?? false;
            return animals.Where(a => a.is_fed == showFed).ToList();
        }

        private List<Animal> ApplySorting(List<Animal> animalsList)
        {
            if (SortComboBox == null || SortComboBox.SelectedIndex == -1)
                return animalsList;

            var sorted = SortComboBox.SelectedIndex switch
            {
                0 => isAscending ? animalsList.OrderBy(a => a.id) : animalsList.OrderByDescending(a => a.id),
                1 => isAscending ? animalsList.OrderBy(a => a.species) : animalsList.OrderByDescending(a => a.species),
                2 => isAscending ? animalsList.OrderBy(a => a.habitat) : animalsList.OrderByDescending(a => a.habitat),
                3 => isAscending ? animalsList.OrderBy(a => a.arrival_date) : animalsList.OrderByDescending(a => a.arrival_date),
                _ => animalsList.AsEnumerable()
            };

            return sorted.ToList();
        }

        private void SearchResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultsListBox.SelectedItem is Animal animal)
                LoadAnimalForModification(animal);
        }

        private void LoadAnimalForModification(Animal animal)
        {
            selectedAnimal = animal;

            ModifyIdTextBox.Text = animal.id.ToString();
            ModifyNameTextBox.Text = animal.species;
            ModifyArrivalDatePicker.SelectedDate = DateTime.TryParse(animal.arrival_date, out DateTime arrivalDate) ? arrivalDate : null;
            ModifyIsFedCheckBox.IsChecked = animal.is_fed;

            SetComboBoxSelection(ModifyHabitatComboBox, animal.habitat);
            SetComboBoxSelection(ModifyHealthStatusComboBox, animal.health_status);

            ModifyGrid.IsEnabled = true;
            ModifyButton.IsEnabled = true;
            DeleteButton.IsEnabled = true;
        }

        private void SetComboBoxSelection(ComboBox comboBox, string value)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == value)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void ModifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedAnimal == null)
            {
                MessageBox.Show("No animal selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!ValidateModifyInput())
            {
                MessageBox.Show("Please fill in all fields correctly.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            UpdateSelectedAnimal();
            SaveAnimals();
            MessageBox.Show("Animal updated successfully.", "Success");
            UpdateSearchResults();
            ClearModifyForm();
        }

        private bool ValidateModifyInput()
        {
            return !string.IsNullOrEmpty(ModifyNameTextBox.Text) &&
                   ModifyHabitatComboBox.SelectedItem != null &&
                   ModifyHealthStatusComboBox.SelectedItem != null &&
                   ModifyArrivalDatePicker.SelectedDate != null;
        }

        private void UpdateSelectedAnimal()
        {
            selectedAnimal.species = ModifyNameTextBox.Text;
            selectedAnimal.habitat = ((ComboBoxItem)ModifyHabitatComboBox.SelectedItem).Content.ToString();
            selectedAnimal.health_status = ((ComboBoxItem)ModifyHealthStatusComboBox.SelectedItem).Content.ToString();
            selectedAnimal.arrival_date = ModifyArrivalDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
            selectedAnimal.is_fed = ModifyIsFedCheckBox.IsChecked ?? true;
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
                UpdateSearchResults();
                ClearModifyForm();
            }
        }

        private void ClearModifyForm()
        {
            selectedAnimal = null;
            ModifyIdTextBox.Text = string.Empty;
            ModifyNameTextBox.Text = string.Empty;
            ModifyHabitatComboBox.SelectedIndex = -1;
            ModifyHealthStatusComboBox.SelectedIndex = -1;
            ModifyArrivalDatePicker.SelectedDate = null;
            ModifyIsFedCheckBox.IsChecked = false;

            ModifyGrid.IsEnabled = false;
            ModifyButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var dataToExport = GetExportData();
            
            if (dataToExport.Count == 0)
            {
                MessageBox.Show("No data to export.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = "json",
                FileName = $"zoo_export_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string json = JsonSerializer.Serialize(dataToExport, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(saveFileDialog.FileName, json);
                    MessageBox.Show($"Successfully exported {dataToExport.Count} animal(s) to:\n{saveFileDialog.FileName}", 
                        "Export Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private List<Animal> GetExportData()
        {
            if (ExportSearchRadio.IsChecked == true)
            {
                if (SearchResultsListBox.ItemsSource is IEnumerable<Animal> searchResults)
                    return searchResults.ToList();
                return new List<Animal>();
            }

            return animals.ToList();
        }
    }
}