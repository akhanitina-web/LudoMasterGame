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
        [SerializeField] private Color tokenShadowColor = new(0f, 0f, 0f, 0.2f);
        [SerializeField] private Vector3 tokenShadowOffset = new(0.06f, -0.07f, 0f);
        [SerializeField] private float tokenShadowScale = 0.92f;
        [SerializeField] private Color tokenHighlightColor = new(1f, 1f, 1f, 0.6f);
        [SerializeField] private float tokenHighlightScale = 1.2f;
        [Header("Optional token prefabs")]
        [SerializeField] private GameObject redTokenPrefab;
        [SerializeField] private GameObject blueTokenPrefab;
        [SerializeField] private GameObject greenTokenPrefab;
        [SerializeField] private GameObject yellowTokenPrefab;

        private void Awake()
        {
            TryLoadMissingPrefabs();
        }

        public void BuildDefaultTokens(Transform tokenRoot, TokenSystem tokenSystem)
        {
            if (tokenRoot == null || tokenSystem == null)
            {
                Debug.LogWarning("TokenSpawner requires tokenRoot and tokenSystem references.", this);
                return;
            }

            TryLoadMissingPrefabs();

            var tokens = new List<TokenController>(16);
            foreach (PlayerColor color in System.Enum.GetValues(typeof(PlayerColor)))
            {
                Vector3 center = GetBaseCenter(color);
                Color tint = GetTint(color);

                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = new((i % 2 == 0 ? -1 : 1) * spacing, (i < 2 ? 1 : -1) * spacing);
                    GameObject tokenObject = CreateTokenInstance(color, i + 1);
                    tokenObject.transform.SetParent(tokenRoot, false);
                    tokenObject.transform.position = center + new Vector3(offset.x, offset.y, 0f);

                    SpriteRenderer renderer = tokenObject.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        if (renderer.sprite == null)
                        {
                            renderer.sprite = BuildTokenSprite();
                            renderer.color = tint;
                        }

                        renderer.sortingOrder = 2;
                    }

                    CircleCollider2D collider = tokenObject.GetComponent<CircleCollider2D>();
                    if (collider == null)
                    {
                        tokenObject.AddComponent<CircleCollider2D>();
                    }

                    if (renderer != null && renderer.sprite != null && tokenObject.transform.Find("Shadow") == null)
                    {
                        CreateShadow(tokenObject.transform, renderer.sprite);
                    }

                    if (renderer != null && renderer.sprite != null && tokenObject.transform.Find("Highlight") == null)
                    {
                        CreateHighlight(tokenObject.transform, renderer.sprite);
                    }

                    TokenController token = tokenObject.GetComponent<TokenController>();
                    if (token == null)
                    {
                        token = tokenObject.AddComponent<TokenController>();
                    }

                    token.Initialize(color, new CoreTokenData { TokenId = i });
                    tokens.Add(token);
                }
            }

            tokenSystem.RegisterTokens(tokens);
        }

        private GameObject CreateTokenInstance(PlayerColor color, int tokenNumber)
        {
            GameObject prefab = GetTokenPrefab(color);
            if (prefab != null)
            {
                return Instantiate(prefab, transform);
            }

            return new GameObject($"Token_{color}_{tokenNumber}", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(TokenController));
        }

        private GameObject GetTokenPrefab(PlayerColor color)
        {
            return color switch
            {
                PlayerColor.Red => redTokenPrefab,
                PlayerColor.Blue => blueTokenPrefab,
                PlayerColor.Green => greenTokenPrefab,
                PlayerColor.Yellow => yellowTokenPrefab,
                _ => null
            };
        }

        private void OnValidate()
        {
            TryLoadMissingPrefabs();
        }

        private void TryLoadMissingPrefabs()
        {
#if UNITY_EDITOR
            redTokenPrefab ??= UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Generated/Token_Red.prefab");
            blueTokenPrefab ??= UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Generated/Token_Blue.prefab");
            greenTokenPrefab ??= UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Generated/Token_Green.prefab");
            yellowTokenPrefab ??= UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Generated/Token_Yellow.prefab");
#endif
        }

        private void CreateShadow(Transform tokenTransform, Sprite tokenSprite)
        {
            GameObject shadowObject = new("Shadow", typeof(SpriteRenderer));
            shadowObject.transform.SetParent(tokenTransform, false);
            shadowObject.transform.localPosition = tokenShadowOffset;
            shadowObject.transform.localScale = Vector3.one * tokenShadowScale;

            SpriteRenderer shadowRenderer = shadowObject.GetComponent<SpriteRenderer>();
            shadowRenderer.sprite = tokenSprite;
            shadowRenderer.color = tokenShadowColor;
            shadowRenderer.sortingOrder = 1;
        }

        private void CreateHighlight(Transform tokenTransform, Sprite tokenSprite)
        {
            GameObject highlightObject = new("Highlight", typeof(SpriteRenderer));
            highlightObject.transform.SetParent(tokenTransform, false);
            highlightObject.transform.localPosition = Vector3.zero;
            highlightObject.transform.localScale = Vector3.one * tokenHighlightScale;

            SpriteRenderer highlightRenderer = highlightObject.GetComponent<SpriteRenderer>();
            highlightRenderer.sprite = tokenSprite;
            highlightRenderer.color = tokenHighlightColor;
            highlightRenderer.sortingOrder = 0;
            highlightRenderer.enabled = false;
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

            Texture2D texture = new(64, 64, TextureFormat.RGBA32, false);
            Vector2 center = new(31.5f, 31.5f);
            const float radius = 30f;

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
