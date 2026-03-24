using System.Collections;
using LudoMaster.Core;
using LudoMaster.Signals;
using UnityEngine;

namespace LudoMaster.Gameplay
{
    /// <summary>
    /// Controls one token's movement animation and state transitions.
    /// </summary>
    public class TokenController : MonoBehaviour
    {
        [SerializeField] private float stepMoveDuration = 0.12f;
        [SerializeField] private AnimationCurve movementEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float jumpHeight = 0.15f;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D tokenCollider;
        [SerializeField] private float deselectedAlpha = 0.95f;
        [SerializeField] private float selectedAlpha = 1f;
        [SerializeField] private float unselectableAlpha = 0.45f;

        public PlayerColor OwnerColor { get; private set; }
        public CoreTokenData Data { get; private set; }
        public Vector3 SpawnPosition { get; private set; }
        public bool IsSelectable { get; private set; }

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (tokenCollider == null)
            {
                tokenCollider = GetComponent<Collider2D>();
            }

            SetSelectable(false);
        }

        public void Initialize(PlayerColor ownerColor, CoreTokenData data)
        {
            OwnerColor = ownerColor;
            Data = data;
            SpawnPosition = transform.position;
            SetSelectable(false);
        }

        /// <summary>
        /// Smoothly moves token transform to given world position.
        /// </summary>
        public IEnumerator MoveTo(Vector3 worldPosition)
        {
            Vector3 start = transform.position;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, stepMoveDuration);
                float eased = movementEase.Evaluate(Mathf.Clamp01(t));
                Vector3 pos = Vector3.LerpUnclamped(start, worldPosition, eased);
                pos.y += Mathf.Sin(eased * Mathf.PI) * jumpHeight;
                transform.position = pos;
                yield return null;
            }

            transform.position = worldPosition;
        }

        /// <summary>
        /// Sets token world position directly, useful on load/sync.
        /// </summary>
        public void SnapTo(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        public void ResetToSpawn()
        {
            transform.position = SpawnPosition;
        }

        public void SetSelectable(bool selectable)
        {
            IsSelectable = selectable;
            if (tokenCollider != null)
            {
                tokenCollider.enabled = selectable;
            }

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = selectable ? selectedAlpha : (Data != null && Data.State == TokenState.InBase ? deselectedAlpha : unselectableAlpha);
                spriteRenderer.color = c;
            }
        }

        private void OnMouseDown()
        {
            if (IsSelectable && Data != null)
            {
                GameSignals.OnTokenSelected?.Invoke(OwnerColor, Data.TokenId);
            }
        }
    }
}
