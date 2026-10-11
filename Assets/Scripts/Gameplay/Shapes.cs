using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlockBlast
{
    /// <summary>Một hình khối. Mỗi ô là Vector2Int với x = cột, y = hàng (hàng 0 ở trên cùng).</summary>
    public class Shape
    {
        public Vector2Int[] Cells { get; }
        public int Width { get; }
        public int Height { get; }

        public Shape(Vector2Int[] cells, int width, int height)
        {
            Cells = cells;
            Width = width;
            Height = height;
        }

        /// <summary>Tâm của hình tính theo ô, vd: khối 3x2 có tâm (1, 0.5).</summary>
        public Vector2 Center => new Vector2((Width - 1) / 2f, (Height - 1) / 2f);

        /// <summary>Tạo hình từ chuỗi mẫu: 'X' = có ô, '.' = trống, '/' = xuống hàng. Vd: "XX./.XX".</summary>
        public static Shape Parse(string pattern)
        {
            string[] rows = pattern.Split('/');
            var cells = new List<Vector2Int>();
            for (int y = 0; y < rows.Length; y++)
                for (int x = 0; x < rows[y].Length; x++)
                    if (rows[y][x] == 'X') cells.Add(new Vector2Int(x, y));
            return new Shape(cells.ToArray(), rows[0].Length, rows.Length);
        }

        /// <summary>Xoay 90 độ theo chiều kim đồng hồ.</summary>
        public Shape Rotate()
        {
            var rotated = new Vector2Int[Cells.Length];
            for (int i = 0; i < Cells.Length; i++)
                rotated[i] = new Vector2Int(Height - 1 - Cells[i].y, Cells[i].x);
            return new Shape(rotated, Height, Width);
        }

        /// <summary>Chuỗi mô tả hình (vd: "2x2:XXXX"); hai hình giống hệt nhau cho cùng một chuỗi.</summary>
        public override string ToString()
        {
            var grid = new char[Width * Height];
            for (int i = 0; i < grid.Length; i++) grid[i] = '.';
            foreach (var cell in Cells) grid[cell.y * Width + cell.x] = 'X';
            return Width + "x" + Height + ":" + new string(grid);
        }
    }

    /// <summary>Các dạng khối của game và cách chọn ngẫu nhiên theo trọng số.</summary>
    public static class ShapeLibrary
    {
        // (mẫu hình, trọng số xuất hiện). Mỗi mẫu được sinh thêm các phiên bản xoay.
        static readonly (string Pattern, int Weight)[] Patterns =
        {
            ("X", 2),
            ("XX", 3),
            ("XXX", 3),
            ("XXXX", 2),
            ("XXXXX", 1),
            ("XX/XX", 3),
            ("XXX/XXX", 2),
            ("XXX/XXX/XXX", 1),
            ("X./XX", 3),          // chữ L nhỏ
            ("X../X../XXX", 2),    // chữ L lớn
            ("X./X./XX", 2),       // L
            (".X/.X/XX", 2),       // J
            ("XXX/.X.", 2),        // T
            (".XX/XX.", 2),        // S
            ("XX./.XX", 2),        // Z
        };

        static readonly List<Shape>[] Rotations = BuildRotations();
        static readonly int TotalWeight = Patterns.Sum(p => p.Weight);

        /// <summary>Chọn mẫu theo trọng số, rồi chọn ngẫu nhiên một hướng xoay của mẫu đó.</summary>
        public static Shape GetRandom()
        {
            int pick = Random.Range(0, TotalWeight);
            int i = 0;
            while (pick >= Patterns[i].Weight)
            {
                pick -= Patterns[i].Weight;
                i++;
            }
            var rotations = Rotations[i];
            return rotations[Random.Range(0, rotations.Count)];
        }

        /// <summary>Với mỗi mẫu, sinh các hướng xoay khác nhau (bỏ hướng trùng, vd: khối vuông chỉ có 1).</summary>
        static List<Shape>[] BuildRotations()
        {
            var result = new List<Shape>[Patterns.Length];
            for (int i = 0; i < Patterns.Length; i++)
            {
                var seen = new HashSet<string>();
                var shape = Shape.Parse(Patterns[i].Pattern);
                result[i] = new List<Shape>();
                for (int turn = 0; turn < 4; turn++, shape = shape.Rotate())
                    if (seen.Add(shape.ToString())) result[i].Add(shape);
            }
            return result;
        }
    }
}
