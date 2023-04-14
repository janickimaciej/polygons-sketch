using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Polygons {
	public partial class MainWindow : Window {
		enum Mode {
			Draw,
			Delete,
			AddMiddle,
			Move,
			SetLength,
			RemoveLength,
			AddParallel,
			RemoveParallel,
			Bresenham
		}

		private mModel model = new mModel();
		Mode mode = Mode.Draw;

		public MainWindow() {
			InitializeComponent();
			PreviewKeyDown += new KeyEventHandler(HandleKey);
			//PredefinedScene();
		}

		private void HandleKey(object sender, KeyEventArgs e) {
			switch(e.Key) {
				case Key.Enter:
					HandleEnter();
					break;
				case Key.Escape:
					HandleEsc();
					break;
			}
		}

		private void HandleEnter() {
			switch(mode) {
				case Mode.SetLength:
					if(lengthApplyButton.IsEnabled) {
						applyLength();
					}
					break;
			}
		}

		private void HandleEsc() {
			switch(mode) {
				case Mode.Draw:
					leaveDrawMode();
					break;
				case Mode.SetLength:
					leaveSetLengthMode();
					break;
				case Mode.AddParallel:
					model.StopParallel();
					break;
			}
		}

		private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			Point cursorPosition = Mouse.GetPosition(canvas);

			switch(mode) {
				case Mode.Move:
					model.StartMoving(cursorPosition.X, cursorPosition.Y);
					break;
				case Mode.SetLength:
					double sideLength = model.GetSideLength(cursorPosition.X, cursorPosition.Y);
					if(sideLength >= 0) {
						lengthTextBox.IsEnabled = true;
						lengthApplyButton.IsEnabled = true;
						lengthTextBox.Text = string.Format("{0:0.00}", sideLength);
					} else {
						leaveSetLengthMode();
					}
					break;
				case Mode.RemoveLength:
					model.RemoveSideLength(cursorPosition.X, cursorPosition.Y);
					break;
				case Mode.AddParallel:
					if(!model.AddParallel(cursorPosition.X, cursorPosition.Y)) {
						popupText.Text = "Invalid constraint";
						popup.IsOpen = true;
						leaveAddParallelMode();
					}
					break;
				case Mode.RemoveParallel:
					model.RemoveParallel(cursorPosition.X, cursorPosition.Y);
					break;
			}
		}

		private void canvas_MouseMove(object sender, MouseEventArgs e) {
			Point cursorPosition = Mouse.GetPosition(canvas);

			switch(mode) {
				case Mode.Draw:
					model.UpdateCurrentLine(cursorPosition.X, cursorPosition.Y);
					break;
				case Mode.Move:
					model.Move(cursorPosition.X, cursorPosition.Y);
					break;
			}
		}

		private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			Point cursorPosition = Mouse.GetPosition(canvas);

			switch(mode) {
				case Mode.Draw:
					List<UIElement> list = model.DrawPolygon(cursorPosition.X, cursorPosition.Y);
					if(list != null) {
						foreach(UIElement element in list) {
							canvas.Children.Add(element);
						}
					} else {
						leaveDrawMode();
					}
					break;
				case Mode.Delete:
					foreach(UIElement element in model.Delete(cursorPosition.X, cursorPosition.Y)) {
						canvas.Children.Remove(element);
					}
					break;
				case Mode.AddMiddle:
					foreach(UIElement element in model.AddMiddleVertex(cursorPosition.X, cursorPosition.Y)) {
						canvas.Children.Add(element);
					}
					break;
				case Mode.Move:
					model.StopMoving();
					break;
			}
		}

		private void drawButton_Checked(object sender, RoutedEventArgs e) {
			leaveCurrentMode();
			mode = Mode.Draw;
		}

		private void deleteButton_Checked(object sender, RoutedEventArgs e) {
			leaveCurrentMode();
			mode = Mode.Delete;
		}

		private void addMiddleButton_Checked(object sender, RoutedEventArgs e) {
			leaveCurrentMode();
			mode = Mode.AddMiddle;
		}

		private void moveButton_Checked(object sender, RoutedEventArgs e) {
			leaveCurrentMode();
			mode = Mode.Move;
		}

		private void setLengthButton_Checked(object sender, RoutedEventArgs e) {
			leaveCurrentMode();
			mode = Mode.SetLength;
		}

		private void removeLengthButton_Checked(object sender, RoutedEventArgs e) {
			leaveCurrentMode();
			mode = Mode.RemoveLength;
		}

		private void addParallelButton_Checked(object sender, RoutedEventArgs e) {
			leaveCurrentMode();
			mode = Mode.AddParallel;
		}

		private void removeParallelButton_Checked(object sender, RoutedEventArgs e) {
			leaveCurrentMode();
			mode = Mode.RemoveParallel;
		}

		private void lengthApplyButton_Click(object sender, RoutedEventArgs e) {
			applyLength();
		}

		private void popupButton_Click(object sender, RoutedEventArgs e) {
			popup.IsOpen = false;
		}

		private void bresenhamButton_Checked(object sender, RoutedEventArgs e) {
			leaveCurrentMode();
			mode = Mode.Bresenham;
			canvas.Visibility = Visibility.Hidden;
			image.Visibility = Visibility.Visible;
			WriteableBitmap bitmap = new WriteableBitmap((int)imageContainer.ActualWidth,
				(int)imageContainer.ActualHeight, 96, 96, PixelFormats.Bgra32, null);
			model.DrawBresenham(bitmap, (int)imageContainer.ActualWidth, (int)imageContainer.ActualHeight);
			image.Source = bitmap;
		}

		private void leaveCurrentMode() {
			switch(mode) {
				case Mode.Draw:
					leaveDrawMode();
					break;
				case Mode.Move:
					leaveMoveMode();
					break;
				case Mode.SetLength:
					leaveSetLengthMode();
					break;
				case Mode.AddParallel:
					leaveAddParallelMode();
					break;
				case Mode.Bresenham:
					leaveBresenhamMode();
					break;
			}
		}

		private void leaveDrawMode() {
			List<UIElement> UIElements = model.StopDrawing();
			foreach(UIElement element in UIElements) {
				canvas.Children.Remove(element);
			}
		}

		private void leaveMoveMode() {
			model.StopMoving();
		}

		private void leaveSetLengthMode() {
			model.StopSetSideLength();
			lengthTextBox.Text = "";
			lengthTextBox.IsEnabled = false;
			lengthApplyButton.IsEnabled = false;
		}

		private void leaveAddParallelMode() {
			model.StopParallel();
		}

		private void leaveBresenhamMode() {
			if(canvas != null) canvas.Visibility = Visibility.Visible;
			if(image != null) image.Visibility = Visibility.Hidden;
		}

		private void applyLength() {
			double length;
			if(!double.TryParse(lengthTextBox.Text, out length) || length < 0) {
				popupText.Text = "Invalid length value";
				popup.IsOpen = true;
				return;
			}
			if(!model.SetSideLength(length)) {
				popupText.Text = "Invalid constraint";
				popup.IsOpen = true;
				return;
			}
			leaveSetLengthMode();
		}

		private void PredefinedScene() {
			PseudoClick(300, 300);
			PseudoClick(380, 280);
			PseudoClick(460, 300);
			PseudoClick(400, 460);
			PseudoClick(350, 400);
			PseudoClick(300, 300);

			PseudoClick(600, 250);
			PseudoClick(680, 280);
			PseudoClick(760, 300);
			PseudoClick(680, 460);
			PseudoClick(700, 400);
			PseudoClick(600, 250);
			model.PredefinedScene();
		}

		private void PseudoClick(double x, double y) {
			List<UIElement> list = model.DrawPolygon(x, y);
			if(list != null) {
				foreach(UIElement element in list) {
					canvas.Children.Add(element);
				}
			} else {
				leaveDrawMode();
			}
		}
	}
}
