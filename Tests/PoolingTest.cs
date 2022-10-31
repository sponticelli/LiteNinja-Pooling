using UnityEngine;
using NUnit.Framework;

namespace LiteNinja.Pooling.Tests
{
    

    public class PoolingTest
    {
        private class PositionalSpawnable : MonoBehaviour, ISpawnable
        {
            public static readonly Vector3 disabledPosition = new(0, -10000, 0);

            private Vector3 previousPosition;

            void ISpawnable.OnSpawn(bool active)
            {
                if (!active)
                {
                    previousPosition = transform.position;
                }
                else
                {
                    previousPosition = transform.position;
                    transform.position = disabledPosition;
                }
            }
        }

        private class TestComponent : MonoBehaviour
        {

        }

        private GameObject prefabSimple;
        private TestComponent prefabSimpleComponent;
        private GODespawner prefabWithGoDespawner;
        private GameObject prefabWithISpawnable;

        [SetUp]
        public void SetUp()
        {
            prefabSimple = new GameObject("Prefab (Simple)");
            prefabSimpleComponent = prefabSimple.AddComponent<TestComponent>();

            var go = new GameObject("Prefab (with GODespawner)");
            prefabWithGoDespawner = go.AddComponent<GODespawner>();

            prefabWithISpawnable = new GameObject("Prefab (with ISpawnable)");
            prefabWithISpawnable.AddComponent<PositionalSpawnable>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(prefabSimple);
            Object.DestroyImmediate(prefabWithGoDespawner.gameObject);
            Object.DestroyImmediate(prefabWithISpawnable);

            PoolManager.PurgePools();
        }

        [Test]
        public void Spawn()
        {
            var instance = PoolManager.Spawn(prefabSimple);

            Assert.IsNotNull(instance);
            Assert.IsNotNull(instance.GetComponent<TestComponent>());
        }
        
        [Test]
        public void SpawnWithTransformation()
        {
            var newPosition = new Vector3(10, 0, 0);
            var newRotation = Quaternion.Euler(45, 12, 0);

            var instance = PoolManager.Spawn(prefabSimple, newPosition, newRotation);
            Assert.AreEqual(newPosition, instance.transform.position);
            Assert.IsTrue(instance.transform.rotation == newRotation);

        }

        [Test]
        public void SpawnWithComponentReference()
        {
            var instanceComponent = PoolManager.Spawn(prefabSimpleComponent);

            Assert.IsNotNull(instanceComponent);
        }

        [Test]
        public void SpawnAndDespawn()
        {
            var instance = PoolManager.Spawn(prefabSimple);

            Assert.IsTrue(instance.activeSelf);

            PoolManager.Despawn(prefabSimple, instance);

            Assert.IsNotNull(instance);
            Assert.IsFalse(instance.activeSelf);
        }

        [Test]
        public void DespawnWithGODespawner()
        {
            var instance = PoolManager.Spawn(prefabWithGoDespawner);

            Assert.IsTrue(instance.gameObject.activeSelf);

            PoolManager.Despawn(instance);

            Assert.IsNotNull(instance);
            Assert.IsFalse(instance.gameObject.activeSelf);
        }

        [Test]
        public void Recycle()
        {
            var instance = PoolManager.Spawn(prefabSimple, 1);
            PoolManager.Despawn(prefabSimple, instance);

            var newInstance = PoolManager.Spawn(prefabSimple);
            Assert.AreEqual(instance, newInstance);
        }

        [Test]
        public void RecycleWithTransformation()
        {
            var instance = PoolManager.Spawn(prefabSimple, Vector3.zero, Quaternion.identity, 1);
            PoolManager.Despawn(prefabSimple, instance);

            var newPosition = new Vector3(10, 0, 0);
            var newRotation = Quaternion.Euler(45, 12, 0);

            instance = PoolManager.Spawn(prefabSimple, newPosition, newRotation);
            Assert.AreEqual(newPosition, instance.transform.position);
            Assert.IsTrue(instance.transform.rotation == newRotation);
        }

        [Test]
        public void ReturnWithISpawnable()
        {
            var instance = PoolManager.Spawn(prefabWithISpawnable);

            Assert.IsTrue(instance.activeSelf);

            PoolManager.Despawn(prefabWithISpawnable, instance);

            Assert.IsNotNull(instance);
            Assert.IsTrue(instance.activeSelf);
            Assert.AreEqual(PositionalSpawnable.disabledPosition, instance.transform.position);
        }
    }
}
