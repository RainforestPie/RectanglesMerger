using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GF2DarkZoneMapEditor
{
    /// <summary>
    /// 将相邻矩形合并成一个多边形，绘制其边
    /// 参考： https://stackoverflow.com/questions/13746284/merging-multiple-adjacent-rectangles-into-one-polygon
    /// 基本思路：
    /// 1. 去除共享顶点（最多保留1个）
    /// 2. 分别按照x和y坐标排序
    /// 3. 从x排序的点集中，解析出所有垂直方向上的边
    /// 4. 从y排序的点集中，解析出所有水平方向上的边
    /// </summary>
    public static class RectanglesMerger
    {
        /// <summary>
        /// 获取合并后的边
        /// </summary>
        /// <param name="boxColliders"></param>
        /// <returns></returns>
        public static List<(Vector2 startPoint, Vector2 endPoint)> GetEdgesAfterMerge(BoxCollider2D[] boxColliders)
        {
            var rectangles = GetRectangles(boxColliders);
            var uniquePoints = GetUniquePoints(rectangles);

            var pointsSortByX = uniquePoints.ToList();
            pointsSortByX.Sort(new XThenYComparer());
            var pointsSortByY = uniquePoints.ToList();
            pointsSortByY.Sort(new YThenXComparer());

            var edges = ResolveEdges(pointsSortByX, pointsSortByY);

            return edges;
        }

        public static void GetPolygonsAfterMerge(BoxCollider2D[] boxColliders)
        {
            var rectangles = GetRectangles(boxColliders);
            var uniquePoints = GetUniquePoints(rectangles);

            var pointsSortByX = uniquePoints.ToList();
            pointsSortByX.Sort(new XThenYComparer());
            var pointsSortByY = uniquePoints.ToList();
            pointsSortByY.Sort(new YThenXComparer());

            var edgesH = new Dictionary<Vector2, Vector2>();
            var edgesV = new Dictionary<Vector2, Vector2>();
            ResolveEdges(pointsSortByX, pointsSortByY, edgesH, edgesV);
            List<List<Vector2>> polygons = GetPolygons(edgesH, edgesV);

            foreach (var polygon in polygons)
            {
                for (int j = 0; j < polygon.Count; j++)
                {
                    Vector2 startPoint = polygon[j];
                    //Vector2 endPoint = polygon[(j + 1) % polygon.Count];
                    //Debug.DrawLine(startPoint, endPoint, Color.red, 2f); // Draw the edges of the polygon
                    Debug.Log(startPoint);
                    Debug.DrawLine(new Vector2(startPoint.x - 0.1f, startPoint.y), new Vector2(startPoint.x + 0.1f, startPoint.y), Color.red);
                }
            }
        }

        /// <summary>
        /// 根据boxCollider，得到对应的矩形
        /// </summary>
        /// <param name="boxColliders"></param>
        /// <returns></returns>
        private static List<Vector2[]> GetRectangles(BoxCollider2D[] boxColliders)
        {
            // 获取所有boxColliders的矩形顶点，存为List<Vector2[]> rectangles
            List<Vector2[]> rectangles = new List<Vector2[]>();

            foreach (var boxCollider in boxColliders)
            {
                // Calculate the corner points of the rectangle
                Vector2 p1 = new Vector2(boxCollider.bounds.min.x, boxCollider.bounds.min.y);
                Vector2 p2 = new Vector2(boxCollider.bounds.max.x, boxCollider.bounds.min.y);
                Vector2 p3 = new Vector2(boxCollider.bounds.max.x, boxCollider.bounds.max.y);
                Vector2 p4 = new Vector2(boxCollider.bounds.min.x, boxCollider.bounds.max.y);

                rectangles.Add(new Vector2[] { p1, p2, p3, p4 });
            }

            return rectangles;
        }

        /// <summary>
        /// 矩形列表所有顶点集合（去除矩形的共享顶点。奇数个共享点，则保留1个；偶数个共享点，则全都去除）
        /// </summary>
        /// <param name="rectangles"></param>
        private static List<Vector2> GetUniquePoints(List<Vector2[]> rectangles)
        {
            var uniquePoints = new List<Vector2>();

            foreach (var rect in rectangles)
            {
                foreach (var point in rect)
                {
                    if (uniquePoints.Any(o => o == point))
                    {
                        uniquePoints.RemoveAll(o => o == point);
                    }
                    else
                    {
                        uniquePoints.Add(point);
                    }
                }
            }
            return uniquePoints;
        }

        /// <summary>
        /// 解析合并后的多边形边
        /// </summary>
        /// <param name="pointsSortByX"></param>
        /// <param name="pointsSortByY"></param>
        /// <param name="edges"></param>
        private static List<(Vector2, Vector2)> ResolveEdges(List<Vector2> pointsSortByX, List<Vector2> pointsSortByY)
        {
            List<(Vector2, Vector2)> edges = new List<(Vector2, Vector2)>();

            int i = 0;
            var pointCount = pointsSortByX.Count;
            while (i < pointCount)
            {
                float currY = pointsSortByY[i].y;
                while (i < pointCount && pointsSortByY[i].y == currY)
                {
                    var startPoint = pointsSortByY[i];
                    var endPoint = pointsSortByY[i + 1];
                    edges.Add((startPoint, endPoint));
                    i += 2;
                }
            }

            i = 0;
            while (i < pointCount)
            {
                float currX = pointsSortByX[i].x;
                while (i < pointCount && pointsSortByX[i].x == currX)
                {
                    var startPoint = pointsSortByX[i];
                    var endPoint = pointsSortByX[i + 1];
                    edges.Add((startPoint, endPoint));
                    i += 2;
                }
            }
            return edges;
        }

        private static void ResolveEdges(List<Vector2> pointsSortByX, List<Vector2> pointsSortByY, Dictionary<Vector2, Vector2> edgesH, Dictionary<Vector2, Vector2> edgesV)
        {
            var pointCount = pointsSortByX.Count;
            int i = 0;
            while (i < pointCount)
            {
                float currY = pointsSortByY[i].y;
                while (i < pointCount && pointsSortByY[i].y == currY)
                {
                    var startPoint = pointsSortByY[i];
                    var endPoint = pointsSortByY[i + 1];
                    edgesH[startPoint] = endPoint;
                    edgesH[endPoint] = startPoint;
                    i += 2;
                }
            }

            i = 0;
            while (i < pointCount)
            {
                float currX = pointsSortByX[i].x;
                while (i < pointCount && pointsSortByX[i].x == currX)
                {
                    var startPoint = pointsSortByX[i];
                    var endPoint = pointsSortByX[i + 1];
                    edgesV[startPoint] = endPoint;
                    edgesV[endPoint] = startPoint;
                    i += 2;
                }
            }
        }

        private static List<List<Vector2>> GetPolygons(Dictionary<Vector2, Vector2> edgesH, Dictionary<Vector2, Vector2> edgesV)
        {
            List<List<Vector2>> polygons = new List<List<Vector2>>();

            while (edgesH.Count > 0)
            {
                // 可以任选一点开始。这里选第一个
                Vector2 current = edgesH.Keys.First();
                edgesH.Remove(current);
                var polygon = new List<Vector2>();
                bool findVertical = true;
                polygon.Add(current);

                while (true)
                {
                    current = polygon[polygon.Count - 1];
                    if (findVertical)
                    {
                        Vector2 nextVertex = edgesV[current];
                        edgesV.Remove(current);
                        polygon.Add(nextVertex);
                        findVertical = false;
                    }
                    else
                    {
                        Vector2 nextVertex = edgesH[current];
                        edgesH.Remove(current);
                        polygon.Add(nextVertex);
                        findVertical = true;
                    }

                    if (polygon[polygon.Count - 1] == polygon[0])
                    {
                        // Closed polygon
                        polygon.RemoveAt(polygon.Count - 1);
                        break;
                    }
                }

                foreach (var vertex in polygon)
                {
                    edgesH.Remove(vertex);
                    edgesV.Remove(vertex);
                }

                polygons.Add(polygon);
            }

            return polygons;
        }

        /// <summary>
        /// 先比较y坐标，再比较x坐标。通过 Mathf.Approximately 避免浮点数误差（——导致比较结果不符合预期）
        /// </summary>
        private class YThenXComparer : IComparer<Vector2>
        {
            public int Compare(Vector2 a, Vector2 b)
            {
                if (Mathf.Approximately(a.y, b.y))
                {
                    if (Mathf.Approximately(a.x, b.x))
                    {
                        return 0;
                    }
                    else if (a.x < b.x)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else if (a.y < b.y)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
        }

        /// <summary>
        /// 先比较x坐标，再比较y坐标。通过 Mathf.Approximately 避免浮点数误差（——导致比较结果不符合预期）
        /// </summary>
        private class XThenYComparer : IComparer<Vector2>
        {
            public int Compare(Vector2 a, Vector2 b)
            {
                if (Mathf.Approximately(a.x, b.x))
                {
                    if (Mathf.Approximately(a.y, b.y))
                    {
                        return 0;
                    }
                    else if (a.y < b.y)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else if (a.x < b.x)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
        }
    }
}