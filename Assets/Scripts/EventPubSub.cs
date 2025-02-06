using System;
using System.Collections.Generic;

public interface IEventPublisher<T>
{
    void Publish(GameEvent evt, T data);
}

public interface IEventSubscribable<T>
{
    void Subscribe(GameEvent evt, Action<T> callback);
    void Unsubscribe(GameEvent evt, Action<T> callback);
}

public class EventPubSub<T> : IEventPublisher<T>, IEventSubscribable<T>
{
    private readonly Dictionary<GameEvent, List<Delegate>> _subscribers = new Dictionary<GameEvent, List<Delegate>>();

    public void Subscribe(GameEvent evt, Action<T> callback)
    {
        if (!_subscribers.ContainsKey(evt))
        {
            _subscribers[evt] = new List<Delegate>();
        }

        _subscribers[evt].Add(callback);
    }

    public void Unsubscribe(GameEvent evt, Action<T> callback)
    {
        if (_subscribers.ContainsKey(evt))
        {
            _subscribers[evt].Remove(callback);
            if (_subscribers[evt].Count == 0)
            {
                _subscribers.Remove(evt);
            }
        }
    }

    public void Publish(GameEvent evt, T data)
    {
        if (!_subscribers.ContainsKey(evt))
        {
            return;
        }

        foreach (var callback in _subscribers[evt])
        {
            ((Action<T>)callback)(data);
        }
    }
}