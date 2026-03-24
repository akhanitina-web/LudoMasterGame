using System.Collections.Generic;
using UnityEngine;

namespace LudoMaster.UI
{
    /// <summary>
    /// Runtime generated polished sprites used by board and dice UI.
    /// </summary>
    public static class LudoSpriteFactory
    {
        private static Sprite boardSprite;
        private static readonly Dictionary<int, Sprite> diceSprites = new();

        public static Sprite GetBoardSprite()
        {
            if (boardSprite != null)
            {
                return boardSprite;
            }

            const int size = 1024;
            Texture2D tex = new(size, size, TextureFormat.RGBA32, false);
            Fill(tex, new Color(0.94f, 0.97f, 1f, 1f));

            DrawQuadrant(tex, 0, 0, size / 2, new Color(0.93f, 0.27f, 0.29f, 1f));
            DrawQuadrant(tex, size / 2, 0, size / 2, new Color(0.2f, 0.55f, 0.95f, 1f));
            DrawQuadrant(tex, 0, size / 2, size / 2, new Color(0.2f, 0.75f, 0.32f, 1f));
            DrawQuadrant(tex, size / 2, size / 2, size / 2, new Color(0.97f, 0.84f, 0.2f, 1f));

            DrawCross(tex, new Color(1f, 1f, 1f, 1f));
            DrawCenterStar(tex);
            tex.Apply();

            boardSprite = Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
            return boardSprite;
        }

        public static Sprite GetDiceFaceSprite(int value)
        {
            value = Mathf.Clamp(value, 1, 6);
            if (diceSprites.TryGetValue(value, out Sprite existing))
            {
                return existing;
            }

            const int size = 128;
            Texture2D tex = new(size, size, TextureFormat.RGBA32, false);
            Fill(tex, Color.white);
            DrawRoundedBorder(tex, new Color(0.16f, 0.2f, 0.28f, 1f));

            foreach (Vector2 p in GetPips(value))
            {
                DrawCircle(tex, (int)p.x, (int)p.y, 10, new Color(0.12f, 0.16f, 0.2f, 1f));
            }

            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
            diceSprites[value] = sprite;
            return sprite;
        }

        private static void Fill(Texture2D tex, Color color)
        {
            Color[] pixels = new Color[tex.width * tex.height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
        }

        private static void DrawQuadrant(Texture2D tex, int startX, int startY, int size, Color color)
        {
            int margin = size / 8;
            for (int y = startY + margin; y < startY + size - margin; y++)
            {
                for (int x = startX + margin; x < startX + size - margin; x++)
                {
                    tex.SetPixel(x, y, color);
                }
            }

            int homeSize = size / 3;
            int homeStartX = startX + (size - homeSize) / 2;
            int homeStartY = startY + (size - homeSize) / 2;
            for (int y = homeStartY; y < homeStartY + homeSize; y++)
            {
                for (int x = homeStartX; x < homeStartX + homeSize; x++)
                {
                    tex.SetPixel(x, y, Color.white);
                }
            }
        }

        private static void DrawCross(Texture2D tex, Color color)
        {
            int thickness = tex.width / 9;
            int center = tex.width / 2;
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = center - thickness; x <= center + thickness; x++)
                {
                    tex.SetPixel(x, y, color);
                }
            }

            for (int x = 0; x < tex.width; x++)
            {
                for (int y = center - thickness; y <= center + thickness; y++)
                {
                    tex.SetPixel(x, y, color);
                }
            }
        }

        private static void DrawCenterStar(Texture2D tex)
        {
            int c = tex.width / 2;
            DrawTriangle(tex, new Vector2Int(c, c + 90), new Vector2Int(c - 90, c), new Vector2Int(c, c), new Color(0.2f, 0.75f, 0.32f, 1f));
            DrawTriangle(tex, new Vector2Int(c, c + 90), new Vector2Int(c + 90, c), new Vector2Int(c, c), new Color(0.93f, 0.27f, 0.29f, 1f));
            DrawTriangle(tex, new Vector2Int(c - 90, c), new Vector2Int(c, c - 90), new Vector2Int(c, c), new Color(0.2f, 0.55f, 0.95f, 1f));
            DrawTriangle(tex, new Vector2Int(c + 90, c), new Vector2Int(c, c - 90), new Vector2Int(c, c), new Color(0.97f, 0.84f, 0.2f, 1f));
        }

        private static void DrawTriangle(Texture2D tex, Vector2Int p0, Vector2Int p1, Vector2Int p2, Color color)
        {
            int minX = Mathf.Min(p0.x, Mathf.Min(p1.x, p2.x));
            int maxX = Mathf.Max(p0.x, Mathf.Max(p1.x, p2.x));
            int minY = Mathf.Min(p0.y, Mathf.Min(p1.y, p2.y));
            int maxY = Mathf.Max(p0.y, Mathf.Max(p1.y, p2.y));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (PointInTriangle(new Vector2(x, y), p0, p1, p2))
                    {
                        tex.SetPixel(x, y, color);
                    }
                }
            }
        }

        private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
            float s = 1f / (2f * area) * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
            float t = 1f / (2f * area) * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
            return s >= 0f && t >= 0f && (s + t) <= 1f;
        }

        private static void DrawCircle(Texture2D tex, int cx, int cy, int radius, Color color)
        {
            int r2 = radius * radius;
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y <= r2)
                    {
                        tex.SetPixel(cx + x, cy + y, color);
                    }
                }
            }
        }

        private static void DrawRoundedBorder(Texture2D tex, Color borderColor)
        {
            int width = tex.width;
            int thickness = 4;
            for (int x = 0; x < width; x++)
            {
                for (int t = 0; t < thickness; t++)
                {
                    tex.SetPixel(x, t, borderColor);
                    tex.SetPixel(x, width - 1 - t, borderColor);
                }
            }

            for (int y = 0; y < width; y++)
            {
                for (int t = 0; t < thickness; t++)
                {
                    tex.SetPixel(t, y, borderColor);
                    tex.SetPixel(width - 1 - t, y, borderColor);
                }
            }
        }

        private static IEnumerable<Vector2> GetPips(int value)
        {
            Vector2 c = new(64, 64);
            Vector2 tl = new(36, 92);
            Vector2 tr = new(92, 92);
            Vector2 ml = new(36, 64);
            Vector2 mr = new(92, 64);
            Vector2 bl = new(36, 36);
            Vector2 br = new(92, 36);

            return value switch
            {
                1 => new[] { c },
                2 => new[] { tl, br },
                3 => new[] { tl, c, br },
                4 => new[] { tl, tr, bl, br },
                5 => new[] { tl, tr, c, bl, br },
                _ => new[] { tl, tr, ml, mr, bl, br }
            };
        }
    }
}
