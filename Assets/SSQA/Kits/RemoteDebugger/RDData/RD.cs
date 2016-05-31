using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public static class ShowPanelDataSet {
    public static void InitDataSet() {
        ms_rdgameobjectDict.Clear();
        ms_lstRootRDObjs.Clear();

        ms_rdComponentDict.Clear();

        ms_currentSelectComps = null;
        ms_remoteRDComponent = null;

        if (ms_remoteGameObject == null) {
            ms_remoteGameObject = new GameObject("_RemoteDebugger");
            ms_remoteGameObject.SetActive(false);
        }
        if (ms_remoteComponent != null) {
            GameObject.DestroyImmediate(ms_remoteComponent);
        }
    }

    public static void AddRdGameObject(RDGameObject rd) {
        int nInstanceID = rd.nInstanceID;
        if (!ms_rdgameobjectDict.ContainsKey(nInstanceID)) {
            ms_rdgameobjectDict.Add(nInstanceID, rd);
        }
        if (rd.nParentID == -1) {
            ms_lstRootRDObjs.Add(rd);
        }
    }

    public static bool TryGetRDGameObject(int nInstanceID, out RDGameObject rd) {
        return ms_rdgameobjectDict.TryGetValue(nInstanceID, out rd);
    }

    public static void AddRdComponent(RDComponent rdComp) {
        int nInstanceID = rdComp.nInstanceID;
        if (ms_rdComponentDict.ContainsKey(nInstanceID)) {
            ms_rdComponentDict[nInstanceID] = rdComp;
        }
        else {
            ms_rdComponentDict.Add(nInstanceID, rdComp);
        }
    }

    public static void AddRemoteComponent(string szComponentType) {
        //Type t = Type.GetType(szComponentType + ",UnityEngine");

        Type t = GetComponentType(szComponentType);

        if (t == null || ms_remoteGameObject == null) {
            return;
        }

        if (ms_remoteGameObject.GetComponent(t) != null) {
            return;
        }

        ms_remoteGameObject.AddComponent(t);
    }

    public static Type GetComponentType(string TypeName) {
        Type T = Type.GetType(TypeName + ",UnityEngine");

        if (T == null)
            T = Type.GetType(TypeName + ",UnityEngine.UI");

        if (T == null)
            T = Type.GetType(TypeName + ",UnityEngine.Networking");

        if (T == null)
            T = Type.GetType(TypeName);

        if (T == null)
            return null;

        else {
            return T;
        }
    }

    public static Dictionary<int, RDGameObject> ms_rdgameobjectDict = new Dictionary<int, RDGameObject>();
    public static List<RDGameObject> ms_lstRootRDObjs = new List<RDGameObject>();

    public static Dictionary<int, RDComponent> ms_rdComponentDict = new Dictionary<int, RDComponent>();
    public static RDComponent[] ms_currentSelectComps = null;
    public static RDProperty[] ms_currentSelectProperty = null;
    public static RDGameObject ms_selectRDGameObject = null;
    public static GameObject ms_remoteGameObject = null;
    public static Component ms_remoteComponent = null;
    public static RDComponent ms_remoteRDComponent = null;

    //public static List<string> AllFolderPath = new List<string>();
    //public static Dictionary<int, ObjNode> ms_objNodeDict = new Dictionary<int, ObjNode>();


    public static void ClearAllData() {
        if (ms_rdgameobjectDict.Count > 0) {
            ms_rdgameobjectDict.Clear();
        }
        if (ms_lstRootRDObjs.Count > 0) {
            ms_lstRootRDObjs.Clear();
        }
        if (ms_rdComponentDict.Count > 0) {
            ms_rdComponentDict.Clear();
        }
        if (ms_remoteGameObject != null) {
            MonoBehaviour.DestroyImmediate(ms_remoteGameObject);
        }
    }
}


public static class GameRunTimeDataSet {
    public static void InitDataSet() {
        ms_gameObjectDict.Clear();
        ms_componentDict.Clear();
    }

    public static void AddGameObject(GameObject obj) {
        int nInstanceID = obj.GetInstanceID();
        if (!ms_gameObjectDict.ContainsKey(nInstanceID)) {
            ms_gameObjectDict.Add(nInstanceID, obj);
        }
    }

    public static bool TryGetGameObject(int nInstanceID, out GameObject go) {
        return ms_gameObjectDict.TryGetValue(nInstanceID, out go);
    }

    public static void AddComponent(Component comp) {
        int nInstanceID = comp.GetInstanceID();
        if (!ms_componentDict.ContainsKey(nInstanceID)) {
            ms_componentDict.Add(nInstanceID, comp);
        }
    }

    public static bool TryGetComponent(int nInstanceID, out UnityEngine.Component comp) {
        return ms_componentDict.TryGetValue(nInstanceID, out comp);
    }


    public static Dictionary<int, GameObject> ms_gameObjectDict = new Dictionary<int, GameObject>();
    public static Dictionary<int, Component> ms_componentDict = new Dictionary<int, Component>();
}
