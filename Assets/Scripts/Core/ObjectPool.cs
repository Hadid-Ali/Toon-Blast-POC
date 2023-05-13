using System.Collections.Generic;
using UnityEngine;

    public class ObjectPool : MonoBehaviour
    {
        public GameObject prefab;
        public int initialSize;

        private readonly Stack<GameObject> instances = new Stack<GameObject>();

        private void Start()
        {
            for (var i = 0; i < initialSize; i++)
            {
                var obj = CreateInstance();
                obj.SetActive(false);
                instances.Push(obj);
            }
        }

        public GameObject GetObject()
        {
            var obj = instances.Count > 0 ? instances.Pop() : CreateInstance();
            obj.SetActive(true);
            return obj;
        }

        public void ReturnObject(GameObject obj)
        {
            var pooledObject = obj.GetComponent<PooledObject>();
            obj.SetActive(false);
            instances.Push(obj);
        }
        public void Reset()
        {
            var objectsToReturn = new List<GameObject>();
            foreach (var instance in transform.GetComponentsInChildren<PooledObject>())
            {
                if (instance.gameObject.activeSelf)
                {
                    objectsToReturn.Add(instance.gameObject);
                }
            }
            foreach (var instance in objectsToReturn)
            {
                ReturnObject(instance);
            }
        }

        private GameObject CreateInstance()
        {
            var obj = Instantiate(prefab);
            var pooledObject = obj.AddComponent<PooledObject>();
            pooledObject.pool = this;
            obj.transform.SetParent(transform);
            return obj;
        }
    }

    public class PooledObject : MonoBehaviour
    {
        public ObjectPool pool;
    }
