using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MyCGAndDIPApp2
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
		public static bool IsNavViewPaneOpen = false;

		private double NavViewCompactModeThresholdWidth { get { return MainNavView.CompactModeThresholdWidth; } }

		private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}

		private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
		{
			("home", typeof(HomePage)),
			("lines", typeof(LinesPage)),
			("linescut", typeof(LinesCutPage))
		};

		public MainPage()
        {
            this.InitializeComponent();

			var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
			coreTitleBar.ExtendViewIntoTitleBar = true;
		}

		//private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
		//{
		//	// Get the size of the caption controls area and back button 
		//	// (returned in logical pixels), and move your content around as necessary.


		//	// Update title bar control size as needed to account for system size changes.
		//	AppTitleBar.Height = coreTitleBar.Height;
		//}


		//private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
		//{
		//	UpdateTitleBarLayout(sender);
		//}


		private void MainNavView_Loaded(object sender, RoutedEventArgs e)
		{
			ContentFrame.Navigated += On_Navigated;

			MainNavView.SelectedItem = MainNavView.MenuItems[0];
			MainNavView_Navigate("home", new Windows.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());

			var goBack = new KeyboardAccelerator { Key = Windows.System.VirtualKey.GoBack };
			goBack.Invoked += BackInvoked;
			this.KeyboardAccelerators.Add(goBack);

			var altLeft = new KeyboardAccelerator
			{
				Key = Windows.System.VirtualKey.Left,
				Modifiers = Windows.System.VirtualKeyModifiers.Menu
			};
			altLeft.Invoked += BackInvoked;
			this.KeyboardAccelerators.Add(altLeft);
		}

		private void MainNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		{
			if (args.IsSettingsSelected == true)
			{
				MainNavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
			}
			else if (args.SelectedItemContainer != null)
			{
				var navItemTag = args.SelectedItemContainer.Tag.ToString();
				MainNavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
			}

		}

		private void MainNavView_Navigate(string navItemTag, Windows.UI.Xaml.Media.Animation.NavigationTransitionInfo transitionInfo)
		{
			Type _page = null;
			if (navItemTag == "settings")
			{
				_page = typeof(SettingsPage);
			}
			else
			{
				var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
				_page = item.Page;
			}
			// Get the page type before navigation so you can prevent duplicate
			// entries in the backstack.
			var preNavPageType = ContentFrame.CurrentSourcePageType;

			// Only navigate if the selected page isn't currently loaded.
			if (!(_page is null) && !Type.Equals(preNavPageType, _page))
			{
				ContentFrame.Navigate(_page, null, transitionInfo);
			}
		}

		private void MainNavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
		{
			if (args.IsSettingsInvoked == true)
			{
				MainNavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
			}
			else if (args.InvokedItemContainer != null)
			{
				var navItemTag = args.InvokedItemContainer.Tag.ToString();
				MainNavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
			}
		}

		private void MainNavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
		{
			On_BackRequested();
		}
		private void BackInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			On_BackRequested();
			args.Handled = true;
		}
		private bool On_BackRequested()
		{
			if (!ContentFrame.CanGoBack)
				return false;

			// Don't go back if the nav pane is overlayed.
			if (MainNavView.IsPaneOpen &&
				(MainNavView.DisplayMode == NavigationViewDisplayMode.Compact ||
				 MainNavView.DisplayMode == NavigationViewDisplayMode.Minimal))
				return false;

			ContentFrame.GoBack();
			return true;
		}

		private void On_Navigated(object sender, NavigationEventArgs e)
		{
			MainNavView.IsBackEnabled = ContentFrame.CanGoBack;

			if (ContentFrame.SourcePageType == typeof(SettingsPage))
			{
				// SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
				MainNavView.SelectedItem = (NavigationViewItem)MainNavView.SettingsItem;
				MainNavView.Header = "Settings";
			}
			else if (ContentFrame.SourcePageType != null)
			{
				var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

				MainNavView.SelectedItem = MainNavView.MenuItems
					.OfType<NavigationViewItem>()
					.First(n => n.Tag.Equals(item.Tag));

				MainNavView.Header =
					((NavigationViewItem)MainNavView.SelectedItem)?.Content?.ToString();
			}
		}

		private void MainNavView_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
		{
			AppTitleText.Visibility = Visibility.Collapsed;
			IsNavViewPaneOpen = false;
		}

		private void MainNavView_PaneOpening(NavigationView sender, object args)
		{
			AppTitleText.Visibility = Visibility.Visible;
			IsNavViewPaneOpen = true;
		}
	}
}
