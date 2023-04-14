using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace Polygons {
	public class mPolygon : mClickable {
		public List<mSide> Sides { get; private set; } = new List<mSide>();
		public List<mVertex> Vertices { get; private set; } = new List<mVertex>();
		private mCenter center;

		public bool IsClicked(double x, double y) {
			return Utils.ArePointCenterClose(x, y, this);
		}

		public void AddSide(mSide side) {
			Sides.Add(side);
		}

		public void AddVertex(mVertex vertex) {
			Vertices.Add(vertex);
		}

		public void AddCenter() {
			(double x, double y) = CalculateCenter();
			center = new mCenter(x, y);
		}

		public List<UIElement> DeleteVertex(mVertex deletedVertex) {
			List<UIElement> returnedElements = new List<UIElement>();
			returnedElements.Add(deletedVertex.nextSide.gLine);
			returnedElements.Add(deletedVertex.nextSide.TextClassNumber);
			returnedElements.Add(deletedVertex.gDot);

			deletedVertex.nextSide.nextVertex.prevSide = deletedVertex.prevSide;
			deletedVertex.prevSide.nextVertex = deletedVertex.nextSide.nextVertex;
			deletedVertex.prevSide.UpdateLine();

			deletedVertex.prevSide.IsLengthSet = false;
			Sides.Remove(deletedVertex.nextSide);
			Vertices.Remove(deletedVertex);
			UpdateCenter();

			return returnedElements;
		}

		public List<UIElement> AddMiddleVertex(mSide side) {
			double newX = (side.prevVertex.X + side.nextVertex.X)/2;
			double newY = (side.prevVertex.Y + side.nextVertex.Y)/2;

			Ellipse newDot = Utils.CreateDot(newX, newY);
			mVertex newVertex = new mVertex(newX, newY, newDot, this);
			Line newLine = Utils.CreateLine(newVertex, side.nextVertex);
			TextBlock newTextBlock = Utils.CreateTextBlock(newVertex, side.nextVertex);
			mSide newSide = new mSide(newVertex, side.nextVertex, newLine, newTextBlock, this);
			side.nextVertex = newVertex;
			newVertex.prevSide = side;
			newVertex.nextSide = newSide;
			newSide.nextVertex.prevSide = newSide;
			side.UpdateLine();

			List<UIElement> returnedElements= new List<UIElement>();
			returnedElements.Add(newDot);
			returnedElements.Add(newLine);
			returnedElements.Add(newTextBlock);

			side.IsLengthSet = false;
			Sides.Insert(Sides.FindIndex(s => s == side.nextVertex.nextSide.nextVertex.nextSide), newSide);
			Vertices.Insert(Vertices.FindIndex(v => v == side.nextVertex.nextSide.nextVertex), newVertex);
			UpdateCenter();

			return returnedElements;
		}

		public void MoveVertex(double newX, double newY, mVertex vertex) {
			vertex.Move(newX, newY);
			vertex.prevSide.UpdateLine();
			vertex.nextSide.UpdateLine();
			UpdateCenter();
		}

		public void Move(double newX, double newY) {
			(double oldX, double oldY) = GetCenterCoordinates();
			double dx = newX - oldX;
			double dy = newY - oldY;
			foreach(mVertex vertex in Vertices) {
				vertex.Translate(dx, dy);
			}
			foreach(mSide side in Sides) {
				side.UpdateLine();
			}
			UpdateCenter();
		}

		private void UpdateCenter() {
			(double x, double y) = CalculateCenter();
			center.Update(x, y);
		}

		private (double, double) CalculateCenter() {
			double centerX = 0, centerY = 0;
			foreach(mVertex vertex in Vertices) {
				centerX += vertex.X;
				centerY += vertex.Y;
			}
			return (centerX/Vertices.Count, centerY/Vertices.Count);
		}

		public (Line, Line) GetCenterLines() {
			return center.GetLines();
		}

		public (double, double) GetCenterCoordinates() {
			return (center.X, center.Y);
		}
	}
}
