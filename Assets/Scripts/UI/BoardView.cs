using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BlockBlast
{
    /// <summary>Hiển thị bảng 8x8: khung, ô nền, các khối, bóng mờ khi đang kéo khối và hiệu ứng rung.</summary>
    public class BoardView : MonoBehaviour
    {
        public const float CellSize = 118f;
        const float Mid = (Board.Size - 1) / 2f; // toạ độ (theo ô) của tâm bảng
        static readonly Color GhostColor = new Color(1, 1, 1, 0.45f);

        Board board;
        Skin skin;
        RectTransform frame, grid;
        readonly Image[,] blocks = new Image[Board.Size, Board.Size];
        readonly List<Vector2Int> previewCells = new List<Vector2Int>();
        Vector2 homePosition;
        Coroutine shake;

        public static BoardView Create(Transform parent, Vector2 position, Board board, Skin skin)
        {
            float gridSize = CellSize * Board.Size + 28f;
            var frame = UIKit.MakeImage("BoardFrame", parent, SpriteFactory.RoundedRect(36), Theme.BoardFrame,
                Vector2.one * (gridSize + 18f), position, true).rectTransform;

            var view = frame.gameObject.AddComponent<BoardView>();
            view.board = board;
            view.skin = skin;
            view.frame = frame;
            view.homePosition = position;
            view.grid = UIKit.MakeImage("Board", frame, SpriteFactory.RoundedRect(30), Theme.Navy,
                Vector2.one * gridSize, Vector2.zero, true).rectTransform;
            view.BuildCells();
            return view;
        }

        void BuildCells()
        {
            // tạo hết ô nền trước để các khối luôn nằm phía trên
            var slotSprite = SpriteFactory.RoundedRect(12);
            for (int row = 0; row < Board.Size; row++)
                for (int col = 0; col < Board.Size; col++)
                    UIKit.MakeImage("Slot", grid, slotSprite, Theme.EmptySlot, Vector2.one * (CellSize - 8),
                        CellToLocal(new Vector2(col, row)), true);

            for (int row = 0; row < Board.Size; row++)
                for (int col = 0; col < Board.Size; col++)
                {
                    var img = UIKit.MakeImage("Block_" + row + "_" + col, grid, null, Color.white,
                        Vector2.one * (CellSize - 4), CellToLocal(new Vector2(col, row)));
                    img.enabled = false;
                    blocks[row, col] = img;
                }
        }

        // ---------------------------------------------------------------- toạ độ

        /// <summary>Vị trí tâm ô trong bảng. cell tính theo ô (x = cột, y = hàng), có thể là số lẻ.</summary>
        static Vector2 CellToLocal(Vector2 cell) => new Vector2(cell.x - Mid, Mid - cell.y) * CellSize;

        /// <summary>Vị trí thế giới của tâm ô (dùng để đặt hiệu ứng).</summary>
        public Vector3 CellToWorld(Vector2 cell) => grid.TransformPoint(CellToLocal(cell));

        /// <summary>Ô trên-trái mà shape sẽ chiếm nếu tâm của shape nằm ở worldPos.</summary>
        public Vector2Int ShapeOriginAt(Shape shape, Vector3 worldPos)
        {
            Vector2 local = grid.InverseTransformPoint(worldPos);
            var center = new Vector2(local.x / CellSize + Mid, Mid - local.y / CellSize);
            return Vector2Int.RoundToInt(center - shape.Center);
        }

        // ---------------------------------------------------------------- vẽ bảng

        /// <summary>Vẽ lại toàn bộ bảng theo dữ liệu của Board.</summary>
        public void Refresh()
        {
            for (int row = 0; row < Board.Size; row++)
                for (int col = 0; col < Board.Size; col++)
                    Refresh(new Vector2Int(col, row));
        }

        public void Refresh(Vector2Int cell)
        {
            var img = Block(cell);
            img.rectTransform.localScale = Vector3.one;
            img.enabled = board.IsFilled(cell);
            if (img.enabled) Paint(img, skin.Blocks[board[cell]], Color.white);
        }

        /// <summary>Vẽ các ô vừa đặt kèm hiệu ứng nảy.</summary>
        public void ShowPlaced(Shape shape, Vector2Int origin)
        {
            foreach (var c in shape.Cells)
            {
                var cell = origin + c;
                Refresh(cell);
                Tweener.Punch(Block(cell).transform, 1.15f, 0.18f);
            }
        }

        /// <summary>
        /// Hiện bóng mờ chỗ sẽ đặt khối, đồng thời tô các hàng / cột sắp bị phá bằng màu của khối đang kéo.
        /// Gọi ClearPreview trước khi hiện bóng mờ mới.
        /// </summary>
        public void ShowPreview(Shape shape, Vector2Int origin, int color, CompletedLines lines)
        {
            var sprite = skin.Blocks[color];
            foreach (var cell in shape.Cells) SetPreview(origin + cell, sprite, GhostColor);
            foreach (var cell in lines.Cells)
                if (board.IsFilled(cell)) SetPreview(cell, sprite, Color.white);
        }

        public void ClearPreview()
        {
            foreach (var cell in previewCells) Refresh(cell);
            previewCells.Clear();
        }

        /// <summary>Đổi các khối trên một hàng sang màu xám (khi thua).</summary>
        public void GreyOutRow(int row)
        {
            for (int col = 0; col < Board.Size; col++)
            {
                var cell = new Vector2Int(col, row);
                if (!board.IsFilled(cell)) continue;
                Block(cell).sprite = skin.GreyBlock;
                Tweener.Punch(Block(cell).transform, 1.1f, 0.15f);
            }
        }

        public void Shake(float strength)
        {
            Tweener.Stop(shake);
            shake = Tweener.Run(frame, 0.3f,
                t => frame.anchoredPosition = homePosition + Random.insideUnitCircle * (strength * (1f - t)),
                () => frame.anchoredPosition = homePosition);
        }

        /// <summary>Đưa bảng về trạng thái ban đầu: dừng rung, xoá bóng mờ, vẽ lại theo Board.</summary>
        public void ResetView()
        {
            Tweener.Stop(shake);
            frame.anchoredPosition = homePosition;
            previewCells.Clear();
            Refresh();
        }

        void SetPreview(Vector2Int cell, Sprite sprite, Color color)
        {
            var img = Block(cell);
            img.enabled = true;
            Paint(img, sprite, color);
            previewCells.Add(cell);
        }

        static void Paint(Image img, Sprite sprite, Color color)
        {
            img.sprite = sprite;
            img.color = color;
        }

        Image Block(Vector2Int cell) => blocks[cell.y, cell.x];
    }
}
