using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Shell;

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
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void BackToMainFromSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if(Frame.CanGoBack) Frame.GoBack(new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void SaveCopiesButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> savePaths = new List<string>();
            string textBoxPath = PathTextBox.Text.Replace("/", @"\" /*le windows*/).Replace(@"\\", @"\").Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");

            if (textBoxPath == "") textBoxPath = PathTextBox.PlaceholderText;

            if (textBoxPath.LastIndexOf(@"\") > textBoxPath.LastIndexOf("*") || !textBoxPath.Contains("*"))
            { textBoxPath = textBoxPath + "*_edited"; }

            if (textBoxPath.Contains(":")) // absulute path
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

                foreach (string pcbName in Core.PCBNames)
                {
                    string savePath = textBoxPath.Replace("/", @"\").Replace(@"\\", @"\").Replace("*", pcbName) + ".kicad_pcb";
                    savePaths.Add(savePath);

                    try
                    {
                        File.Copy(Core.FullPath(pcbName)!, savePath, true);
                        ListViewItem successfulLog = Intermediate.PrepareListViewItem(Path.GetFileName(savePath), savePath);

                        successfulLog.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 223, 246, 221));

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

                foreach (string pcbPath in Core.FullPaths)
                {
                    string savePath = (System.IO.Path.GetDirectoryName(pcbPath) + @"\" + textBoxPath).Replace("/", @"\").Replace(@"\\", @"\").Replace("*", System.IO.Path.GetFileNameWithoutExtension(pcbPath)) + ".kicad_pcb";
                    // creatung every missing directory
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
                List<string> alreadyOpenedFolders = new List<string>();

                foreach (string file in savePaths)
                {
                    if (alreadyOpenedFolders.Contains(System.IO.Path.GetDirectoryName(file)!)) continue;

                    Process.Start("explorer.exe", "/select, \"" + file + "\"");
                    alreadyOpenedFolders.Add(System.IO.Path.GetDirectoryName(file)!);
                }
            }
            SavedFileLogListView.ScrollIntoView(SavedFileLogListView.Items.ElementAt(SavedFileLogListView.Items.Count - 1));
        }
    }
}
