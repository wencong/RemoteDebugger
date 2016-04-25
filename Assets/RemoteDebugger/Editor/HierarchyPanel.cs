#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq; 
using System.Collections;
using System.Collections.Generic;
using LitJson;


public class HierarchyPanel : EditorWindow{
	// Use this for initialization
	void Start () {

	}

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
    bool PropertyWasModified = true;

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

		select_obj.bActive = GUILayout.Toggle(select_obj.bActive, select_obj.szName);

		for (int i = 0; i < ShowPanelDataSet.ms_currentSelectComps.Length; ++i) {
			RDComponent rdComp = ShowPanelDataSet.ms_currentSelectComps[i];

			rdComp.OnGUI();
		}
        GUILayout.EndScrollView();
        
		GUILayout.EndVertical();
    }


    private void ShowPropertyPanel() {
        GUILayout.BeginVertical("Box", GUILayout.Width(300));

        scroll_view_nodeComponentstatus_pos = GUILayout.BeginScrollView(scroll_view_nodeComponentstatus_pos);
        ShowComponentStatus();
        GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    void OnGUI() {
        activeStyle = new GUIStyle(GUI.skin.button);
        unActiveStyle = new GUIStyle(GUI.skin.button);
        CompQueryStyle = new GUIStyle(GUI.skin.button);

        activeStyle.normal.textColor = Color.green;
        unActiveStyle.normal.textColor = Color.red;
        CompQueryStyle.focused.textColor = Color.green;

        activeStyle.alignment = TextAnchor.MiddleLeft;
        unActiveStyle.alignment = TextAnchor.MiddleLeft;
        CompQueryStyle.alignment = TextAnchor.MiddleLeft;

        ShowConnectPanel();

        GUILayout.BeginHorizontal();
        {
            ShowGameObjectsPanel();

            ShowComponentPanel();

            ShowPropertyPanel();
        }
        GUILayout.EndHorizontal();

    }

    void Update() {
        if (net_client != null) {
            net_client.Update();
        }
        frameCount();
        //Debug.Log(Time.deltaTime);
    }

    private ObjNode select_node = null;
    private RDGameObject select_obj = null;

    private CompNode select_comp_Node = null;
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

    private void ShowNodeStatus() {
		if (select_obj == null) {
            return;
        }
        GUILayout.BeginHorizontal("box");
        bool active = select_node.active;
        if (ShowPanelDataSet.select_node_components.Find(comp => comp.name.Equals("RemoteServer")) == null) {
            select_node.active = GUILayout.Toggle(select_node.active, select_node.name);
        }
        else GUILayout.Label(select_node.name);
        GUILayout.EndHorizontal();

        if (select_node.active != active && select_node.instance_id != 0) {
            int nActive = select_node.active ? 1 : 0;
            //string data = string.Format("{0},{1},{2}", select_node.instance_id, select_node.name, nActive);

            Cmd cmd = new Cmd();
            cmd.WriteNetCmd(NetCmd.C2S_CmdSetObjActive);
            cmd.WriteInt32(select_node.instance_id);
            cmd.WriteInt32(nActive);
            //cmd.WriteString(data);
            net_client.SendCmd(cmd);
        }

        if (ShowPanelDataSet.select_node_components != null)
            for (int i = 0; i < ShowPanelDataSet.select_node_components.Count; i++) {
                Type T = Type.GetType(ShowPanelDataSet.select_node_components[i].name + ",UnityEngine");
                if (T == null)
                    T = Type.GetType(ShowPanelDataSet.select_node_components[i].name + ",UnityEngine.UI");
                if (T == null)
                    T = Type.GetType(ShowPanelDataSet.select_node_components[i].name + ",UnityEngine.Networking");
                if (T == null)
                    T = Type.GetType(ShowPanelDataSet.select_node_components[i].name);
                if(T != null)
                    if (ShowPanelDataSet.ms_gameObj.GetComponent(T) == null)
                        ShowPanelDataSet.ms_gameObj.AddComponent(T);
            }

        GUILayout.BeginVertical();
        if (ShowPanelDataSet.select_node_components != null) {
            for (int i = 0; i < ShowPanelDataSet.select_node_components.Count; ++i) {
                CompNode compNode = ShowPanelDataSet.select_node_components[i];
                active = compNode.enabled;
                GUILayout.BeginHorizontal();
				if (compNode.contain_enable && !compNode.name.Equals("RemoteServer")) {
                    compNode.enabled = EditorGUILayout.Toggle(compNode.enabled, GUILayout.Width(20));
                }
                if (FilterList.HideComponent.Find((s => s.Equals(compNode.name))) == null) {

                    string s = "";
                    if (select_comp_Node == compNode) {
                        s = " *";
                        CompQueryStyle.normal.textColor = Color.green;
                    }
                    else CompQueryStyle.normal.textColor = Color.white;
                    GUI.SetNextControlName("SelectComponent");
                    if (GUILayout.Button(compNode.name + s, CompQueryStyle, GUILayout.Width(250))) {
                        select_comp_Node = compNode;
                        FilterList.HidePropertyList.TryGetValue(select_comp_Node.name, out HideProperty);
                        if (HideProperty == null)
                            HideProperty = new List<string>();
                        Cmd cmd = new Cmd();
                        cmd.WriteNetCmd(NetCmd.C2S_GetComponentProperty);
                        cmd.WriteInt32(compNode.instance_id);
                        net_client.SendCmd(cmd);
                        GUI.FocusControl("SelectComponent");
                        OnUpdateUI();
                    }
                }
				else {
					GUILayout.Label(compNode.name);
				}
                
                GUILayout.EndHorizontal();

                if (active != compNode.enabled) {
                    Cmd cmd = new Cmd();
                    cmd.WriteNetCmd(NetCmd.C2S_EnableComponent);
                    int nActive = compNode.enabled ? 1 : 0;
                    //string data = string.Format("{0},{1},{2}", select_node.instance_id, compNode.instance_id, nActive);
                    //cmd.WriteString(data);
                    //cmd.WriteInt32(select_node.instance_id);
                    cmd.WriteInt32(compNode.instance_id);
                    cmd.WriteInt32(nActive);
                    net_client.SendCmd(cmd);
                    if (select_comp_Node == compNode) {
                        Cmd m_cmd = new Cmd();
                        m_cmd.WriteNetCmd(NetCmd.C2S_GetComponentProperty);
                        m_cmd.WriteInt32(select_comp_Node.instance_id);
                        net_client.SendCmd(m_cmd);
                    }
                }
            }
        }
        GUILayout.EndVertical();
    }

    private void ShowComponentStatus(){
        if (select_comp_Node == null) {
            return;
        }
        if (ShowPanelDataSet.ms_component != null && ShowPanelDataSet.ms_component.GetType().ToString() == select_comp_Node.name) {
            SerializedObject obj = new SerializedObject(ShowPanelDataSet.ms_component);
            SerializedProperty m_Property = obj.GetIterator();
            m_Property.NextVisible(true);
            ShowAllProperty(obj, m_Property);
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

                    List<CompProperty> CompPropertyList = HandleData.GetComponentProperty(ShowPanelDataSet.ms_component);

                    foreach (CompProperty Property in CompPropertyList){
                        Cmd cmd = new Cmd();
                        cmd.WriteNetCmd(NetCmd.C2S_CustomComponent);
                        cmd.WriteInt32(select_comp_Node.instance_id);
                        string Json_Property = JsonMapper.ToJson(Property);
                        cmd.WriteString(Json_Property);

                        if (Property.isUnityBaseType) {
                            string Json_Value = JsonMapper.ToJson(Property.value);
                            cmd.WriteString(Json_Value);
                        }

                        net_client.SendCmd(cmd);
                    }

                    Cmd m_cmd = new Cmd();
                    m_cmd.WriteNetCmd(NetCmd.C2S_GetComponentProperty);
                    m_cmd.WriteInt32(select_comp_Node.instance_id);
                    net_client.SendCmd(m_cmd);
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
        select_node = null;

		ShowPanelDataSet.ClearAllData();
        FilterList.ClearList();
    }
}

#endif