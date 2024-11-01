using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;

namespace Silky
{
   internal static class Intermediate
   {
      public struct PresetText
      {
         public const string Default = "Preset Options";

         public const string Modified = " with Modifications"; // TextBlock.Text = PresetText.Handsoldering; [...] TextBlock.Text += PresetText.Modified;

         public static string CurrentPreset = Default; // to communicate the state to the save dialog, so the export fab files checkbox is checked whenever htl 10 is selected
         public static string HandSoldering { get { return CurrentPreset = "Hand Soldering"; } }
         public static string BlankPCB { get { return CurrentPreset = "Blank PCB"; } }
         public static string HTL10Values { get { return CurrentPreset = "HTL Wien 10 - Values"; } }
         public static string HTL10References { get { return CurrentPreset = "HTL Wien 10 - References"; } }
      }

      public static Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
      public static Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
      

      public static Window MainWindow { get; set; }

      public static ListView SavedFileLogListView { get; set; } // If something goes wrong during saving in another class like SilkyCore.cs, it should have access to the save log to display the error

      public static ListViewItem PrepareListViewItem(string displayName, string filePath, Exception e = null)
      {
         ListViewItem item = new ListViewItem();

         item.Content = displayName;

         if (e is not null)
         {
            item.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 253, 231, 233));
            item.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 26, 26, 26));

            item.Content += " · " + e.Message;
            ToolTipService.SetToolTip(item, e.Message);


            MenuFlyout errorContextMenu = new MenuFlyout();
            MenuFlyoutItem copyErrorItem = new MenuFlyoutItem { Text = "Copy to Clipboard" };
            copyErrorItem.DataContext = e.Message;
            copyErrorItem.Click += CopyErrorMessage;
            copyErrorItem.Icon = new FontIcon { Glyph = "\uE8C8" };
            errorContextMenu.Items.Add(copyErrorItem);
            item.ContextFlyout = errorContextMenu;
            return item;
         }

         ToolTipService.SetToolTip(item, filePath);
         item.DataContext = filePath;
         item.DoubleTapped += OpenEnclosingFolder;

         MenuFlyout contextMenu = new MenuFlyout();
         MenuFlyoutItem openFileMenuItem = new MenuFlyoutItem { Text = "Open in PCB Editor" };
         openFileMenuItem.DataContext = filePath;
         openFileMenuItem.Click += StartKiCad;
         openFileMenuItem.Icon = new FontIcon { Glyph = "\uE8A7" };
         contextMenu.Items.Add(openFileMenuItem);

         item.ContextFlyout = contextMenu;

         return item;
      }

      public static void StartKiCad(object sender, RoutedEventArgs e)
      {
         MenuFlyoutItem menuItem = sender as MenuFlyoutItem;
         string filePath = menuItem!.DataContext as string;
         Process.Start("explorer.exe", filePath!);
      }

      public static void OpenEnclosingFolder(object sender, RoutedEventArgs e)
      {
         ListViewItem listViewItem = sender as ListViewItem;
         string filePath = listViewItem.DataContext as string;
         string arg = "/select, \"" + filePath + "\"";
         Process.Start("explorer.exe", arg);
      }

      public static void CopyErrorMessage(object sender, RoutedEventArgs e)
      {

         MenuFlyoutItem menuItem = sender as MenuFlyoutItem;
         string errorMessage = menuItem!.DataContext as string;

         var package = new DataPackage();
         package.SetText(errorMessage);
         Clipboard.SetContent(package);
      }
   }
}