using UnityEngine;
using UnityEngine.Pool;

public interface IManagedObject
{
    void Release();

    GameObject GameObject { get; }
}

public class ManagedObject : IManagedObject
{
    IObjectPool<GameObject> pool;
    GameObject gameObj;

    public GameObject GameObject => gameObj;

    public ManagedObject(IObjectPool<GameObject> pool)
    {
        this.pool = pool;
        gameObj = pool.Get();
    }

    public void Release()
    {
        pool.Release(gameObj);
        pool = null;
        gameObj = null;
    }
}

public class SimpleObjectManager : IObjectPool<GameObject>
{
    private GameObject prefab;

    private Transform parent;

    public SimpleObjectManager(GameObject prefab, Transform parent)
    {
        this.prefab = prefab;
        this.parent = parent;
    }

    public int CountInactive {get { return 0; }}

    public void Clear() {}

    public GameObject Get()
    {
        return GameObject.Instantiate(prefab, parent);
    }

    public PooledObject<GameObject> Get(out GameObject v)
    {
        v = Get();
        return new PooledObject<GameObject>(v, this);
    }

    public void Release(GameObject item)
    {
        GameObject.Destroy(item);
    }
}

// ObjectManagerFactory
// Creates Pools of GameObjects (objects implementing IObjectPool<GameObject>)
// option: kind of manager, unpooled/object pool
// option: max pool size
// prefab
// No need to write a new ManagedObject class. Use PooledObject<T> instead.
//   Actually we seem to need it afterall. PooledObject does not implement IDisposable (?) or anything equivalent.
//  
/// </summary>
// ObjectManagerFactory could be a generic class, but I see no need for managing anything other than GameObjects.
// One ObjectManagerFactory per prefab => one object manager per prefab.

public class ObjectManagerFactory
{
    public enum PoolType
    {
        None,
        Stack
    }

    private PoolType poolType;

    private GameObject prefab;

    private Transform parent;

    // Collection checks will throw errors if we try to release an item that is already in the pool.
    public bool collectionChecks = true;
    public int maxPoolSize = 200;

    IObjectPool<GameObject> m_Pool;

    public IObjectPool<GameObject> Pool
    {
        get
        {
            if (m_Pool == null)
            {
                if (poolType == PoolType.None)
                {
                    m_Pool = new SimpleObjectManager(prefab, parent);
                }
                else
                {
                    m_Pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, 10, maxPoolSize);
                }
            }
            return m_Pool;
        }
    }

    public void Initialize(GameObject prefab, Transform parent, PoolType poolType)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.poolType = poolType;
    }

    GameObject CreatePooledItem()
    {
        return GameObject.Instantiate(prefab, parent);
    }

    // Called when an item is returned to the pool using Release
    void OnReturnedToPool(GameObject system)
    {
    }

    // Called when an item is taken from the pool using Get
    void OnTakeFromPool(GameObject system)
    {
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    void OnDestroyPoolObject(GameObject gameObj)
    {
        //Destroy(gameObj);
        GameObject.Destroy(gameObj);
    }
}