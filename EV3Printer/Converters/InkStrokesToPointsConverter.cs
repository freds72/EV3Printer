using EV3Printer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace EV3Printer.Converters
{
    class InkStrokesToPointsConverter
    {
        public PointStrokeCollection Convert(InkStrokeContainer strokes, double simplification, bool hq)
        {
            var pointStrokes = new PointStrokeCollection();
            foreach (InkStroke stroke in strokes.GetStrokes())
            {
                var inkPoints = stroke.GetInkPoints();
                var points = simplify(inkPoints, simplification, hq);
                pointStrokes.Add(points);
            }
            return pointStrokes;
        }

        // taken from: http://mourner.github.io/simplify-js/
        // square distance between 2 points
        double getSqDist(Point p1, Point p2)
        {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;

            return dx * dx + dy * dy;
        }

        // square distance from a point to a segment
        double getSqSegDist(Point p, Point p1, Point p2)
        {

            var x = p1.X;
            var y = p1.Y;
            var dx = p2.X - x;
            var dy = p2.Y - y;

            if (dx != 0 || dy != 0)
            {
                var t = ((p.X - x) * dx + (p.Y - y) * dy) / (dx * dx + dy * dy);

                if (t > 1)
                {
                    x = p2.X;
                    y = p2.Y;
                }
                else if (t > 0)
                {
                    x += dx * t;
                    y += dy * t;
                }
            }

            dx = p.X - x;
            dy = p.Y - y;

            return dx * dx + dy * dy;
        }
        // rest of the code doesn't care about point format

        // basic distance-based simplification
        List<Point> simplifyRadialDist(IReadOnlyList<InkPoint> points, double sqTolerance)
        {
            var prevPoint = points[0].Position;
            var newPoints = new List<Point>();
            Point point;

            for (int i = 1; i < points.Count; i++)
            {
                point = points[i].Position;

                if (getSqDist(point, prevPoint) > sqTolerance)
                {
                    newPoints.Add(point);
                    prevPoint = point;
                }
            }

            if (prevPoint != point) newPoints.Add(point);

            return newPoints;
        }

        void simplifyDPStep(IReadOnlyList<InkPoint> points, int first, int last, double sqTolerance, List<Point> simplified)
        {
            var maxSqDist = sqTolerance;
            int index = 0;

            for (int i = first + 1; i < last; i++)
            {
                var sqDist = getSqSegDist(points[i].Position, points[first].Position, points[last].Position);

                if (sqDist > maxSqDist)
                {
                    index = i;
                    maxSqDist = sqDist;
                }
            }

            if (maxSqDist > sqTolerance)
            {
                if (index - first > 1) simplifyDPStep(points, first, index, sqTolerance, simplified);
                simplified.Add(points[index].Position);
                if (last - index > 1) simplifyDPStep(points, index, last, sqTolerance, simplified);
            }
        }

        // simplification using Ramer-Douglas-Peucker algorithm
        List<Point> simplifyDouglasPeucker(IReadOnlyList<InkPoint> points, double sqTolerance)
        {
            var last = points.Count - 1;

            var simplified = new List<Point>();
            simplified.Add(points[0].Position);
            simplifyDPStep(points, 0, last, sqTolerance, simplified);
            simplified.Add(points[last].Position);

            return simplified;
        }

        // both algorithms combined for awesome performance
        List<Point> simplify(IReadOnlyList<InkPoint> points, double tolerance, bool highestQuality)
        {
            if (points.Count <= 2) return points.Select(p => p.Position).ToList();
            if ( tolerance == 0 ) return points.Select(p => p.Position).ToList();

            var sqTolerance = tolerance * tolerance;

            var tmp = highestQuality ? points.Select(p => p.Position).ToList() : simplifyRadialDist(points, sqTolerance);
            tmp = simplifyDouglasPeucker(points, sqTolerance);

            return tmp;
        }
    }
}
