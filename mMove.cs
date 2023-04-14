using System.Collections.Generic;

namespace Polygons {
	enum MovingMode {
		None,
		Vertex,
		Side,
		Polygon
	}

	public class mMove {
		private readonly List<mPolygon> polygons;
		private readonly List<List<mSide>> parallelClasses = new List<List<mSide>>();

		private mVertex movedVertex;
		private mSide movedSide;
		private mPolygon movedPolygon;
		private MovingMode mode = MovingMode.None;
		private (double x, double y) vecCursorPrevVertex;
		private (double x, double y) vecCursorNextVertex;
		private mSide parallelSide = null;

		public mMove(List<mPolygon> polygons) {
			this.polygons = polygons;
		}

		public void StartMovingVertex(double x, double y, mVertex vertex) {
			movedVertex = vertex;
			mode = MovingMode.Vertex;
			MoveVertex(x, y, vertex);
		}

		public void StartMovingSide(double x, double y, mSide side) {
			movedSide = side;
			mode = MovingMode.Side;
			(vecCursorPrevVertex, vecCursorNextVertex) = Utils.CalculateVecCursorVertices(x, y, side);
			MoveSide(x, y, side);
		}

		public void StartMovingPolygon(double x, double y, mPolygon polygon) {
			movedPolygon = polygon;
			mode = MovingMode.Polygon;
			MovePolygon(x, y, polygon);
		}

		public void Move(double x, double y, List<mPolygon> polygons) {
			switch(mode) {
				case MovingMode.None:
					return;
				case MovingMode.Vertex:
					MoveVertex(x, y, movedVertex);
					return;
				case MovingMode.Side:
					MoveSide(x, y, movedSide);
					return;
				case MovingMode.Polygon:
					MovePolygon(x, y, movedPolygon);
					break;
			}
		}

		public void StopMoving() {
			mode = MovingMode.None;
		}

		public void MoveVertex(double x, double y, mVertex vertex) {
			vertex.Parent.MoveVertex(x, y, vertex);
			CorrectVertexByLength(vertex, vertex.prevSide.prevVertex, vertex.prevSide);
			CorrectVertexByLength(vertex, vertex.nextSide.nextVertex, vertex.nextSide);
			AdjustClassToSide(vertex.prevSide);
			AdjustClassToSide(vertex.nextSide);
		}

		public void MoveSide(double x, double y, mSide side) {
			side.prevVertex.Parent.MoveVertex(x + vecCursorPrevVertex.x, y + vecCursorPrevVertex.y,
				side.prevVertex);
			side.nextVertex.Parent.MoveVertex(x + vecCursorNextVertex.x, y + vecCursorNextVertex.y,
				side.nextVertex);
			CorrectVertexByLength(side.prevVertex, side.prevVertex.prevSide.prevVertex,
				side.prevVertex.prevSide);
			CorrectVertexByLength(side.nextVertex, side.nextVertex.nextSide.nextVertex,
				side.nextVertex.nextSide);
			AdjustClassToSide(side.prevVertex.prevSide);
			AdjustClassToSide(side.nextVertex.nextSide);
		}

		private void CorrectVertexByLength(mVertex constVertex, mVertex vertex,
			mSide side) {
			if((mode == MovingMode.Vertex && vertex == movedVertex) ||
				(mode == MovingMode.Side && (vertex == movedSide.prevVertex ||
				vertex == movedSide.nextVertex))) {
				(vertex, constVertex) = (constVertex, vertex);
			}
			if(side.IsLengthSet && !Utils.AreEqual(side.ActualLength, side.Length)) {
				(double newX, double newY) = Utils.CalculateVertexByLength(constVertex.X, constVertex.Y,
					vertex.X, vertex.Y, side.Length);
				MoveVertex(newX, newY, vertex);
			}
		}

		private void AdjustSideToClass(mSide side) {
			if(side.ParallelClass == null) return;
			mSide referenceSide = null;
			foreach(mSide classSide in side.ParallelClass) {
				if(classSide != side) {
					referenceSide = classSide;
					break;
				}
			}
			AdjustSideToSide(referenceSide, side);
		}

		private void AdjustClassToSide(mSide side) {
			if(side.ParallelClass == null) return;
			foreach(mSide classSide in side.ParallelClass) {
				if(classSide != side) {
					AdjustSideToSide(side, classSide);
				}
			}
		}

