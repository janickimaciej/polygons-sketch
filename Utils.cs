using System;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace Polygons {
	public static class Utils {
		public static Ellipse CreateDot(int x, int y) {
			Ellipse dot;
			dot = new Ellipse();
			dot.Width = 3;
			dot.Height = 3;
			dot.Stroke = System.Windows.Media.Brushes.Black;
			dot.Fill = System.Windows.Media.Brushes.Black;
			Canvas.SetLeft(dot, x - dot.Width/2);
			Canvas.SetTop(dot, y - dot.Height/2);
			return dot;
		}

		public static Ellipse CreateDot(double x, double y) {
			return CreateDot((int)x, (int)y);
		}

		public static Line CreateLine(int x1, int y1, int x2, int y2) {
			Line line = new Line();
			line.X1 = x1;
			line.Y1 = y1;
			line.X2 = x2;
			line.Y2 = y2;
			line.Stroke = System.Windows.Media.Brushes.Black;
			return line;
		}

		public static Line CreateLine(double x1, double y1, double x2, double y2) {
			return CreateLine((int)x1, (int)y1, (int)x2, (int)y2);
		}

		public static Line CreateLine(mVertex vertex1, mVertex vertex2) {
			return CreateLine(vertex1.X, vertex1.Y, vertex2.X, vertex2.Y);
		}

		public static TextBlock CreateTextBlock(mVertex vertex1, mVertex vertex2) {
			TextBlock textBlock = new TextBlock();
			Canvas.SetLeft(textBlock, (vertex1.X + vertex2.X)/2);
			Canvas.SetTop(textBlock, (vertex1.Y + vertex2.Y)/2);
			textBlock.Text = "";
			return textBlock;
		}

		public static bool ArePointsClose(double x1, double y1, double x2, double y2) {
			const double range = 7;
			return (x1 - x2)*(x1 - x2) + (y1 - y2)*(y1 - y2) <= range*range;
		}

		public static bool ArePointSideClose(double x, double y, mSide side) {
			const double range = 5;
			if((x < side.prevVertex.X && x < side.nextVertex.X) ||
				(x > side.prevVertex.X && x > side.nextVertex.X) ||
				(y < side.prevVertex.Y && y < side.nextVertex.Y) ||
				(y > side.prevVertex.Y && y > side.nextVertex.Y)) return false;
			double A = side.nextVertex.Y - side.prevVertex.Y;
			double B = side.prevVertex.X - side.nextVertex.X;
			double C = side.nextVertex.X*side.prevVertex.Y - side.prevVertex.X*side.nextVertex.Y;
			return (A*x + B*y + C)*(A*x + B*y + C) <= range*range*(A*A + B*B);
		}

		public static bool ArePointCenterClose(double x, double y, mPolygon polygon) {
			const double range = 5;
			(double centerX, double centerY) = polygon.GetCenterCoordinates();
			return x > centerX - range && x < centerX + range && y > centerY - range && y < centerY + range;
		}

		public static bool AreEqual(double x, double y) {
			const double epsilon = 1e-6;
			if(Math.Abs(x - y) < epsilon) return true;
			else return false;
		}

		public static ((double, double), (double, double)) CalculateVecCursorVertices(double x, double y,
			mSide side) {
			double x1 = side.prevVertex.X;
			double y1 = side.prevVertex.Y;
			double x2 = side.nextVertex.X;
			double y2 = side.nextVertex.Y;
			double dx = x2 - x1;
			double dy = y2 - y1;

			(double x, double y) cursorProjection;
			cursorProjection.x = (dy*dy*x1 + dx*dy*(y - y1) + dx*dx*x)/(dx*dx + dy*dy);
			cursorProjection.y = (dy*dy*y + dx*dy*(x - x1) + dx*dx*y1)/(dx*dx + dy*dy);

			return ((x1 - cursorProjection.x, y1 - cursorProjection.y),
				(x2 - cursorProjection.x, y2 - cursorProjection.y));
		}

		public static (double, double) CalculateVertexByLength(double xConst, double yConst, double xMoved,
			double yMoved, double length) {
			if(xConst == xMoved) {
				yMoved = yMoved < yConst ? yConst - length : yConst + length;
			} else {
				double a = (yMoved - yConst)/(xMoved - xConst);
				double b = yConst - a*xConst;
				double dx = length/Math.Sqrt(a*a + 1);
				xMoved = xMoved < xConst ? xConst - dx : xConst + dx;
				yMoved = a*xMoved + b;
			}
			return(xMoved, yMoved);
		}

		public static (double, double) CalculateNextVertexByParallel(mSide constSide, mSide movedSide) {
			double x11 = constSide.prevVertex.X;
			double y11 = constSide.prevVertex.Y;
			double x12 = constSide.nextVertex.X;
			double y12 = constSide.nextVertex.Y;
			double x21 = movedSide.prevVertex.X;
			double y21 = movedSide.prevVertex.Y;
			double x22 = movedSide.nextVertex.X;
			double y22 = movedSide.nextVertex.Y;

			if(x11 == x12) {
				x22 = x21;
				y22 = y22 < y21 ?
					y21 - movedSide.Length : y21 + movedSide.Length;
			} else {
				double a = (y12 - y11)/(x12 - x11);
				if(Math.Abs(a) > 1) {
					a = 1/a;
					double b = x21 - a*y21;
					double dy = movedSide.Length/Math.Sqrt(a*a + 1);
					y22 = y22 < y21 ? y21 - dy : y21 + dy;
					x22 = a*y22 + b;
				} else {
					double b = y21 - a*x21;
					double dx = movedSide.Length/Math.Sqrt(a*a + 1);
					x22 = x22 < x21 ? x21 - dx : x21 + dx;
					y22 = a*x22 + b;
				}
			}
			return (x22, y22);
		}

		public static bool HaveCommonVertex(mSide side1, mSide side2) {
			return side1.prevVertex == side2.prevVertex || side1.prevVertex == side2.nextVertex ||
				side1.nextVertex == side2.prevVertex || side1.nextVertex == side2.nextVertex;
		}

		public static bool HaveCommonVertex(List<mSide> list, mSide loneSide) {
			foreach(mSide side in list) {
				if(HaveCommonVertex(side, loneSide)) return true;
			}
			return false;
		}

		public static bool HaveCommonVertex(List<mSide> list1, List<mSide> list2) {
			foreach(mSide side in list2) {
				if(HaveCommonVertex(list1, side)) return true;
			}
			return false;
		}
	}
}
