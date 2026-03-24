using System.Collections.Generic;
using LudoMaster.Core;
using UnityEngine;

namespace LudoMaster.Gameplay
{
    /// <summary>
    /// Creates four colored token sets and registers them into TokenSystem.
    /// </summary>
    public class TokenSpawner : MonoBehaviour
    {
        [SerializeField] private float spacing = 0.55f;

        public void BuildDefaultTokens(Transform tokenRoot, TokenSystem tokenSystem)
        {
            var tokens = new List<TokenController>(16);
            foreach (PlayerColor color in System.Enum.GetValues(typeof(PlayerColor)))
            {
                Vector3 center = GetBaseCenter(color);
                Color tint = GetTint(color);

                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = new Vector2((i % 2 == 0 ? -1 : 1) * spacing, (i < 2 ? 1 : -1) * spacing);
                    var go = new GameObject($"{color}Token_{i + 1}", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(TokenController));
                    go.transform.SetParent(tokenRoot);
                    go.transform.position = center + new Vector3(offset.x, offset.y, 0f);

                    var renderer = go.GetComponent<SpriteRenderer>();
                    renderer.sprite = BuildTokenSprite();
                    renderer.color = tint;
                    renderer.sortingOrder = 2;

                    var token = go.GetComponent<TokenController>();
                    token.Initialize(color, new CoreTokenData { TokenId = i });
                    tokens.Add(token);
                }
            }

            tokenSystem.RegisterTokens(tokens);
        }

        private static Vector3 GetBaseCenter(PlayerColor color)
        {
            return color switch
            {
                PlayerColor.Red => new Vector3(-3f, 3f),
                PlayerColor.Green => new Vector3(3f, 3f),
                PlayerColor.Blue => new Vector3(-3f, -3f),
                PlayerColor.Yellow => new Vector3(3f, -3f),
                _ => Vector3.zero
            };
        }

        private static Color GetTint(PlayerColor color)
        {
            return color switch
            {
                PlayerColor.Red => new Color(0.93f, 0.22f, 0.24f),
                PlayerColor.Blue => new Color(0.16f, 0.47f, 0.96f),
                PlayerColor.Green => new Color(0.2f, 0.73f, 0.27f),
                PlayerColor.Yellow => new Color(0.95f, 0.85f, 0.17f),
                _ => Color.white
            };
        }

        private static Sprite circleSprite;

        private static Sprite BuildTokenSprite()
        {
            if (circleSprite != null)
            {
                return circleSprite;
            }

            Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2(31.5f, 31.5f);
            float radius = 30f;

            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    texture.SetPixel(x, y, distance <= radius ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            circleSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            return circleSprite;
        }
    }
}
