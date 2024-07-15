using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.UI.Xaml.Input;

namespace Silky
{
   internal static class Intermediate
   {
      public static Window MainWindow { get; set; }

      public static ListViewItem PrepareListViewItem(string displayName, string filePath, Exception e = null)
      {
         ListViewItem item = new ListViewItem();
         
         item.Content = displayName;

         if (e is not null)
         {
            item.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 253, 231, 233));
            item.Content += " · " + e.Message;
            ToolTipService.SetToolTip(item, e.Message);
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
   }
}
