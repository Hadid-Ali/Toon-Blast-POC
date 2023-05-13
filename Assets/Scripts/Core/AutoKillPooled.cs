using UnityEngine;

public class AutoKillPooled : MonoBehaviour
{
    public float time = 2.0f;

    private PooledObject pooledObject;
    private float accTime;

    private void OnEnable()
    {
        accTime = 0.0f;
    }

    private void Start()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    private void Update()
    {
        accTime += Time.deltaTime;
        if (accTime >= time)
        {
            pooledObject.pool.ReturnObject(gameObject);
        }
    }
}