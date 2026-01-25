using System.Collections;
using UnityEngine;

namespace Scp053.Components.Features;

public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner _instance;
    
    public static CoroutineRunner Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[Scp053] CoroutineRunner");
                _instance = go.AddComponent<CoroutineRunner>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public static Coroutine Run(IEnumerator routine) => Instance.StartCoroutine(routine);
    public static void Stop(Coroutine coroutine) => Instance.StopCoroutine(coroutine);
    public static void StopAll() => Instance.StopAllCoroutines();
}