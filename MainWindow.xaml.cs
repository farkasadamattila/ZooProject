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
        private List<Animal> animalsList;
        private const string JsonPath = "animals.json";
        private DispatcherTimer clockTimer;
        private Animal selectedAnimal;
        private bool isAscending;

        public MainWindow()
        {
            InitializeComponent();
            animalsList = new List<Animal>();
            isAscending = true;

            Clock();

            LoadAnimals();

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSearchResults();
        }

        // óra/időhöz kell, lopottxd
        private void Clock()
        {
            clockTimer = new DispatcherTimer();
            clockTimer.Interval = TimeSpan.FromSeconds(1);
            clockTimer.Tick += timePassing;
            clockTimer.Start();
        }

        private void timePassing(object sender, EventArgs e)
        {
            if (ClockTextBlock != null)
            {
                string currentTime = DateTime.Now.ToString("HH:mm:ss");
                ClockTextBlock.Text = currentTime;
            }
        }

        //beolvasás
        private void LoadAnimals()
        {
            string json = File.ReadAllText(JsonPath);
            List<Animal> loadedAnimals = JsonSerializer.Deserialize<List<Animal>>(json);

            if (loadedAnimals != null)
            {animalsList = loadedAnimals;}
            else
            {animalsList = new List<Animal>();}
        }

        //ez a 2 a komment alatt a mentéshez kell
        private JsonSerializerOptions GetJsonOptions()
        { return new JsonSerializerOptions { WriteIndented = true };}
        private void SaveAnimals()
        {
            string json = JsonSerializer.Serialize(animalsList, GetJsonOptions());
            File.WriteAllText(JsonPath, json);
        }

        //állatok hozzáadása (az első csinálja, a második függvény csak ellenőrzi)
        private void AddAnimalButton_Click(object sender, RoutedEventArgs e)
        {
            int id;
            string habitat;
            string healthStatus;

            bool isInputValid = ValidateAddAnimalInput(out id, out habitat, out healthStatus);

            if (!isInputValid)
            {MessageBox.Show("Please fill in all fields correctly.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;}

            bool idAlreadyExists = animalsList.Any(a => a.id == id);

            if (idAlreadyExists)
            {MessageBox.Show("An animal with this ID already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;}

            var newAnimal = new Animal();
            newAnimal.id = id;
            newAnimal.species = NameTextBox.Text;
            newAnimal.habitat = habitat;
            newAnimal.arrival_date = ArrivalDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
            newAnimal.health_status = healthStatus;

            bool isFedValue = IsFedCheckBox.IsChecked ?? true;
            newAnimal.is_fed = isFedValue;

            animalsList.Add(newAnimal);
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

            bool isIdValid = int.TryParse(IdTextBox.Text, out id);
            bool isNameEmpty = string.IsNullOrEmpty(NameTextBox.Text);
            bool isDateSelected = ArrivalDatePicker.SelectedDate != null;

            if (!isIdValid || isNameEmpty || !isDateSelected)
            { return false; }

            habitat = GetComboBoxValue(HabitatComboBox);
            healthStatus = GetComboBoxValue(HealthStatusComboBox);

            return habitat != null && healthStatus != null;
        }

        //űrlap tisztítása hozzáadás után
        private void ClearAddAnimalForm()
        {
            IdTextBox.Text = string.Empty;
            NameTextBox.Text = string.Empty;
            HabitatComboBox.SelectedIndex = -1;
            ArrivalDatePicker.SelectedDate = null;
            HealthStatusComboBox.SelectedIndex = 0;
            IsFedCheckBox.IsChecked = true;
        }

        //kereséshez szükséges részek
        private void SearchRadio_Checked(object sender, RoutedEventArgs e)
        {
            bool controlsAreNotReady = SearchTextBox == null || SearchComboBox == null || SearchFedCheckBox == null;
            if (controlsAreNotReady)
            {
                return;
            }

            HideAllSearchInputs();
            SearchComboBox.Items.Clear();

            bool isSearchById = SearchByIdRadio.IsChecked == true;
            bool isSearchBySpecies = SearchBySpeciesRadio.IsChecked == true;
            bool isSearchByHabitat = SearchByHabitatRadio.IsChecked == true;
            bool isSearchByHealth = SearchByHealthRadio.IsChecked == true;
            bool isSearchByFed = SearchByFedRadio.IsChecked == true;

            if (isSearchById || isSearchBySpecies)
            {SearchTextBox.Visibility = Visibility.Visible;
            SearchTextBox.Text = string.Empty;}
            else if (isSearchByHabitat)
            {SetupHabitatSearchComboBox();}
            else if (isSearchByHealth)
            {SetupHealthSearchComboBox();}
            else if (isSearchByFed)
            {SearchFedCheckBox.Visibility = Visibility.Visible;
            SearchFedCheckBox.IsChecked = false;
            UpdateSearchResults();}
        }
        private void HideAllSearchInputs()
        {
            SearchTextBox.Visibility = Visibility.Collapsed;
            SearchComboBox.Visibility = Visibility.Collapsed;
            SearchFedCheckBox.Visibility = Visibility.Collapsed;
        }

        //kereséshez szükséges combobox(ok) feltöltése
        private void SetupHabitatSearchComboBox()
        {SetupSearchComboBox(new[] { "All", "Savannah", "Rainforest", "Desert", "Aquatic", "Mountain", "Grassland", "Arctic" });}
        private void SetupHealthSearchComboBox()
        {SetupSearchComboBox(new[] { "All", "Excellent", "Good", "Fair", "Poor" });}
        private void SetupSearchComboBox(string[] items)
        {
            SearchComboBox.Visibility = Visibility.Visible;
            SearchComboBox.Items.Clear();

            foreach (string item in items)
            {ComboBoxItem comboBoxItem = new ComboBoxItem();
            comboBoxItem.Content = item;
            SearchComboBox.Items.Add(comboBoxItem);}

            SearchComboBox.SelectedIndex = 0;
        }

        //keresés eseménykezelők (tudom nagyon elegáns)
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {UpdateSearchResults();}

        private void SearchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {UpdateSearchResults();}

        private void SearchFedCheckBox_Changed(object sender, RoutedEventArgs e)
        {UpdateSearchResults();}

        //rendezés eseménykezelők
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {UpdateSearchResults();}
        private void SortOrderButton_Click(object sender, RoutedEventArgs e)
        {
            isAscending = !isAscending;

            if (isAscending)
            {SortOrderButton.Content = "⬆";
            SortOrderButton.ToolTip = "Ascending";}
            else
            {SortOrderButton.Content = "⬇";
            SortOrderButton.ToolTip = "Descending";}

            UpdateSearchResults();
        }

        //kersése és rendezés logikája
        private void UpdateSearchResults()
        {
            if (SearchResultsListBox == null)
            {return;}

            List<Animal> results = GetFilteredAnimals();
            List<Animal> sortedResults = ApplySorting(results);
            SearchResultsListBox.ItemsSource = sortedResults;
        }
        private List<Animal> GetFilteredAnimals()
        {
            if (SearchByIdRadio.IsChecked == true) return FilterById();
            if (SearchBySpeciesRadio.IsChecked == true) return FilterBySpecies();
            if (SearchByHabitatRadio.IsChecked == true) return FilterByHabitat();
            if (SearchByHealthRadio.IsChecked == true) return FilterByHealth();
            if (SearchByFedRadio.IsChecked == true) return FilterByFedStatus();

            return animalsList.ToList();
        }

        //szűrés minden fiszem-faszom alapján
        private List<Animal> FilterById()
        {
            string searchText = SearchTextBox.Text;
            int searchId;
            bool isValidNumber = int.TryParse(searchText, out searchId);

            if (isValidNumber)
            {
                List<Animal> matchingAnimals = new List<Animal>();
                foreach (Animal animal in animalsList)
                {
                    if (animal.id == searchId)
                    { matchingAnimals.Add(animal);}
                }
                return matchingAnimals;
            }
            return new List<Animal>();
        }

        private List<Animal> FilterBySpecies()
        {
            string searchText = SearchTextBox.Text;
            string searchTextLower = searchText.ToLower();

            List<Animal> matchingAnimals = new List<Animal>();

            foreach (Animal animal in animalsList)
            {
                string speciesLower = animal.species.ToLower();
                bool containsSearchText = speciesLower.Contains(searchTextLower);

                if (containsSearchText)
                {matchingAnimals.Add(animal);}
            }

            return matchingAnimals;
        }

        private List<Animal> FilterByHabitat()
        {return FilterByComboBoxSelection(a => a.habitat);}

        private List<Animal> FilterByHealth()
        {return FilterByComboBoxSelection(a => a.health_status);}

        private List<Animal> FilterByComboBoxSelection(Func<Animal, string> propertySelector)
        {
            ComboBoxItem selectedItem = SearchComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                string selectedValue = selectedItem.Content.ToString();

                if (selectedValue == "All")
                {return animalsList.ToList();}

                List<Animal> matchingAnimals = new List<Animal>();
                foreach (Animal animal in animalsList)
                {
                    if (propertySelector(animal) == selectedValue)
                    {matchingAnimals.Add(animal);}
                }
                return matchingAnimals;
            }
            return animalsList.ToList();
        }

        private List<Animal> FilterByFedStatus()
        {
            bool showFed = SearchFedCheckBox.IsChecked ?? false;

            List<Animal> matchingAnimals = new List<Animal>();

            foreach (Animal animal in animalsList)
            {
                if (animal.is_fed == showFed)
                {matchingAnimals.Add(animal);}
            }
            return matchingAnimals;
        }

        //rendezés logikája
        private List<Animal> ApplySorting(List<Animal> animalsList)
        {
            bool sortingNotReady = SortComboBox == null || SortComboBox.SelectedIndex == -1;
            if (sortingNotReady)
            {return animalsList;}

            int selectedSortOption = SortComboBox.SelectedIndex;
            List<Animal> sortedList = new List<Animal>();

            if (selectedSortOption == 0) // ID szerint 
            {
                if (isAscending)
                {sortedList = animalsList.OrderBy(a => a.id).ToList();}
                else
                {sortedList = animalsList.OrderByDescending(a => a.id).ToList();}
            }
            else if (selectedSortOption == 1) // Fajta szerint
            {
                if (isAscending)
                {sortedList = animalsList.OrderBy(a => a.species).ToList();}
                else
                {sortedList = animalsList.OrderByDescending(a => a.species).ToList();}
            }
            else if (selectedSortOption == 2) // Élőhely szerint
            {
                if (isAscending)
                {sortedList = animalsList.OrderBy(a => a.habitat).ToList();}
                else
                { sortedList = animalsList.OrderByDescending(a => a.habitat).ToList();}
            }
            else if (selectedSortOption == 3) // megérkezés dátum szerint
            {
                if (isAscending)
                {sortedList = animalsList.OrderBy(a => a.arrival_date).ToList();}
                else
                {sortedList = animalsList.OrderByDescending(a => a.arrival_date).ToList();}
            }
            else
            {sortedList = animalsList;}
            return sortedList;
        }


        //állat módosítása és törlése (minimálisan bonyolult) - itt még csak megkapjuk hogy melyik állatot és mit akarunk módosítani
        private void SearchResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Animal selectedAnimalFromList = SearchResultsListBox.SelectedItem as Animal;
            if (selectedAnimalFromList != null)
            {LoadAnimalForModification(selectedAnimalFromList);}
        }

        private void LoadAnimalForModification(Animal animal)
        {
            selectedAnimal = animal;

            ModifyIdTextBox.Text = animal.id.ToString();
            ModifyNameTextBox.Text = animal.species;

            DateTime arrivalDate;
            bool isParsed = DateTime.TryParse(animal.arrival_date, out arrivalDate);
            if (isParsed)
            {ModifyArrivalDatePicker.SelectedDate = arrivalDate;}
            else
            {ModifyArrivalDatePicker.SelectedDate = null;}

            ModifyIsFedCheckBox.IsChecked = animal.is_fed;

            SetComboBoxSelection(ModifyHabitatComboBox, animal.habitat);
            SetComboBoxSelection(ModifyHealthStatusComboBox, animal.health_status);

            ModifyGrid.IsEnabled = true;
            ModifyButton.IsEnabled = true;
            DeleteButton.IsEnabled = true;
        }

        private void SetComboBoxSelection(ComboBox comboBox, string value)
        {
            foreach (object item in comboBox.Items)
            {
                ComboBoxItem comboBoxItem = item as ComboBoxItem;

                if (comboBoxItem != null)
                {
                    string itemContent = comboBoxItem.Content.ToString();

                    if (itemContent == value)
                    {comboBox.SelectedItem = comboBoxItem;
                    break;}
                }
            }
        }

        private void ModifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedAnimal == null)
            {MessageBox.Show("No animal selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
            }

            bool isInputValid = ValidateModifyInput();

            if (!isInputValid)
            {MessageBox.Show("Please fill in all fields correctly.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;}

            UpdateSelectedAnimal();
            SaveAnimals();
            MessageBox.Show("Animal updated successfully.", "Success");
            UpdateSearchResults();
            ClearModifyForm();
        }

        //módosítás ellenőrzése
        private bool ValidateModifyInput()
        {
            bool isNameFilled = !string.IsNullOrEmpty(ModifyNameTextBox.Text);
            bool isHabitatSelected = ModifyHabitatComboBox.SelectedItem != null;
            bool isHealthSelected = ModifyHealthStatusComboBox.SelectedItem != null;
            bool isDateSelected = ModifyArrivalDatePicker.SelectedDate != null;

            bool allFieldsValid = isNameFilled && isHabitatSelected && isHealthSelected && isDateSelected;
            return allFieldsValid;
        }

        //módosítás alkalmazása
        private void UpdateSelectedAnimal()
        {
            selectedAnimal.species = ModifyNameTextBox.Text;
            selectedAnimal.habitat = GetComboBoxValue(ModifyHabitatComboBox);
            selectedAnimal.health_status = GetComboBoxValue(ModifyHealthStatusComboBox);
            selectedAnimal.arrival_date = ModifyArrivalDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
            selectedAnimal.is_fed = ModifyIsFedCheckBox.IsChecked ?? true;
        }

        //állat törlése
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedAnimal == null)
            {MessageBox.Show("No animal selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
            }

            string confirmationMessage = $"Are you sure you want to delete animal ID {selectedAnimal.id} ({selectedAnimal.species})?";
            MessageBoxResult result = MessageBox.Show(confirmationMessage, "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            bool userConfirmed = result == MessageBoxResult.Yes;
            if (userConfirmed)
            {
                animalsList.Remove(selectedAnimal);
                SaveAnimals();
                MessageBox.Show("Animal deleted successfully.", "Success");
                UpdateSearchResults();
                ClearModifyForm();
            }
        }

        //törlés v. módosítás után lenullázza az űrlapot
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


        //exportálás és annak csodálatos functionjei
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            List<Animal> dataToExport = GetExportData();

            bool hasNoData = dataToExport.Count == 0;
            if (hasNoData)
            {MessageBox.Show("No data to export.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            saveFileDialog.DefaultExt = "json";

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            saveFileDialog.FileName = $"zoo_export_{timestamp}.json";

            bool? dialogResult = saveFileDialog.ShowDialog();
            bool userClickedSave = dialogResult == true;

            if (userClickedSave)
            {
                try
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.WriteIndented = true;

                    string json = JsonSerializer.Serialize(dataToExport, GetJsonOptions());
                    File.WriteAllText(saveFileDialog.FileName, json);

                    string successMessage = $"Successfully exported {dataToExport.Count} animal(s) to:\n{saveFileDialog.FileName}";
                    MessageBox.Show(successMessage, "Export Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Error exporting data: {ex.Message}";
                    MessageBox.Show(errorMessage, "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private List<Animal> GetExportData()
        {
            bool shouldExportSearchResults = ExportSearchRadio.IsChecked == true;

            if (shouldExportSearchResults)
            {
                IEnumerable<Animal> searchResults = SearchResultsListBox.ItemsSource as IEnumerable<Animal>;

                if (searchResults != null)
                {return searchResults.ToList();}

                return new List<Animal>();
            }
            return animalsList.ToList();
        }

        private string GetComboBoxValue(ComboBox comboBox)
        {
            ComboBoxItem item = comboBox.SelectedItem as ComboBoxItem;
            return item?.Content.ToString();
        }
    }
}