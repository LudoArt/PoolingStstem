using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoolingSystem
{
    public static class PrefabPoolingSystem
    {
        static Dictionary<GameObject, PrefabPool> _prefabToPoolMap = new Dictionary<GameObject, PrefabPool>();
        static Dictionary<GameObject, PrefabPool> _goToPoolMap = new Dictionary<GameObject, PrefabPool>();

        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
		{
            if (!_prefabToPoolMap.ContainsKey(prefab))
			{
                _prefabToPoolMap.Add(prefab, new PrefabPool());
			}

            PrefabPool pool = _prefabToPoolMap[prefab];
            GameObject go = pool.Spawn(prefab, position, rotation);
            _goToPoolMap.Add(go, pool);
            return go;
		}

        public static GameObject Spawn(GameObject prefab)
		{
            return Spawn(prefab, Vector3.zero, Quaternion.identity);
        }

        public static bool Despawn(GameObject obj)
		{
            if (!_goToPoolMap.ContainsKey(obj))
			{
                Debug.LogError(string.Format("Object {0} not managed by pool system!", obj.name));
                return false;
			}

            PrefabPool pool = _goToPoolMap[obj];
            if (pool.Despawn(obj))
			{
                _goToPoolMap.Remove(obj);
                return true;
			}

            return false;
		}

        public static void Prespawn(GameObject prefab, int numToSpawn)
		{
			List<GameObject> spawnedObjects = new List<GameObject>();

			for (int i = 0; i < numToSpawn; i++)
			{
				spawnedObjects.Add(Spawn(prefab));
			}

			for (int i = 0; i < numToSpawn; i++)
			{
				Despawn(spawnedObjects[i]);
			}

			spawnedObjects.Clear();
		}

		public static void Reset()
		{
			_prefabToPoolMap.Clear();
			_goToPoolMap.Clear();
		}
    }


    public struct PoolablePrefabData
	{
        public GameObject go;
        public IPoolableComponent[] poolableComponents;
	}


    public class PrefabPool
	{
        Dictionary<GameObject, PoolablePrefabData> _activeList = new Dictionary<GameObject, PoolablePrefabData>();
        Queue<PoolablePrefabData> _inactiveList = new Queue<PoolablePrefabData>();

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
		{
            PoolablePrefabData data;

            if (_inactiveList.Count > 0)
			{
                data = _inactiveList.Dequeue();
			}
            else
			{
                GameObject newGO = GameObject.Instantiate(prefab, position, rotation) as GameObject;
                data = new PoolablePrefabData();
                data.go = newGO;
                data.poolableComponents = newGO.GetComponents<IPoolableComponent>();
			}

			data.go.SetActive(true);
			data.go.transform.position = position;
			data.go.transform.rotation = rotation;

			for (int i = 0; i < data.poolableComponents.Length; i++)
			{
				data.poolableComponents[i].Spawned();
			}
			_activeList.Add(data.go, data);

            return data.go;
		}

		public bool Despawn(GameObject objToDespawn)
		{
			if (!_activeList.ContainsKey(objToDespawn))
			{
				Debug.LogError(string.Format("This Object ({0}) is not managed by this object pool!", objToDespawn.name));
				return false;
			}

			PoolablePrefabData data = _activeList[objToDespawn];

			for (int i = 0; i < data.poolableComponents.Length; i++)
			{
				data.poolableComponents[i].Despawned();
			}

			data.go.SetActive(false);
			_activeList.Remove(objToDespawn);
			_inactiveList.Enqueue(data);

			return true;
		}
    }
}
