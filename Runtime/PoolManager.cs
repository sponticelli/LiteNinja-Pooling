using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace LiteNinja.Pooling
{
    public static class PoolManager
    {
        private const int DefaultInitialPoolSize = 5;
        private static readonly Dictionary<GameObject, GOPool> _pools = new();
        public static bool ObjectsAreVisibleInHierarchy { private set; get; }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            SceneManager.sceneUnloaded += _ => ClearPools();
        }

        #region Spawn

        /// <summary>
        /// Returns an active clone of the prefab, taken from its pool if available.
        /// </summary>
        public static GameObject Spawn(GameObject prefab, int initialPoolSize = DefaultInitialPoolSize)
        {
            if (_pools.TryGetValue(prefab, out var pool))
            {
                return pool.Spawn();
            }
            // Create a new pool for the prefab.
            pool = new GOPool(prefab, initialPoolSize, out var result);
            _pools.Add(prefab, pool);
            return result;
        }

        /// <summary>
        /// Returns an active clone of the prefab, taken from its pool if available.
        /// Applies the given position and rotation to the object.
        /// </summary>
        public static GameObject Spawn(GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            int initialPoolSize = DefaultInitialPoolSize)
        {
            if (_pools.TryGetValue(prefab, out var pool))
            {
                return pool.Spawn(position, rotation);
            }

            pool = new GOPool(prefab, initialPoolSize, out var result, position, rotation);
            _pools.Add(prefab, pool);
            return result;
        }

        /// <summary>
        /// Returns an active clone of the prefab, taken from its pool if available.
        /// </summary>
        public static T Spawn<T>(T prefab, int initialPoolSize = DefaultInitialPoolSize) where T : Component
        {
            return Spawn(prefab.gameObject, initialPoolSize).GetComponent<T>();
        }

        /// <summary>
        /// Returns an active clone of the prefab, taken from its pool if available.
        /// Applies the given position and rotation to the object.
        /// </summary>
        public static T Spawn<T>(T prefab,
            Vector3 position,
            Quaternion rotation,
            int initialPoolSize = DefaultInitialPoolSize) where T : Component
        {
            var result = Spawn(prefab, initialPoolSize);
            var transform = result.transform;
            transform.position = position;
            transform.rotation = rotation;
            return result;
        }

        #endregion

        #region Despawn

        /// <summary>
        /// Disables <paramref name="instance"/>, an active clone of the prefab,
        /// and returns it to the pool.
        /// The passed <paramref name="instance"/> must be a clone of prefab.
        /// </summary>
        public static void Despawn(GameObject prefab, GameObject instance)
        {
            if (_pools.TryGetValue(prefab, out var pool))
            {
                pool.Despawn(instance);
            }
            else
            {
                throw new System.ArgumentException("The passed prefab does not have a pool."
                                                   + " Only return an object to a pool if it has been taken out of it.");
            }
        }

        /// <summary>
        /// Disables <paramref name="instance"/>, an active clone of the prefab,
        /// and returns it to the pool.
        /// The passed <paramref name="instance"/> must be a clone of prefab.
        /// </summary>
        public static void Despawn<T>(T prefab, T instance) where T : Component
        {
            Despawn(prefab.gameObject, instance.gameObject);
        }

        /// <summary>
        /// Disables <paramref name="instance"/> and returns it to the pool.
        /// Is only supposed to be called by the PoolReturner instance itself.
        /// </summary>
        public static void Despawn(GODespawner instance)
        {
            Despawn(instance.prefab, instance.gameObject);
        }

        #endregion

        #region Prefilling

        /// <summary>
        /// Fills the corresponding pool of the given prefab to have at least <paramref name="target"/> items in it.
        /// This is including the currently active (not pooled) objects.
        /// </summary>
        public static void Fill(GameObject prefab, int target)
        {
            if (_pools.TryGetValue(prefab, out var pool))
            {
                pool.Fill(target);
            }
            else
            {
                pool = new GOPool(prefab, target);
                _pools.Add(prefab, pool);
            }
        }

        /// <summary>
        /// Creates inactive instances of the given prefab and adds them to its corresponding pool.
        /// Use this to create prefabs before they're needed, to avoid hiccups during the game.
        /// </summary>
        public static void CreateObjects(GameObject prefab, int amount)
        {
            if (_pools.TryGetValue(prefab, out var pool))
            {
                pool.InstantiateBatch(amount);
            }
            else
            {
                pool = new GOPool(prefab, amount);
                _pools.Add(prefab, pool);
            }
        }

        /// <summary>
        /// Creates inactive instances of the given prefab and adds them to its corresponding pool.
        /// Use this to create prefabs before they're needed, to avoid hiccups during the game.
        /// </summary>
        public static void CreateObjects(Component prefab, int amount)
        {
            CreateObjects(prefab.gameObject, amount);
        }

        #endregion

        private static void ClearPools()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
        }

        /// <summary>
        /// Destroy all pooled objects and clear the pools.
        /// </summary>
        public static void PurgePools()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Purge();
            }

            _pools.Clear();
        }

#if UNITY_EDITOR
        public static void SetEditorObjectVisibility(bool visible)
        {
            if (ObjectsAreVisibleInHierarchy == visible) return;

            ObjectsAreVisibleInHierarchy = visible;
            foreach (var pool in _pools.Values)
            {
                pool.SetEditorObjectVisibility(visible);
            }
        }

        public static Dictionary<GameObject, GOPool> GetPools()
        {
            return _pools;
        }
#endif
    }
}