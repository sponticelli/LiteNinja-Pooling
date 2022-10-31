using UnityEngine;
using System;
using System.Collections;

namespace LiteNinja.Pooling
{
    /// <summary>
    /// Instantiate a certain amount of prefabs in the pool, so that the game doesn't stutter when instantiating new objects.
    /// If createOnAwake is true, the prefabs will be instantiated on Awake() otherwise you'll have to call the Create() method.
    /// </summary>
    [AddComponentMenu("LiteNinja/Pooling/Pool Warmup")]
    public class WarmupGOPool : MonoBehaviour
    {
        [Serializable]
        internal struct Chunk
        {
            public GameObject prefab;
            public int amount;

            public void Create()
            {
                if (prefab != null && amount > 0)
                {
                    PoolManager.Fill(prefab, amount);
                }
            }
        }

        [SerializeField] private Chunk[] items = default;
        [SerializeField] private bool createOnAwake = true;

        private void Awake()
        {
            if (createOnAwake)
            {
                Create();
            }
        }

        /// <summary>
        /// Create the prefabs specified in the items array.
        /// </summary>
        /// <param name="maxSecondsPerChunk">
        /// The maximum amount of seconds to wait between each chunk of prefabs is created.
        /// If 0 then all the prefabs are created immediately without pausing
        /// </param>
        /// <param name="progressCallback">A callback called when a chunk of prefabs is created.</param>
        public void Create(float maxSecondsPerChunk = 0f, Action<float> progressCallback = null)
        {
            if (maxSecondsPerChunk <= 0f)
            {
                foreach (var item in items)
                {
                    item.Create();
                }

                progressCallback?.Invoke(1f);
            }
            else
            {
                StartCoroutine(CreateInChunks(maxSecondsPerChunk, progressCallback));
            }
        }

        private IEnumerator CreateInChunks(float maxSecondsPerChunk, Action<float> progressCallback)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            
            var millisecondsPerChunk = maxSecondsPerChunk * 1000f;
            var oneOverLength = 1f / items.Length;

            for (var i = 0; i < items.Length; i++)
            {
                items[i].Create();

                if (stopwatch.ElapsedMilliseconds < millisecondsPerChunk) continue;

                progressCallback?.Invoke(i * oneOverLength);
                yield return null;
                stopwatch.Restart();
            }
        }
    }
}