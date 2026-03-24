using System.Collections;
using LudoMaster.Core;
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

        public PlayerColor OwnerColor { get; private set; }
        public CoreTokenData Data { get; private set; }
        public Vector3 SpawnPosition { get; private set; }

        public void Initialize(PlayerColor ownerColor, CoreTokenData data)
        {
            OwnerColor = ownerColor;
            Data = data;
            SpawnPosition = transform.position;
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
    }
}
