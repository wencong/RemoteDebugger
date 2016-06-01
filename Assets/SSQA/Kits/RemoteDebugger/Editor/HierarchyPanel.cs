#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using LitJsonEx;


public class HierarchyPanel : EditorWindow {
    /*
    public class TestProperty : IMetaObj{

        public TestProperty() : base("", "", null) { }

        public TestProperty(Matrix4x4 mat)
            : base("TestProperty", mat.GetType().ToString(), mat) {
            
        }
        public int n = 0;
        public string name = "hello";
    }

    public class TestClass {
        public int n;
        public System.Object mat;
        
    }
    */

    /*
    [MenuItem("SSQA/Serializer")]
    public static void ParentName() {
        
        RDGameObject rd = new RDGameObject(Selection.activeGameObject);

        string json = RDDataBase.Serializer(rd);

        RDGameObject rd1 = RDDataBase.Deserializerr<RDGameObject>(json);

        Debug.LogFormat((string)rd1.value);
        
    }
    */

    /*
    [MenuItem("SSQA/FindAllObject")]
    public static void FindAllObject() {
        GameObject[] listGameObject = GameObject.FindObjectsOfType<GameObject>();
    }

    [MenuItem("SSQA/FindAllTransform")]
    public static void FindAllTransform() {
        Transform[] listTransform = Transform.FindObjectsOfType<Transform>();
        Transform[] aa = Selection.activeGameObject.GetComponentsInChildren<Transform>();
    }
    */
    private string m_szIPAddr = "127.0.0.1";
    private string m_szPort = "4996";

    private GUIStyle m_uiStyleActive = null;
    private GUIStyle m_uiStyleInActive = null;
    private GUIStyle CompQueryStyle = null;

    private RDGameObject select_obj = null;
    private RDComponent select_comp = null;

    private Vector2 scroll_view_node_pos = Vector2.zero;
    private Vector2 scroll_view_nodestatus_pos = Vector2.zero;
    private Vector2 scroll_view_nodeComponentstatus_pos = Vector2.zero;

    private NetClient net_client = new NetClient();

    /*
    [MenuItem("SSQA/GetTypeTest")]
    public static void GetTypeTest() {
        Util.GetTypeByName(typeof(Transform).ToString());
        Util.GetTypeByName("Game");
        Util.GetTypeByName("TS");

        string path = AssetDatabase.GetAssetPath(Selection.activeGameObject);
        Debug.Log(path);
    }
    */

    [MenuItem("SSQA/RemoteDebugger")]
    public static void OnShowWindow() {
        GetWindow<HierarchyPanel>().Show();
    }

    public void OnUpdateUI() {
        Repaint();
    }

    public static BatchOption DisplayeBatchOptionDialog(string propertyName) {
        int nRet = EditorUtility.DisplayDialogComplex("Change " + propertyName,
                                        "Do you want to change the " + propertyName + " for all the child",
                                        "Yes,change children", "No,this object only", "Cancel");

        return (BatchOption)nRet;
    }

    public void Awake() {
        S2CHandlers.Instance.OnUpdateData = OnUpdateUI;
    }

    /*
    int frame = 0;
    bool handlePropertyFlag = true;
    bool PropertyWasModified = false;

    public void frameCount() {
        frame += 1;
        if (frame == 10) {
            handlePropertyFlag = true;
            frame = 0;
        }
    }
    */

