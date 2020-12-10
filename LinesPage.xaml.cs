using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml.Hosting;

namespace MyCGAndDIPApp2
{
	internal class Line
	{
		public Vector2 start;
		public Vector2 end;
		public Color color;

		public Line(Vector2 start, Vector2 end, Color color)
		{
			this.start = start;
			this.end = end;
			this.color = color;
		}
	}

	public sealed partial class LinesPage : Page
	{
		//DDA 画法
		public static void DrawLineDDA(CanvasControl sender, CanvasDrawEventArgs args, Vector2 vectorStart, Vector2 vectorEnd, float radius, Color color)
		{
			//args.DrawingSession.DrawLine(vectorStart, vectorEnd, Colors.Azure);

			int x0 = (int)vectorStart.X;
			int y0 = (int)vectorStart.Y;
			int x1 = (int)vectorEnd.X;
			int y1 = (int)vectorEnd.Y;

			float dx = x1 - x0;
			float dy = y1 - y0;
			float length;
			if (Math.Abs(dx) >= Math.Abs(dy))
			{
				length = dx;
			}
			else
			{
				length = dy;
			}

			dx /= Math.Abs(length);
			dy /= Math.Abs(length);

			float pointX = x0;
			float pointY = y0;

			int i = 1;
			while (i <= Math.Abs(length))
			{
				args.DrawingSession.FillCircle((int)pointX, (int)pointY, radius, color);
				pointX += dx;
				pointY += dy;
				i++;
			}
		}

		//Bresenham 画法
		public static void DrawLineBresenham(CanvasControl sender, CanvasDrawEventArgs args, Vector2 vectorStart, Vector2 vectorEnd, float radius, Color color)
		{
			int x0 = (int)vectorStart.X;
			int y0 = (int)vectorStart.Y;
			int x1 = (int)vectorEnd.X;
			int y1 = (int)vectorEnd.Y;

			bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
			if (steep)
			{
				Swap(ref x0, ref y0);
				Swap(ref x1, ref y1);
			}

			if (x0 > x1)
			{
				Swap(ref x0, ref x1);
				Swap(ref y0, ref y1);
			}

			int dx = x1 - x0;
			int dy = y1 - y0;
			int derr = 2 * Math.Abs(dy);
			int err = 0;
			int ystep = y0 < y1 ? 1 : -1;

			int x = x0;
			int y = y0;
			if (steep)
			{
				for (; x <= x1; x++)
				{
					args.DrawingSession.FillCircle(y, x, radius, color);

					err += derr;
					if (err > dx)
					{
						y += ystep;
						err -= 2 * dx;
					}
				}
			}
			else
			{
				for (; x <= x1; x++)
				{
					args.DrawingSession.FillCircle(x, y, radius, color);
					err += derr;
					if (err > dx)
					{
						y += ystep;
						err -= 2 * dx;
					}
				}
			}
		}
		//垂直线段

		//if (Math.Abs(x - x1) == 0)
		//{
		//	if (y0 > y1)
		//	{
		//		Swap(ref y0, ref y1);
		//		Swap(ref x0, ref x1);
		//	}


		//	for (x = y0; x < y1; x++)
		//	{
		//		args.DrawingSession.FillCircle(x, y, radius, color);

		//	}
		//}

		private static void Swap<T>(ref T left, ref T right)
		{
			T temp = left;
			left = right;
			right = temp;
		}

		Dictionary<uint, Pointer> pointers;

		List<Line> lines;
		List<Vector2> points;

		int PointNumber;
		Color currentColor;
		CanvasControl currentCanvas;

		public LinesPage()
		{
			this.InitializeComponent();

			pointers = new Dictionary<uint, Pointer>();
			points = new List<Vector2>();
			lines = new List<Line>();
			currentColor = Colors.Azure;

			this.Loaded += delegate { this.Focus(FocusState.Programmatic); };
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
			currentCanvas.RemoveFromVisualTree();
			currentCanvas = null;
		}

