using UnityEngine;

namespace BlockBlast
{
    /// <summary>Bảng màu dùng chung của game.</summary>
    public static class Theme
    {
        /// <summary>7 màu khối theo thứ tự: đỏ, cam, vàng, xanh lá, xanh ngọc, xanh dương, tím.</summary>
        public static readonly Color[] BlockColors =
        {
            Hex("#F04A3E"),
            Hex("#FF9A2E"),
            Hex("#FFD23A"),
            Hex("#4CD964"),
            Hex("#2FD0F0"),
            Hex("#3F7BFF"),
            Hex("#B05CF5"),
        };

        // nút bấm & chữ
        public static readonly Color Green = Hex("#43C95A");
        public static readonly Color Orange = Hex("#FFA62B");
        public static readonly Color Blue = Hex("#3F7BFF");
        public static readonly Color Gold = Hex("#FFD23A");
        public static readonly Color Grey = Hex("#7A84A8");
        public static readonly Color Cyan = Hex("#7CF0FF");
        public static readonly Color Pink = Hex("#FF8BEA");
        public static readonly Color PaleBlue = Hex("#8FB4FF");
        public static readonly Color TextOutline = new Color(0.03f, 0.05f, 0.2f, 0.75f);

        // nền, bảng, popup
        public static readonly Color CameraBackground = Hex("#0B1033");
        public static readonly Color BackgroundTop = Hex("#2B44A8");
        public static readonly Color BackgroundBottom = Hex("#0A0F35");
        public static readonly Color BoardFrame = Hex("#4F74E8");
        public static readonly Color Navy = Hex("#0D1540"); // nền bảng, ô điểm trong Game Over
        public static readonly Color EmptySlot = Hex("#1A2766");
        public static readonly Color PopupFrame = Hex("#6A95FF");
        public static readonly Color PopupPanel = Hex("#1D3796");

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }
    }
}
