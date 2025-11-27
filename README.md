# Zoo Manager Application

## Overview
A comprehensive WPF application for managing zoo animals with a modern, responsive interface.

## Features Implemented

### ? Add New Animal
- **Form Fields**:
  - ID (TextBox) - Unique identifier
  - Species/Name (TextBox)
  - Habitat (ComboBox) - Loaded from predefined list (Savannah, Rainforest, Desert, Aquatic, Mountain, Grassland, Arctic)
  - Arrival Date (DatePicker)
  - Health Status (ComboBox) - Excellent, Good, Fair, Poor
  - Feeding Status (CheckBox) - Is Fed
- **Validation**: Ensures all fields are filled and IDs are unique
- **Data Persistence**: Saves to `animals.json`

### ?? Search Functionality
- **Search by Multiple Criteria** using RadioButtons:
  - **ID**: Direct ID lookup
  - **Species**: Text search (partial match)
  - **Habitat**: Dropdown selection from available habitats
  - **Health Status**: Dropdown selection
  - **Feeding Status**: Checkbox filter for fed/not fed animals
- **Live Search Results**: Updates dynamically as you type/select
- **Interactive Results**: Click on search results to load animal for modification

### ?? View All Animals
- **Two Display Modes**:
  1. **List View**: Structured ListBox with formatted templates
  2. **Labels View**: Simple label-based display
- **Sorting Options**:
  - Sort by ID
  - Sort by Species
  - Sort by Habitat
  - Sort by Arrival Date
- **Interactive**: Click on any animal to load it for modification

### ?? Modify & Delete
- **Modify**: 
  - Select animal from search or view sections
  - Edit all fields except ID (read-only)
  - Save changes with validation
- **Delete**: 
  - Select animal
  - Confirmation dialog before deletion
  - Updates all views automatically

### ?? Export Functionality
- **Export to JSON**: Export all animals in current sort order
- **File Dialog**: Choose custom location and filename
- **Timestamped Default**: Auto-generates filename with timestamp

### ?? UI/UX Features
- **Responsive Design**: 
  - Adjusts layout based on window size
  - ScrollViewer for overflow content
  - Minimum window size enforced
- **Collapsible Sections**: Expanders for each major feature
- **Header Bar**:
  - Logo (??) on the left
  - Title in center
  - Real-time clock (HH:mm:ss) on the right
- **Color Scheme**: Nature-themed green palette
- **Modern Controls**: Rounded corners, proper spacing, consistent styling

## Data Structure

### Animal Class
```csharp
public class Animal
{
    public int id { get; set; }
    public string species { get; set; }
    public string habitat { get; set; }
    public string arrival_date { get; set; }
    public string health_status { get; set; }
    public bool is_fed { get; set; }
}
```

## Data Source
- **JSON File**: `animals.json` in application directory
- **Format**: Array of Animal objects
- **Auto-created**: File created automatically if missing
- **Auto-saved**: Updates saved immediately on add/modify/delete

## Technical Requirements Met

### ? Adatfelvitel (Data Input)
- ? TextBox (ID, Species)
- ? ComboBox with data from file (Habitat, Health Status)
- ? CheckBox (Feeding Status)
- ? DatePicker (Arrival Date)

### ? Adatmegjelenítés (Data Display)
- ? ListBox with custom template
- ? Labels in StackPanel

### ? Additional Features
- ? Modify functionality
- ? Delete functionality with confirmation
- ? Search with multiple criteria and RadioButtons
- ? Export to JSON with sorting
- ? Responsive design
- ? Real-time clock
- ? Collapsible menus

## Usage Instructions

1. **Start Application**: Application opens directly to main window
2. **Add Animals**: Use "Add New Animal" section
3. **Search**: Expand "Search Animals", select criteria, search
4. **View All**: Expand "View All Animals", choose display mode
5. **Modify/Delete**: Select animal from any list, use "Modify/Delete" section
6. **Export**: Click "Export to JSON" in view section

## Future Enhancements (Optional)
- Import from external JSON files
- Advanced filtering (multiple criteria simultaneously)
- Animal images/photos
- Veterinary records
- Feeding schedules
- Reports and statistics
