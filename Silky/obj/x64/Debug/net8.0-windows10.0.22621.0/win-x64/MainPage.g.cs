﻿#pragma checksum "C:\Users\Adrian\source\repos\Silky\Silky\MainPage.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "C07B1E381A5DB00CB8FD1F3FADC280560EB64214DDFE2B9FCEE387E418B1CFF1"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Silky
{
    partial class MainPage : 
        global::Microsoft.UI.Xaml.Controls.Page, 
        global::Microsoft.UI.Xaml.Markup.IComponentConnector
    {

        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2406")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1: // MainPage.xaml line 2
                {
                    global::Microsoft.UI.Xaml.Controls.Page element1 = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Page>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Page)element1).KeyDown += this.Page_KeyDown;
                    ((global::Microsoft.UI.Xaml.Controls.Page)element1).PointerPressed += this.Page_PointerPressed;
                    ((global::Microsoft.UI.Xaml.Controls.Page)element1).DragEnter += this.Grid_DragEnter;
                    ((global::Microsoft.UI.Xaml.Controls.Page)element1).Drop += this.Page_Drop;
                }
                break;
            case 2: // MainPage.xaml line 36
                {
                    this.PCBListView = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.ListView>(target);
                    ((global::Microsoft.UI.Xaml.Controls.ListView)this.PCBListView).SelectionChanged += this.PCBListView_SelectionChanged;
                }
                break;
            case 3: // MainPage.xaml line 169
                {
                    this.OverrideAllButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.OverrideAllButton).Click += this.OverrideAllButton_Click;
                }
                break;
            case 4: // MainPage.xaml line 176
                {
                    this.SaveAllAsButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.SaveAllAsButton).Click += this.SaveAllAsButton_Click;
                }
                break;
            case 5: // MainPage.xaml line 150
                {
                    this.OperationsListView = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.ListView>(target);
                    ((global::Microsoft.UI.Xaml.Controls.ListView)this.OperationsListView).SelectionChanged += this.OperationsListView_SelectionChanged;
                }
                break;
            case 6: // MainPage.xaml line 153
                {
                    this.PreviewButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.PreviewButton).Click += this.PreviewButton_Click;
                }
                break;
            case 7: // MainPage.xaml line 135
                {
                    this.AddOperationButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.AddOperationButton).Click += this.AddOperationButton_Click;
                }
                break;
            case 8: // MainPage.xaml line 141
                {
                    this.RemoveOperationButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.RemoveOperationButton).Click += this.RemoveOperationButton_Click;
                }
                break;
            case 9: // MainPage.xaml line 84
                {
                    this.PresetFontIcon = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.FontIcon>(target);
                }
                break;
            case 10: // MainPage.xaml line 85
                {
                    this.PresetTextBlock = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBlock>(target);
                }
                break;
            case 11: // MainPage.xaml line 91
                {
                    this.HandSolderingPreset = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.MenuFlyoutItem>(target);
                    ((global::Microsoft.UI.Xaml.Controls.MenuFlyoutItem)this.HandSolderingPreset).Click += this.HandSolderingPreset_Click;
                }
                break;
            case 12: // MainPage.xaml line 97
                {
                    this.BlankPCBPreset = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.MenuFlyoutItem>(target);
                    ((global::Microsoft.UI.Xaml.Controls.MenuFlyoutItem)this.BlankPCBPreset).Click += this.BlankPCBPreset_Click;
                }
                break;
            case 13: // MainPage.xaml line 103
                {
                    this.HTL10Preset = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.MenuFlyoutItem>(target);
                    ((global::Microsoft.UI.Xaml.Controls.MenuFlyoutItem)this.HTL10Preset).Click += this.HTL10ValuesPreset_Click;
                }
                break;
            case 14: // MainPage.xaml line 109
                {
                    this.HTL10ReferencePreset = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.MenuFlyoutItem>(target);
                    ((global::Microsoft.UI.Xaml.Controls.MenuFlyoutItem)this.HTL10ReferencePreset).Click += this.HTL10ReferencePreset_Click;
                }
                break;
            case 15: // MainPage.xaml line 70
                {
                    this.FromLayerListView = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.ListView>(target);
                    ((global::Microsoft.UI.Xaml.Controls.ListView)this.FromLayerListView).SelectionChanged += this.FromLayerListView_SelectionChanged;
                }
                break;
            case 16: // MainPage.xaml line 74
                {
                    this.ToLayerListView = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.ListView>(target);
                }
                break;
            case 17: // MainPage.xaml line 78
                {
                    this.ApplyToPartListView = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.ListView>(target);
                }
                break;
            case 18: // MainPage.xaml line 45
                {
                    this.LoadPCBFileButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.LoadPCBFileButton).Click += this.LoadPCBFileButton_Click;
                }
                break;
            case 19: // MainPage.xaml line 51
                {
                    this.RemovePCBFileButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.RemovePCBFileButton).Click += this.RemovePCBFileButton_Click;
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2406")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Microsoft.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Microsoft.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

