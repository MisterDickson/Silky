using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;

namespace Silky
{
    /// <summary>
    /// Interaction logic for SaveAsDialog.xaml
    /// </summary>
    public partial class SaveAsDialog : Window
    {
        public SaveAsDialog()
        {
            InitializeComponent();
            SavePathTextBox.Focus();
            SavePathTextBox.SelectAll();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            EasterEgg ErrorMessage = new EasterEgg();
            ErrorMessage.Owner = this;

            List<string> savePaths = new List<string>();
            string textBoxPath = SavePathTextBox.Text.Replace("/", @"\" /*le windows*/).Replace(@"\\", @"\").Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");

            if (textBoxPath == "")
            { ErrorMessage.ShowDialog("Looks like the provided path is too short to make sense."); SavePathTextBox.Focus(); return; }

            // count the occurances of colons in textBoxPath
            if(textBoxPath.Length - textBoxPath.Replace(":", "").Length > 1)
            { ErrorMessage.ShowDialog("Looks like you have too many colons in your path."); SavePathTextBox.Focus(); return; }
            
            if (textBoxPath.LastIndexOf(@"\") > textBoxPath.LastIndexOf("*") || !textBoxPath.Contains("*"))
            { textBoxPath = textBoxPath + "*_edited"; }

            if (textBoxPath.Contains(":")) // absulute path
            {
                if (!Directory.Exists(textBoxPath.Substring(0, textBoxPath.IndexOf(@"\")+1)))
                { ErrorMessage.ShowDialog("How about a path that exists?"); SavePathTextBox.Focus(); return; }

                // creating every missing directory
                string[] directories = textBoxPath.Split('\\').SkipLast(1).ToArray();
                string path = "";
                foreach (string directory in directories)
                {
                    path += directory + "\\";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }

                foreach (string pcbName in Core.PCBNames)
                {
                    string savePath = textBoxPath.Replace("/", @"\").Replace(@"\\",@"\").Replace("*", pcbName) + ".kicad_pcb";
                    savePaths.Add(savePath);
                    File.Copy(Core.FullPath(pcbName)!, savePath, true);
                }
            }
            else
            {
                if (textBoxPath.ElementAt(0) != '\\')
                { textBoxPath = @"\" + textBoxPath; }

                foreach (string pcbPath in Core.FullPaths())
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
                    File.Copy(pcbPath, savePath, true);
                }
            }

            Core.ExecuteOperationsOnFiles(savePaths);

            if (OpenFileAfterSave.IsChecked == true)
            {
                foreach (string file in savePaths) Process.Start("explorer.exe", file);
            }

            if (OpenFolderAfterSave.IsChecked == true)
            {
                List<string> alreadyOpenedFolders = new List<string>();

                foreach (string file in savePaths)
                {
                    if (alreadyOpenedFolders.Contains(System.IO.Path.GetDirectoryName(file)!)) continue;

                    Process.Start("explorer.exe", "/select, \"" + file + "\"");
                    alreadyOpenedFolders.Add(System.IO.Path.GetDirectoryName(file)!);
                }
            }
            Close();
        }

        private void SavePathTextBox_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveButton.Focus();
                SaveButton_Click(sender, e);
            }
            else if (e.Key == Key.Escape) Close();
        }
    }
}
