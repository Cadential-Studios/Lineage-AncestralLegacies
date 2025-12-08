using UnityEngine;
using System.Collections.Generic;

namespace Lineage.Core
{
    /// <summary>
    /// Generic object pool for frequently instantiated objects.
    /// Reduces GC pressure and improves performance for pop spawning/death cycles.
    /// </summary>
    /// <typeparam name="T">Type of component to pool (e.g., Pop, Projectile, Effect)</typeparam>
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Queue<T> availableObjects = new Queue<T>();
        private readonly List<T> activeObjects = new List<T>();
        private readonly Transform poolContainer;
        private readonly int initialSize;
        private readonly int maxSize;
        
        /// <summary>
        /// Creates a new object pool
        /// </summary>
        /// <param name="prefab">Prefab to instantiate</param>
        /// <param name="initialSize">Number of objects to pre-create</param>
        /// <param name="maxSize">Maximum pool size (0 = unlimited)</param>
        /// <param name="containerName">Name for the pool container GameObject</param>
        public ObjectPool(T prefab, int initialSize = 10, int maxSize = 100, string containerName = "ObjectPool")
        {
            this.prefab = prefab;
            this.initialSize = initialSize;
            this.maxSize = maxSize;
            
            // Create container for pooled objects
            GameObject container = new GameObject(containerName);
            poolContainer = container.transform;
            Object.DontDestroyOnLoad(container);
            
            // Pre-warm the pool
            for (int i = 0; i < initialSize; i++)
            {
                T newObj = CreateNewObject();
                newObj.gameObject.SetActive(false);
                availableObjects.Enqueue(newObj);
            }
            
            UnityEngine.Debug.Log($"[ObjectPool] Created pool for {typeof(T).Name}: {initialSize} objects");
        }
        
        /// <summary>
        /// Gets an object from the pool or creates a new one if none available
        /// </summary>
        public T Get()
        {
            T obj;
            
            if (availableObjects.Count > 0)
            {
                obj = availableObjects.Dequeue();
            }
            else
            {
                if (maxSize > 0 && activeObjects.Count >= maxSize)
                {
                    UnityEngine.Debug.LogWarning($"[ObjectPool] Max size ({maxSize}) reached for {typeof(T).Name}");
                    return null;
                }
                
                obj = CreateNewObject();
            }
            
            obj.gameObject.SetActive(true);
            activeObjects.Add(obj);
            return obj;
        }
        
        /// <summary>
        /// Gets an object at a specific position and rotation
        /// </summary>
        public T Get(Vector3 position, Quaternion rotation)
        {
            T obj = Get();
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
            }
            return obj;
        }
        
        /// <summary>
        /// Returns an object to the pool
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null) return;
            
            if (!activeObjects.Contains(obj))
            {
                UnityEngine.Debug.LogWarning($"[ObjectPool] Trying to return object that wasn't from this pool: {obj.name}");
                return;
            }
            
            activeObjects.Remove(obj);
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(poolContainer);
            availableObjects.Enqueue(obj);
        }
        
        /// <summary>
        /// Returns all active objects to the pool
        /// </summary>
        public void ReturnAll()
        {
            // Create copy to avoid modification during iteration
            var objectsToReturn = new List<T>(activeObjects);
            
            foreach (var obj in objectsToReturn)
            {
                Return(obj);
            }
        }
        
        /// <summary>
        /// Gets the number of currently active objects
        /// </summary>
        public int ActiveCount => activeObjects.Count;
        
        /// <summary>
        /// Gets the number of available objects in the pool
        /// </summary>
        public int AvailableCount => availableObjects.Count;
        
        /// <summary>
        /// Gets the total size of the pool (active + available)
        /// </summary>
        public int TotalSize => ActiveCount + AvailableCount;
        
        /// <summary>
        /// Clears the pool and destroys all objects
        /// </summary>
        public void Clear()
        {
            foreach (var obj in activeObjects)
            {
                if (obj != null)
                {
                    Object.Destroy(obj.gameObject);
                }
            }
            activeObjects.Clear();
            
            while (availableObjects.Count > 0)
            {
                T obj = availableObjects.Dequeue();
                if (obj != null)
                {
                    Object.Destroy(obj.gameObject);
                }
            }
            
            if (poolContainer != null)
            {
                Object.Destroy(poolContainer.gameObject);
            }
        }
        
        private T CreateNewObject()
        {
            T newObj = Object.Instantiate(prefab, poolContainer);
            newObj.name = $"{prefab.name} (Pooled)";
            return newObj;
        }
    }
    
    /// <summary>
    /// Static helper class for managing multiple object pools
    /// </summary>
    public static class PoolManager
    {
        private static Dictionary<string, object> pools = new Dictionary<string, object>();
        
        /// <summary>
        /// Gets or creates a pool for the specified prefab
        /// </summary>
        public static ObjectPool<T> GetPool<T>(T prefab, int initialSize = 10, int maxSize = 100, string poolName = null) where T : Component
        {
            string key = poolName ?? prefab.GetType().Name;
            
            if (!pools.ContainsKey(key))
            {
                pools[key] = new ObjectPool<T>(prefab, initialSize, maxSize, $"{key}Pool");
            }
            
            return pools[key] as ObjectPool<T>;
        }
        
        /// <summary>
        /// Clears all pools
        /// </summary>
        public static void ClearAllPools()
        {
            pools.Clear();
        }
    }
}
