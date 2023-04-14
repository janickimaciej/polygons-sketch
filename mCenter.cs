using System.Windows.Shapes;

namespace Polygons {
	public class mCenter {
		const double size = 5;
		public double X { get; set; }
		public double Y { get; set; }
		private readonly Line gHorizontalLine;
		private readonly Line gVerticalLine;

		public mCenter(double x, double y) {
			X = x;
			Y = y;
			gHorizontalLine = Utils.CreateLine(x - size, y, x + size, y);
			gVerticalLine = Utils.CreateLine(x, y - size, x, y + size);
		}

		public void Update(double x, double y) {
			X = x;
			Y = y;
			gHorizontalLine.X1 = x - size;
			gHorizontalLine.Y1 = y;
			gHorizontalLine.X2 = x + size;
			gHorizontalLine.Y2 = y;
			gVerticalLine.X1 = x;
			gVerticalLine.Y1 = y - size;
			gVerticalLine.X2 = x;
			gVerticalLine.Y2 = y + size;
		}

		public (Line, Line) GetLines() {
			return (gHorizontalLine, gVerticalLine);
		}
	}
}
