using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Silky
{
   /// <summary>
   /// An empty page that can be used on its own or navigated to within a Frame.
   /// </summary>
   public sealed partial class SaveDialog : Page
   {
      public SaveDialog()
      {
         InitializeComponent();
         NavigationCacheMode = NavigationCacheMode.Enabled;
      }

      public void AdjustUIGrammar()
      {
         OpenFilesAfterSave.Content = "Open Files after saving";
         OpenFoldersAfterSave.Content = "Open Folders after saving";

         if (Core is null) return;

         if (Core.Source.Items.Count == 1)
         {
            OpenFilesAfterSave.Content = "Open File after saving";
            OpenFoldersAfterSave.Content = "Open Folder after saving";
         }
         if (Core.Source.Items.Count > 1)
         {
            OpenFilesAfterSave.Content = "Open Files after saving";

            ListViewItem firstEntry = Core.Source.Items[0] as ListViewItem;
            string firstDirectory = Path.GetDirectoryName(firstEntry.DataContext.ToString());

            for (int i = 1; i < Core.Source.Items.Count; i++)
            {
               ListViewItem otherEntry = Core.Source.Items[i] as ListViewItem;
               string otherDirectory = Path.GetDirectoryName(otherEntry.DataContext.ToString());
               if (firstDirectory != otherDirectory)
               {
                  OpenFoldersAfterSave.Content = "Open Folders after saving";
                  return;
               }
            }

            OpenFoldersAfterSave.Content = "Open Folder after saving";
         }
      }

      SilkyCore Core;
      protected override void OnNavigatedTo(NavigationEventArgs e)
      {
         Core = e.Parameter as SilkyCore;
         base.OnNavigatedTo(e);

         ExportHTL10FabFilesCheckBox.IsChecked = Intermediate.PresetText.CurrentPreset.Contains("HTL");

         AdjustUIGrammar();
      }

      private void BackToMainFromSaveButton_Click(object sender, RoutedEventArgs e)
      {
         if (Frame.CanGoBack) Frame.GoBack(new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
      }

      private void SaveCopiesButton_Click(object sender, RoutedEventArgs e)
      {
         if (Core.operations.Count < 1) return;

         List<string> savePaths = [];
         string textBoxPath = PathTextBox.Text.Replace("/", @"\" /*le windows*/).Replace(@"\\", @"\").Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");

         if (textBoxPath == "") textBoxPath = PathTextBox.PlaceholderText;

         if (textBoxPath.LastIndexOf('\\') > textBoxPath.LastIndexOf('*') || !textBoxPath.Contains('*'))
         { textBoxPath += "*_edited"; }

         if (textBoxPath.Contains(':')) // absulute path
         {
            // creating every missing directory
            string[] directories = textBoxPath.Split('\\').SkipLast(1).ToArray();
            string path = "";
            foreach (string directory in directories)
            {
               path += directory + "\\";
               if (!Directory.Exists(path))
               {
                  try
                  {
                     Directory.CreateDirectory(path);
                  }
                  catch (Exception ex)
                  {
                     SavedFileLogListView.Items.Add(Intermediate.PrepareListViewItem("Something went terribly wrong", "", ex));
                  }
               }
            }

            foreach (ListViewItem PCBListViewEntry in Core.Source.Items.Cast<ListViewItem>())
            {
               string savePath = textBoxPath.Replace("/", @"\").Replace(@"\\", @"\").Replace("*", PCBListViewEntry.Content.ToString()) + ".kicad_pcb";
               savePaths.Add(savePath);

               try
               {
                  File.Copy(PCBListViewEntry.DataContext.ToString(), savePath, true);
                  ListViewItem successfulLog = Intermediate.PrepareListViewItem(Path.GetFileName(savePath), savePath);
                  successfulLog.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 223, 246, 221));
                  successfulLog.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 26, 26, 26));
                  if (ExportHTL10FabFilesCheckBox.IsChecked == true) ExportHTL10FabFiles(savePath);

                  SavedFileLogListView.Items.Add(successfulLog);
               }
               catch (Exception ex)
               {
                  SavedFileLogListView.Items.Add(Intermediate.PrepareListViewItem(Path.GetFileName(savePath), savePath, ex));
               }
            }
         }
         else
         {
            if (textBoxPath.ElementAt(0) != '\\')
            { textBoxPath = @"\" + textBoxPath; }

            foreach (ListViewItem PCBListViewEntry in Core.Source.Items.Cast<ListViewItem>())
            {
               string pcbPath = PCBListViewEntry.DataContext.ToString();
               string savePath = (System.IO.Path.GetDirectoryName(pcbPath) + @"\" + textBoxPath).Replace("/", @"\").Replace(@"\\", @"\").Replace("*", System.IO.Path.GetFileNameWithoutExtension(pcbPath)) + ".kicad_pcb";
               // creating every missing directory
               string[] directories = System.IO.Path.GetDirectoryName(textBoxPath.Replace("*", System.IO.Path.GetFileNameWithoutExtension(pcbPath))! + ".kicad_pcb")!.Split('\\');
               string path = System.IO.Path.GetDirectoryName(pcbPath)!;
               foreach (string directory in directories)
               {
                  path += directory + "\\";
                  if (!Directory.Exists(path))
                  {
                     Directory.CreateDirectory(path);
                  }
               }
               savePaths.Add(savePath);

               try
               {
                  File.Copy(pcbPath, savePath, true);
                  ListViewItem successfulLog = Intermediate.PrepareListViewItem(Path.GetFileName(savePath), savePath);
                  successfulLog.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 223, 246, 221));
                  successfulLog.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 26, 26, 26));

                  if (ExportHTL10FabFilesCheckBox.IsChecked == true) ExportHTL10FabFiles(savePath);

                  SavedFileLogListView.Items.Add(successfulLog);
               }
               catch (Exception ex)
               {
                  SavedFileLogListView.Items.Add(Intermediate.PrepareListViewItem(Path.GetFileName(savePath), savePath, ex));
               }

            }
         }

         Core.ExecuteOperationsOnFiles(savePaths);

         if (OpenFilesAfterSave.IsChecked == true)
         {
            foreach (string file in savePaths) Process.Start("explorer.exe", file);
         }

         if (OpenFoldersAfterSave.IsChecked == true)
         {
            List<string> alreadyOpenedFolders = [];

            foreach (string file in savePaths)
            {
               if (alreadyOpenedFolders.Contains(System.IO.Path.GetDirectoryName(file)!)) continue;

               Process.Start("explorer.exe", "/select, \"" + file + "\"");
               alreadyOpenedFolders.Add(System.IO.Path.GetDirectoryName(file)!);
            }
         }

         SavedFileLogListView.ScrollIntoView(SavedFileLogListView.Items.ElementAt(SavedFileLogListView.Items.Count - 1));
      }

      private void Page_PointerPressed(object sender, PointerRoutedEventArgs e)
      {
         if (e.GetCurrentPoint(this).Properties.IsXButton1Pressed) BackToMainFromSaveButton_Click(sender, e);
      }

      public void ExportHTL10FabFiles(string filePath)
      {
         // 1) Try to find kicad-cli.exe by looking through some common paths
         // 2) If unsuccessful ask the user to locate it
         // 3) If still unsuccessful fuck it

         if (Intermediate.localSettings.Values["cliPath"] is null || !Path.Exists(Intermediate.localSettings.Values["cliPath"].ToString())) // 1)
         {
            for (int commonVersionNumber = 9; commonVersionNumber >= 4; commonVersionNumber--)
            {
               string commonPath = "C:\\Program Files\\KiCad\\"+commonVersionNumber+".0\\bin\\kicad-cli.exe";
               if (Path.Exists(commonPath))
               {
                  Intermediate.localSettings.Values["cliPath"] = commonPath;
                  break;
               }
            }
         }

         if (Intermediate.localSettings.Values["cliPath"] is null || !Path.Exists(Intermediate.localSettings.Values["cliPath"].ToString())) // 2)
            PickKiCadDialog();

         if (Intermediate.localSettings.Values["cliPath"] is null || !Path.Exists(Intermediate.localSettings.Values["cliPath"].ToString())) return; // 3)

         string saveDirectory = Path.GetDirectoryName(filePath) + "\\" + Path.GetFileNameWithoutExtension(filePath) + "-" + DateAndTime.Now.ToString().Replace(':', '-');
         Directory.CreateDirectory(saveDirectory);
         Directory.SetCurrentDirectory(saveDirectory);

         string bCuExportCommand = "pcb export svg -o \""+saveDirectory+"\\"+ Path.GetFileNameWithoutExtension(filePath)+"-Bottom.svg\" --layers B.Cu -n --black-and-white --page-size-mode 2 --exclude-drawing-sheet " + filePath;
         string fCuExportCommand = "pcb export svg -o \"" + saveDirectory + "\\"+Path.GetFileNameWithoutExtension(filePath)+"-Top.svg\" --layers F.Cu -n --black-and-white --page-size-mode 2 --exclude-drawing-sheet " + filePath;
         string drillExportCommand = "pcb export drill --drill-origin plot --excellon-zeros-format suppressleading -u in --excellon-min-header " + filePath;

         Process.Start(Intermediate.localSettings.Values["cliPath"].ToString(), bCuExportCommand);
         Process.Start(Intermediate.localSettings.Values["cliPath"].ToString(), fCuExportCommand);
         Process.Start(Intermediate.localSettings.Values["cliPath"].ToString(), drillExportCommand).WaitForExit();

         string sourceFile = Path.GetFileNameWithoutExtension(filePath) + ".drl";
         string toolInfoFile = Path.GetFileNameWithoutExtension(filePath) + "-Bohrdurchmesser.txt";
         string excellonFile = Path.GetFileNameWithoutExtension(filePath) + ".exc";

         using (StreamReader reader = new StreamReader(sourceFile))
         {
            string line;
            using (StreamWriter writer = new StreamWriter(toolInfoFile))
            {
               while ((line = reader.ReadLine()) != null)
               {
                  int tool_number = 1;

                  if (line[0] != 'T') continue;
                  if (line == "T1") break;

                  int seperator_index = line.IndexOf('C'); // index of C in T12C0.0135 for example
                  double metric_diameter = double.Parse(line[(seperator_index + 1)..], System.Globalization.CultureInfo.InvariantCulture) * 25.4;

                  writer.WriteLine("T" + tool_number + string.Format(": {0:F2}", metric_diameter) + (metric_diameter < 0.8 ? " mm - Nicht im Sortiment (< 0.8 mm)" : " mm"));
                  tool_number++;
               }
            }
            using (StreamWriter writer = new StreamWriter(excellonFile))
            {
               do
               {
                  if (line.Contains("X-"))
                     line = line.Replace("X-", "X");
                  writer.WriteLine(line);
               } while ((line = reader.ReadLine()) != null);
            }
         }

         File.Delete(sourceFile);
      }

      private async void PickKiCadDialog()
      {
         ContentDialog subscribeDialog = new ContentDialog
         {
            Title = "KiCad CLI not found",
            Content = "Please manually select the Location of your KiCad CLI installtion. The Path will be saved for Future Sessions.",
            CloseButtonText = "Cancel Fabrication Export",
            PrimaryButtonText = "Select CLI Path",
            DefaultButton = ContentDialogButton.Primary
         };
         subscribeDialog.XamlRoot = XamlRoot;
         ContentDialogResult result = await subscribeDialog.ShowAsync();

         if (result != ContentDialogResult.Primary) return;

         var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
         var window = Intermediate.MainWindow;
         var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
         WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

         openPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
         openPicker.FileTypeFilter.Add(".exe");

         StorageFile cliFile = await openPicker.PickSingleFileAsync();

         Intermediate.localSettings.Values["cliPath"] = cliFile.Path;
      }
   }
}
