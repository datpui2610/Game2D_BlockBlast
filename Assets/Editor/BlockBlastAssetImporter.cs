using System.IO;
using UnityEditor;
using UnityEngine;

namespace BlockBlast.EditorTools
{
    /// <summary>
    /// Thiết lập import cho ảnh trong Assets/Resources/BlockBlast khi ảnh được thêm vào lần đầu:
    /// dạng Sprite, không nén, không mipmap. Riêng các nút (button_*) được chia 3 phần trái / giữa / phải
    /// để kéo giãn theo chiều ngang mà không làm méo hai đầu bo tròn.
    /// </summary>
    class BlockBlastAssetImporter : AssetPostprocessor
    {
        const string Folder = "Assets/Resources/BlockBlast/";
        const int ButtonCap = 44; // bề rộng (pixel) của mỗi đầu bo tròn trên ảnh nút

        void OnPreprocessTexture()
        {
            var importer = (TextureImporter)assetImporter;
            if (!assetPath.StartsWith(Folder) || !importer.importSettingsMissing) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            if (Path.GetFileName(assetPath).StartsWith("button_"))
                importer.spriteBorder = new Vector4(ButtonCap, 0, ButtonCap, 0); // trái, dưới, phải, trên
        }
    }
}
