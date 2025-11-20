using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System;

namespace Zoo
{
    public partial class MainWindow : Window
    {
        private List<Animal> animals = new List<Animal>();
        private const string JsonPath = "animals.json";

        public MainWindow()
        {
            InitializeComponent();
            LoadAnimals();
        }

        private void LoadAnimals()
        {
                string json = File.ReadAllText(JsonPath);
                animals = JsonSerializer.Deserialize<List<Animal>>(json);
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
                ArrivalDatePicker.SelectedDate != null)
            {
                animals.Add(new Animal
                {
                    id = id,
                    species = NameTextBox.Text,
                    habitat = selectedHabitat.Content.ToString(),
                    arrival_date = ArrivalDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd")
                });
                SaveAnimals();
                
                MessageBox.Show("Animal added.", "Success");
                IdTextBox.Text = "";
                NameTextBox.Text = "";
                HabitatComboBox.SelectedIndex = -1;
                ArrivalDatePicker.SelectedDate = null;
            }
            else
            {
                MessageBox.Show("TÖLTSD KI HELYESEN TE KIS GECI, NEHOGY KIHAGYJ BÁRMIT IS", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowListButton_Click(object sender, RoutedEventArgs e)
        {
            var listBox = new ListBox
            {
                ItemsSource = animals,
                Margin = new Thickness(0, 10, 0, 0),
                ItemTemplate = (DataTemplate)FindResource("AnimalListTemplate")
            };
            DataDisplay.Content = listBox;
        }

        private void ShowLabelsButton_Click(object sender, RoutedEventArgs e)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };
            foreach (var animal in animals)
            {
                var label = new Label
                {
                    Content = $"ID: {animal.id}, Species: {animal.species}, Habitat: {animal.habitat}, Arrival: {animal.arrival_date}",
                    FontSize = 14,
                    Foreground = System.Windows.Media.Brushes.DarkSlateGray,
                    Margin = new Thickness(0, 2, 0, 2)
                };
                panel.Children.Add(label);
            }
            DataDisplay.Content = panel;
        }
    }
}

//TODO: módosítás valahogy, törlés, keresés?, lekérdezés, valahova valami helyére radiobutton vagy checkboc (talán ha lekérdezünk hogy mit akarunk lekérdezni)
//      design propertiesben megcsinálni (RAHHHHHHHHHHHHHHHHH)