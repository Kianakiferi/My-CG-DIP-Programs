using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.FileProperties;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Numerics;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input;

namespace MyCGAndDIPApp2
{
	public sealed partial class LinesCutPage : Page
	{
		internal class Line
		{
			public Vector2 Start;
			public Vector2 End;

			public Line(Vector2 vectorLeft, Vector2 vectorRight)
			{
				Start = vectorLeft;
				End = vectorRight;
			}
		}

		Dictionary<uint, Pointer> pointers;
		List<Line> lines;
		List<Vector2> points;
		private int leftEdge = 100, rightEdge = 200, topEdge = 100, buttomEdge = 200;
		private int CodeLeft = 1;
		private int CodeRight = 2;
		private int CodeButtom = 4;
		private int CodeTop = 8;

		public LinesCutPage()
		{
			this.InitializeComponent();
			pointers = new Dictionary<uint, Pointer>();
			points = new List<Vector2>();
			lines = new List<Line>();
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
			this.canvas.RemoveFromVisualTree();
			this.canvas = null;
		}

		private void canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
		{
			int halfHeight = (int)(sender.ActualHeight / 2);
			int halfWidth = (int)(sender.ActualWidth / 2);
			int size = (int)Slider_Rectangle.Value * 5;
			leftEdge = halfWidth - size;
			rightEdge = halfWidth + size;
			topEdge = halfHeight - size;
			buttomEdge = halfHeight + size;

			args.DrawingSession.DrawLine(new Vector2(leftEdge, topEdge), new Vector2(rightEdge, topEdge), Colors.Azure);
			args.DrawingSession.DrawLine(new Vector2(leftEdge, topEdge), new Vector2(leftEdge, buttomEdge), Colors.Azure);
			args.DrawingSession.DrawLine(new Vector2(rightEdge, topEdge), new Vector2(rightEdge, buttomEdge), Colors.Azure);
			args.DrawingSession.DrawLine(new Vector2(rightEdge, buttomEdge), new Vector2(leftEdge, buttomEdge), Colors.Azure);

			if (points.Count != 0)
			{
				foreach (Vector2 point in points)
				{
					args.DrawingSession.FillCircle(point, 2, Colors.Orange);
				}
			}

			if (lines.Count != 0)
			{
				foreach (Line item in lines)
				{
					args.DrawingSession.DrawLine(item.Start, item.End, Colors.Gray, 2);
					Line cutedLine = CohenSutherland(item.Start, item.End);
					args.DrawingSession.DrawLine(cutedLine.Start, cutedLine.End, Colors.Red, 3);
				}
			}
		}

		private void Slider_Rectangle_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			canvas.Invalidate();
		}

		private Line CohenSutherland(Vector2 startPoint, Vector2 endPoint)
		{
			int P0X = (int)startPoint.X;
			int P0Y = (int)startPoint.Y;
			int P1X = (int)endPoint.X;
			int P1Y = (int)endPoint.Y;

			int Coded1 = Coding(P0X, P0Y);
			int Coded2 = Coding(P1X, P1Y);
			int C;

			int Px = 0, Py = 0;//记录交点
			while (Coded1 != 0 || Coded2 != 0)//两个点（P1x,P1y）,（P2x,P2y）不都在矩形框内；都在内部就画出线段
			{
				if ((Coded1 & Coded2) != 0)   //两个点在矩形框的同一外侧 → 不可见
				{
					P0X = 0;
					P0Y = 0;
					P1X = 0;
					P1Y = 0;
					break;
				}
				C = Coded1;
				if (Coded1 == 0)// 判断P1 P2谁在矩形框内（可能是P1，也可能是P2）
				{
					C = Coded2;
				}

				if ((C & CodeLeft) != 0)//用与判断的点在左侧 
				{
					Px = leftEdge;
					Py = P0Y + (int)(Convert.ToDouble(P1Y - P0Y) / (P1X - P0X) * (leftEdge - P0X));
				}
				else if ((C & CodeRight) != 0)//用与判断的点在右侧 
				{
					Px = rightEdge;
					Py = P0Y + (int)(Convert.ToDouble(P1Y - P0Y) / (P1X - P0X) * (rightEdge - P0X));
				}
				else if ((C & CodeTop) != 0)//用与判断的点在上方
				{
					Py = topEdge;
					Px = P0X + (int)(Convert.ToDouble(P1X - P0X) / (P1Y - P0Y) * (topEdge - P0Y));
				}
				else if ((C & CodeButtom) != 0)//用与判断的点在下方
				{
					Py = buttomEdge;
					Px = P0X + (int)(Convert.ToDouble(P1X - P0X) / (P1Y - P0Y) * (buttomEdge - P0Y));
				}

				if (C == Coded1) //上面判断使用的是哪个端点就替换该端点为新值
				{
					P0X = Px;
					P0Y = Py;
					Coded1 = Coding(P0X, P0Y);
				}
				else
				{
					P1X = Px;
					P1Y = Py;
					Coded2 = Coding(P1X, P1Y);
				}
			}

			return new Line(new Vector2(P0X, P0Y), new Vector2(P1X, P1Y));
		}

		//端点编码
		private int Coding(int x, int y) 
		{
			int result = 0;
			if (x < leftEdge)
			{
				result |= CodeLeft;
			}
			if (x > rightEdge)
			{
				result |= CodeRight;
			}
			if (y < topEdge)
			{
				result |= CodeTop;
			}
			if (y > buttomEdge)
			{
				result |= CodeButtom;
			}
			return result;
		}

