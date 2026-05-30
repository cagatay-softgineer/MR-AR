using System;
using System.IO;
using ARFishing.Creatures;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace ARFishing.Editor
{
    /// <summary>
    /// Generates a unique high-feature procedural PNG for each creature in CreatureDatabase
    /// suitable for ARCore image tracking. Each marker has a creatureId-derived color palette
    /// and layout, so ARCore can distinguish 20 markers in the same library. Also attempts
    /// to programmatically populate CardLibrary.asset entries; falls back to manual
    /// instructions if Unity's XRReferenceImageLibrary serialization doesn't match.
    /// </summary>
    public static class CreateTestMarkerSetMenu
    {
        const string MarkersDir = "Assets/ARFishing/Content/CardImages";
        const string CardLibraryPath = "Assets/ARFishing/Content/CardLibrary.asset";
        const int Size = 1024;

        [MenuItem("ARFishing/Create Test Marker Set (20 creatures + CardLibrary)")]
        public static void Create()
        {
            var db = LoadCreatureDatabase();
            if (db == null) return;

            EnsureDir(MarkersDir);

            // Always regenerate (no skip-existing) so algorithm tweaks take effect on re-run.
            int generated = 0;
            foreach (var def in db.All)
            {
                if (def == null || string.IsNullOrEmpty(def.CreatureId)) continue;
                var path = $"{MarkersDir}/marker_{def.CreatureId}.png";
                WriteMarkerPng(path, def.CreatureId);
                generated++;
            }
            int skipped = 0;

            AssetDatabase.Refresh();

            foreach (var def in db.All)
            {
                if (def == null || string.IsNullOrEmpty(def.CreatureId)) continue;
                var path = $"{MarkersDir}/marker_{def.CreatureId}.png";
                ConfigureImporter(path);
            }

            Debug.Log($"[ARFishing] Test marker textures: generated {generated}, skipped {skipped} (already existed).");

            int populated = TryPopulateCardLibrary(db);
            if (populated < 0)
            {
                Debug.LogWarning(
                    "[ARFishing] Could not auto-populate CardLibrary entries (Unity AR Foundation serialization format mismatch). " +
                    "Open Content/CardLibrary.asset, click 'Add Image' per creature, set Name = creatureId, Specify Size = on, " +
                    "physical width = 0.105 m, drag the matching marker_<creatureId>.png into Texture.");
            }
            else
            {
                Debug.Log($"[ARFishing] CardLibrary populated: {populated} entries created/updated.");
            }
        }

        // ---------- texture synthesis ----------

        static void WriteMarkerPng(string assetPath, string creatureId)
        {
            var pixels = SynthesizeForCreature(creatureId);
            var tex = new Texture2D(Size, Size, TextureFormat.RGB24, false);
            tex.SetPixels32(pixels);
            tex.Apply();
            File.WriteAllBytes(Path.GetFullPath(assetPath), tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        static Color32[] SynthesizeForCreature(string creatureId)
        {
            // FIXED parameters matching the proven CreateTestMarkerMenu (Phase 3, octopus that
            // ARCore reliably tracked). Only the random SEED varies per creature → same overall
            // feature density (ARCore-friendly), unique placement (distinguishable).
            int seed = StableHash(creatureId);
            var rng = new System.Random(seed);
            var pixels = new Color32[Size * Size];

            // Background: low-amplitude gray noise (no creature-specific tint).
            for (int i = 0; i < pixels.Length; i++)
            {
                byte v = (byte)(70 + rng.Next(0, 140));
                pixels[i] = new Color32(v, v, v, 255);
            }

            // Layer 1: 24 large colored circles (vivid random palette, NOT creature-tinted).
            for (int i = 0; i < 24; i++)
            {
                int cx = rng.Next(60, Size - 60);
                int cy = rng.Next(60, Size - 60);
                int r = rng.Next(40, 140);
                DrawCircle(pixels, cx, cy, r, RandomVividColor(rng));
            }

            // Layer 2: 60 medium random rectangles for hard edges.
            for (int i = 0; i < 60; i++)
            {
                int x = rng.Next(0, Size - 30);
                int y = rng.Next(0, Size - 30);
                int w = rng.Next(15, 70);
                int h = rng.Next(15, 70);
                DrawRect(pixels, x, y, w, h, RandomVividColor(rng));
            }

            // Layer 3: 120 small accent circles for keypoint density.
            for (int i = 0; i < 120; i++)
            {
                int cx = rng.Next(10, Size - 10);
                int cy = rng.Next(10, Size - 10);
                int r = rng.Next(4, 18);
                DrawCircle(pixels, cx, cy, r, RandomVividColor(rng));
            }

            // Layer 4: 40 high-contrast crisscrossing lines.
            for (int i = 0; i < 40; i++)
            {
                int x0 = rng.Next(0, Size);
                int y0 = rng.Next(0, Size);
                int x1 = rng.Next(0, Size);
                int y1 = rng.Next(0, Size);
                Color32 color = (i % 2 == 0) ? new Color32(20, 20, 20, 255) : new Color32(235, 235, 235, 255);
                DrawLine(pixels, x0, y0, x1, y1, color, thickness: 2);
            }

            // Asymmetry bias: 30 extra shapes in a creatureId-specific corner.
            int corner = seed & 3;
            BiasClusterToCorner(pixels, corner, rng);

            // Unique signature: a large high-contrast block placed at one of 20 distinct
            // grid positions (5×4 grid). Each creature gets a unique (position + inner shape)
            // pair via hash, giving ARCore a STRONG distinguishing feature so it can tell
            // the 20 markers apart instead of matching them all to one entry.
            DrawCreatureSignature(pixels, seed);

            return pixels;
        }

        static void DrawCreatureSignature(Color32[] pixels, int seed)
        {
            uint useed = (uint)seed;
            int positionIdx = (int)(useed % 20);
            int gridX = positionIdx % 5;
            int gridY = positionIdx / 5;

            // Grid: 5 cols × 4 rows over 1024×1024 → cell size ~205×256.
            // Place signature center at cell center.
            int cellW = Size / 5;
            int cellH = Size / 4;
            int cx = cellW * gridX + cellW / 2;
            int cy = cellH * gridY + cellH / 2;

            int outerSize = 110;
            int innerSize = 55;

            bool outerDark = ((useed >> 1) & 1) == 0;
            bool useSquare = ((useed >> 2) & 1) == 0;

            Color32 outer = outerDark ? new Color32(15, 15, 15, 255) : new Color32(245, 245, 245, 255);
            Color32 inner = outerDark ? new Color32(245, 245, 245, 255) : new Color32(15, 15, 15, 255);

            if (useSquare)
            {
                DrawRect(pixels, cx - outerSize, cy - outerSize, 2 * outerSize, 2 * outerSize, outer);
                DrawRect(pixels, cx - innerSize, cy - innerSize, 2 * innerSize, 2 * innerSize, inner);
            }
            else
            {
                DrawCircle(pixels, cx, cy, outerSize, outer);
                DrawCircle(pixels, cx, cy, innerSize, inner);
            }

            // Add a small accent dot in the center of the inner shape for extra keypoint richness.
            DrawCircle(pixels, cx, cy, 12, outer);
        }

        static Color32 RandomVividColor(System.Random rng)
        {
            // Vivid: at least one channel close to 0 and one close to 255.
            int hue = rng.Next(0, 6);
            byte high = (byte)rng.Next(200, 256);
            byte mid = (byte)rng.Next(60, 200);
            byte low = (byte)rng.Next(0, 60);
            return hue switch
            {
                0 => new Color32(high, mid, low, 255),
                1 => new Color32(high, low, mid, 255),
                2 => new Color32(mid, high, low, 255),
                3 => new Color32(low, high, mid, 255),
                4 => new Color32(mid, low, high, 255),
                _ => new Color32(low, mid, high, 255),
            };
        }

        static void BiasClusterToCorner(Color32[] pixels, int corner, System.Random rng)
        {
            int xMin = 0, xMax = Size / 2, yMin = 0, yMax = Size / 2;
            switch (corner)
            {
                case 0: xMin = 30; xMax = Size / 2 - 30; yMin = 30; yMax = Size / 2 - 30; break; // TL
                case 1: xMin = Size / 2 + 30; xMax = Size - 30; yMin = 30; yMax = Size / 2 - 30; break; // TR
                case 2: xMin = 30; xMax = Size / 2 - 30; yMin = Size / 2 + 30; yMax = Size - 30; break; // BL
                case 3: xMin = Size / 2 + 30; xMax = Size - 30; yMin = Size / 2 + 30; yMax = Size - 30; break; // BR
            }
            for (int i = 0; i < 30; i++)
            {
                int cx = rng.Next(xMin, xMax);
                int cy = rng.Next(yMin, yMax);
                int r = rng.Next(8, 24);
                DrawCircle(pixels, cx, cy, r, RandomVividColor(rng));
            }
        }

        // ---------- drawing primitives ----------

        static void DrawCircle(Color32[] pixels, int cx, int cy, int r, Color32 color)
        {
            int r2 = r * r;
            int xMin = Mathf.Max(0, cx - r);
            int xMax = Mathf.Min(Size - 1, cx + r);
            int yMin = Mathf.Max(0, cy - r);
            int yMax = Mathf.Min(Size - 1, cy + r);
            for (int y = yMin; y <= yMax; y++)
            {
                int dy = y - cy;
                int rowBase = y * Size;
                for (int x = xMin; x <= xMax; x++)
                {
                    int dx = x - cx;
                    if (dx * dx + dy * dy <= r2)
                    {
                        pixels[rowBase + x] = color;
                    }
                }
            }
        }

        static void DrawRect(Color32[] pixels, int x, int y, int w, int h, Color32 color)
        {
            int xMin = Mathf.Max(0, x);
            int yMin = Mathf.Max(0, y);
            int xMax = Mathf.Min(Size - 1, x + w);
            int yMax = Mathf.Min(Size - 1, y + h);
            for (int py = yMin; py <= yMax; py++)
            {
                int row = py * Size;
                for (int px = xMin; px <= xMax; px++)
                {
                    pixels[row + px] = color;
                }
            }
        }

        static void DrawLine(Color32[] pixels, int x0, int y0, int x1, int y1, Color32 color, int thickness)
        {
            int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            while (true)
            {
                for (int oy = -thickness; oy <= thickness; oy++)
                {
                    int py = y0 + oy;
                    if (py < 0 || py >= Size) continue;
                    int row = py * Size;
                    for (int ox = -thickness; ox <= thickness; ox++)
                    {
                        int px = x0 + ox;
                        if (px < 0 || px >= Size) continue;
                        pixels[row + px] = color;
                    }
                }
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        // ---------- CardLibrary auto-population ----------

        static int TryPopulateCardLibrary(CreatureDatabase db)
        {
            try
            {
                var library = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(CardLibraryPath);
                if (library == null)
                {
                    library = ScriptableObject.CreateInstance<XRReferenceImageLibrary>();
                    AssetDatabase.CreateAsset(library, CardLibraryPath);
                }

                var so = new SerializedObject(library);
                var imagesProp = so.FindProperty("m_Images");
                if (imagesProp == null) return -1;

                int updated = 0;
                foreach (var def in db.All)
                {
                    if (def == null || string.IsNullOrEmpty(def.CreatureId)) continue;
                    var texPath = $"{MarkersDir}/marker_{def.CreatureId}.png";
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                    if (tex == null) continue;

                    int existingIdx = -1;
                    for (int i = 0; i < imagesProp.arraySize; i++)
                    {
                        var nameProp = imagesProp.GetArrayElementAtIndex(i).FindPropertyRelative("m_Name");
                        if (nameProp != null && nameProp.stringValue == def.CreatureId)
                        {
                            existingIdx = i;
                            break;
                        }
                    }

                    SerializedProperty entry;
                    if (existingIdx >= 0)
                    {
                        entry = imagesProp.GetArrayElementAtIndex(existingIdx);
                    }
                    else
                    {
                        imagesProp.arraySize++;
                        entry = imagesProp.GetArrayElementAtIndex(imagesProp.arraySize - 1);
                    }

                    var nameP = entry.FindPropertyRelative("m_Name");
                    var sizeP = entry.FindPropertyRelative("m_Size");
                    var specifyP = entry.FindPropertyRelative("m_SpecifySize");
                    var textureP = entry.FindPropertyRelative("m_Texture");
                    var textureGuidP = entry.FindPropertyRelative("m_TextureGuid");

                    if (nameP == null || sizeP == null || specifyP == null) return -1;

                    nameP.stringValue = def.CreatureId;
                    specifyP.boolValue = true;
                    sizeP.vector2Value = new Vector2(0.105f, 0.105f);

                    if (textureP != null)
                    {
                        textureP.objectReferenceValue = tex;
                    }
                    if (textureGuidP != null)
                    {
                        var guid = AssetDatabase.AssetPathToGUID(texPath);
                        WriteGuidToProperty(textureGuidP, guid);
                    }

                    updated++;
                }

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(library);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return updated;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ARFishing] CardLibrary auto-populate failed: {e.Message}");
                return -1;
            }
        }

        static void WriteGuidToProperty(SerializedProperty guidProp, string guid)
        {
            if (string.IsNullOrEmpty(guid) || guid.Length < 32) return;
            // SerializableGuid stores two ulong fields in Unity AR Foundation.
            var lowProp = guidProp.FindPropertyRelative("m_GuidLow");
            var highProp = guidProp.FindPropertyRelative("m_GuidHigh");
            if (lowProp == null || highProp == null) return;

            ulong low = 0, high = 0;
            if (ulong.TryParse(guid.Substring(0, 16), System.Globalization.NumberStyles.HexNumber, null, out high) &&
                ulong.TryParse(guid.Substring(16, 16), System.Globalization.NumberStyles.HexNumber, null, out low))
            {
                lowProp.longValue = unchecked((long)low);
                highProp.longValue = unchecked((long)high);
            }
        }

        // ---------- helpers ----------

        static CreatureDatabase LoadCreatureDatabase()
        {
            var guids = AssetDatabase.FindAssets("t:CreatureDatabase");
            if (guids.Length == 0)
            {
                Debug.LogError("[ARFishing] CreatureDatabase asset not found. Run 'Create MVP Content' first.");
                return null;
            }
            return AssetDatabase.LoadAssetAtPath<CreatureDatabase>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        static void ConfigureImporter(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            bool changed = false;
            if (importer.textureType != TextureImporterType.Default)
            {
                importer.textureType = TextureImporterType.Default;
                changed = true;
            }
            if (!importer.isReadable)
            {
                importer.isReadable = true;
                changed = true;
            }
            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }
            if (changed) importer.SaveAndReimport();
        }

        static int StableHash(string s)
        {
            int hash = 17;
            foreach (var c in s) hash = unchecked(hash * 31 + c);
            return hash;
        }

        static void EnsureDir(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;
            var parent = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            var leaf = Path.GetFileName(assetPath);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureDir(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
