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

    public void UnInit() {
        m_handlers.Clear();
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

    public Dictionary<string, CustomCmdDelegate> m_handlers = new Dictionary<string, CustomCmdDelegate>();
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
    private CustomCmd() {

    }

    public static CustomCmd Instance {
        get {
            if (_inst == null) {
                _inst = new CustomCmd();
            }
            return _inst;
        }
    }

    /*
    [CustomCmdHandler("JustTest")]
    public bool JustTest(string[] args) {
        Debug.LogFormat("Just Test Cmd : {0}", args[0]);
        return true;
    }

    [CustomCmdHandler("MainPlayerName")]
    public bool MainPlayerName(string[] args) {
        Player player = FamilyMgr.m_myFamily.GetActivePlayer();
        Debug.LogFormat(player.name);
        return true;
    }
    */

    [CustomCmdHandler("FrustumCull")]
    public bool CullAllObjectInFrustum(string[] args) {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) {
            return false;
        }
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        Renderer[] rs = UnityEngine.GameObject.FindObjectsOfType<Renderer>();

        for (int i = 0; i < rs.Length; ++i) {
            Renderer r = rs[i];
            if (GeometryUtility.TestPlanesAABB(planes, r.bounds)) {
                r.enabled = false;
            }
        }
        return true;
    }

    [CustomCmdHandler("EnableStaticBatch")]
    public bool EnableStaticBatch(string[] args) {
        //bool bEnable = System.Boolean.Parse(args[1]);
        //AssetBind.bBatch = bEnable;
        return true;
    }

    [CustomCmdHandler("MeshCombine")]
    public bool MeshesCombine(string[] args) {
        GameObject model = GameObject.Find("Env");
        if (model != null) {
            //MeshCombine.WorkWithLightMap(model.transform);
            StaticBatchingUtility.Combine(model);
        }
        
        return true;
    }
}