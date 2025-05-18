using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentSingleton<T> : MonoBehaviour where T:Component
{
    private static T _instance;
    public T Instance
    {
        get { return _instance; }
    }
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            Destroy(this);
        }
        DontDestroyOnLoad(Instance);
    }
    
}