    private void ShowConnectPanel() {
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("IP:", GUILayout.Width(40));
            m_szIPAddr = GUILayout.TextField(m_szIPAddr, GUILayout.Width(100));
            m_szPort = GUILayout.TextField(m_szPort, GUILayout.Width(40));

            if (GUILayout.Button("Connect", GUILayout.Width(100))) {
                net_client.Connect(m_szIPAddr, int.Parse(m_szPort));
            }

            if (net_client.IsConnected) {
                if (GUILayout.Button("Query", GUILayout.Width(100))) {
                    ShowPanelDataSet.InitDataSet();

                    Cmd cmd = new Cmd();
                    cmd.WriteNetCmd(NetCmd.C2S_CmdQueryAllObjs);
                    net_client.SendCmd(cmd);
                }
            }
        }
        GUILayout.EndHorizontal();
    }

    private void ShowGameObjectsPanel() {
        GUILayout.BeginVertical("Box", GUILayout.Width(250));

        scroll_view_node_pos = GUILayout.BeginScrollView(scroll_view_node_pos);

        for (int i = 0; i < ShowPanelDataSet.ms_lstRootRDObjs.Count; ++i) {
            RDGameObject rd = ShowPanelDataSet.ms_lstRootRDObjs[i];
            GUIStyle uiStyle = rd.bActive ? m_uiStyleActive : m_uiStyleInActive;
            ShowNode(rd, 0, uiStyle);
        }

        GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    private void ShowGameObjectHeadInfo(RDGameObject select_obj) {
        GUILayout.BeginVertical("Box");

        GUILayout.BeginHorizontal();

        bool bActive = select_obj.bActive;
        select_obj.bActive = GUILayout.Toggle(select_obj.bActive, select_obj.szName);
        #region SetGameObjectActive
        if (bActive != select_obj.bActive) {
            string szObj = RDDataBase.Serializer<RDGameObject>(select_obj);

            Cmd cmd = new Cmd(szObj.Length);

            cmd.WriteNetCmd(NetCmd.C2S_CmdSetObjActive);
            cmd.WriteString(szObj);

            net_client.SendCmd(cmd);
        }

        #endregion

        bool bStatic = select_obj.bStatic;
        select_obj.bStatic = GUILayout.Toggle(select_obj.bStatic, "Static");
        #region SetGameObjectStatic
        if (bStatic != select_obj.bStatic) {
            BatchOption eBatchOption = DisplayeBatchOptionDialog("Static flags");

            if (eBatchOption != BatchOption.eCancle) {
                string szObj = RDDataBase.Serializer<RDGameObject>(select_obj);

                Cmd cmd = new Cmd(szObj.Length);
                cmd.WriteNetCmd(NetCmd.C2S_CmdSetObjStatic);
                cmd.WriteString(szObj);
                cmd.WriteInt32((int)eBatchOption);
                net_client.SendCmd(cmd);
            }
            else {
                select_obj.bStatic = !select_obj.bStatic;
            }
        }
        #endregion

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        string szTag = select_obj.szTag;
        GUILayout.Label("Tag");
        select_obj.szTag = EditorGUILayout.TagField(select_obj.szTag);
        #region SetGameObjectTag
        if (!szTag.Equals(select_obj.szTag)) {
            BatchOption eBatchOption = DisplayeBatchOptionDialog("Tag");

            if (eBatchOption != BatchOption.eCancle) {
                string szObj = RDDataBase.Serializer<RDGameObject>(select_obj);

                Cmd cmd = new Cmd(szObj.Length);
                cmd.WriteNetCmd(NetCmd.C2S_CmdSetObjTag);
                cmd.WriteString(szObj);
                cmd.WriteInt32((int)eBatchOption);
                net_client.SendCmd(cmd);
            }
            else {
                select_obj.szTag = szTag;
            }
            
        }
        #endregion

        int nLayer = select_obj.nLayer;
        GUILayout.Label("Layer");
        select_obj.nLayer = EditorGUILayout.LayerField(select_obj.nLayer);
        #region SetGameObjectLayer
        if (!nLayer.Equals(select_obj.nLayer)) {
            BatchOption eBatchOption = DisplayeBatchOptionDialog("Layer");

            if (eBatchOption != BatchOption.eCancle) {
                string szObj = RDDataBase.Serializer<RDGameObject>(select_obj);

                Cmd cmd = new Cmd(szObj.Length);
                cmd.WriteNetCmd(NetCmd.C2S_CmdSetObjLayer);
                cmd.WriteString(szObj);
                cmd.WriteInt32((int)eBatchOption);
                net_client.SendCmd(cmd);
            }
            else {
                select_obj.nLayer = nLayer;
            }
        }

        #endregion

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    private void ShowComponentPanel() {
        if (select_obj == null || ShowPanelDataSet.ms_currentSelectComps == null) {
            return;
        }

        GUILayout.BeginVertical("Box", GUILayout.Width(300));
        scroll_view_nodestatus_pos = GUILayout.BeginScrollView(scroll_view_nodestatus_pos);

        ShowGameObjectHeadInfo(select_obj);

        for (int i = 0; i < ShowPanelDataSet.ms_currentSelectComps.Length; ++i) {
            RDComponent rdComp = ShowPanelDataSet.ms_currentSelectComps[i];

            GUILayout.BeginHorizontal();

            if (!rdComp.bContainEnable || rdComp.szName.Equals("RemoteServer")) {
                GUILayout.Label("", GUILayout.Width(25));
            }
            else {
                bool bEnable = rdComp.bEnable;
                rdComp.bEnable = GUILayout.Toggle(rdComp.bEnable, "", GUILayout.Width(25));
                if (bEnable != rdComp.bEnable) {
                    string data = RDDataBase.Serializer<RDComponent>(rdComp);

                    Cmd cmd = new Cmd(data.Length);
                    cmd.WriteNetCmd(NetCmd.C2S_EnableComponent);
                    cmd.WriteString(data);

                    net_client.SendCmd(cmd);
                }
            }
            string mark = "";
            //if (FilterList.HideComponent.Find(compType => compType.Equals(rdComp.szName)) == null) {
            if (select_comp == rdComp) {
                mark = "*";
            }

            if (GUILayout.Button(rdComp.szName + mark)) {
                select_comp = rdComp;

                Type PropertyType = Util.GetTypeByName(rdComp.szName);

                ShowPanelDataSet.ms_remoteComponent = ShowPanelDataSet.ms_remoteGameObject.GetComponent(PropertyType);

                /*
                //FilterList.HidePropertyList.TryGetValue(rdComp.szName, out HideProperty);

                if (HideProperty == null)
                    HideProperty = new List<string>();
                */

                string data = RDDataBase.Serializer<RDComponent>(rdComp);

                Cmd cmd = new Cmd();
                cmd.WriteNetCmd(NetCmd.C2S_GetComponentProperty);
                cmd.WriteString(data);

                net_client.SendCmd(cmd);
            }
            //}
                /*
            else {
                GUILayout.Label(rdComp.szName, CompQueryStyle);
            }*/

            GUILayout.EndHorizontal();

		}
        GUILayout.EndScrollView();
        
		GUILayout.EndVertical();
    }


    private void ShowPropertyPanel() {
        if (select_comp == null || ShowPanelDataSet.ms_remoteComponent == null) {
            return;
        }

        GUILayout.BeginVertical("Box", GUILayout.Width(300));

        scroll_view_nodeComponentstatus_pos = GUILayout.BeginScrollView(scroll_view_nodeComponentstatus_pos);

        SerializedObject obj = new SerializedObject(ShowPanelDataSet.ms_remoteComponent);

        SerializedProperty property = obj.GetIterator();

        bool bRet = property.NextVisible(true);

        while (bRet) {
            EditorGUILayout.PropertyField(property, true);

            if (obj.ApplyModifiedProperties()) {
                RDProperty[] rdPropertys = ShowPanelDataSet.ms_remoteComponent.GetPropertys();

                for (int i = 0; i < rdPropertys.Length; ++i) {
                    rdPropertys[i].nComponentID = select_comp.nInstanceID;
                }
                //rdPropertys[0].nComponentID = ShowPanelDataSet.ms_remoteRDComponent.nInstanceID;
                //string szSend = RDDataBase.Serializer<RDProperty[]>(rdPropertys);
                string szSend = RDDataBase.SerializerArray(rdPropertys);

                Cmd Cmd = new Cmd(szSend.Length);

                Cmd.WriteNetCmd(NetCmd.C2S_ModifyComponentProperty);
                Cmd.WriteString(szSend);
                net_client.SendCmd(Cmd);

                string data = RDDataBase.Serializer<RDComponent>(select_comp);

                Cmd cmd = new Cmd(data.Length);

                cmd.WriteNetCmd(NetCmd.C2S_GetComponentProperty);
                cmd.WriteString(data);
                net_client.SendCmd(cmd);

                obj.Update();
            }

            bRet = property.NextVisible(false);
        }

        //ShowAllProperty(obj, m_Property);
        
        GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    void OnGUI() {
        m_uiStyleActive = new GUIStyle(GUI.skin.button);
        m_uiStyleInActive = new GUIStyle(GUI.skin.button);
        CompQueryStyle = new GUIStyle(GUI.skin.label);

        m_uiStyleActive.normal.textColor = Color.green;
        m_uiStyleInActive.normal.textColor = Color.red;
        CompQueryStyle.focused.textColor = Color.green;

        m_uiStyleActive.alignment = TextAnchor.MiddleLeft;
        m_uiStyleInActive.alignment = TextAnchor.MiddleLeft;
        CompQueryStyle.alignment = TextAnchor.MiddleCenter;

        ShowConnectPanel();

        GUILayout.BeginHorizontal();

        ShowGameObjectsPanel();

        ShowComponentPanel();

        ShowPropertyPanel();
        
        GUILayout.EndHorizontal();
    }

    void Update() {
        if (net_client != null) {
            net_client.Update();
        }
        //frameCount();
    }

    private void ShowNode(RDGameObject obj, int split, GUIStyle btnStyle) {
        if (obj == null) {
            return;
        }

        if (!obj.bActive) {
            btnStyle = m_uiStyleInActive;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(split));

        if (obj.arrChildren.Length > 0) {
            string szExpand = obj.bExpand ? "-" : "+";
            if (GUILayout.Button(szExpand, GUILayout.Width(25))) {
                obj.bExpand = !obj.bExpand;
            }
        }
        else {
            GUILayout.Label("", GUILayout.Width(25));
        }
        string s = select_obj == obj ? " *" : "";

        if (GUILayout.Button(obj.szName + s, btnStyle, GUILayout.Width(150))) {
            select_obj = obj;
            string data = RDDataBase.Serializer<RDGameObject>(obj);

            Cmd cmd = new Cmd(data.Length);
            cmd.WriteNetCmd(NetCmd.C2S_QueryComponent);
            cmd.WriteString(data);

            net_client.SendCmd(cmd);

            OnUpdateUI();
        }
        GUILayout.EndHorizontal();

        if (obj.bExpand && obj.arrChildren.Length > 0) {
            for (int i = 0; i < obj.arrChildren.Length; ++i) {
                RDGameObject rd = null;
                ShowPanelDataSet.TryGetRDGameObject(obj.arrChildren[i], out rd);
                if (rd != null) {
                    ShowNode(rd, 25 + split, btnStyle);
                }
            }
        }
    }

    /*
    void ShowAllProperty(SerializedObject obj, SerializedProperty m_Property) {
        
        //if (FilterList.m_AvailableTypeList.Find(s => s.Equals(m_Property.type)) != null && m_Property.editable) {
        //    if (HideProperty.Find(s => s.Equals(m_Property.displayName)) == null){
        
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < m_Property.depth; i++)
            GUILayout.Space(20);
        EditorGUILayout.PropertyField(m_Property, true);

        EditorGUILayout.EndHorizontal();

        if (obj.ApplyModifiedProperties()) {
            PropertyWasModified = true;
        }

        if (PropertyWasModified && handlePropertyFlag) {

            RDProperty[] rdPropertys = ShowPanelDataSet.ms_remoteComponent.GetPropertys();

            rdPropertys[0].nComponentID = ShowPanelDataSet.ms_remoteRDComponent.nInstanceID;
                    
            //string szSend = RDDataBase.Serializer<RDProperty[]>(rdPropertys);
            string szSend = RDDataBase.SerializerArray(rdPropertys);

            Cmd Cmd = new Cmd(new byte[szSend.Length + 1000]);

            Cmd.WriteNetCmd(NetCmd.C2S_ModifyComponentProperty);
            Cmd.WriteString(szSend);
            net_client.SendCmd(Cmd);

            string data = RDDataBase.Serializer<RDComponent>(ShowPanelDataSet.ms_remoteRDComponent);

            Cmd cmd = new Cmd(new byte[data.Length + 200]);

            cmd.WriteNetCmd(NetCmd.C2S_GetComponentProperty);
            cmd.WriteString(data);
            net_client.SendCmd(cmd);

            obj.Update();

            PropertyWasModified = false;
            handlePropertyFlag = false;

        }

        if (m_Property.NextVisible(false)) {
            ShowAllProperty(obj, m_Property);
        }
            
    }
    */

    void OnDisable() {
        if (net_client != null) {
            net_client.Dispose();
            net_client = null;
        }

		ShowPanelDataSet.ClearAllData();
        //FilterList.ClearList();
    }
}

#endif