		private void Pivot_PivotItemLoading(Pivot sender, PivotItemEventArgs args)
		{
			var item = (string)args.Item.Tag;
			switch (item)
			{
				case "dda":
					{
						currentCanvas = canvas_D;
						TextBlock_PaneHeaderText.Text = "DDA Algorithm";
						break;
					}
				case "bresenham":
					{
						currentCanvas = canvas_B;
						TextBlock_PaneHeaderText.Text = "Bresenham Algorithm";
						break;
					}
				default:
					{
						throw new Exception("NULL currentCanvas!");
					}
			}

			currentCanvas.Invalidate();
		}

		private void canvas_D_OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
		{
			float height = (float)(sender.ActualHeight);
			float width = (float)(sender.ActualWidth);
			args.DrawingSession.DrawLine(0, height / 2, width, height / 2, Colors.Azure);
			args.DrawingSession.DrawLine(width / 2, 0, width / 2, height, Colors.Azure);

			if (points.Count != 0)
			{
				foreach (var point in points)
				{
					args.DrawingSession.FillCircle(point, 2, Colors.Orange);
				}
			}

			if (lines.Count != 0)
			{
				foreach (var item in lines)
				{
					Debug.WriteLine("DrawLineDDA");
					DrawLineDDA(sender, args, item.start, item.end, (float)Slider_thickness.Value, item.color);
				}
			}
		}

		private void canvas_B_OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
		{
			float height = (float)sender.ActualHeight;
			float width = (float)sender.ActualWidth;
			args.DrawingSession.DrawLine(0, height / 2, width, height / 2, Colors.Azure);
			args.DrawingSession.DrawLine(width / 2, 0, width / 2, height, Colors.Azure);

			if (points.Count != 0)
			{
				foreach (var point in points)
				{
					args.DrawingSession.FillCircle(point, 2, Colors.Orange);
				}
			}

			if (lines.Count != 0)
			{
				foreach (var item in lines)
				{
					Debug.WriteLine("DrawLineBresenham");
					DrawLineBresenham(sender, args, item.start, item.end, (float)Slider_thickness.Value, item.color);
				}
			}
		}

