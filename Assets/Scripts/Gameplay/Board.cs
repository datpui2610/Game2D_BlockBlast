using System.Collections.Generic;
using UnityEngine;

namespace BlockBlast
{
    /// <summary>
    /// Luật chơi trên bảng 8x8, không phụ thuộc giao diện.
    /// Toạ độ ô là Vector2Int: x = cột, y = hàng (hàng 0 ở trên cùng), giống với Shape.
    /// </summary>
    public class Board
    {
        public const int Size = 8;
        const int Empty = -1;

        readonly int[,] colors = new int[Size, Size]; // [hàng, cột] = chỉ số màu của khối, Empty = ô trống

        public Board() => Reset();

        /// <summary>Chỉ số màu của khối trong ô (ô phải đang có khối).</summary>
        public int this[Vector2Int cell] => colors[cell.y, cell.x];

        public bool IsFilled(Vector2Int cell) => this[cell] != Empty;

        public bool IsEmpty
        {
            get
            {
                foreach (int color in colors)
                    if (color != Empty) return false;
                return true;
            }
        }

        public static bool Contains(Vector2Int cell) =>
            cell.x >= 0 && cell.x < Size && cell.y >= 0 && cell.y < Size;

        public void Reset()
        {
            for (int row = 0; row < Size; row++)
                for (int col = 0; col < Size; col++)
                    colors[row, col] = Empty;
        }

        /// <summary>Đặt được shape với ô trên-trái tại origin không (nằm trong bảng, không đè khối khác).</summary>
        public bool CanPlace(Shape shape, Vector2Int origin)
        {
            foreach (var cell in shape.Cells)
            {
                var p = origin + cell;
                if (!Contains(p) || IsFilled(p)) return false;
            }
            return true;
        }

        /// <summary>Shape còn chỗ đặt ở bất kỳ đâu trên bảng không.</summary>
        public bool FitsAnywhere(Shape shape)
        {
            for (int row = 0; row <= Size - shape.Height; row++)
                for (int col = 0; col <= Size - shape.Width; col++)
                    if (CanPlace(shape, new Vector2Int(col, row))) return true;
            return false;
        }

        public void Place(Shape shape, Vector2Int origin, int color)
        {
            foreach (var cell in shape.Cells) Set(origin + cell, color);
        }

        /// <summary>Các hàng / cột sẽ đầy nếu đặt shape tại origin (dùng cả khi xem trước lẫn khi đặt thật).</summary>
        public CompletedLines FindCompletedLines(Shape shape, Vector2Int origin)
        {
            var filled = new bool[Size, Size];
            for (int row = 0; row < Size; row++)
                for (int col = 0; col < Size; col++)
                    filled[row, col] = colors[row, col] != Empty;
            foreach (var cell in shape.Cells)
            {
                var p = origin + cell;
                filled[p.y, p.x] = true;
            }

            var lines = new CompletedLines();
            for (int i = 0; i < Size; i++)
            {
                bool rowFull = true, colFull = true;
                for (int j = 0; j < Size; j++)
                {
                    rowFull &= filled[i, j];
                    colFull &= filled[j, i];
                }
                if (rowFull) lines.AddRow(i);
                if (colFull) lines.AddColumn(i);
            }
            return lines;
        }

        public void ClearLines(CompletedLines lines)
        {
            foreach (var cell in lines.Cells) Set(cell, Empty);
        }

        void Set(Vector2Int cell, int color) => colors[cell.y, cell.x] = color;
    }

    /// <summary>Những hàng / cột đã đầy và sẽ bị phá.</summary>
    public class CompletedLines
    {
        /// <summary>Tổng số hàng + cột đầy.</summary>
        public int Count { get; private set; }

        /// <summary>Các ô thuộc những hàng / cột đó (không trùng lặp).</summary>
        public HashSet<Vector2Int> Cells { get; } = new HashSet<Vector2Int>();

        public void AddRow(int row)
        {
            Count++;
            for (int col = 0; col < Board.Size; col++) Cells.Add(new Vector2Int(col, row));
        }

        public void AddColumn(int col)
        {
            Count++;
            for (int row = 0; row < Board.Size; row++) Cells.Add(new Vector2Int(col, row));
        }
    }
}
