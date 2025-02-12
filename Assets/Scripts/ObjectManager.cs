using UnityEngine;
using UnityEngine.Pool;

public interface IManagedObject
{
    void Release();

    //GameObject GameObject { get; }
}

/*public interface IManagedObject3
{
    IManagedObjectReleaser Releaser { get; set; } 
    void Deactivate();

    void Reactivate();
}*/

public interface IManagedObjectReleaser
{
    void Release();
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

public class ManagedObject2 : MonoBehaviour, IManagedObject
{
    IObjectPool<ManagedObject2> pool;
    //GameObject gameObj;

    public IObjectPool<ManagedObject2> Pool
    {
        get => pool;
        set => pool = value;
    }

    public GameObject GameObject => gameObject;

    public void Release()
    {
        pool.Release(this);
        pool = null; //unnecessary?
    }
}

public class ManagedObject3 : MonoBehaviour
{
    public IManagedObjectReleaser releaser;

    public IManagedObjectReleaser Releaser {
        get => releaser;
        set => releaser = value;
    }

    // Overridable methods
    public virtual void Deactivate() {}
    public virtual void Reactivate() {}
    public virtual void Release() {
        releaser.Release();
    }
}

public class PooledObjectReleaser : IManagedObjectReleaser
{
    IObjectPool<ManagedObject3> pool;
    ManagedObject3 managedObj;

    public PooledObjectReleaser(IObjectPool<ManagedObject3> pool, ManagedObject3 gameObj)
    {
        this.pool = pool;
        this.managedObj = gameObj;
    }

    public void Release()
    {
        pool.Release(managedObj);
        pool = null;
        managedObj = null;
    }
}

public class DestroyObjectReleaser : IManagedObjectReleaser
{
    ManagedObject3 managedObj;

    public DestroyObjectReleaser(ManagedObject3 gameObj)
    {
        this.managedObj = gameObj;
    }

    public void Release()
    {
        GameObject.Destroy(managedObj);
        managedObj = null;
    }
}

public class NoopObjectReleaser : IManagedObjectReleaser
{
    private static NoopObjectReleaser instance;

    public static NoopObjectReleaser Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new NoopObjectReleaser();
            }
            return instance;
        }
    }
    public void Release() {}
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

    private bool deactivateOnRelease;

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

    public ObjectManagerFactory(GameObject prefab, Transform parent, PoolType poolType, bool deactivateOnRelease = false)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.poolType = poolType;
        this.deactivateOnRelease = deactivateOnRelease;
    }

    GameObject CreatePooledItem()
    {
        return GameObject.Instantiate(prefab, parent);
    }

    // Called when an item is returned to the pool using Release
    void OnReturnedToPool(GameObject gameObj)
    {
        if (deactivateOnRelease) {
            gameObj.SetActive(false);
        }
    }

    // Called when an item is taken from the pool using Get
    void OnTakeFromPool(GameObject gameObj)
    {
        if (deactivateOnRelease) {
            gameObj.SetActive(true);
        }
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    void OnDestroyPoolObject(GameObject gameObj)
    {
        GameObject.Destroy(gameObj);
    }
}

public class ObjectManagerFactory2
{
    public enum PoolType
    {
        None,
        Stack
    }

    private PoolType poolType;

    private ManagedObject2 prefab;

    private Transform parent;

    private bool deactivateOnRelease;

    // Collection checks will throw errors if we try to release an item that is already in the pool.
    public bool collectionChecks = true;
    public int maxPoolSize = 200;

    IObjectPool<ManagedObject2> m_Pool;

    public IObjectPool<ManagedObject2> Pool
    {
        get
        {
            if (m_Pool == null)
            {
                /*
                if (poolType == PoolType.None)
                {
                    m_Pool = new SimpleObjectManager(prefab, parent);
                }
                else
                {*/
                    m_Pool = new ObjectPool<ManagedObject2>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, 10, maxPoolSize);
                //}
            }
            return m_Pool;
        }
    }

    public ObjectManagerFactory2(ManagedObject2 prefab, Transform parent, PoolType poolType, bool deactivateOnRelease = false)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.poolType = poolType;
        this.deactivateOnRelease = deactivateOnRelease;
    }

    ManagedObject2 CreatePooledItem()
    {
        var ret = GameObject.Instantiate(prefab, parent);
        ret.Pool = m_Pool;
        return ret;
    }

    // Called when an item is returned to the pool using Release
    void OnReturnedToPool(ManagedObject2 obj)
    {
        if (deactivateOnRelease) {
            obj.gameObject.SetActive(false);
        }
    }

    // Called when an item is taken from the pool using Get
    void OnTakeFromPool(ManagedObject2 obj)
    {
        if (deactivateOnRelease) {
            obj.gameObject.SetActive(true);
        }
        obj.Pool = m_Pool;
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    void OnDestroyPoolObject(ManagedObject2 obj)
    {
        GameObject.Destroy(obj.gameObject);
    }
}

public class ObjectManagerFactory3
{
    public enum PoolType
    {
        None,
        Stack
    }

    private PoolType poolType;

    private ManagedObject3 prefab;

    private Transform parent;

    private bool deactivateOnRelease;

    // Collection checks will throw errors if we try to release an item that is already in the pool.
    public bool collectionChecks = true;
    public int maxPoolSize = 200;

    IObjectPool<ManagedObject3> m_Pool;

    public IObjectPool<ManagedObject3> Pool
    {
        get
        {
            if (m_Pool == null)
            {
                /*
                if (poolType == PoolType.None)
                {
                    m_Pool = new SimpleObjectManager(prefab, parent);
                }
                else
                {*/
                    m_Pool = new ObjectPool<ManagedObject3>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, 10, maxPoolSize);
                //}
            }
            return m_Pool;
        }
    }

    public ObjectManagerFactory3(ManagedObject3 prefab, Transform parent, PoolType poolType, bool deactivateOnRelease = true)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.poolType = poolType;
        this.deactivateOnRelease = deactivateOnRelease;
    }

    ManagedObject3 CreatePooledItem()
    {
        var ret = GameObject.Instantiate(prefab, parent);
        return ret;
    }

    // Called when an item is returned to the pool using Release
    void OnReturnedToPool(ManagedObject3 obj)
    {
        if (deactivateOnRelease) {
            obj.Deactivate();
        }
    }

    // Called when an item is taken from the pool using Get
    void OnTakeFromPool(ManagedObject3 obj)
    {
        if (deactivateOnRelease) {
            obj.Reactivate();
        }
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    void OnDestroyPoolObject(ManagedObject3 obj)
    {
        GameObject.Destroy(obj.gameObject);
    }
}