		private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key == VirtualKey.Escape)
			{
				StopHandlePointerEvent();
			}
		}

		private void Canvas_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			// Prevent most handlers along the event route from handling the same event again.
			e.Handled = true;
			PointerPoint ptrPt = e.GetCurrentPoint(currentCanvas);

			// Check if pointer already exists (if enter occurred prior to down).
			if (!pointers.ContainsKey(ptrPt.PointerId))
			{
				// Add contact to dictionary.
				pointers[ptrPt.PointerId] = e.Pointer;
			}
			// Display pointer details.
			UpdateInfo(ptrPt);
		}

		private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
		{
			// Prevent most handlers along the event route from handling the same event again.
			e.Handled = true;
			PointerPoint ptrPt = e.GetCurrentPoint(currentCanvas);

			// Display pointer details.
			UpdateInfo(ptrPt);
		}

		private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			// Prevent most handlers along the event route from handling the same event again.
			e.Handled = true;
			PointerPoint ptrPt = e.GetCurrentPoint(currentCanvas);
			// Lock the pointer to the target.
			currentCanvas.CapturePointer(e.Pointer);

			// Check if pointer exists in dictionary (ie, enter occurred prior to press).
			if (!pointers.ContainsKey(ptrPt.PointerId))
			{
				// Add contact to dictionary.
				pointers[ptrPt.PointerId] = e.Pointer;
			}

			// Multiple, simultaneous mouse button inputs are processed here.
			// Mouse input is associated with a single pointer assigned when 
			// mouse input is first detected. 
			// Clicking additional mouse buttons (left, wheel, or right) during 
			// the interaction creates secondary associations between those buttons 
			// and the pointer through the pointer pressed event. 
			// The pointer released event is fired only when the last mouse button 
			// associated with the interaction (not necessarily the initial button) 
			// is released. 
			// Because of this exclusive association, other mouse button clicks are 
			// routed through the pointer move event.          

			if (ptrPt.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
			{
				if (ptrPt.Properties.IsLeftButtonPressed)
				{
					//UpdateEventLog("Left button: " + ptrPt.PointerId);
					StopHandlePointerEvent();
					pointers[ptrPt.PointerId] = null;
					pointers.Remove(ptrPt.PointerId);

					AddDrawingThings(ptrPt, PointNumber);

					return;
				}
				if (ptrPt.Properties.IsRightButtonPressed)
				{
					//UpdateEventLog("Right button: " + ptrPt.PointerId);

					StopHandlePointerEvent();
					pointers[ptrPt.PointerId] = null;
					pointers.Remove(ptrPt.PointerId);

					CleanInfo();

					return;
				}
			}

			// Display pointer details.
			UpdateInfo(ptrPt);
		}

		private void Canvas_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
		{
			// Prevent most handlers along the event route from handling the same event again.
			e.Handled = true;
			PointerPoint ptrPt = e.GetCurrentPoint(currentCanvas);

			// Remove contact from dictionary.
			if (pointers.ContainsKey(ptrPt.PointerId))
			{
				pointers[ptrPt.PointerId] = null;
				pointers.Remove(ptrPt.PointerId);
			}
		}
		private void Canvas_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			// Prevent most handlers along the event route from handling the same event again.
			e.Handled = true;
			PointerPoint ptrPt = e.GetCurrentPoint(currentCanvas);

			// Remove contact from dictionary.
			if (pointers.ContainsKey(ptrPt.PointerId))
			{
				pointers[ptrPt.PointerId] = null;
				pointers.Remove(ptrPt.PointerId);
			}
		}

		private void StartHandlePointerEvent()
		{
			currentCanvas.PointerEntered += new PointerEventHandler(Canvas_PointerEntered);
			currentCanvas.PointerMoved += new PointerEventHandler(Canvas_PointerMoved);
			currentCanvas.PointerPressed += new PointerEventHandler(Canvas_PointerPressed);

			currentCanvas.PointerCaptureLost += new PointerEventHandler(Canvas_PointerCaptureLost);
			currentCanvas.PointerExited += new PointerEventHandler(Canvas_PointerExited);
		}

		private void StopHandlePointerEvent()
		{
			Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
			currentCanvas.PointerEntered -= Canvas_PointerEntered;
			currentCanvas.PointerMoved -= Canvas_PointerMoved;
			currentCanvas.PointerPressed -= Canvas_PointerPressed;

			currentCanvas.PointerCaptureLost -= Canvas_PointerCaptureLost;
			currentCanvas.PointerExited -= Canvas_PointerExited;
		}

		private void UpdateInfo(PointerPoint ptrPt)
		{
			switch (PointNumber)
			{
				case 0:
					{
						TextBox_Point0X.Text = Math.Round(ptrPt.Position.X).ToString();
						TextBox_Point0Y.Text = Math.Round(ptrPt.Position.Y).ToString();
						break;
					}
				case 1:
					{
						TextBox_Point1X.Text = Math.Round(ptrPt.Position.X).ToString();
						TextBox_Point1Y.Text = Math.Round(ptrPt.Position.Y).ToString();
						break;
					}
				default:
					break;
			}
		}
		private void CleanInfo()
		{
			switch (PointNumber)
			{
				case 0:
					{
						TextBox_Point0X.Text = "";
						TextBox_Point0Y.Text = "";
						break;
					}
				case 1:
					{
						TextBox_Point1X.Text = "";
						TextBox_Point1Y.Text = "";
						break;
					}
				default:
					break;
			}
		}
		private void AddDrawingThings(PointerPoint ptrPt, int pointNumber)
		{
			if (points.Count == 1)
			{
				lines.Add(new Line(points[0], new Vector2((float)Math.Round(ptrPt.Position.X), (float)Math.Round(ptrPt.Position.Y)), currentColor));
				points.Clear();
			}
			else
			{
				points.Add(new Vector2((float)Math.Round(ptrPt.Position.X), (float)Math.Round(ptrPt.Position.Y)));
			}
			//Debug.WriteLine(currentCanvas.Name);
			currentCanvas.Invalidate();
		}

		AppWindow LinesInfoWindow;

		private async void BtmLinesInfo_AppWindow_Click(object sender, RoutedEventArgs e)
		{
			if (LinesInfoWindow == null)
			{				
				LinesInfoContainer.Children.Remove(StackPanel_LinesInfo);
				BtmLinesInfo_AppWindow.Visibility = Visibility.Collapsed;
				LinesInfoContainer.Visibility = Visibility.Collapsed;

				Grid appWindowRootGrid = new Grid();
				appWindowRootGrid.Children.Add(StackPanel_LinesInfo);

				LinesInfoWindow = await AppWindow.TryCreateAsync();
				LinesInfoWindow.RequestSize(new Size(300, 500));
				LinesInfoWindow.Presenter.RequestPresentation(AppWindowPresentationKind.CompactOverlay);
				
				LinesInfoWindow.Title = "Lines";
				ElementCompositionPreview.SetAppWindowContent(LinesInfoWindow, appWindowRootGrid);

				LinesInfoWindow.Closed += delegate
				{
					appWindowRootGrid.Children.Remove(StackPanel_LinesInfo);
					appWindowRootGrid = null;
					LinesInfoWindow = null;

					LinesInfoContainer.Children.Add(StackPanel_LinesInfo);
					BtmLinesInfo_AppWindow.Visibility = Visibility.Visible;
					LinesInfoContainer.Visibility = Visibility.Visible;
				};
			}
			await LinesInfoWindow.TryShowAsync();
		}

		private void BtmPoint0_Pin_Click(object sender, RoutedEventArgs e)
		{
			PointNumber = 0;

			// Declare the pointer event handlers.
			StartHandlePointerEvent();

			Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Cross, 1);
		}

		private void BtmPoint1_Pin_Click(object sender, RoutedEventArgs e)
		{
			PointNumber = 1;

			// Declare the pointer event handlers.
			StartHandlePointerEvent();

			Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Cross, 1);
		}

		private void BtmCleanAll_Click(object sender, RoutedEventArgs e)
		{
			StopHandlePointerEvent();
			CleanCanvas();
			Debug.WriteLine("CleanAll");
		}

		private void CleanCanvas()
		{
			points.Clear();
			lines.Clear();
			currentCanvas.Invalidate();
			TextBox_Point0X.Text = "";
			TextBox_Point0Y.Text = "";
			TextBox_Point1X.Text = "";
			TextBox_Point1Y.Text = "";
		}

		private void myColorButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
		{
			var rectangle = (Windows.UI.Xaml.Shapes.Rectangle)sender.Content;
			var color = ((Windows.UI.Xaml.Media.SolidColorBrush)rectangle.Fill).Color;

			RecCurrentColor.Fill = new SolidColorBrush(color);
			currentColor = color;
		}

		private void ColorButton_Click(object sender, RoutedEventArgs e)
		{
			// Extract the color of the button that was clicked.
			Button clickedColor = (Button)sender;
			var rectangle = (Windows.UI.Xaml.Shapes.Rectangle)clickedColor.Content;
			var color = ((Windows.UI.Xaml.Media.SolidColorBrush)rectangle.Fill).Color;

			RecCurrentColor.Fill = new SolidColorBrush(color);
			myColorButton.Flyout.Hide();
			currentColor = color;
		}

		private void Slider_thickness_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			if (currentCanvas != null)
			{
				currentCanvas.Invalidate();
			}
		}
	}
}
