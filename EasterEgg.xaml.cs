using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace Silky
{
    /// <summary>
    /// Interaction logic for EasterEgg.xaml
    /// </summary>
    public partial class EasterEgg : Window
    {
        public EasterEgg()
        {
            InitializeComponent();
            OKButton.Focus();
            Storyboard? storyboard = FindResource("SpinStoryboard") as Storyboard;
            if (storyboard != null)
            {
                // Create a TimeSpan representing the delay of 5 seconds
                TimeSpan delay = TimeSpan.FromSeconds(5);

                // Set the BeginTime property of the storyboard to apply the delay
                storyboard.BeginTime = delay;

                // Start the storyboard
                storyboard.Begin(ErrorImage, HandoffBehavior.SnapshotAndReplace, true);
            }
        }

        public bool? ShowDialog(string message)
        {
            MessageTextBlock.Text = message;
            return ShowDialog();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }
    }
}
