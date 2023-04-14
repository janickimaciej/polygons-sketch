using System;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace Polygons {
	public class mSide : mClickable {
		public Line gLine { get; set; }
		public TextBlock TextClassNumber { get; set; }
		public mVertex prevVertex { get; set; }
		public mVertex nextVertex { get; set; }
		public mPolygon Parent { get; }
		private bool isLengthSet = false;
		public bool IsLengthSet {
			get => isLengthSet;
			set {
				gLine.Stroke = value ? System.Windows.Media.Brushes.Blue : System.Windows.Media.Brushes.Black;
				isLengthSet = value;
			}
		}
		private double length;
		public double Length {
			get {
				if(IsLengthSet) return length;
				else return ActualLength;
			}
			set => length = value;
		}
		public double ActualLength {
			get => Math.Sqrt((nextVertex.X - prevVertex.X)*(nextVertex.X - prevVertex.X) +
				(nextVertex.Y - prevVertex.Y)*(nextVertex.Y - prevVertex.Y));
		}
		public List<mSide> ParallelClass { get; set; } = null;
		public int ClassNumber { get; private set; }

		public mSide(mVertex vertex1, mVertex vertex2, Line line, TextBlock textClassNumber, mPolygon parent) {
			prevVertex = vertex1;
			nextVertex = vertex2;
			gLine = line;
			TextClassNumber = textClassNumber;
			Parent = parent;
		}

		public bool IsClicked(double x, double y) {
			return Utils.ArePointSideClose(x, y, this);
		}

		public void Select() {
			gLine.Stroke = System.Windows.Media.Brushes.Red;
		}

		public void Unselect() {
			gLine.Stroke = IsLengthSet ? System.Windows.Media.Brushes.Blue : System.Windows.Media.Brushes.Black;
		}

		public void UpdateLine() {
			gLine.X1 = prevVertex.X;
			gLine.Y1 = prevVertex.Y;
			gLine.X2 = nextVertex.X;
			gLine.Y2 = nextVertex.Y;
			Canvas.SetLeft(TextClassNumber, (prevVertex.X + nextVertex.X)/2);
			Canvas.SetTop(TextClassNumber, (prevVertex.Y + nextVertex.Y)/2);
		}

		public void UpdateClassNumber(int number) {
			ClassNumber = number;
			TextClassNumber.Text = number.ToString();
		}

		public void RemoveParallel() {
			ParallelClass.Remove(this);
			ParallelClass = null;
			TextClassNumber.Text = "";
		}
	}
}
