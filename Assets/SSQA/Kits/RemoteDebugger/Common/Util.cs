using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using LitJsonEx;

public enum BatchOption {
    eContainChildren,
    eOnlySelf,
    eCancle
}

public class RDDataBase {
    public static string Serializer<T>(T obj) {
        return JsonMapper.ToJson(obj);
    }

    public static T Deserializer<T>(string data){

        System.Object result = JsonMapper.ToObject<T>(data);

        return (T)result;
    }
}

public class RDGameObject {
    public int nInstanceID;
    public string szName;

    public bool bActive;
    public bool bStatic;

    public string szTag;
    public int nLayer;

    public int nParentID;
    public int[] arrChildren;

    public bool bExpand;

    public RDGameObject() {

    }

    public RDGameObject(GameObject gameobject) {
        this.nInstanceID = gameobject.GetInstanceID();
        this.szName  = gameobject.name;
        this.bActive = gameobject.activeSelf;
        this.bStatic = gameobject.isStatic;
        this.szTag   = gameobject.tag;
        this.nLayer  = gameobject.layer;

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
		this.szName = comp.GetType().ToString();

        if (comp.ContainProperty("enabled")) {
            bContainEnable = true;
            bEnable = comp.GetValue<bool>("enabled");
        }
        else {
            bContainEnable = bEnable = false;
        }
    }

    public void OnGUI() {
#if UNITY_EDITOR
        GUILayout.BeginHorizontal();

        if (!bContainEnable) {
            GUILayout.Label("", GUILayout.Width(25));
        }
        else {
            GUILayout.Toggle(bEnable, "", GUILayout.Width(25));
        }

        GUILayout.Button(szName);

        GUILayout.EndHorizontal();
#endif
    }
}

public class RDProperty {
    public int nComponentID = 0;
    
    public int nMemType;
    
    public string szTypeName;

    public string szName;

    public System.Object value;

    public bool bIsEnum = false;

    public bool bIsPrimitive = true;

    public RDProperty() { 

    }

    public bool Serializer() {
        Type valueType = Util.GetType(szTypeName);

        if (valueType == null) {
            return false;
        }

        if (valueType.IsEnum) {
            this.bIsEnum = true;
            this.value = this.value.ToString();
        }

        else if (!valueType.IsPrimitive && !valueType.Equals(typeof(System.String))) {
            this.bIsPrimitive = false;
            this.value = JsonMapper.ToJson(this.value);
        }

        return true;
    }

    public bool Deserializer() {
        Type valueType = Util.GetType(szTypeName);

        if (valueType == null) {
            return false;
        }

        if (this.bIsEnum) {
            this.value = (Enum)Enum.Parse(valueType, value.ToString());
        }

        if (!this.bIsPrimitive && !valueType.Equals(typeof(System.String))) {
            ParameterInfo[] pinfos = null;
            MethodInfo mi = typeof(JsonMapper).GetMethods().First(
                    m => m.Name.Equals("ToObject") && m.IsGenericMethod
                    && (pinfos = m.GetParameters()).Length == 1
                    && pinfos[0].ParameterType.Equals(typeof(string))
                    ).MakeGenericMethod(valueType);
            this.value = mi.Invoke(null, new System.Object[] { this.value });
        }

        return true;
    }

    public RDProperty(Component comp, MemberInfo mi) {
        this.nComponentID = comp.GetInstanceID();
        this.nMemType = (int)mi.MemberType;

        this.szName = mi.Name;

        if (mi.MemberType.Equals(MemberTypes.Property)) {
            this.szTypeName = ((PropertyInfo)mi).PropertyType.ToString();
            this.value = ((PropertyInfo)mi).GetValue(comp, null);
        }
        else if (mi.MemberType.Equals(MemberTypes.Field)) {
            this.szTypeName = ((FieldInfo)mi).FieldType.ToString();
            this.value = ((FieldInfo)mi).GetValue(comp);
        }

        /*
        //this.isUnityBaseType = true;
        #region GetValue
        if (t.Equals(typeof(Material))) {
            //this.value = (comp as Renderer).sharedMaterial;
            //this.value = pi.GetValue(comp, null);
            if (this.value != null) {
                this.value = (this.value as Material).name.Replace(" (Instance)", "");
            }

            else this.value = "null";
        }

        else if (t.Equals(typeof(Material).MakeArrayType())) {

            //this.value = (comp as Renderer).sharedMaterials;
            //this.value = pi.GetValue(comp, null);
            string s = "";
            string materials = "";

            for (int i = 0; i < (this.value as Array).Length; i++) {

                if ((this.value as Array).GetValue(i) != null) {
                    s = ((this.value as Array).GetValue(i) as Material).name.Replace(" (Instance)", "");
                }

                else s = "null";
                materials = materials + s + ",";
            }

            materials = materials.Substring(0, materials.Length - 1);
            this.value = materials;
        }
        else if (t.IsEnum) {
            //this.value = pi.GetValue(comp, null);
            this.value = this.value.ToString();
            this.isEnum = true;
        }
        
        else if (t.IsValueType && !t.IsPrimitive || t.Equals(typeof(RectOffset))) {

            this.isUnityBaseType = true;
            //System.Object value = pi.GetValue(comp, null);
            this.value = JsonMapper.ToJson(this.value);
        }
        */

        /*else {
            this.value = pi.GetValue(comp, null);
        }*/
        
    }
}


public static class Util {
    public static Type GetType(string szTypeName) {
        Type T = null;

        if (szTypeName.Contains("UnityEngine")) {
            T = Type.GetType(szTypeName + ",UnityEngine");
        }
        else {
            T = Type.GetType(szTypeName);
        }
        return T;
    }
}

/*public class CompNode {
    public int instance_id;
    public string name;
	public bool contain_enable;
    public bool enabled;
}*/

/*public class ObjNode {
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
}*/

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

    //public static Dictionary<int, RDGameObject> ms_rdgameobjectDict = new Dictionary<int, RDGameObject>();

    //public static Dictionary<int, ObjNode> ms_nodeDict = null;
    //public static Dictionary<int, Component> ms_componentDict = null;
}

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