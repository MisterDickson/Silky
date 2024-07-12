using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Silky
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            FromLayerListView.Items.Add("Part Value");
            FromLayerListView.Items.Add("Part Reference");
        }

        private void SaveAllAsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SaveDialog), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
        }


        private async void LoadPCBFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            var window = Intermediate.MainWindow;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".kicad_pcb");

            IReadOnlyList<StorageFile> files = await openPicker.PickMultipleFilesAsync();
            if (files.Count > 0)
            {
                foreach (StorageFile file in files)
                {
                    Intermediate.filePaths.Add(file.Path);
                }

                AddPCBs(Intermediate.filePaths);
            }
        }

        public void AddPCBs(List<string> fileNames)
        {
            foreach (string fullFilePath in fileNames)
            {
                if (Core.FullPaths.Contains(fullFilePath)) continue;

                string pcbDisplayName = Core.AddFile(fullFilePath);

                PCBListView.Items.Add(Intermediate.PrepareListViewItem(pcbDisplayName, fullFilePath));
            }

            foreach (string layerName in Core.LayerNames)
            {
                if (FromLayerListView.Items.Contains(layerName)) continue;
                FromLayerListView.Items.Add(layerName);

                if (ToLayerListView.Items.Contains(layerName)) continue;
                ToLayerListView.Items.Add(layerName);
            }

            foreach (char partAcronym in Core.PartAcronyms)
            {
                if (ApplyToPartListView.Items.Contains(partAcronym)) continue;
                ApplyToPartListView.Items.Add(partAcronym);
            }
        }

        private void RemovePCBFileButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(ListViewItem selectedItem in PCBListView.SelectedItems)
            {
                Intermediate.filePaths.Remove(selectedItem.DataContext.ToString());
            }

            Core.operations.Clear();
            Core.OperationNames.Clear();
            Core.LayerNames.Clear();
            Core.PartAcronyms.Clear();
            Core.PCBFiles.Clear();
            Core.PCBNames.Clear();
            Core.FullPaths.Clear();
            FromLayerListView.Items.Clear();
            ToLayerListView.Items.Clear();
            ApplyToPartListView.Items.Clear();
            OperationsListView.ItemsSource = null;
            OperationsListView.Items.Clear();
            PCBListView.Items.Clear();

            FromLayerListView.Items.Add("Part Value");
            FromLayerListView.Items.Add("Part Reference");

            AddPCBs(Intermediate.filePaths);
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
            OperationsListView.ItemsSource = Core.OperationNames;
        }

        private void RemoveOperationButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (String ListEntree in OperationsListView.SelectedItems)
                Core.RemoveOperation(ListEntree);

            OperationsListView.ItemsSource = null;
            OperationsListView.ItemsSource = Core.OperationNames;
        }
        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> fullTempFilePaths = new List<string>();

            foreach (ListViewItem PCBFileNameItem in PCBListView.Items)
            {
                fullTempFilePaths.Add(Path.GetTempPath() + Core.FullPath(PCBFileNameItem?.Content?.ToString()!)!.Replace(":", "").Replace("\\", "_")); // double wrap needed to avoid double name conflicts
                File.Copy(Core.FullPath(PCBFileNameItem?.Content?.ToString()!)!, fullTempFilePaths.Last(), true);
            }

            Core.ExecuteOperationsOnFiles(fullTempFilePaths);

            foreach (string file in fullTempFilePaths)
            {
                Process.Start("explorer.exe", file);
            }
        }

        private void OverrideAllButton_Click(object sender, RoutedEventArgs e)
        {
            Core.ExecuteOperationsOnFiles(Core.FullPaths);
        }

        private void FromLayerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If the user selects anything other than "Part Value" or "Part Reference" then it does not apply to parts and
            // therefore the "Apply to Part" listview is disabled
            ApplyToPartListView.IsEnabled = FromLayerListView.SelectedIndex < 2;
        }

        private void HandSolderingPreset_Click(object sender, RoutedEventArgs e)
        {
            if (PCBListView.Items.Count < 1) return;

            foreach (String ListEntree in OperationsListView.Items)
                Core.RemoveOperation(ListEntree);

            OperationsListView.ItemsSource = null;
            OperationsListView.ItemsSource = Core.OperationNames;

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

        private void BlankPCBPreset_Click(object sender, RoutedEventArgs e)
        {
            if (PCBListView.Items.Count < 1) return;

            foreach (String ListEntree in OperationsListView.Items)
                Core.RemoveOperation(ListEntree);

            OperationsListView.ItemsSource = null;
            OperationsListView.ItemsSource = Core.OperationNames;

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

        private void HTL10ValuesPreset_Click(object sender, RoutedEventArgs e)
        {
            if (PCBListView.Items.Count < 1) return;

            foreach (String ListEntree in OperationsListView.Items)
                Core.RemoveOperation(ListEntree);

            OperationsListView.ItemsSource = null;
            OperationsListView.ItemsSource = Core.OperationNames;

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

            if (ToLayerListView.Items.IndexOf("F.Cu") < 0)
                ToLayerListView.Items.Add("F.Cu");

            FromLayerListView.SelectedIndex = 0;
            ToLayerListView.SelectedItems.Clear();
            ToLayerListView.SelectedItems.Add("F.Cu");
            ApplyToPartListView.SelectAll();

            AddOperationButton_Click(this, e);
        }

        private void HTL10ReferencePreset_Click(object sender, RoutedEventArgs e)
        {
            if (PCBListView.Items.Count < 1) return;

            foreach (String ListEntree in OperationsListView.Items)
                Core.RemoveOperation(ListEntree);

            OperationsListView.ItemsSource = null;
            OperationsListView.ItemsSource = Core.OperationNames;

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

            if (ToLayerListView.Items.IndexOf("F.Cu") < 0)
                ToLayerListView.Items.Add("F.Cu");

            FromLayerListView.SelectedIndex = 1;
            ToLayerListView.SelectedItems.Clear();
            ToLayerListView.SelectedItems.Add("F.Cu");
            ApplyToPartListView.SelectAll();

            AddOperationButton_Click(this, e);
        }
    }
}
