using UnityEngine;
using System.Collections;

namespace LiteNinja.Pooling
{
    /// <summary>
    /// A GameObject with a GODespawner componnent is able to return itself to the pool.
    /// Otherwise you have to call PoolManager.Despawn(this.gameObject) method.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("LiteNinja/Pooling/GameObject Despawner")]
    public sealed class GODespawner : MonoBehaviour, IDespawnable
    {
        /// <summary>
        ///  The prefab used to spawn this object. it is automatically set when the object is spawned by GOPool 
        /// </summary>
        internal GameObject prefab;

        /// <summary>
        /// Return this instance to the pool.
        /// </summary>
        public void Despawn()
        {
            StopAllCoroutines();
            if (prefab)
            {
                PoolManager.Despawn(this);
            }
            else
            {
                Destroy(gameObject); // if the prefab is not set, destroy the object because it is not spawned by GOPool
            }
        }

        /// <summary>
        /// Return this instance to the pool after a specified delay.
        /// </summary>
        public void DespawnAfter(float delay)
        {
            StopAllCoroutines();
            StartCoroutine(DespawnAfterCoroutine(delay));
        }

        private IEnumerator DespawnAfterCoroutine(float delay)
        {
            while (delay > 0)
            {
                yield return null;
                delay -= Time.deltaTime;
            }

            Despawn();
        }
    }
}