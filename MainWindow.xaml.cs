using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Silky
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            FromLayerListView.Items.Add("Part Value");
            FromLayerListView.Items.Add("Part Reference");
        }

        private void PCBAddButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openPCBFileDialog = new OpenFileDialog();
            openPCBFileDialog.Filter = "KiCad PCB files (*.kicad_pcb)|*.kicad_pcb";
            openPCBFileDialog.Multiselect = true;

            if (openPCBFileDialog.ShowDialog() != true) return;

            foreach (string fileName in openPCBFileDialog.FileNames)
            {
                Core.AddFile(fileName);
            }
            PCBListView.ItemsSource = Core.PCBNames;

            foreach (string layerName in Core.LayerNames())
            {
                if (FromLayerListView.Items.Contains(layerName)) continue;
                FromLayerListView.Items.Add(layerName);

                if (ToLayerListView.Items.Contains(layerName)) continue;
                ToLayerListView.Items.Add(layerName);
            }

            foreach (char partAcronym in Core.PartAcronyms())
            {
                if (ApplyToPartListView.Items.Contains(partAcronym)) continue;
                ApplyToPartListView.Items.Add(partAcronym);
            }

            PresetComboBox.SelectedIndex = 0; // "No Preset" is selected
        }

        private void PCBRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            EasterEgg easterEgg = new EasterEgg();
            easterEgg.Owner = this;
            easterEgg.ShowDialog();
        }

        private void OverrideButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddOperationButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (String fromLayer in FromLayerListView.SelectedItems)
            {
                foreach (String toLayer in ToLayerListView.SelectedItems)
                {
                    if (ApplyToPartListView.IsEnabled == false)
                    {
                        Core.AddUniqueOperation(fromLayer, toLayer, (char)0);
                        continue;
                    }

                    foreach (char partAcronym in ApplyToPartListView.SelectedItems)
                    {
                        Core.AddUniqueOperation(fromLayer, toLayer, partAcronym);
                    }
                }
            }
            OperationListView.ItemsSource = Core.OperationNames;

            if (sender != this) // If this function was called by the preset combobox. The combobox sends this
                PresetComboBox.SelectedIndex = 0; // "No Preset" is selected
        }

        private void PresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PCBListView.Items.Count < 1) return;
            if (PresetComboBox.SelectedIndex < 1) return;


            foreach (String ListEntree in OperationListView.Items)
                Core.RemoveOperation(ListEntree);

            OperationListView.ItemsSource = null;
            OperationListView.ItemsSource = Core.OperationNames;

            if (PresetComboBox.SelectedIndex == 1) // "Hand Soldering"
            {

                int ToLayerFSilkSIndex = ToLayerListView.Items.IndexOf("F.SilkS");
                int ToLayerFFabIndex = ToLayerListView.Items.IndexOf("F.Fab");

                if (ToLayerFSilkSIndex < 0) ToLayerListView.Items.Add("F.SilkS");
                if (ToLayerFFabIndex < 0) ToLayerListView.Items.Add("F.Fab");

                ApplyToPartListView.SelectAll();

                FromLayerListView.SelectedIndex = 0;
                ToLayerListView.SelectedIndex = ToLayerFSilkSIndex;

                AddOperationButton_Click(this, e);

                FromLayerListView.SelectedIndex = 1;
                ToLayerListView.SelectedIndex = ToLayerFFabIndex;

                AddOperationButton_Click(this, e);
            }
            else if (PresetComboBox.SelectedIndex == 2) // "Blank PCB"
            {
                int FromLayerFSilkSIndex = FromLayerListView.Items.IndexOf("F.SilkS");
                int FromLayerBSilkSIndex = FromLayerListView.Items.IndexOf("B.SilkS");
                int ToLayerFFabIndex = ToLayerListView.Items.IndexOf("F.Fab");

                if (FromLayerFSilkSIndex < 0) FromLayerListView.Items.Add("F.SilkS");
                if (FromLayerBSilkSIndex < 0) FromLayerListView.Items.Add("B.SilkS");
                if (ToLayerFFabIndex < 0) ToLayerListView.Items.Add("F.Fab");

                FromLayerListView.SelectedItems.Clear();
                ToLayerListView.SelectedItems.Clear();

                FromLayerListView.SelectedItems.Add("F.SilkS");
                FromLayerListView.SelectedItems.Add("B.SilkS");

                ToLayerListView.SelectedItems.Add("F.Fab");
                
                AddOperationButton_Click(this, e);
            }

            
        }

        private void FromLayerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If the user selects anything other than "Part Value" or "Part Reference" then it does not apply to parts and
            // therefore the "Apply to Part" listview is disabled
            ApplyToPartListView.IsEnabled = FromLayerListView.SelectedIndex < 2;
        }

        private void RemoveOperationButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (String ListEntree in OperationListView.SelectedItems)
                Core.RemoveOperation(ListEntree);

            OperationListView.ItemsSource = null;
            OperationListView.ItemsSource = Core.OperationNames;

            PresetComboBox.SelectedIndex = 0; // "No Preset" is selected
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Core.PCBNames.ElementAt(PCBListView.SelectedIndex);
            string tempFilePath = Path.GetTempFileName();



            string path = Core.operations.ElementAt(0).Execute("C:\\Users\\Ari\\Desktop\\analog_board.kicad_pcb");

            // delete the original file
            File.Delete(filePath);

            // rename the temporary file to the original file name
            File.Move(tempFilePath, filePath);

            MessageBox.Show(path);
        }
    }
}
