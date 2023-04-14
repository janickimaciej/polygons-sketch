using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Polygons {
	public class Bresenham {
		public void Draw(WriteableBitmap bitmap, int width, int height, List<mSide> sides,
			List<mVertex> vertices, List<mPolygon> polygons) {
			try {
				bitmap.Lock();
				foreach(mSide side in sides) {
					DrawLine(bitmap, (int)side.prevVertex.X, (int)side.prevVertex.Y,
						(int)side.nextVertex.X, (int)side.nextVertex.Y);
				}
				foreach(mVertex vertex in vertices) {
					DrawDot(bitmap, (int)vertex.X, (int)vertex.Y);
				}
			} finally {
				bitmap.Unlock();
			}
		}

		public void DrawLine(WriteableBitmap bitmap, int x1, int y1, int x2, int y2) {
			if(x1 == x2) {
				if(y1 > y2) (y1, y2) = (y2, y1);
				for(int i = y1; i < y2; i++) {
					DrawPixel(bitmap, x1, i);
				}
			}
			if(x1 > x2) {
				(x1, x2) = (x2, x1);
				(y1, y2) = (y2, y1);
			}
			double a = ((double)(y2 - y1))/(x2 - x1);
			int direction;
			if(a < -1) direction = -2;
			else if(a < 0) direction = -1;
			else if(a < 1) direction = 1;
			else direction = 2;
			switch(direction) {
				case -2:
					y1 *= -1;
					y2 *= -1;
					(x1, y1) = (y1, x1);
					(x2, y2) = (y2, x2);
					break;
				case -1:
					y1 *= -1;
					y2 *= -1;
					break;
				case 2:
					(x1, y1) = (y1, x1);
					(x2, y2) = (y2, x2);
					break;
			}

			int dx = x2 - x1;
			int dy = y2 - y1;
			int d = 2*dy - dx;
			int incrE = 2*dy;
			int incrNE = 2*(dy - dx);
			int x = x1;
			int y = y1;
			DrawPixel(bitmap, x, y, direction);
			while(x < x2) {
				if(d < 0) {
					d += incrE;
					x++;
				} else {
					d += incrNE;
					x++;
					y++;
				}
				DrawPixel(bitmap, x, y, direction);
			}
		}

		public void DrawDot(WriteableBitmap bitmap, int x, int y) {
			for(int i = -1; i <= 1; i++) {
				for(int j = -1; j <= 1; j++) {
					DrawPixel(bitmap, x + i, y + j);
				}
			}
		}

		public void DrawPixel(WriteableBitmap bitmap, int x, int y, int direction) {
			switch(direction) {
				case -2:
					DrawPixel(bitmap, y, -x);
					break;
				case -1:
					DrawPixel(bitmap, x, -y);
					break;
				case 1:
					DrawPixel(bitmap, x, y);
					break;
				case 2:
					DrawPixel(bitmap, y, x);
					break;
			}
		}

		public void DrawPixel(WriteableBitmap bitmap, int x, int y) {
			byte blue = 0;
			byte green = 0;
			byte red = 0;
			byte alpha = 255;
			byte[] colorData = new byte[]{ blue, green, red, alpha };
			Int32Rect rect = new Int32Rect(x, y, 1, 1);
			bitmap.WritePixels(rect, colorData, 4, 0);
		}
	}
}
