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

        public PlayerColor OwnerColor { get; private set; }
        public CoreTokenData Data { get; private set; }

        public void Initialize(PlayerColor ownerColor, CoreTokenData data)
        {
            OwnerColor = ownerColor;
            Data = data;
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
                t += Time.deltaTime / stepMoveDuration;
                transform.position = Vector3.Lerp(start, worldPosition, t);
                yield return null;
            }
        }

        /// <summary>
        /// Sets token world position directly, useful on load/sync.
        /// </summary>
        public void SnapTo(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }
    }
}
