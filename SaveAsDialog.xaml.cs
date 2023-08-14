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
            string textBoxPath = SavePathTextBox.Text;

            if (textBoxPath == "")
            {
                ErrorMessage.ShowDialog("Looks like the provided path is too short to make sense.");
                return;
            }

            if (textBoxPath.Contains(":")) // absulute path
            {
                if (!Directory.Exists(textBoxPath.Substring(0, textBoxPath.IndexOf(@"\")+1)))
                {
                    ErrorMessage.ShowDialog("How about a path that exists?");
                    return;
                }
                // creating every missing directory
                string[] directories = textBoxPath.Split('\\');
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
                    string savePath = SavePathTextBox.Text.Replace(@"\\",@"\").Replace("*", pcbName) + ".kicad_pcb";
                    savePaths.Add(savePath);
                    File.Copy(Core.FullPath(pcbName), savePath, true);
                }
            }
            else
            {
                foreach (string pcbPath in Core.FullPaths())
                {
                    string savePath = Path.GetDirectoryName(pcbPath) + SavePathTextBox.Text;
                }
            }
        }
    }
}
