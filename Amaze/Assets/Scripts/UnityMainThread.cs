using UnityEngine;
using System;
using System.Collections.Generic;

public class UnityMainThread : MonoBehaviour
{
    private static UnityMainThread instance;
    private static readonly Queue<Action> actions = new Queue<Action>();

    public static void Init()
    {
        if (instance == null)
        {
            GameObject obj = new GameObject("UnityMainThread");
            instance = obj.AddComponent<UnityMainThread>();
            DontDestroyOnLoad(obj);
        }
    }

    public static void Run(Action action)
    {
        lock (actions)
        {
            actions.Enqueue(action);
        }
    }

    void Update()
    {
        while (actions.Count > 0)
        {
            Action a;
            lock (actions) a = actions.Dequeue();
            a?.Invoke();
        }
    }
}
