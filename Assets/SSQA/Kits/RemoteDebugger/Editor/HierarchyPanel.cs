#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using LitJson;


public class HierarchyPanel : EditorWindow {

    [MenuItem("Tools/RemoteDebugger/Hierarchy")]
    public static void OnShowWindow() {
        GetWindow<HierarchyPanel>().Show();
    }

    private string m_szIPAddr = "127.0.0.1";
    private string m_szPort = "4996";

    private NetClient net_client = new NetClient();

    public void OnUpdateUI() {
        Repaint();
    }

    GUIStyle activeStyle = null;
    GUIStyle unActiveStyle = null; 
    GUIStyle CompQueryStyle = null;
    List<string> HideProperty = null;

    public void Awake() {
        S2CHandlers.Instance.OnUpdateData = OnUpdateUI;
        FilterList.InitializeDict();
        FilterList.ListToAvailableTypeList(FilterList.readTextFileToList("FilterList/AvailableTypeList.txt"));
        FilterList.ListTo_m_AvailableTypeList(FilterList.readTextFileToList("FilterList/m_AvailableTypeList.txt"));
        FilterList.ListToHideComponent(FilterList.readTextFileToList("FilterList/HideComponent.txt"));
        FilterList.ListToPropertyHideList(FilterList.readTextFileToList("FilterList/HidePropertyList.txt"));
    }

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

