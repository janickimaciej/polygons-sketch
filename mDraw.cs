using System.Collections.Generic;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows;

namespace Polygons {
	public class mDraw {
		private bool isPolygonBeingDrawn = false;
		private mPolygon newPolygon;
		private List<Line> newPolygonLines;
		private List<Ellipse> newPolygonDots;
		private List<TextBlock> newPolygonTextBlocks;
		private mVertex firstVertex;
		private mVertex previousVertex;
		private Line currentLine = null;

		public (List<UIElement>, mPolygon) DrawPolygon(double x, double y) {
			List<UIElement> returnedElements = new List<UIElement>();
			mPolygon returnedPolygon = null;

			if(!isPolygonBeingDrawn) {
				DrawPolygonStart(x, y, returnedElements);
			} else {
				bool isPolygonClosed = Utils.ArePointsClose(x, y,
					firstVertex.X, firstVertex.Y);
				if(isPolygonClosed) {
					if(newPolygon.Vertices.Count < 3) return (null, null);
					DrawPolygonEnd(returnedElements);
					returnedPolygon = newPolygon;
				} else {
					DrawPolygonContinue(x, y, returnedElements);
				}
			}

			return (returnedElements, returnedPolygon);
		}

		private void DrawPolygonStart(double x, double y, List<UIElement> returnedElements) {
			isPolygonBeingDrawn = true;
			newPolygon = new mPolygon();

			Ellipse currentDot = Utils.CreateDot(x, y);
			mVertex currentVertex = new mVertex(x, y, currentDot, newPolygon);
			currentLine = Utils.CreateLine(currentVertex, currentVertex);

			newPolygon.AddVertex(currentVertex);

			newPolygonLines = new List<Line>();
			newPolygonLines.Add(currentLine);
			newPolygonDots = new List<Ellipse>();
			newPolygonDots.Add(currentDot);
			newPolygonTextBlocks = new List<TextBlock>();
			returnedElements.Add(currentLine);
			returnedElements.Add(currentDot);

			firstVertex = currentVertex;
			previousVertex = currentVertex;
		}

		private void DrawPolygonContinue(double x, double y, List<UIElement> returnedElements) {
			Ellipse currentDot = Utils.CreateDot(x, y);
			mVertex currentVertex = new mVertex(x, y, currentDot, newPolygon);
			currentLine.X2 = currentVertex.X;
			currentLine.Y2 = currentVertex.Y;

			TextBlock newTextBlock = Utils.CreateTextBlock(previousVertex, currentVertex);
			mSide newSide = new mSide(previousVertex, currentVertex, currentLine, newTextBlock, newPolygon);
			previousVertex.nextSide = newSide;
			currentVertex.prevSide = newSide;
			newPolygon.AddSide(newSide);
			newPolygon.AddVertex(currentVertex);

			currentLine = Utils.CreateLine(currentVertex, currentVertex);

			newPolygonLines.Add(currentLine);
			newPolygonDots.Add(currentDot);
			newPolygonTextBlocks.Add(newTextBlock);
			returnedElements.Add(currentLine);
			returnedElements.Add(currentDot);
			returnedElements.Add(newTextBlock);

			previousVertex = currentVertex;
		}

		private void DrawPolygonEnd(List<UIElement> returnedElements) {
			isPolygonBeingDrawn = false;

			currentLine.X2 = firstVertex.X;
			currentLine.Y2 = firstVertex.Y;

			TextBlock newTextBlock = Utils.CreateTextBlock(previousVertex, firstVertex);
			mSide newSide = new mSide(previousVertex, firstVertex, currentLine, newTextBlock, newPolygon);
			previousVertex.nextSide = newSide;
			firstVertex.prevSide = newSide;
			newPolygon.AddSide(newSide);
			newPolygon.AddCenter();

			(Line horizontalLine, Line verticalLine) = newPolygon.GetCenterLines();
			returnedElements.Add(horizontalLine);
			returnedElements.Add(verticalLine);
			returnedElements.Add(newTextBlock);
		}

		public void UpdateCurrentLine(double x, double y) {
			if(!isPolygonBeingDrawn) return;
			currentLine.X2 = x;
			currentLine.Y2 = y;
		}

		public List<UIElement> StopDrawing() {
			List<UIElement> returnedElements = new List<UIElement>();
			if(isPolygonBeingDrawn) {
				isPolygonBeingDrawn = false;
				foreach(Line line in newPolygonLines) {
					returnedElements.Add(line);
				}
				foreach(Ellipse dot in newPolygonDots) {
					returnedElements.Add(dot);
				}
				foreach(TextBlock textBlock in newPolygonTextBlocks) {
					returnedElements.Add(textBlock);
				}
			}
			return returnedElements;
		}
	}
}
