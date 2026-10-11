using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace BlockBlast
{
    /// <summary>
    /// Điều phối game Block Blast: kéo 3 khối ở khay vào bảng 8x8, lấp đầy hàng / cột để phá và ghi điểm.
    /// Hết chỗ đặt khối => Game Over. Toàn bộ UI được dựng bằng code trong Awake.
    /// </summary>
    public class BlockBlastGame : MonoBehaviour
    {
        const string BestKey = "BB_BEST";
        const int TraySize = 3;

        // tính điểm
        const int PointsPerLine = 20;
        const int PerfectClearBonus = 300;
        const int MovesToLoseCombo = 3; // số lượt liên tiếp không phá được hàng nào thì mất combo
        static readonly string[] Praise = { "GREAT!", "EXCELLENT!", "AMAZING!", "UNBELIEVABLE!" }; // 2, 3, 4, 5+ hàng

        // bố cục, tính theo độ phân giải tham chiếu 1080x1920
        static readonly Vector2 ScreenSize = new Vector2(1080, 1920);
        static readonly Vector2 BoardPosition = new Vector2(0, 90);
        const float TrayY = -720f;
        const float DragLift = 120f;    // khối được nâng lên trên ngón tay khi kéo để không bị che
        const float DragThreshold = 10f; // phải di chuyển quá khoảng này (pixel) mới tính là kéo

        [Header("Tuỳ chọn: kéo sprite xuất từ Figma vào đây (để trống = dùng hình vẽ bằng code)")]
        [Tooltip("7 sprite khối theo thứ tự: đỏ, cam, vàng, xanh lá, xanh ngọc, xanh dương, tím")]
        public Sprite[] customBlockSprites;
        public Sprite customBackground;
        public Sprite customLogo;

        // ------------------------------------------------------------------ trạng thái game
        readonly Board board = new Board();
        readonly PieceView[] tray = new PieceView[TraySize];
        int score, best, bestAtStart, combo, movesSinceClear;
        float shownScore; // điểm đang hiển thị, chạy dần tới score
        bool gameOver;

        // ------------------------------------------------------------------ giao diện
        Skin skin;
        HomeScreen home;
        RectTransform gameScreen, dragLayer;
        readonly RectTransform[] traySlots = new RectTransform[TraySize];
        Text scoreText, bestText;
        BoardView boardView;
        Effects effects;
        Popups popups;

        // ------------------------------------------------------------------ kéo thả
        PieceView dragged;
        int dragPointer;
        Vector2 pressPosition;
        bool moved;
        Vector2Int? target; // ô sẽ đặt khối nếu thả tay lúc này, null = không đặt được

        // ================================================================== khởi tạo

        void Awake()
        {
            Application.targetFrameRate = 60;
            best = PlayerPrefs.GetInt(BestKey, 0);

            SetupScene();
            SoundManager.Instance.PlayMusic();
            skin = new Skin(customBlockSprites, customBackground, customLogo);
            BuildUI();
            ShowHome();
        }

        /// <summary>Đảm bảo scene có camera, AudioListener và EventSystem để chạy được cả trong scene trống.</summary>
        static void SetupScene()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                cam = new GameObject("Main Camera").AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.orthographic = true;
                cam.transform.position = new Vector3(0, 0, -10);
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Theme.CameraBackground;

            if (FindAnyObjectByType<AudioListener>() == null) cam.gameObject.AddComponent<AudioListener>();
            if (FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        void BuildUI()
        {
            var canvas = UIKit.MakeCanvas("Canvas", transform, ScreenSize).transform;
            BuildBackground(canvas);

            var root = UIKit.MakeRect("Root", canvas, ScreenSize, Vector2.zero);
            home = HomeScreen.Create(root, ScreenSize, skin, StartGame, () => popups.ShowSettings());
            gameScreen = BuildGameScreen(root);
            effects = Effects.Create(root, ScreenSize, skin.Blocks);
            dragLayer = UIKit.MakeRect("Drag", root, ScreenSize, Vector2.zero);
            popups = Popups.Create(canvas);
        }

        void BuildBackground(Transform canvas)
        {
            var background = UIKit.MakeImage("Background", canvas, skin.Background, Color.white, Vector2.zero,
                Vector2.zero);
            UIKit.StretchFull(background.rectTransform);

            // vài đốm sáng mờ cho nền có chiều sâu
            UIKit.MakeImage("Glow", canvas, SpriteFactory.Glow(), new Color(0.45f, 0.6f, 1f, 0.18f),
                new Vector2(1400, 1400), new Vector2(-350, 600));
            UIKit.MakeImage("Glow", canvas, SpriteFactory.Glow(), new Color(0.7f, 0.4f, 1f, 0.12f),
                new Vector2(1200, 1200), new Vector2(420, -650));
        }

        RectTransform BuildGameScreen(Transform parent)
        {
            var screen = UIKit.MakeRect("GameScreen", parent, ScreenSize, Vector2.zero);

            // thanh trên cùng: nút tạm dừng, điểm cao nhất, điểm hiện tại
            var pause = UIKit.MakeButton("Pause", screen, Theme.Blue, new Vector2(124, 124), new Vector2(-430, 830),
                null, 0, ShowPause);
            var barSprite = SpriteFactory.RoundedRect(6);
            UIKit.MakeImage("Bar", pause.transform, barSprite, Color.white, new Vector2(20, 56), new Vector2(-15, 5),
                true);
            UIKit.MakeImage("Bar", pause.transform, barSprite, Color.white, new Vector2(20, 56), new Vector2(15, 5),
                true);
            bestText = UIKit.MakeText("Best", screen, "BEST 0", 48, Theme.Gold, new Vector2(600, 70),
                new Vector2(0, 840));
            scoreText = UIKit.MakeText("Score", screen, "0", 124, Color.white, new Vector2(800, 150),
                new Vector2(0, 705), TextAnchor.MiddleCenter, 5f);

            boardView = BoardView.Create(screen, BoardPosition, board, skin);

            // khay chứa 3 khối
            UIKit.MakeImage("Tray", screen, SpriteFactory.RoundedRect(40), new Color(0, 0, 0, 0.2f),
                new Vector2(1040, 380), new Vector2(0, TrayY), true);
            for (int i = 0; i < TraySize; i++)
                traySlots[i] = UIKit.MakeRect("TraySlot" + i, screen, new Vector2(330, 330),
                    new Vector2((i - 1) * 345f, TrayY));

            return screen;
        }

        // ================================================================== vòng lặp

        void Update()
        {
            if (!gameScreen.gameObject.activeSelf) return;

            if (shownScore < score)
            {
                float speed = Mathf.Max(30f, (score - shownScore) * 8f);
                shownScore = Mathf.MoveTowards(shownScore, score, speed * Time.unscaledDeltaTime);
                scoreText.text = Mathf.RoundToInt(shownScore).ToString();
            }

            if (Input.GetKeyDown(KeyCode.Escape) && !gameOver)
            {
                if (popups.IsOpen) popups.Close();
                else ShowPause();
            }
        }

        // ================================================================== luồng game

        void ShowHome()
        {
            ResetRound();
            SaveBest();
            home.SetBest(best);
            home.gameObject.SetActive(true);
            gameScreen.gameObject.SetActive(false);
        }

        void StartGame()
        {
            ResetRound();
            gameOver = false;
            score = 0;
            shownScore = 0;
            combo = 0;
            movesSinceClear = 0;
            bestAtStart = best;
            scoreText.text = "0";
            bestText.text = "BEST " + best;

            home.gameObject.SetActive(false);
            gameScreen.gameObject.SetActive(true);
            SpawnPieces();
            UpdateTray();
        }

        void ShowPause()
        {
            if (!gameOver) popups.ShowPause(StartGame, ShowHome);
        }

        /// <summary>Dọn sạch ván đang chơi: popup, bảng, khay, khối đang kéo, hiệu ứng.</summary>
        void ResetRound()
        {
            StopAllCoroutines();
            popups.Close();
            dragged = null;
            target = null;

            board.Reset();
            boardView.ResetView();
            foreach (var piece in tray)
                if (piece != null) Destroy(piece.gameObject);
            Array.Clear(tray, 0, TraySize);
            UIKit.DestroyChildren(dragLayer);
            effects.Clear();
        }

        void SpawnPieces()
        {
            var shapes = PickShapes();
            var colors = PickDistinctColors(TraySize);
            for (int i = 0; i < TraySize; i++)
            {
                var piece = PieceView.Create(traySlots[i], shapes[i], colors[i], skin.Blocks[colors[i]]);
                piece.Pressed += OnPiecePressed;
                piece.Dragged += OnPieceDragged;
                piece.Released += OnPieceReleased;
                piece.PopIn(i * 0.08f);
                tray[i] = piece;
            }
        }

        /// <summary>
        /// Chọn ngẫu nhiên 3 khối. Ưu tiên bộ mà cả 3 khối đều đặt được;
        /// sau nhiều lần thử không được thì chỉ cần ít nhất 1 khối đặt được.
        /// </summary>
        Shape[] PickShapes()
        {
            var shapes = new Shape[TraySize];
            for (int attempt = 0; attempt < 40; attempt++)
            {
                int fitting = 0;
                for (int i = 0; i < TraySize; i++)
                {
                    shapes[i] = ShapeLibrary.GetRandom();
                    if (board.FitsAnywhere(shapes[i])) fitting++;
                }
                if (fitting == TraySize || (attempt >= 20 && fitting > 0)) break;
            }
            return shapes;
        }

        static int[] PickDistinctColors(int count)
        {
            var available = new List<int>();
            for (int i = 0; i < Theme.BlockColors.Length; i++) available.Add(i);

            var colors = new int[count];
            for (int i = 0; i < count; i++)
            {
                int k = Random.Range(0, available.Count);
                colors[i] = available[k];
                available.RemoveAt(k);
            }
            return colors;
        }

        /// <summary>Làm mờ các khối không còn chỗ đặt; nếu không khối nào đặt được thì thua.</summary>
        void UpdateTray()
        {
            bool anyFits = false;
            foreach (var piece in tray)
            {
                if (piece == null) continue;
                bool fits = board.FitsAnywhere(piece.Shape);
                piece.SetPlaceable(fits);
                anyFits |= fits;
            }
            if (!anyFits) StartCoroutine(GameOverRoutine());
        }

        bool IsTrayEmpty()
        {
            foreach (var piece in tray)
                if (piece != null) return false;
            return true;
        }

        // ================================================================== kéo thả

        void OnPiecePressed(PieceView piece, PointerEventData e)
        {
            if (dragged != null || gameOver || popups.IsOpen) return;
            dragged = piece;
            dragPointer = e.pointerId;
            pressPosition = e.position;
            moved = false;

            piece.PickUp(dragLayer);
            SoundManager.Instance.Play(Sfx.Pick, 0.8f);
            FollowPointer(e.position);
        }

        void OnPieceDragged(PieceView piece, PointerEventData e)
        {
            if (piece != dragged || e.pointerId != dragPointer) return;
            if ((e.position - pressPosition).sqrMagnitude > DragThreshold * DragThreshold) moved = true;
            FollowPointer(e.position);
        }

        void OnPieceReleased(PieceView piece, PointerEventData e)
        {
            if (piece != dragged || e.pointerId != dragPointer) return;
            var origin = target;
            dragged = null;
            target = null;
            boardView.ClearPreview();

            if (origin.HasValue)
            {
                PlacePiece(piece, origin.Value);
            }
            else
            {
                if (moved) SoundManager.Instance.Play(Sfx.Invalid, 0.6f);
                piece.ReturnToSlot();
            }
        }

        void FollowPointer(Vector2 screenPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, screenPosition, null, out var local);
            float lift = dragged.Shape.Height * BoardView.CellSize * 0.5f + DragLift;
            dragged.Rect.anchoredPosition = local + new Vector2(0, lift);
            UpdatePreview();
        }

        /// <summary>Tìm ô sẽ đặt khối đang kéo; nếu đặt được thì hiện bóng mờ ở đó.</summary>
        void UpdatePreview()
        {
            boardView.ClearPreview();
            target = null;
            if (!moved) return;

            var shape = dragged.Shape;
            var origin = boardView.ShapeOriginAt(shape, dragged.Rect.position);
            if (!board.CanPlace(shape, origin)) return;

            target = origin;
            boardView.ShowPreview(shape, origin, dragged.ColorIndex, board.FindCompletedLines(shape, origin));
        }

        // ================================================================== luật chơi

        void PlacePiece(PieceView piece, Vector2Int origin)
        {
            var shape = piece.Shape;
            var lines = board.FindCompletedLines(shape, origin);
            board.Place(shape, origin, piece.ColorIndex);
            boardView.ShowPlaced(shape, origin);

            tray[Array.IndexOf(tray, piece)] = null;
            Destroy(piece.gameObject);
            SoundManager.Instance.Play(Sfx.Place);
            AddScore(shape.Cells.Length);

            if (lines.Count > 0) ClearLines(lines, origin + shape.Center);
            else if (++movesSinceClear >= MovesToLoseCombo) combo = 0;

            if (IsTrayEmpty()) SpawnPieces();
            UpdateTray();
        }

        /// <summary>Phá các hàng / cột đầy và cộng điểm. Hiệu ứng vỡ lan ra từ center (tâm khối vừa đặt).</summary>
        void ClearLines(CompletedLines lines, Vector2 center)
        {
            combo++;
            movesSinceClear = 0;

            foreach (var cell in lines.Cells)
            {
                float delay = Vector2.Distance(cell, center) * 0.035f;
                effects.BreakBlock(boardView.CellToWorld(cell), board[cell], delay);
            }
            board.ClearLines(lines);
            foreach (var cell in lines.Cells) boardView.Refresh(cell);

            // phá nhiều hàng cùng lúc được thưởng thêm: 1 hàng = 20, 2 hàng = 60, 3 hàng = 120... rồi nhân combo
            int points = PointsPerLine * lines.Count * (lines.Count + 1) / 2 * combo;
            AddScore(points);
            SoundManager.Instance.PlayClear(combo);
            boardView.Shake(lines.Count >= 2 ? 16f : 7f);
            effects.FloatingText("+" + points, boardView.CellToWorld(center), 84, Color.white);
            ShowClearBanners(lines.Count);

            if (board.IsEmpty)
            {
                AddScore(PerfectClearBonus);
                effects.Banner("PERFECT CLEAR!\n+" + PerfectClearBonus, Theme.Pink, BoardPosition + new Vector2(0, -60),
                    96, 0.35f);
                SoundManager.Instance.Play(Sfx.Fanfare);
            }
        }

        void ShowClearBanners(int lineCount)
        {
            float y = 220f;
            if (lineCount >= 2)
            {
                string praise = Praise[Mathf.Min(lineCount - 2, Praise.Length - 1)];
                effects.Banner(praise, Theme.Gold, BoardPosition + new Vector2(0, y), 110);
                y -= 140f;
            }
            if (combo >= 2) effects.Banner("COMBO x" + combo, Theme.Cyan, BoardPosition + new Vector2(0, y), 92);
        }

        void AddScore(int points)
        {
            score += points;
            if (score > best)
            {
                best = score;
                bestText.text = "BEST " + best;
            }
            Tweener.Punch(scoreText.transform, 1.12f, 0.2f);
        }

        IEnumerator GameOverRoutine()
        {
            gameOver = true;
            SaveBest();
            yield return new WaitForSecondsRealtime(0.45f);
            SoundManager.Instance.Play(Sfx.GameOver);

            // các khối trên bảng chuyển xám dần từ trên xuống
            for (int row = 0; row < Board.Size; row++)
            {
                boardView.GreyOutRow(row);
                yield return new WaitForSecondsRealtime(0.06f);
            }
            yield return new WaitForSecondsRealtime(0.5f);

            popups.ShowGameOver(score, best, score > bestAtStart, ShowHome, StartGame);
        }

        // ================================================================== lưu điểm cao nhất

        void SaveBest()
        {
            if (best <= PlayerPrefs.GetInt(BestKey, 0)) return;
            PlayerPrefs.SetInt(BestKey, best);
            PlayerPrefs.Save();
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) SaveBest();
        }

        void OnApplicationQuit() => SaveBest();
    }

    /// <summary>Tự tạo game khi bấm Play ở bất kỳ scene nào chưa có BlockBlastGame.</summary>
    static class BlockBlastBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            if (UnityEngine.Object.FindAnyObjectByType<BlockBlastGame>() == null)
                new GameObject(nameof(BlockBlastGame)).AddComponent<BlockBlastGame>();
        }
    }
}
