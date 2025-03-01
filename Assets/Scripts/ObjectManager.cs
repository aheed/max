using System;
using UnityEngine;
using UnityEngine.Pool;

public class ManagedObject : MonoBehaviour
{
    public ManagedObjectReference selfReference;

    public void Release() {
        selfReference.Release();
    }

    // Overridable methods
    public virtual void Deactivate() {}
    public virtual void Reactivate() {}    
}

public class ManagedObjectReference
{
    public ManagedObject managedObject;
    private IObjectPool<ManagedObject> pool;
    
    public ManagedObjectReference(ManagedObject managedObject, IObjectPool<ManagedObject> pool)
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

/// <summary>
/// Objects are created and destroyed as needed. No actual pooling is done.
/// </summary>
public class ZeroObjectPool : IObjectPool<ManagedObject>
{
    private readonly ManagedObject prefab;

    private readonly Transform parent;

    public ZeroObjectPool(ManagedObject prefab, Transform parent)
    {
        this.prefab = prefab;
        this.parent = parent;
    }

    public int CountInactive {get { return 0; }}

    public void Clear() {}

    public ManagedObject Get()
    {
        return GameObject.Instantiate(prefab, parent);
    }

    public PooledObject<ManagedObject> Get(out ManagedObject v)
    {
        v = Get();
        return new PooledObject<ManagedObject>(v, this);
    }

    public void Release(ManagedObject item)
    {
        GameObject.Destroy(item.gameObject);
    }
}

public class ObjectManager
{
    public enum PoolType
    {
        None,
        Stack
    }

    private readonly PoolType poolType;

    private readonly ManagedObject prefab;

    public readonly Transform parent;

    private readonly bool deactivateOnRelease;

    // Collection checks will throw errors if we try to release an item that is already in the pool.
    public bool collectionChecks = false;
    public int maxPoolSize = 200;

    private readonly IObjectPool<ManagedObject> m_Pool;

    private IObjectPool<ManagedObject> CreatePool()
    {
        if (poolType == PoolType.None)
        {
            return new ZeroObjectPool(prefab, parent);
        }
        else
        {
            return new ObjectPool<ManagedObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, 10, maxPoolSize);
        }
    }

    public ObjectManager(ManagedObject prefab, Transform parent, PoolType poolType, bool deactivateOnRelease = true)
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

    ManagedObject CreatePooledItem()
    {
        var ret = GameObject.Instantiate(prefab, parent);
        return ret;
    }

    // Called when an item is returned to the pool using Release
    void OnReturnedToPool(ManagedObject obj)
    {
        if (deactivateOnRelease) {
            obj.Deactivate();
        }
    }

    // Called when an item is taken from the pool using Get
    void OnTakeFromPool(ManagedObject obj)
    {
        if (deactivateOnRelease) {
            obj.Reactivate();
        }
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    void OnDestroyPoolObject(ManagedObject obj)
    {
        GameObject.Destroy(obj.gameObject);
    }
}
