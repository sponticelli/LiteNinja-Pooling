namespace LiteNinja.Pooling
{
    public interface IPool<T> where T : class
    {
        /// <summary>
        /// Amount of objects spawned by the pool, active or inactive.
        /// </summary>
        public int Count { get; }
        
        /// <summary>
        /// Instantiates as many inactive objects as needed to get the pool to the given target count.
        /// </summary>
        public void Fill(int target);

        /// <summary>
        /// Returns an object from the pool.
        /// </summary>
        public T Spawn();

        /// <summary>
        /// Returns a clone to the pool.
        /// </summary>
        public void Despawn(T obj);

        /// <summary>
        /// Clears the pool of all destroyed objects.
        /// </summary>
        public void Clear();

        /// <summary>
        /// Destroys all objects known to the pool.
        /// </summary>
        public void Purge();
        
        
#if UNITY_EDITOR
        public void SetEditorObjectVisibility(bool visible);

        public int ActiveObjectCount { get; }

        public int InactiveObjectCount { get; }
#endif
    }
}