		private void AdjustSideToSide(mSide constSide, mSide side) {
			(double newX, double newY) = Utils.CalculateNextVertexByParallel(constSide, side);
			if(!Utils.AreEqual(newX, side.nextVertex.X) || !Utils.AreEqual(newY, side.nextVertex.Y)) {
				MoveVertex(newX, newY, side.nextVertex);
			}
		}

		public void MovePolygon(double x, double y, mPolygon polygon) {
			polygon.Move(x, y);
		}

		public double GetSideLength(mSide side) {
			if(movedSide != null) movedSide.Unselect();
			side.Select();
			movedSide = side;
			return side.Length;
		}

		public void StopSetSideLength() {
			if(movedSide != null) movedSide.Unselect();
			movedSide = null;
		}

		public bool SetSideLength(double length) {
			double otherSidesLength = 0;
			bool areAllLengthsSet = true;
			foreach(mSide side in movedSide.Parent.Sides) {
				if(side != movedSide) {
					if(!side.IsLengthSet) {
						areAllLengthsSet = false;
						break;
					}
					otherSidesLength += side.Length;
				}
			}
			if(areAllLengthsSet && length > otherSidesLength) return false;

			movedSide.Length = length;
			movedSide.IsLengthSet = true;

			mode = MovingMode.Vertex;
			movedVertex = movedSide.prevVertex;
			CorrectVertexByLength(movedSide.prevVertex, movedSide.nextVertex, movedSide);
			mode = MovingMode.None;
			return true;
		}

		public void ResetParallel() {
			if(parallelSide != null) {
				parallelSide.Unselect();
				parallelSide = null;
			}
		}

		public bool AddParallel(mSide side) {
			if(parallelSide == null) {
				parallelSide = side;
				parallelSide.Select();
				return true;
			}

			parallelSide.Unselect();
			side.Unselect();
			if(parallelSide.ParallelClass == null && side.ParallelClass == null) {
				if(Utils.HaveCommonVertex(parallelSide, side)) return false;

				CreateClass(parallelSide, side);
			} else if(parallelSide.ParallelClass != null && side.ParallelClass != null) {
				if(Utils.HaveCommonVertex(parallelSide.ParallelClass, side.ParallelClass)) return false;

				MergeClasses(parallelSide.ParallelClass, side.ParallelClass);
			} else {
				if(side.ParallelClass != null) (side, parallelSide) = (parallelSide, side);
				if(Utils.HaveCommonVertex(parallelSide.ParallelClass, side)) return false;

				parallelSide.ParallelClass.Add(side);
				side.ParallelClass = parallelSide.ParallelClass;
				side.UpdateClassNumber(parallelSide.ClassNumber);
				AdjustSideToClass(side);
			}
			parallelSide = null;
			return true;
		}

		private void CreateClass(mSide side1, mSide side2) {
			List<mSide> newClass = new List<mSide>();
			parallelClasses.Add(newClass);

			newClass.Add(side1);
			side1.ParallelClass = newClass;
			side1.UpdateClassNumber(parallelClasses.Count - 1);
			newClass.Add(side2);
			side2.ParallelClass = newClass;
			side2.UpdateClassNumber(parallelClasses.Count - 1);
			AdjustSideToClass(side2);
		}

		private void MergeClasses(List<mSide> constList, List<mSide> removedList) {
			constList.AddRange(removedList);
			int constListIndex = constList[0].ClassNumber;
			int removedListIndex = removedList[0].ClassNumber;
			foreach(mSide side in removedList) {
				side.ParallelClass = constList;
				side.UpdateClassNumber(constListIndex);
				AdjustSideToClass(side);
			}
			RemoveParallelClass(removedListIndex);
		}

		public void RemoveParallel(mSide side) {
			if(side.ParallelClass == null) return;
			if(side.ParallelClass.Count == 2) {
				mSide anotherSide = side == side.ParallelClass[0] ?
					side.ParallelClass[1] : side.ParallelClass[0];
				RemoveParallelClass(side.ClassNumber);
				anotherSide.RemoveParallel();
			}
			side.RemoveParallel();
		}

		private void RemoveParallelClass(int removedListIndex) {
			for(int i = removedListIndex + 1; i < parallelClasses.Count; i++) {
				foreach(mSide side in parallelClasses[i]) {
					side.UpdateClassNumber(side.ClassNumber - 1);
				}
			}
			parallelClasses.RemoveAt(removedListIndex);
		}
	}
}
