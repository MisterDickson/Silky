using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            foreach (string PCBName in Core.PCBNames)
            {
                ListViewItem item = new ListViewItem();
                item.Content = PCBName;

                PCBListView.Items.Add(item);
            }

            foreach (ListViewItem item in PCBListView.Items)
            {
                string fileName = item.Content.ToString();
                string filePath = Core.FullPath(fileName);

                item.ToolTip = filePath;
                item.MouseDoubleClick += PCBName_DoubleClick;
            }

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

        private void PCBName_DoubleClick(object sender, RoutedEventArgs e)
        {
            string fileName = ((ListViewItem)sender).Content.ToString();
            string filePath = Core.FullPath(fileName);

            Process.Start("explorer.exe", "/select, \"" + filePath + "\"");
        }

        private void PCBRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            EasterEgg easterEgg = new EasterEgg();
            easterEgg.Owner = this;
            easterEgg.ShowDialog();
        }

        private void OverrideButton_Click(object sender, RoutedEventArgs e)
        {
            Core.ExecuteOperationsOnFiles(Core.FullPaths());
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveAsDialog saveAsDialog = new SaveAsDialog();
            saveAsDialog.Owner = this;
            saveAsDialog.ShowDialog();
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

            PCBListView.SelectAll();
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
            List<string> fullTempFilePaths = new List<string>();

            foreach (ListViewItem PCBFileNameItem in PCBListView.SelectedItems)
            {
                fullTempFilePaths.Add(Path.GetTempPath() + PCBFileNameItem.Content + ".kicad_pcb");
                File.Copy(Core.FullPath(PCBFileNameItem.Content.ToString()), fullTempFilePaths.Last(), true);
            }

            Core.ExecuteOperationsOnFiles(fullTempFilePaths);

            foreach (string file in fullTempFilePaths)
            {
                Process.Start("explorer.exe", file);
            }
        }
    }
}
