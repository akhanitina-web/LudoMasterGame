using System.Collections.Generic;
using LudoMaster.Core;
using UnityEngine;

namespace LudoMaster.Gameplay
{
    /// <summary>
    /// Creates 4 tokens per player color using existing Art sprites and registers them in <see cref="TokenSystem"/>.
    /// </summary>
    public class TokenSpawner : MonoBehaviour
    {
        [SerializeField] private float spacing = 0.62f;

        [Header("Token sprites (auto-loaded from Assets/Art if missing)")]
        [SerializeField] private Sprite redTokenSprite;
        [SerializeField] private Sprite greenTokenSprite;
        [SerializeField] private Sprite blueTokenSprite;
        [SerializeField] private Sprite yellowTokenSprite;

        [Header("Optional token prefabs")]
        [SerializeField] private GameObject redTokenPrefab;
        [SerializeField] private GameObject blueTokenPrefab;
        [SerializeField] private GameObject greenTokenPrefab;
        [SerializeField] private GameObject yellowTokenPrefab;

        private void Awake()
        {
            TryLoadMissingAssets();
        }

        public void BuildDefaultTokens(Transform tokenRoot, TokenSystem tokenSystem)
        {
            if (tokenRoot == null || tokenSystem == null)
            {
                Debug.LogWarning("TokenSpawner requires tokenRoot and tokenSystem references.", this);
                return;
            }

            TryLoadMissingAssets();

            var tokens = new List<TokenController>(16);
            foreach (PlayerColor color in System.Enum.GetValues(typeof(PlayerColor)))
            {
                Vector3 center = GetBaseCenter(color);

                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = new((i % 2 == 0 ? -1 : 1) * spacing, (i < 2 ? 1 : -1) * spacing);
                    GameObject tokenObject = CreateTokenInstance(color, i + 1);
                    tokenObject.transform.SetParent(tokenRoot, false);
                    tokenObject.transform.position = center + new Vector3(offset.x, offset.y, 0f);

                    SpriteRenderer renderer = tokenObject.GetComponent<SpriteRenderer>();
                    if (renderer == null)
                    {
                        renderer = tokenObject.AddComponent<SpriteRenderer>();
                    }

                    renderer.sprite = GetSpriteForColor(color);
                    renderer.color = Color.white;
                    renderer.sortingOrder = 2;

                    CircleCollider2D collider = tokenObject.GetComponent<CircleCollider2D>();
                    if (collider == null)
                    {
                        tokenObject.AddComponent<CircleCollider2D>();
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

        private Sprite GetSpriteForColor(PlayerColor color)
        {
            return color switch
            {
                PlayerColor.Red => redTokenSprite,
                PlayerColor.Blue => blueTokenSprite,
                PlayerColor.Green => greenTokenSprite,
                PlayerColor.Yellow => yellowTokenSprite,
                _ => null
            };
        }

        private void OnValidate()
        {
            TryLoadMissingAssets();
        }

        private void TryLoadMissingAssets()
        {
#if UNITY_EDITOR
            redTokenPrefab ??= UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Generated/Token_Red.prefab");
            blueTokenPrefab ??= UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Generated/Token_Blue.prefab");
            greenTokenPrefab ??= UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Generated/Token_Green.prefab");
            yellowTokenPrefab ??= UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Generated/Token_Yellow.prefab");

            redTokenSprite ??= UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/red_token.png");
            greenTokenSprite ??= UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/green_token.png");
            blueTokenSprite ??= UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/blue_token.png");
            yellowTokenSprite ??= UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/yellow_token.png");
#endif
        }

        private static Vector3 GetBaseCenter(PlayerColor color)
        {
            return color switch
            {
                PlayerColor.Red => new Vector3(-3.15f, 3.15f),
                PlayerColor.Green => new Vector3(3.15f, 3.15f),
                PlayerColor.Blue => new Vector3(-3.15f, -3.15f),
                PlayerColor.Yellow => new Vector3(3.15f, -3.15f),
                _ => Vector3.zero
            };
        }
    }
}
