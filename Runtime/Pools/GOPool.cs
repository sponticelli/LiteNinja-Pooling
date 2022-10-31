using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace LiteNinja.Pooling
{
    /// <summary>
    /// Represents the GameObject pool for the clones of one specific prefab.
    /// Getting and returning objects to and from it activates and deactivates them.
    /// </summary>
    public class GOPool : IPool<GameObject>
    {
        private struct Item
        {
            public readonly GameObject Instance;
            public readonly ISpawnable Spawnable;

            public Item(GameObject instance, bool hasPoolingBehaviour)
            {
                this.Instance = instance;
                Spawnable = hasPoolingBehaviour ? instance.GetComponent<ISpawnable>() : null;
            }

            public void SetActive(bool active)
            {
                if (Spawnable != null)
                {
                    try
                    {
                        Spawnable.OnSpawn(active);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                else
                {
                    Instance.SetActive(active);
                }
            }

            public override bool Equals(object obj)
            {
                if (obj is not Item other) return false;
                return other.Instance == Instance;
            }

            public override int GetHashCode()
            {
                return Instance.GetInstanceID();
            }
        }

        // The prefab for the pool objects.
        private GameObject prefab;
        private bool prefabHasPoolingBehaviour;

        private bool prefabHasPoolReturner;

        
        public int Count { get; private set; }

        // All inactive/available objects in the pool that have never been activated before.
        // The first time they are used, they have to additionally have gameObject.SetActive(true) called,
        // as the prefab was deactivated before instantiating them, regardless of it having a PoolingBehaviour.
        // This is to not trigger an OnEnable event when instantiating directly into the pool.
        private Queue<Item> freshObjects = new();

        // All inactive/available objects in the pool, ready to be taken out.
        private Queue<Item> availableObjects = new();

        // All clones created by this pool that are currently active in the scene.
        private Dictionary<GameObject, Item> activeObjects = new();

        // All objects managed by the pool, regardnless of their state.
        private HashSet<GameObject> allObjects = new();

        /// <summary>
        /// Creates a new pool and fills it with a certain amount of clones of the given <paramref name="prefab"/>.
        /// <param name="prefab">The prefab for the objects in this pool.</param>
        /// <param name="initialCount">
        /// The amount of initially created objects.
        /// </param>
        /// </summary>
        public GOPool(GameObject prefab, int initialCount)
        {
            if (initialCount < 0)
            {
                initialCount = 0;
            }

            InitializePool(prefab);

            if (initialCount > 0)
            {
                InstantiateBatch(initialCount);
            }
        }

        /// <summary>
        /// Creates a new pool, fills it with a certain amount of clones of the given <paramref name="prefab"/>,
        /// and passes one of the clones back via <paramref name="firstObject"/>.
        /// <param name="prefab">The prefab for the objects in this pool.</param>
        /// <param name="initialCount">
        /// The amount of initially created objects.
        /// Will be handled as at least 1.
        /// </param>
        /// <param name="firstObject">
        /// The first object of the pool.
        /// The referenced object will be active.
        /// </param>
        /// </summary>
        public GOPool(GameObject prefab, int initialCount, out GameObject firstObject)
        {
            if (initialCount <= 0)
            {
                initialCount = 1;
            }

            InitializePool(prefab);

            firstObject = InstantiateBatchAndReturnOneActive(initialCount);
        }

        /// <summary>
        /// Creates a new pool, fills it with a certain amount of clones of the given <paramref name="prefab"/>,
        /// and passes one of the clones back via <paramref name="firstObject"/>,
        /// which has the given <paramref name="position"/> and <paramref name="rotation"/> applied.
        /// <param name="prefab">The prefab for the objects in this pool.</param>
        /// <param name="initialCount">
        /// The amount of initially created objects.
        /// Will be handled as at least 1.
        /// </param>
        /// <param name="firstObject">
        /// The first object of the pool.
        /// The referenced object will be active.
        /// </param>
        /// </summary>
        public GOPool(GameObject prefab, int initialCount, out GameObject firstObject, Vector3 position,
            Quaternion rotation)
        {
            if (initialCount <= 0)
            {
                initialCount = 1;
            }

            InitializePool(prefab);

            firstObject = InstantiateBatchAndReturnOneActive(initialCount, position, rotation);
        }

        private void InitializePool(GameObject prefab)
        {
            this.prefab = prefab;

            prefabHasPoolingBehaviour = prefab.GetComponent<ISpawnable>() != null;
            prefabHasPoolReturner = prefab.GetComponent<GODespawner>() != null;
        }

        /// <summary>
        /// Instantiates one active GameObject to return, and amount - 1 inactive ones for the pool.
        /// </summary>
        private GameObject InstantiateBatchAndReturnOneActive(int amount)
        {
            InstantiateBatch(amount - 1);

            var active = Instantiate();
            activeObjects.Add(active.Instance, active);
            active.SetActive(true);
            return active.Instance;
        }

        /// <summary>
        /// Instantiates one active GameObject to return, and amount - 1 inactive ones for the pool.
        /// </summary>
        private GameObject InstantiateBatchAndReturnOneActive(int amount, Vector3 position, Quaternion rotation)
        {
            InstantiateBatch(amount - 1);

            var active = Instantiate(position, rotation);
            active.SetActive(true);
            activeObjects.Add(active.Instance, active);
            return active.Instance;
        }

        /// <summary>
        /// Instantiates amount inactive objects for the pool.
        /// </summary>
        internal void InstantiateBatch(int amount)
        {
            Count += amount;

            var prefabWasEnabled = prefab.activeSelf;
            if (prefabWasEnabled)
            {
                prefab.SetActive(false);
            }

            for (var i = 0; i < amount; i++)
            {
                freshObjects.Enqueue(Instantiate());
            }

            if (prefabWasEnabled)
            {
                prefab.SetActive(true);
            }
        }

        /// <summary>
        /// Instantiates as many inactive objects as needed to get the pool to the given target count.
        /// </summary>
        public void Fill(int target)
        {
            var amount = target - Count;
            if (amount > 0)
            {
                InstantiateBatch(amount);
            }
        }

        
        public GameObject Spawn()
        {
            Item result;
            if (availableObjects.Count > 0)
            {
                result = availableObjects.Dequeue();
            }
            else if (freshObjects.Count > 0)
            {
                result = DequeueFreshObject();
            }
            else
            {
                result = Instantiate();
            }

            activeObjects.Add(result.Instance, result);
            result.SetActive(true);

            return result.Instance;
        }

        /// <summary>
        /// Returns an active clone of the pool's prefab.
        /// Instantiates clones if the pool is empty.
        /// The amount of instantiated clones is the current pool object count.
        /// </summary>
        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            Item result;
            if (availableObjects.Count > 0)
            {
                result = availableObjects.Dequeue();
            }
            else if (freshObjects.Count > 0)
            {
                result = DequeueFreshObject();
            }
            else
            {
                result = Instantiate(position, rotation);
            }

            var transform = result.Instance.transform;
            transform.position = position;
            transform.rotation = rotation;
            activeObjects.Add(result.Instance, result);
            result.SetActive(true);

            return result.Instance;
        }

        private Item DequeueFreshObject()
        {
            var result = freshObjects.Dequeue();
            // Activate the fresh object since it was spawned inactive.
            // Do this now to have Awake called consistently before SetActive.
            result.Instance.SetActive(true);
            return result;
        }

        
        public void Despawn(GameObject go)
        {
            if (activeObjects.TryGetValue(go, out var item))
            {
                activeObjects.Remove(go);
                item.SetActive(false);
                availableObjects.Enqueue(item);
            }
            else if (!allObjects.Contains(go))
            {
                throw new System.ArgumentException(
                    "The object you tried to return to a pool was not instantiated by the pool.");
            }
        }

        #region Clean

        
        public void Clear()
        {
            Clear(ref freshObjects);
            Clear(ref availableObjects);
            Clear(ref activeObjects);
            Clear(ref allObjects);

            Count = freshObjects.Count + availableObjects.Count + activeObjects.Count;
        }

        private void Clear(ref HashSet<GameObject> collection)
        {
            var newCollection = new HashSet<GameObject>();
            foreach (var go in collection.Where(go => go))
            {
                newCollection.Add(go);
            }

            collection = newCollection;
        }

        private static void Clear(ref Queue<Item> collection)
        {
            var newCollection = new Queue<Item>();
            foreach (var item in collection.Where(item => item.Instance))
            {
                newCollection.Enqueue(item);
            }

            collection = newCollection;
        }

        private static void Clear(ref Dictionary<GameObject, Item> collection)
        {
            var newCollection = collection.Where(item => item.Key)
                .ToDictionary(item => item.Key, item => item.Value);

            collection = newCollection;
        }

        #endregion

        #region Purge
        
        public void Purge()
        {
            Purge(freshObjects);
            Purge(availableObjects);
            Purge(activeObjects.Keys);
            Purge(allObjects);

            freshObjects.Clear();
            availableObjects.Clear();
            activeObjects.Clear();
            allObjects.Clear();

            Count = 0;
        }

        private void Purge(IEnumerable<Item> collection)
        {
            foreach (var item in collection)
            {
                if (item.Instance != null)
                {
                    Object.DestroyImmediate(item.Instance);
                }
            }
        }

        private void Purge(IEnumerable<GameObject> collection)
        {
            foreach (var obj in collection)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }
        }

        #endregion

        private Item Instantiate()
        {
            var go = Object.Instantiate(prefab);
            var item = Initialize(go);
            return item;
        }

        private Item Instantiate(Vector3 position, Quaternion rotation)
        {
            var go = Object.Instantiate(prefab, position, rotation);
            var item = Initialize(go);
            return item;
        }

        private Item Initialize(GameObject go)
        {
#if UNITY_EDITOR
            if (!PoolManager.ObjectsAreVisibleInHierarchy)
            {
                go.hideFlags = HideFlags.HideInHierarchy;
            }
#endif
            if (prefabHasPoolReturner)
            {
                go.GetComponent<GODespawner>().prefab = prefab;
            }

            allObjects.Add(go);

            return new Item(go, prefabHasPoolingBehaviour);
        }

#if UNITY_EDITOR
        public void SetEditorObjectVisibility(bool visible)
        {
            var hideFlags = visible ? HideFlags.None : HideFlags.HideInHierarchy;

            foreach (var go in allObjects)
            {
                go.hideFlags = hideFlags;
            }
        }

        public int ActiveObjectCount => activeObjects.Count;

        public int InactiveObjectCount => freshObjects.Count + availableObjects.Count;
#endif
    }
}