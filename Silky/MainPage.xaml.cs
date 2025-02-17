using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Silky
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        SilkyCore Core;
        ListView LastFocussedPCBorOperationsListView; // To target the Delete Key action

        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            Core = new SilkyCore(PCBListView);
            LastFocussedPCBorOperationsListView = PCBListView;
            PresetTextBlock.Text = Intermediate.PresetText.Default;
            LoadCommandLineArgPCBs();
        }

        public async void LoadCommandLineArgPCBs()
        {
            int len = Environment.GetCommandLineArgs().Length;
            if (len < 2) return;

            string[] args = Environment.GetCommandLineArgs();

            for (int i = 1; i < len; i++)
            {
                if (!args[i].EndsWith(".kicad_pcb")) continue;

                StorageFile file = await StorageFile.GetFileFromPathAsync(args[i]);
                if (file is null) continue;

                AddUniquePCBFile(file);
            }
            DisplayLayersAndParts();
        }

        private void SaveAllAsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SaveDialog), Core, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Delete)
            {
                if (LastFocussedPCBorOperationsListView == PCBListView)
                    RemovePCBFileButton_Click(sender, e);
                else if (LastFocussedPCBorOperationsListView == OperationsListView)
                    RemoveOperationButton_Click(sender, e);
            }
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

            if (files.Count < 1) return;

            foreach (StorageFile file in files)
            {
                AddUniquePCBFile(file);
            }

            DisplayLayersAndParts();
        }

        /// <summary>
        /// Adds a ListViewItem with DataContext to PCBListView.
        /// The Content is the File Name, the DataContext is the Full Path.
        /// If the very File is already added to PCBListView nothing happens.
        /// If there is an equally named File with a different path to load,
        /// its full path is added in paranthesis next to its Name to avoid confusion.
        /// </summary>
        /// <param name="file"></param>
        public void AddUniquePCBFile(StorageFile file)
        {
            // As of right now v8.x files should not be mixed with older versions
            bool versionConflict = false;
            using (var sr = new StreamReader(file.Path))
            {
                string line;
                bool version8 = false;
                while ((line = sr.ReadLine()!) != null)
                {
                    if (line.Contains("(generator_version \"8"))
                    {
                        version8 = true;
                        if (SilkyCore.KiCadVersion > 0 && SilkyCore.KiCadVersion < 8) versionConflict = true;
                        else SilkyCore.KiCadVersion = 8;
                        break;
                    }
                }
                if (SilkyCore.KiCadVersion == 8 && !version8) versionConflict = true;
                else if (SilkyCore.KiCadVersion == 0) SilkyCore.KiCadVersion = 7;
            }

            if (versionConflict)
            {
                ContentDialog dialog = new ContentDialog();

                // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                dialog.XamlRoot = this.XamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.Title = "Version Conflict";
                dialog.Content = "PCB Files of Version 8 and newer are not compatible with Version 7 and older Releases of KiCad. Please streamline the File Versions or proceed with one Version at the Time.";
                dialog.PrimaryButtonText = "OK";
                dialog.DefaultButton = ContentDialogButton.Primary;
                _ = dialog.ShowAsync();

                return;
            }

            // Add the path to the displayed name similar named file is already loaded 
            string contains = ContainsPCBFile(file.Name);
            if (contains == null)
            {
                PCBListView.Items.Add(Intermediate.PrepareListViewItem(file.Name, file.Path));
                return;
            }

            if (contains != file.Path) // Files with the same name but different paths
                PCBListView.Items.Add(Intermediate.PrepareListViewItem(file.Name + " (" + file.Path + ")", file.Path));
        }

        public void DisplayLayersAndParts()
        {
            if (PCBListView.Items.Count < 1) return;

            if (!FromLayerListView.Items.Contains("Part Value"))
                FromLayerListView.Items.Add("Part Value");
            if (!FromLayerListView.Items.Contains("Part Reference"))
                FromLayerListView.Items.Add("Part Reference");

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

        /// <summary>
        /// Check if File is already loaded with consideration of Same Name files with different Paths
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>null if there is no occurance whatsoever. if there is an occurance the full path is returned,
        /// so the caller can check if it is a different path or a exact duplicate</returns>
        public string ContainsPCBFile(string fileName)
        {
            foreach (ListViewItem listViewItem in PCBListView.Items)
            {
                if (listViewItem.Content.ToString() == fileName)
                    return listViewItem.DataContext.ToString();
            }
            return null;
        }
        private void RemovePCBFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (PCBListView.SelectedItems.Count < 1) return;

            List<int> selectedItems = new List<int>();
            int i = 0;
            foreach (ListViewItem listViewItem in PCBListView.Items)
            {
                if (listViewItem.IsSelected)
                    selectedItems.Add(i);
                i++;
            }
            for (i = selectedItems.Count - 1; i >= 0; i--) PCBListView.Items.RemoveAt(selectedItems[i]);

            FromLayerListView.Items.Clear();
            ToLayerListView.Items.Clear();
            ApplyToPartListView.Items.Clear();
            Core.operations.Clear();
            OperationsListView.ItemsSource = null;
            OperationsListView.ItemsSource = Core.OperationNames;
            DisplayLayersAndParts();

            if (PCBListView.Items.Count < 1)
            {
                PresetTextBlock.Text = Intermediate.PresetText.Default;
                PresetFontIcon.Glyph = "\uE8FD";

                SilkyCore.KiCadVersion = 0;
            }
        }

        private void AddOperationButton_Click(object sender, RoutedEventArgs e)
        {
            bool somethingChanged = false;
            foreach (string fromLayer in FromLayerListView.SelectedItems)
            {
                foreach (string toLayer in ToLayerListView.SelectedItems)
                {
                    if (ApplyToPartListView.IsEnabled == false)
                    {
                        somethingChanged = Core.AddUniqueOperation(fromLayer, toLayer, (char)0);
                        continue;
                    }

                    foreach (char partAcronym in ApplyToPartListView.SelectedItems)
                    {
                        somethingChanged = Core.AddUniqueOperation(fromLayer, toLayer, partAcronym);
                    }
                }
            }
            OperationsListView.ItemsSource = Core.OperationNames;

            if (somethingChanged && PresetTextBlock.Text is not Intermediate.PresetText.Default && !PresetTextBlock.Text.Contains(Intermediate.PresetText.Modified))
                PresetTextBlock.Text += Intermediate.PresetText.Modified;

        }

        private void RemoveOperationButton_Click(object sender, RoutedEventArgs e)
        {
            if (OperationsListView.SelectedItems.Count < 1) return;

            foreach (string ListEntry in OperationsListView.SelectedItems)
                Core.RemoveOperation(ListEntry);

            OperationsListView.ItemsSource = null;
            OperationsListView.ItemsSource = Core.OperationNames;

            if (PresetTextBlock.Text is not Intermediate.PresetText.Default && !PresetTextBlock.Text.Contains(Intermediate.PresetText.Modified))
                PresetTextBlock.Text += Intermediate.PresetText.Modified;
        }
        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> fullTempFilePaths = new List<string>();

            foreach (ListViewItem PCBListViewEntry in PCBListView.Items)
            {
                fullTempFilePaths.Add(Path.GetTempPath() + PCBListViewEntry.DataContext.ToString().Replace(":", "").Replace("\\", "_")); // double wrap needed to avoid double name conflicts
                File.Copy(PCBListViewEntry.DataContext.ToString(), fullTempFilePaths.Last(), true);
            }

            Core.ExecuteOperationsOnFiles(fullTempFilePaths);

            foreach (string file in fullTempFilePaths)
            {
                Process.Start("explorer.exe", file);
            }
        }

        private void OverrideAllButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> allPaths = new List<string>();

            foreach (ListViewItem PCBListViewEntry in PCBListView.Items)
                allPaths.Add(PCBListViewEntry.DataContext.ToString());

            Core.ExecuteOperationsOnFiles(allPaths);
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

            foreach (string ListEntree in OperationsListView.Items)
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

            PresetTextBlock.Text = Intermediate.PresetText.HandSoldering;
            PresetFontIcon.Glyph = "\uE929";
        }

        private void BlankPCBPreset_Click(object sender, RoutedEventArgs e)
        {
            if (PCBListView.Items.Count < 1) return;

            foreach (string ListEntree in OperationsListView.Items)
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

            PresetTextBlock.Text = Intermediate.PresetText.BlankPCB;
            PresetFontIcon.Glyph = "\uE7C4";
        }

        private void HTL10ValuesPreset_Click(object sender, RoutedEventArgs e)
        {
            if (PCBListView.Items.Count < 1) return;

            foreach (string ListEntry in OperationsListView.Items)
                Core.RemoveOperation(ListEntry);

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

            PresetTextBlock.Text = Intermediate.PresetText.HTL10Values;
            PresetFontIcon.Glyph = "\uEEA3";
        }

        private void HTL10ReferencePreset_Click(object sender, RoutedEventArgs e)
        {
            if (PCBListView.Items.Count < 1) return;

            foreach (string ListEntry in OperationsListView.Items)
                Core.RemoveOperation(ListEntry);

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

            PresetTextBlock.Text = Intermediate.PresetText.HTL10References;
            PresetFontIcon.Glyph = "\uE8B3";
        }

        private async void Page_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();

                foreach (var item in items)
                {
                    StorageFile storageFile = item as StorageFile;
                    if (storageFile is not null && storageFile.FileType == ".kicad_pcb")
                        AddUniquePCBFile(storageFile);
                }

                DisplayLayersAndParts();
            }
        }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.DragUIOverride.Caption = "Drop to load the file";
                e.DragUIOverride.IsContentVisible = true;
                e.DragUIOverride.IsCaptionVisible = true;
            }
            else
            {
                e.DragUIOverride.Caption = "Invalid file type";
                e.DragUIOverride.IsCaptionVisible = true;
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }

        private void PCBListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LastFocussedPCBorOperationsListView = PCBListView;
        }

        private void OperationsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LastFocussedPCBorOperationsListView = OperationsListView;
        }

        private void Page_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsXButton2Pressed)
            {
                if (Frame.CanGoForward) Frame.GoForward();
            }
        }
    }
}