                    OnUpdateUI();
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
            GUIStyle uiStyle = rd.bActive ? activeStyle : unActiveStyle;
            ShowNode(rd, 0, uiStyle);
        }

        GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    private void ShowComponentPanel() {
        if (select_obj == null || ShowPanelDataSet.ms_currentSelectComps == null) {
            return;
        }

        GUILayout.BeginVertical("Box", GUILayout.Width(300));

        scroll_view_nodestatus_pos = GUILayout.BeginScrollView(scroll_view_nodestatus_pos);

        GUILayout.BeginVertical("Box");

        GUILayout.BeginHorizontal();

        #region SetGameObjectActive

        bool bActive = select_obj.bActive;

        select_obj.bActive = GUILayout.Toggle(select_obj.bActive, select_obj.szName);

        if (bActive != select_obj.bActive) {

            confirmationWindow.GameObjHandle("Active flags");

            string szObj = RDDataBase.Serializer<RDGameObject>(select_obj);
            
            Cmd cmd = new Cmd();
            cmd.WriteNetCmd(NetCmd.C2S_CmdSetObjActive);
            cmd.WriteString(szObj);
            cmd.WriteInt32(ShowPanelDataSet.ms_gameObjHandleFlag);
            net_client.SendCmd(cmd);
        }

        #endregion

        //set isStatic doesn't work in runtime

        #region SetGameObjectStatic
        
        bool bStatic = select_obj.bStatic;
        select_obj.bStatic = GUILayout.Toggle(select_obj.bStatic, "Static");

        if (bStatic != select_obj.bStatic) {

            confirmationWindow.GameObjHandle("Static flags");

            string szObj = RDDataBase.Serializer<RDGameObject>(select_obj);

            Cmd cmd = new Cmd();
            cmd.WriteNetCmd(NetCmd.C2S_CmdSetObjStatic);
            cmd.WriteString(szObj);
            cmd.WriteInt32(ShowPanelDataSet.ms_gameObjHandleFlag);
            net_client.SendCmd(cmd);
        }
        
        #endregion

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        #region SetGameObjectTag

        string bTag = select_obj.bTag;
        GUILayout.Label("Tag");
        select_obj.bTag = EditorGUILayout.TagField(select_obj.bTag);

        if (!bTag.Equals(select_obj.bTag)) {

            confirmationWindow.GameObjHandle("Tag");

            string szObj = RDDataBase.Serializer<RDGameObject>(select_obj);

            Cmd cmd = new Cmd();
            cmd.WriteNetCmd(NetCmd.C2S_CmdSetObjTag);
            cmd.WriteString(szObj);
            cmd.WriteInt32(ShowPanelDataSet.ms_gameObjHandleFlag);
            net_client.SendCmd(cmd);
        }

        #endregion

        #region SetGameObjectLayer

        int bLayer = select_obj.bLayer;
        GUILayout.Label("Layer");
        select_obj.bLayer = EditorGUILayout.LayerField(select_obj.bLayer);

        if (!bLayer.Equals(select_obj.bLayer)) {

            confirmationWindow.GameObjHandle("Layer");

            string szObj = RDDataBase.Serializer<RDGameObject>(select_obj);

            Cmd cmd = new Cmd();
            cmd.WriteNetCmd(NetCmd.C2S_CmdSetObjLayer);
            cmd.WriteString(szObj);
            cmd.WriteInt32(ShowPanelDataSet.ms_gameObjHandleFlag);
            net_client.SendCmd(cmd);
        }

        #endregion

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();


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

                    Cmd cmd = new Cmd();
                    cmd.WriteNetCmd(NetCmd.C2S_EnableComponent);
                    cmd.WriteString(data);

                    net_client.SendCmd(cmd);
                }
            }
            string mark = "";
            if (FilterList.HideComponent.Find(compType => compType.Equals(rdComp.szName)) == null) {
                if (ShowPanelDataSet.ms_remoteRDComponent == rdComp){
                    mark = "*";
                }

                if (GUILayout.Button(rdComp.szName + mark)) {
                    ShowPanelDataSet.ms_remoteRDComponent = rdComp;

                    Type PropertyType = ShowPanelDataSet.GetComponentType(rdComp.szName);

                    ShowPanelDataSet.ms_remoteComponent = ShowPanelDataSet.ms_remoteGameObject.GetComponent(PropertyType);

                    FilterList.HidePropertyList.TryGetValue(rdComp.szName, out HideProperty);

                    if (HideProperty == null)
                        HideProperty = new List<string>();

                    string data = RDDataBase.Serializer<RDComponent>(rdComp);

                    Cmd cmd = new Cmd();
                    cmd.WriteNetCmd(NetCmd.C2S_GetComponentProperty);
                    cmd.WriteString(data);

                    net_client.SendCmd(cmd);
                }
            }
            else {
                GUILayout.Label(rdComp.szName, CompQueryStyle);
            }


            GUILayout.EndHorizontal();

		}
        GUILayout.EndScrollView();
        
		GUILayout.EndVertical();
    }


    private void ShowPropertyPanel() {
        if (ShowPanelDataSet.ms_remoteRDComponent == null || ShowPanelDataSet.ms_remoteComponent == null) {
            return;
        }

        GUILayout.BeginVertical("Box", GUILayout.Width(300));

        scroll_view_nodeComponentstatus_pos = GUILayout.BeginScrollView(scroll_view_nodeComponentstatus_pos);

        SerializedObject obj = new SerializedObject(ShowPanelDataSet.ms_remoteComponent);

        SerializedProperty m_Property = obj.GetIterator();

        m_Property.NextVisible(true);

        ShowAllProperty(obj, m_Property);
        
        GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    void OnGUI() {
        activeStyle = new GUIStyle(GUI.skin.button);
        unActiveStyle = new GUIStyle(GUI.skin.button);
        CompQueryStyle = new GUIStyle(GUI.skin.label);

        activeStyle.normal.textColor = Color.green;
        unActiveStyle.normal.textColor = Color.red;
        CompQueryStyle.focused.textColor = Color.green;

        activeStyle.alignment = TextAnchor.MiddleLeft;
        unActiveStyle.alignment = TextAnchor.MiddleLeft;
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
        frameCount();
    }

    private RDGameObject select_obj = null;

	private Vector2 scroll_view_node_pos = Vector2.zero;
	private Vector2 scroll_view_nodestatus_pos = Vector2.zero;
    private Vector2 scroll_view_nodeComponentstatus_pos = Vector2.zero;
    

    private void ShowNode(RDGameObject obj, int split, GUIStyle btnStyle) {
        if (obj == null) {
            return;
        }

        if (obj.bActive == false) {
            btnStyle = unActiveStyle;
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

            Cmd cmd = new Cmd();
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

    void ShowAllProperty(SerializedObject obj, SerializedProperty m_Property) {
        if (FilterList.m_AvailableTypeList.Find(s => s.Equals(m_Property.type)) != null && m_Property.editable) {
            if (HideProperty.Find(s => s.Equals(m_Property.displayName)) == null){

                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < m_Property.depth; i++)
                    GUILayout.Space(20);
                EditorGUILayout.PropertyField(m_Property, true);

                EditorGUILayout.EndHorizontal();

                if(obj.ApplyModifiedProperties())
                    PropertyWasModified = true;

                if (PropertyWasModified && handlePropertyFlag) {

                    RDProperty[] rdPropertys = ShowPanelDataSet.ms_remoteComponent.GetAllProperty();

                    rdPropertys[0].nComponentID = ShowPanelDataSet.ms_remoteRDComponent.nInstanceID;
                    
                    string szSend = RDDataBase.Serializer<RDProperty[]>(rdPropertys);

                    Cmd Cmd = new Cmd(new byte[szSend.Length + 1000]);

                    Cmd.WriteNetCmd(NetCmd.C2S_CustomComponent);
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
            }
        }
        if(m_Property.NextVisible(false))
            ShowAllProperty(obj, m_Property);
    }

    void OnDisable() {
        if (net_client != null) {
            net_client.Dispose();
            net_client = null;
        }

		ShowPanelDataSet.ClearAllData();
        FilterList.ClearList();
    }
}

#endif