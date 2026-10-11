# Block Blast – Game 2D (Unity)

Bài tập môn **Lập trình Game Cơ bản**: làm một game Block Blast hoàn chỉnh dựa trên asset Figma
(xem `link figma.txt`).

## Cách mở

1. Mở **Unity Hub → Add → Add project from disk**, chọn thư mục `Game2D_BlockBlast`.
2. Mở bằng Unity **2022.3.55f1** (cùng phiên bản với các bài trên lớp).
3. Lần đầu mở, project tự tạo scene `Assets/Scenes/BlockBlast.unity`
   (nếu chưa có thì vào menu **BlockBlast → Tạo lại scene game**).
4. Trong cửa sổ **Game**, chọn tỉ lệ màn hình dọc (vd: `1080x1920`), rồi bấm **Play**.

## Cách chơi

- Kéo 3 khối ở khay bên dưới thả vào bảng 8×8.
- Lấp đầy một **hàng** hoặc **cột** để phá khối và ghi điểm.
- Phá liên tiếp để tăng **Combo** (nhân điểm). 3 lượt liên tiếp không phá được hàng nào sẽ mất combo.
- Phá hết bảng: **Perfect Clear +300**.
- Hết chỗ đặt khối → **Game Over**. Điểm cao nhất được lưu lại.
- Phím `Esc`: tạm dừng.

## Tính năng

| Phần | Nội dung |
|---|---|
| Màn hình | Home (logo, best score, Play, Settings), Gameplay, Pause, Game Over |
| Gameplay | Bảng 8×8, 3 khối/lượt, 15 dạng khối + các phiên bản xoay, bóng mờ vị trí đặt, tô sáng hàng/cột sắp phá, làm mờ khối không còn chỗ đặt |
| Điểm | Điểm đặt khối, điểm phá hàng (nhiều hàng cùng lúc thưởng thêm), combo, perfect clear, lưu best score bằng `PlayerPrefs` |
| VFX | Khối nảy khi đặt, khối vỡ + hạt văng, rung bảng, chữ điểm bay lên, banner GREAT / COMBO, khối xám dần khi thua, popup bật nảy |
| Sound | Nhạc nền + âm click, nhấc khối, đặt khối, phá hàng (cao dần theo combo), thua, kỷ lục mới; bật/tắt riêng Sound / Music |

## Cấu trúc code (`Assets/Scripts`)

```
Scripts/
├── BlockBlastGame.cs      điều phối game: luồng màn hình, kéo thả, tính điểm, combo, game over
├── Gameplay/              luật chơi thuần, không phụ thuộc giao diện
│   ├── Board.cs           bảng 8x8: kiểm tra đặt khối, tìm hàng / cột đầy, phá hàng
│   └── Shapes.cs          các dạng khối + phiên bản xoay, random có trọng số
├── UI/                    mọi thứ hiển thị trên màn hình
│   ├── HomeScreen.cs      màn hình chính (logo, best score, Play, Settings)
│   ├── BoardView.cs       vẽ bảng, bóng mờ khi kéo, tô sáng hàng sắp phá, rung bảng
│   ├── PieceView.cs       khối trong khay: animation + sự kiện kéo thả
│   ├── Popups.cs          popup Settings / Paused / Game Over
│   ├── Effects.cs         VFX: khối vỡ, hạt văng, chữ điểm bay, banner GREAT / COMBO
│   ├── ButtonPress.cs     hiệu ứng nhún khi nhấn nút
│   ├── Pulse.cs           hiệu ứng phóng to / thu nhỏ liên tục
│   ├── Theme.cs           bảng màu
│   ├── Skin.cs            nạp hình ảnh (Inspector → Resources → vẽ bằng code)
│   └── UIKit.cs           hàm tiện ích tạo Canvas, Image, Text, Button
└── Systems/               công cụ dùng chung, độc lập với game
    ├── SoundManager.cs    âm thanh (tự tổng hợp bằng code nếu không có file)
    ├── SpriteFactory.cs   vẽ sprite khối, nút, panel bằng code
    └── Tweener.cs         tween bằng coroutine + các hàm Ease
```

Phụ thuộc chỉ đi một chiều: `BlockBlastGame` → `UI` → `Gameplay` / `Systems`. `Gameplay` và `Systems`
không tham chiếu ngược lên UI, nên luật chơi có thể đọc và kiểm tra độc lập với giao diện.

## Thay bằng hình / âm thanh từ Figma

Game chạy được ngay với hình vẽ bằng code. Để dùng asset export từ Figma, đặt file vào
`Assets/Resources/BlockBlast/` với tên:

| File | Ý nghĩa |
|---|---|
| `block_0.png` … `block_6.png` | 7 khối màu: đỏ, cam, vàng, xanh lá, xanh ngọc, xanh dương, tím |
| `background.png` | ảnh nền |
| `logo.png` | logo thay cho chữ BLOCK BLAST |
| `Font.ttf` | font chữ |
| `Audio/Music`, `Audio/Click`, `Audio/Place`, `Audio/Clear`, `Audio/GameOver`... | âm thanh (wav/mp3/ogg) |

Hoặc kéo sprite trực tiếp vào component **BlockBlastGame** trong scene (mục *Custom Block Sprites*).

## Mã nguồn

Repo: <https://github.com/datpui2610/Game2D_BlockBlast>

```bash
git clone https://github.com/datpui2610/Game2D_BlockBlast.git
```
