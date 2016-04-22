using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LitJson;


public class RDDataBase {
    public static string Serializer<T>(T obj) {
        return JsonMapper.ToJson(obj);
    }

    public static T Deserializer<T>(string data){
        return JsonMapper.ToObject<T>(data);
    }
}

public class RDGameObject {
    public int nInstanceID;
    public string szName;
    public bool bActive;

    public int nParentID;
    public int[] arrChildren;

    public bool bExpand;

    public RDGameObject() {

    }

    public RDGameObject(GameObject gameobject) {
        this.nInstanceID = gameobject.GetInstanceID();
        this.szName  = gameobject.name;
        this.bActive = gameobject.activeSelf;

        if (gameobject.transform.parent != null) {
            this.nParentID = gameobject.transform.parent.gameObject.GetInstanceID();
        }
        else {
            this.nParentID = -1;
        }

        arrChildren = new int[gameobject.transform.childCount];

        for (int i = 0; i < arrChildren.Length; ++i) {
            arrChildren[i] = gameobject.transform.GetChild(i).gameObject.GetInstanceID();
        }
    }
}

public class RDComponent {
    public int nInstanceID;
    public string szName;

    public bool bContainEnable;
    public bool bEnable;

    public RDComponent() {

    }

    public RDComponent(Component comp) {
        this.nInstanceID = comp.GetInstanceID();
        this.szName = comp.name;

        if (comp.ContainProperty("enable")) {
            bContainEnable = true;
            bEnable = comp.GetValue<bool>("enabled");
        }
        else {
            bContainEnable = bEnable = false;
        }
    }
}

public class RDProperty {

}

public class CompProperty {
    public string compType;
    public string type;
    public string name;
    public System.Object value;
    public bool isEnum = false;
    public bool isUnityBaseType = false;
    public int arraySize = 0;
    public void UnityBaseTypeDeserialize(string data) {
        Type PropertyType = Type.GetType(this.type + ", UnityEngine");
        if (PropertyType == null) {
            return;
        }
        MethodInfo m = typeof(CompProperty).GetMethod("JsonToObject").MakeGenericMethod(PropertyType);
        this.value = m.Invoke(this, new System.Object[] { data });
    }
    public T JsonToObject<T>(string data) {
        T result = JsonMapper.ToObject<T>(data);
        return result;
    } 
}


public class CompNode {
    public int instance_id;
    public string name;
	public bool contain_enable;
    public bool enabled;
}

public class ObjNode {
    public int instance_id;
    public string name;
    public bool active;
    public bool expand;

    public int parent_id;
    public List<int> self_childrenID = new List<int>();

    public ObjNode() {

    }

    public ObjNode(int id, string name, bool active) {
        this.instance_id = id;
        this.name = name;
        this.active = active;
        expand = false;
    }
    public List<ObjNode> AllChildren(ObjNode rootNode) {
        List<ObjNode> objNodes = new List<ObjNode>();
        ObjNode root_node = rootNode;
        ObjNode node = null;
        for (int i = 0; i < root_node.self_childrenID.Count; i++) {
            GameRunTimeDataSet.ms_nodeDict.TryGetValue(root_node.self_childrenID[i], out node);
            objNodes.Add(node);
            foreach (ObjNode objNode in AllChildren(node))
                objNodes.Add(objNode);
        }
        return objNodes;
    }
    public void AddChildID(ObjNode objNode) {
        objNode.parent_id = instance_id;
        self_childrenID.Add(objNode.instance_id);
    }

    public List<int> ChildrenID() {
        return self_childrenID;
    }

    public void Clear() {
        for (int i = 0; i < self_childrenID.Count; ++i) {
            ObjNode node = null;
            if (GameRunTimeDataSet.ms_nodeDict != null)
                GameRunTimeDataSet.ms_nodeDict.TryGetValue(self_childrenID[i], out node);
            if (node != null) {
                node.Clear();
                continue;
            }
            if (ShowPanelDataSet.ms_objNodeDict != null)
                ShowPanelDataSet.ms_objNodeDict.TryGetValue(self_childrenID[i], out node);
            if (node != null)
                node.Clear();
        }
        self_childrenID.Clear();
    }
}

public class Util {
    private static StringBuilder sb = new StringBuilder(4096 * 4);

    public static void Log(NetServer server, string log) {
        Cmd cmd = new Cmd();
        cmd.WriteNetCmd(NetCmd.S2C_Log);
        cmd.WriteString(log);
        server.SendCommand(cmd);
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

    public static bool TryGetComponent(int nInstanceID, out Component comp) {
        return ms_componentDict.TryGetValue(nInstanceID, out comp);
    }

    public static Dictionary<int, GameObject> ms_gameObjectDict = new Dictionary<int, GameObject>();
    public static Dictionary<int, Component> ms_componentDict = new Dictionary<int, Component>();

    //public static Dictionary<int, RDGameObject> ms_rdgameobjectDict = new Dictionary<int, RDGameObject>();

    public static Dictionary<int, ObjNode> ms_nodeDict = null;
    //public static Dictionary<int, Component> ms_componentDict = null;
}

public static class ShowPanelDataSet {
    public static void InitDataSet() {
        ms_rdgameobjectDict.Clear();
        ms_lstRootRDObjs.Clear();
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

    public static Dictionary<int, RDGameObject> ms_rdgameobjectDict = new Dictionary<int, RDGameObject>();
    public static List<RDGameObject> ms_lstRootRDObjs = new List<RDGameObject>();

    //public static List<string> AllFolderPath = new List<string>();
    public static Dictionary<int, ObjNode> ms_objNodeDict = new Dictionary<int, ObjNode>();
    public static List<CompNode> select_node_components = new List<CompNode>();
    public static GameObject ms_gameObj = null;
    public static Component ms_component = null;
    public static ObjNode ms_rootObj = null;
    public static bool hasCustomProperty = false;


	public static void ClearAllData() {
		if (ms_objNodeDict != null) {
			ms_objNodeDict.Clear();
			ms_objNodeDict = null;
		}
		if (ms_rootObj != null) {
			ms_rootObj.Clear();
			ms_rootObj = null;
		}
        if (ms_gameObj != null) {
            MonoBehaviour.DestroyImmediate(ms_gameObj);
        }
	}
}