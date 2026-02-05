using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SFMCodeGenerator.Views
{
    public partial class ManageCommonItemsWindow : Window
    {
        public ObservableCollection<string> CommonLabels { get; }
        public ObservableCollection<string> CommonItemIds { get; }
        public ObservableCollection<string> CommonFluidIds { get; }
        public ObservableCollection<string> CommonEnergyIds { get; }
        public ObservableCollection<string> CommonGasIds { get; }
        
        private readonly Action _onSave;

        public ManageCommonItemsWindow(
            ObservableCollection<string> labels, 
            ObservableCollection<string> itemIds,
            ObservableCollection<string> fluidIds,
            ObservableCollection<string> energyIds,
            ObservableCollection<string> gasIds,
            Action onSave)
        {
            InitializeComponent();
            
            CommonLabels = labels;
            CommonItemIds = itemIds;
            CommonFluidIds = fluidIds;
            CommonEnergyIds = energyIds;
            CommonGasIds = gasIds;
            _onSave = onSave;
            
            LabelsListBox.ItemsSource = CommonLabels;
            ItemIdsListBox.ItemsSource = CommonItemIds;
            FluidIdsListBox.ItemsSource = CommonFluidIds;
            EnergyIdsListBox.ItemsSource = CommonEnergyIds;
            GasIdsListBox.ItemsSource = CommonGasIds;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) => AddNewItem();

        private void NewItemText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) AddNewItem();
        }

        private void AddNewItem()
        {
            var text = NewItemText.Text?.Trim();
            if (string.IsNullOrEmpty(text)) return;

            var selectedItem = TypeSelector.SelectedItem as System.Windows.Controls.ComboBoxItem;
            var type = selectedItem?.Content?.ToString();

            var targetList = type switch
            {
                "标签" => CommonLabels,
                "物品ID" => CommonItemIds,
                "流体ID" => CommonFluidIds,
                "能量ID" => CommonEnergyIds,
                "气体ID" => CommonGasIds,
                _ => null
            };

            if (targetList != null && !targetList.Contains(text))
            {
                targetList.Add(text);
                _onSave();
            }

            NewItemText.Text = "";
            NewItemText.Focus();
        }

        private void DeleteLabel_Click(object sender, RoutedEventArgs e)
        {
            if (LabelsListBox.SelectedItem is string item) { CommonLabels.Remove(item); _onSave(); }
        }

        private void DeleteItemId_Click(object sender, RoutedEventArgs e)
        {
            if (ItemIdsListBox.SelectedItem is string item) { CommonItemIds.Remove(item); _onSave(); }
        }

        private void DeleteFluidId_Click(object sender, RoutedEventArgs e)
        {
            if (FluidIdsListBox.SelectedItem is string item) { CommonFluidIds.Remove(item); _onSave(); }
        }

        private void DeleteEnergyId_Click(object sender, RoutedEventArgs e)
        {
            if (EnergyIdsListBox.SelectedItem is string item) { CommonEnergyIds.Remove(item); _onSave(); }
        }

        private void DeleteGasId_Click(object sender, RoutedEventArgs e)
        {
            if (GasIdsListBox.SelectedItem is string item) { CommonGasIds.Remove(item); _onSave(); }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}
