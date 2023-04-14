using System.Windows.Controls;
using System.Windows.Shapes;

namespace Polygons {
	public class mVertex : mClickable {
		public double X { get; private set; }
		public double Y { get; private set; }
		public Ellipse gDot { get; }
		public mSide prevSide { get; set; }
		public mSide nextSide { get; set; }
		public mPolygon Parent { get; }

		public mVertex(double x, double y, Ellipse dot, mPolygon parent) {
			X = x;
			Y = y;
			gDot = dot;
			Parent = parent;
		}

		public bool IsClicked(double x, double y) {
			return Utils.ArePointsClose(x, y, X, Y);
		}

		public void Move(double x, double y) {
			X = x;
			Y = y;
			Canvas.SetLeft(gDot, x - gDot.Width/2);
			Canvas.SetTop(gDot, y - gDot.Height/2);
		}

		public void Translate(double dx, double dy) {
			Move(X + dx, Y + dy);
		}
	}
}
