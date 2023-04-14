using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace Polygons {
	public class mModel {
		private readonly List<mPolygon> polygons;
		private readonly mDraw draw = new mDraw();
		private readonly mMove move;
		private readonly Bresenham bresenham = new Bresenham();

		public mModel() {
			polygons = new List<mPolygon>();
			move = new mMove(polygons);
		}

		public void DrawBresenham(WriteableBitmap bitmap, int width, int height) {
			bresenham.Draw(bitmap, width, height, GetSides(), GetVertices(), polygons);
		}

		public List<UIElement> DrawPolygon(double x, double y) {
			(List<UIElement> returnedElements, mPolygon newPolygon) = draw.DrawPolygon(x, y);
			if(newPolygon != null) {
				polygons.Add(newPolygon);
			}
			return returnedElements;
		}

		public void UpdateCurrentLine(double x, double y) {
			draw.UpdateCurrentLine(x, y);
		}

		public List<UIElement> StopDrawing() {
			return draw.StopDrawing();
		}

		public List<UIElement> Delete(double x, double y) {
			mClickable element = GetClickedObject(x, y, true, true, false);
			switch(element) {
				case mVertex vertex:
					return DeleteVertex(vertex);
				case mPolygon polygon:
					return DeletePolygon(polygon);
			}
			return new List<UIElement>();
		}

		private List<UIElement> DeleteVertex(mVertex deletedVertex) {
			if(deletedVertex == null) return new List<UIElement>();
			if(deletedVertex.Parent.Vertices.Count > 3) {
				move.RemoveParallel(deletedVertex.prevSide);
				move.RemoveParallel(deletedVertex.nextSide);
				return deletedVertex.Parent.DeleteVertex(deletedVertex);
			} else {
				return DeletePolygon(deletedVertex.Parent);
			}
		}

		private List<UIElement> DeletePolygon(mPolygon deletedPolygon) {
			List<UIElement> returnedElements = new List<UIElement>();
			foreach(mSide side in deletedPolygon.Sides) {
				move.RemoveParallel(side);
				returnedElements.Add(side.gLine);
			}
			foreach(mVertex vertex in deletedPolygon.Vertices) {
				returnedElements.Add(vertex.gDot);
			}
			(Line line1, Line line2) = deletedPolygon.GetCenterLines();
			returnedElements.Add(line1);
			returnedElements.Add(line2);
			polygons.Remove(deletedPolygon);
			return returnedElements;
		}

		public List<UIElement> AddMiddleVertex(double x, double y) {
			mSide side = (mSide)GetClickedObject(x, y, false, false, true);
			if(side == null) return new List<UIElement>();
			move.RemoveParallel(side);
			return side.Parent.AddMiddleVertex(side);
		}

		public void StartMoving(double x, double y) {
			mClickable element = GetClickedObject(x, y, true, true, true);
			switch(element) {
				case mVertex vertex:
					move.StartMovingVertex(x, y, vertex);
					break;
				case mSide side:
					move.StartMovingSide(x, y, side);
					break;
				case mPolygon polygon:
					move.StartMovingPolygon(x, y, polygon);
					break;
			}
		}

		public void Move(double x, double y) {
			move.Move(x, y, polygons);
		}

		public void StopMoving() {
			move.StopMoving();
		}

		public void StopParallel() {
			move.ResetParallel();
		}

		public double GetSideLength(double x, double y) {
			mSide side = (mSide)GetClickedObject(x, y, false, false, true);
			if(side == null) return -1;
			return move.GetSideLength(side);
		}

		public bool SetSideLength(double length) {
			return move.SetSideLength(length);
		}

		public void StopSetSideLength() {
			move.StopSetSideLength();
		}

		public void RemoveSideLength(double x, double y) {
			mSide side = (mSide)GetClickedObject(x, y, false, false, true);
			if(side == null) return;
			side.IsLengthSet = false;
		}

		public bool AddParallel(double x, double y) {
			mSide side = (mSide)GetClickedObject(x, y, false, false, true);
			if(side == null) {
				move.ResetParallel();
				return true;
			}
			return move.AddParallel(side);
		}

		public void RemoveParallel(double x, double y) {
			mSide side = (mSide)GetClickedObject(x, y, false, false, true);
			if(side == null) return;
			move.RemoveParallel(side);
		}

		private mClickable GetClickedObject(double x, double y, bool includeVertices,
			bool includePolygons, bool includeSides) {
			if(includeVertices) {
				foreach(mVertex vertex in GetVertices()) {
					if(vertex.IsClicked(x, y)) return vertex;
				}
			}
			if(includePolygons) {
				foreach(mPolygon polygon in polygons) {
					if(polygon.IsClicked(x, y)) return polygon;
				}
			}
			if(includeSides) {
				foreach(mSide side in GetSides()) {
					if(side.IsClicked(x, y)) return side;
				}
			}
			return null;
		}

		private List<mVertex> GetVertices() {
			List<mVertex> list = new List<mVertex>();
			foreach(mPolygon polygon in polygons) {
				foreach(mVertex vertex in polygon.Vertices) {
					list.Add(vertex);
				}
			}
			return list;
		}

		private List<mSide> GetSides() {
			List<mSide> list = new List<mSide>();
			foreach(mPolygon polygon in polygons) {
				foreach(mSide side in polygon.Sides) {
					list.Add(side);
				}
			}
			return list;
		}

		public void PredefinedScene() {
			move.AddParallel(polygons[0].Sides[0]);
			move.AddParallel(polygons[1].Sides[0]);
			move.AddParallel(polygons[0].Sides[0]);
			move.AddParallel(polygons[0].Sides[3]);
			move.GetSideLength(polygons[1].Sides[0]);
			move.SetSideLength(100);
			move.GetSideLength(polygons[1].Sides[1]);
			move.SetSideLength(150);
			move.GetSideLength(polygons[1].Sides[2]);
			move.SetSideLength(100);
			move.StopSetSideLength();
		}
	}
}
