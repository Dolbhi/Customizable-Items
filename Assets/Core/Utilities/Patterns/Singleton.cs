using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColbyDoan
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance;
        // {
        //     get
        //     {
        //         if (_instance) return _instance;
        //         else
        //         {

        //             return _instance;
        //         }
        //     }
        //     private set
        //     {
        //         _instance = value;
        //     }
        // }
        // static T _instance;

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("More than 1 " + typeof(T) + " present in scene, my name: " + name + " duplicate name: " + Instance.name);
                Destroy(gameObject);
                //instance = this as T;
            }
            else
                Instance = this as T;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }

    public class FindableByRoot<T> : MonoBehaviour where T : FindableByRoot<T>
    {
        public static Dictionary<Transform, T> instanceFromTransform = new Dictionary<Transform, T>();

        public static T FindFromRoot(Transform root)
        {
            if (instanceFromTransform.TryGetValue(root, out T result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        protected virtual void OnEnable()
        {
            instanceFromTransform.Add(transform.root, this as T);
        }

        protected virtual void OnDisable()
        {
            instanceFromTransform.Remove(transform.root);
        }
    }

    public static class DependancyInjector
    {
        public static void InjectDependancies<T>(T depended) where T : MonoBehaviour
        {
            foreach (IAutoDependancy<T> dependant in depended.GetComponentsInChildren<IAutoDependancy<T>>())
            {
                dependant.Dependancy = depended;
            }
        }
    }

    public interface IAutoDependancy<T> where T : MonoBehaviour
    {
        T Dependancy { set; }
    }
}