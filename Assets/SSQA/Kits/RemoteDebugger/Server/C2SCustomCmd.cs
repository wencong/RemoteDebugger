using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;

public delegate bool CustomCmdDelegate(string[] args);

public class CustomCmdExecutor {
    private static CustomCmdExecutor _inst = null;
    private NetServer net_server = null;

    public static CustomCmdExecutor Instance {
        get {
            if (_inst == null) {
                _inst = new CustomCmdExecutor();
            }
            return _inst;
        }
    }

    public NetServer net_conn {
        get { return net_server; }
        set { net_server = value; }
    }

    public void Init(NetServer net_server) {
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

        if (net_server != null) {
            this.net_server = net_server;
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

    /*[CustomCmdHandler("EnableStaticBatch")]
    public bool EnableStaticBatch(string[] args) {
        bool bEnable = System.Boolean.Parse(args[1]);

        // by uwa4d
        // AssetBind.bBatch = bEnable;
        System.Type assetBindType = LogicAssembly.Instance.GetType("AssetBind");
        assetBindType.GetField("bBatch", BindingFlags.Static | BindingFlags.Public).SetValue(null, bEnable);
        return true;
    }*/

    /*[CustomCmdHandler("MeshCombine")]
    public bool MeshesCombine(string[] args) {
        GameObject model = GameObject.Find("Environment/Models");
        if (model != null) {
            // by uwa4d
            // MeshCombine.WorkWithLightMap(model.transform);
            System.Object[] parameters = { model.transform };
            LogicAssembly.Instance.GetType("MeshCombine")
                .GetMethod("WorkWithLightMap", BindingFlags.Public | BindingFlags.Static).Invoke(null, parameters);
        }
        
        return true;
    }*/

    [CustomCmdHandler("FrustumQuery")]
    public bool FrustumQuery(string[] args) {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) {
            return false;
        }
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        Renderer[] rs = UnityEngine.GameObject.FindObjectsOfType<Renderer>();

        HashSet<int> setObjIds = new HashSet<int>();
        List<RDGameObject> rdGameObjects = new List<RDGameObject>();

        for (int i = 0; i < rs.Length; ++i) {
            Renderer r = rs[i];
            if (GeometryUtility.TestPlanesAABB(planes, r.bounds)) {
                try {
                    AddRdGameObjs(ref setObjIds, ref rdGameObjects, r.transform);               
                    }
                catch (Exception ex) {
                    CustomCmdExecutor.Instance.net_conn.LogMsgToClient(ex.ToString());
                }
            }
        }

        try {
            string rdGameObjList = RDDataBase.SerializerArray<RDGameObject>(rdGameObjects.ToArray());
            Cmd usCmd = new Cmd(rdGameObjList.Length);

            usCmd.WriteNetCmd(NetCmd.S2C_QueryFrustumObjs);
            usCmd.WriteString(rdGameObjList);
            CustomCmdExecutor.Instance.net_conn.SendCommand(usCmd);
        } catch (Exception ex) {
            CustomCmdExecutor.Instance.net_conn.LogMsgToClient(ex.ToString());
        }
        
        return true;
    }

    void AddRdGameObjs(ref HashSet<int> setObjIds, ref List<RDGameObject> rdObjs, Transform ts) {
        int objId = 0;
        GameObject curObj = ts.gameObject;
        
        while (ts.parent != null) {
            objId = ts.parent.gameObject.GetInstanceID();
            if (!setObjIds.Contains(objId)) {
                setObjIds.Add(objId);
                rdObjs.Add(new RDGameObject(ts.parent.gameObject));
            }

            ts = ts.parent;
        }

        objId = curObj.GetInstanceID();
        if (!setObjIds.Contains(objId)) {
            setObjIds.Add(objId);
            rdObjs.Add(new RDGameObject(curObj));
        }
    }

    [CustomCmdHandler("RegionCombine")]
    public bool MeshReginCombine(string[] args) {
        return true;
    }
}