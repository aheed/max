using System;
using UnityEngine;
using UnityEngine.Pool;

public interface IManagedObject
{
    void Release();

    //GameObject GameObject { get; }
}

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
    public Action releaseAction;

    public void Release() {
        releaseAction();
    }

    public void DestroyGameObject() {
        GameObject.Destroy(gameObject);
    }   

    // Overridable methods
    public virtual void Deactivate() {}
    public virtual void Reactivate() {}    
}

public class ManagedObject4 : MonoBehaviour
{
    public ManagedObjectReference selfReference;

    public void Release() {
        selfReference.Release();
    }

    public void DestroyGameObject() {
        GameObject.Destroy(gameObject);
    }   

    // Overridable methods
    public virtual void Deactivate() {}
    public virtual void Reactivate() {}    
}

public class ManagedObjectReference
{
    public ManagedObject4 managedObject;
    private IObjectPool<ManagedObject4> pool;
    
    public ManagedObjectReference(ManagedObject4 managedObject, IObjectPool<ManagedObject4> pool)
    {
        this.managedObject = managedObject;
        this.pool = pool;
    }

    public void Release()
    {
        if (managedObject == null)
        {
            // The object has already been released.
            return;
        }

        pool.Release(managedObject);
        managedObject = null;
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

public class SimpleObjectManager4 : IObjectPool<ManagedObject4>
{
    private ManagedObject4 prefab;

    private Transform parent;

    public SimpleObjectManager4(ManagedObject4 prefab, Transform parent)
    {
        this.prefab = prefab;
        this.parent = parent;
    }

    public int CountInactive {get { return 0; }}

    public void Clear() {}

    public ManagedObject4 Get()
    {
        return GameObject.Instantiate(prefab, parent);
    }

    public PooledObject<ManagedObject4> Get(out ManagedObject4 v)
    {
        v = Get();
        return new PooledObject<ManagedObject4>(v, this);
    }

    public void Release(ManagedObject4 item)
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




public class ObjectManagerFactory4
{
    public enum PoolType
    {
        None,
        Stack
    }

    private readonly PoolType poolType;

    private readonly ManagedObject4 prefab;

    private readonly Transform parent;

    private readonly bool deactivateOnRelease;

    // Collection checks will throw errors if we try to release an item that is already in the pool.
    public bool collectionChecks = false;
    public int maxPoolSize = 200;

    private readonly IObjectPool<ManagedObject4> m_Pool;

    private IObjectPool<ManagedObject4> CreatePool()
    {
        if (poolType == PoolType.None)
        {
            return new SimpleObjectManager4(prefab, parent);
        }
        else
        {
            return new ObjectPool<ManagedObject4>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, 10, maxPoolSize);
        }
    }

    public ObjectManagerFactory4(ManagedObject4 prefab, Transform parent, PoolType poolType, bool deactivateOnRelease = true)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.poolType = poolType;
        this.deactivateOnRelease = deactivateOnRelease;
        m_Pool = CreatePool();
    }

    public ManagedObjectReference Get()
    {
        var managedObjectRef = new ManagedObjectReference(m_Pool.Get(), m_Pool);
        managedObjectRef.managedObject.selfReference = managedObjectRef;
        return managedObjectRef;
    }

    ManagedObject4 CreatePooledItem()
    {
        var ret = GameObject.Instantiate(prefab, parent);
        return ret;
    }

    // Called when an item is returned to the pool using Release
    void OnReturnedToPool(ManagedObject4 obj)
    {
        if (deactivateOnRelease) {
            obj.Deactivate();
        }
    }

    // Called when an item is taken from the pool using Get
    void OnTakeFromPool(ManagedObject4 obj)
    {
        if (deactivateOnRelease) {
            obj.Reactivate();
        }
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    void OnDestroyPoolObject(ManagedObject4 obj)
    {
        GameObject.Destroy(obj.gameObject);
    }
}
