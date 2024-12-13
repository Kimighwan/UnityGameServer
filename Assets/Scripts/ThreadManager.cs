using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 하고 싶은 행위를 Action 데이터형으로 구성된 리스트에 담아 여러 쓰레드를 사용하는 것 처럼 사용합니다.

public class ThreadManager : MonoBehaviour
{
    private static readonly List<Action> executeOnMainThread = new List<Action>();
    private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
    private static bool actionToExecuteOnMainThread = false;

    private void FixedUpdate()
    {
        UpdateMain();
    }

    /// <summary>Sets an action to be executed on the main thread.</summary>
    /// <param name="_action">The action to be executed on the main thread.</param>
    public static void ExecuteOnMainThread(Action _action)
    {
        if (_action == null)
        {
            Console.WriteLine("No action to execute on main thread!");
            return;
        }

        lock (executeOnMainThread)
        {
            executeOnMainThread.Add(_action);
            actionToExecuteOnMainThread = true;
        }
    }

    /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
    public static void UpdateMain()
    {
        if (actionToExecuteOnMainThread)
        {
            executeCopiedOnMainThread.Clear();
            lock (executeOnMainThread)
            {
                executeCopiedOnMainThread.AddRange(executeOnMainThread);
                executeOnMainThread.Clear();
                actionToExecuteOnMainThread = false;
            }

            for (int i = 0; i < executeCopiedOnMainThread.Count; i++)
            {
                executeCopiedOnMainThread[i]();
            }
        }
    }
}