		private void Button_DrawLine_Click(object sender, RoutedEventArgs e)
		{
			StartHandlePointerEvent();

			Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Cross, 1);

		}

		private void Button_CleanAll_Click(object sender, RoutedEventArgs e)
		{
			StopHandlePointerEvent();
			points = new List<Vector2>();
			lines = new List<Line>();

			canvas.Invalidate();
		}

		private void Grid_LinesCutRoot_KeyDown(object sender, KeyRoutedEventArgs e)
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
			PointerPoint ptrPt = e.GetCurrentPoint(canvas);

			// Check if pointer already exists (if enter occurred prior to down).
			if (!pointers.ContainsKey(ptrPt.PointerId))
			{
				// Add contact to dictionary.
				pointers[ptrPt.PointerId] = e.Pointer;
			}
		}

		private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
		{
			// Prevent most handlers along the event route from handling the same event again.
			e.Handled = true;
			PointerPoint ptrPt = e.GetCurrentPoint(canvas);
		}

		private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			// Prevent most handlers along the event route from handling the same event again.
			e.Handled = true;
			PointerPoint ptrPt = e.GetCurrentPoint(canvas);
			// Lock the pointer to the target.
			canvas.CapturePointer(e.Pointer);

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
					AddDrawingThings(ptrPt);

					if (points.Count == 2)
					{
						StopHandlePointerEvent();
						pointers[ptrPt.PointerId] = null;
						pointers.Remove(ptrPt.PointerId);
						return;
					}
				}
				if (ptrPt.Properties.IsRightButtonPressed)
				{
					//UpdateEventLog("Right button: " + ptrPt.PointerId);

					StopHandlePointerEvent();
					pointers[ptrPt.PointerId] = null;
					pointers.Remove(ptrPt.PointerId);

					return;
				}
			}
		}

		private void Canvas_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
		{
			// Prevent most handlers along the event route from handling the same event again.
			e.Handled = true;
			PointerPoint ptrPt = e.GetCurrentPoint(canvas);

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
			PointerPoint ptrPt = e.GetCurrentPoint(canvas);

			// Remove contact from dictionary.
			if (pointers.ContainsKey(ptrPt.PointerId))
			{
				pointers[ptrPt.PointerId] = null;
				pointers.Remove(ptrPt.PointerId);
			}
		}

		private void StartHandlePointerEvent()
		{
			canvas.PointerEntered += new PointerEventHandler(Canvas_PointerEntered);
			canvas.PointerMoved += new PointerEventHandler(Canvas_PointerMoved);
			Grid_LinesCutRoot.PointerPressed += new PointerEventHandler(Canvas_PointerPressed);
			canvas.PointerCaptureLost += new PointerEventHandler(Canvas_PointerCaptureLost);
			canvas.PointerExited += new PointerEventHandler(Canvas_PointerExited);
		}

		private void StopHandlePointerEvent()
		{
			Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);

			canvas.PointerEntered -= Canvas_PointerEntered;
			canvas.PointerMoved -= Canvas_PointerMoved;
			Grid_LinesCutRoot.PointerPressed -= Canvas_PointerPressed;
			canvas.PointerCaptureLost -= Canvas_PointerCaptureLost;
			canvas.PointerExited -= Canvas_PointerExited;
		}

		private void AddDrawingThings(PointerPoint ptrPt)
		{
			if (points.Count == 1)
			{
				lines.Add(new Line(points[0], new Vector2((float)Math.Round(ptrPt.Position.X), (float)Math.Round(ptrPt.Position.Y))));
				points.Clear();
			}
			else
			{
				points.Add(new Vector2((float)Math.Round(ptrPt.Position.X), (float)Math.Round(ptrPt.Position.Y)));
			}
			//Debug.WriteLine(currentCanvas.Name);
			canvas.Invalidate();
		}

		/*
		//Bitmap 旋转变换
		private Bitmap MyRotate(Bitmap bitmap, double Angle)
		{
			double rad = Math.PI / 180.0 * Angle;

			//result = T * bitmap
			Matrix T = new Matrix((float)Math.Cos(rad), (float)-Math.Sin(rad), (float)Math.Sin(rad), (float)Math.Cos(rad), 0, 0);
			int width = bitmap.Width;
			int height = bitmap.Height;
			Bitmap result = new Bitmap((int)(width * Math.Cos(rad) + height * Math.Sin(rad)) + 1, (int)(width * Math.Sin(rad) + height * Math.Cos(rad)) + 1);
			WriteableBitmap writeableBitmap = new WriteableBitmap((int)(width * Math.Cos(rad) + height * Math.Sin(rad)) + 1, (int)(width * Math.Sin(rad) + height * Math.Cos(rad)) + 1);
			Matrix map = new Matrix();
			for (int j = 0; j < height; j++)
			{
				for (int i = 0; i < width; i++)
				{
					map.Elements[0] = i;
					map.Elements[2] = j;
					map.Elements[4] = 1;
					T.Multiply(map);

					Color pixel = bitmap.GetPixel(i, j);
					result.SetPixel((int)map.Elements[0] + (int)(height * Math.Sin(rad)), (int)map.Elements[2], pixel);
				}
			}
			return result;
		}*/


	}
}
