using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;

public delegate bool CustomCmdDelegate(string[] args);

public class CustomCmdExecutor {
    private static CustomCmdExecutor _inst = null;

    public static CustomCmdExecutor Instance {
        get {
            if (_inst == null) {
                _inst = new CustomCmdExecutor();
            }
            return _inst;
        }
    }

    public void Init() {
        foreach (var method in typeof(CustomCmd).GetMethods(BindingFlags.Public | 
                                                            BindingFlags.NonPublic | 
                                                            BindingFlags.Instance)) {

            foreach (var attr in method.GetCustomAttributes(typeof(CustomCmdHandler), false)) {
                try {
                    CustomCmdDelegate del = Delegate.CreateDelegate(typeof(CustomCmdDelegate), CustomCmd.Instance, method) as CustomCmdDelegate;
                    if (del != null) {
                        string szCmd = (attr as CustomCmdHandler).Command;
                        m_handlers[szCmd] = del;
                    }
                }
                catch (Exception ex) {
                    Debug.LogException(ex);
                }
                
            }
        }
    }

    public bool Execute(string[] arrayCmd) {
        CustomCmdDelegate _handler = null;
        if (m_handlers.TryGetValue(arrayCmd[0], out _handler)) {
            return _handler(arrayCmd);
        }
        else {
            return false;
        }
    }

    private Dictionary<string, CustomCmdDelegate> m_handlers = new Dictionary<string, CustomCmdDelegate>();
}


[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomCmdHandler : Attribute {
    public CustomCmdHandler(string cmd) {
        Command = cmd;
    }

    public string Command;
}

public class CustomCmd {

    private static CustomCmd _inst = null;

    public static CustomCmd Instance {
        get {
            if (_inst == null) {
                _inst = new CustomCmd();
            }
            return _inst;
        }
    }

    [CustomCmdHandler("LoadPrefab")]
    public bool LoadPrefab(string[] args) {
        GameObject prefab = Resources.Load<GameObject>("Prefab/Cube");
        if (prefab == null) {
            return false;
        }
        return true;
    }

    [CustomCmdHandler("LoadMaterial")]
    public bool LoadMaterial(string[] args) {
        Material mat = Resources.Load<Material>("Material/Test");
        if (mat == null) {
            return false;
        }
        return true;
    }

    [CustomCmdHandler("LoadTexture")]
    public bool LoadTexture(string[] args) {
        Texture2D tex = Resources.Load<Texture2D>("Texture/long");
        if (tex == null) {
            return false;
        }
        return true;
    }
}