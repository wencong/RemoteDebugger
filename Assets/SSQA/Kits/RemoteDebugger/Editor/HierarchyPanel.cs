#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using LitJsonEx;

using MyUInt32 = System.UInt32;

public class HierarchyPanel : EditorWindow {
    public enum ShowType {
        enum_SceneObjs,
        enum_SearchObjs,
        enum_FrustumObjs
    }

    private string m_szIPAddr = "127.0.0.1";
    private string m_szPort = "4996";

    private GUIStyle m_uiStyleActive = null;
    private GUIStyle m_uiStyleInActive = null;
    private GUIStyle m_uiStyleSelected = null;
    private GUIStyle m_LabelStyleActive = null;
    private GUIStyle m_LabelStyleInActive = null;
    private GUIStyle m_LabelStyleSelected = null;

    private RDGameObject select_obj = null;
    private RDComponent select_comp = null;

    private Vector2 scroll_view_node_pos = Vector2.zero;
    private Vector2 scroll_view_nodestatus_pos = Vector2.zero;
    //private Vector2 scroll_view_nodeComponentstatus_pos = Vector2.zero;

    private NetClient net_client = new NetClient();

    private string[] m_arrayCustomCmds = null;
    private string m_szParam = string.Empty;
    private string m_szObjName = string.Empty;

    private ShowType m_eShowType = ShowType.enum_SceneObjs;
    public static List<RDGameObject> ms_lstResultObjs = new List<RDGameObject>();
    // Test RDDataBase
    /*
    public class Test : IMetaObj{
        public Test(string s1, string s2, System.Object o)
            : base(s1, s2, o) {
        }

        public Test()
            : base("", "", null) {
        }
    }

    public class Ac {
        public System.UInt32 uInt;
    }

    [MenuItem("SSQA/UInt32ToJson")]
    public static void OnFindAssetPath() {
        UInt32 val = 4;
        Test t = new Test("Test", "System.Int32", val);

        string json = RDDataBase.Serializer<Test>(t);

        Test tt = RDDataBase.Deserializer<Test>(json);

        FieldInfo fi = typeof(Ac).GetField("uInt");
        Ac ac = new Ac();
        fi.SetValue(ac, tt.value);
    }
    */

    /*
    [MenuItem("SSQA/AnalyzeMaterial")]
    public static void AnalyzeMaterial() {
        GameObject selectObj = Selection.activeGameObject;
        if (selectObj == null) {
            return;
        }

        MeshFilter mf = selectObj.GetComponent<MeshFilter>();

        int i = mf.sharedMesh.subMeshCount;
        List<Vector2> uv = new List<Vector2>();

        mf.sharedMesh.GetUVs(0, uv);
        mf.sharedMesh.GetUVs(1, uv);
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
        CustomCmdExecutor.Instance.Init(null);
        m_arrayCustomCmds = CustomCmdExecutor.Instance.m_handlers.Keys.ToArray<string>();

        S2CHandlers.Instance.OnUpdateData = OnUpdateUI;
    }

    private int m_nCmdIndex = 0;
    
    private void ShowSearchPanel() {
        if (!net_client.IsConnected) {
            return;
        }

        int nCount = ShowPanelDataSet.ms_lstRootRDObjs.Count;
        
        if (nCount > 0) {
            GUILayout.BeginHorizontal();
            {
                m_szObjName = GUILayout.TextField(m_szObjName, GUILayout.Width(145));

                if (GUILayout.Button("Reset", GUILayout.Width(100))) {
                    m_szObjName = string.Empty;
                }
            }
            GUILayout.EndHorizontal();
        }
    }

    private void ShowCustomCmdPanel() {
        if (!net_client.IsConnected) {
            return;
        }

        GUILayout.BeginHorizontal();
        {
            //GUILayout.Label("Cmd:", GUILayout.Width(40));
            m_nCmdIndex = EditorGUILayout.Popup(m_nCmdIndex, m_arrayCustomCmds, GUILayout.Width(145));
            m_szParam = GUILayout.TextField(m_szParam, GUILayout.Width(145));
            if (GUILayout.Button("Execute", GUILayout.Width(100))) {
                string szCmd = string.Format("{0} {1}", m_arrayCustomCmds[m_nCmdIndex], m_szParam);
                if (!string.IsNullOrEmpty(szCmd)) {
                    Cmd cmd = new Cmd(szCmd.Length);
                    cmd.WriteNetCmd(NetCmd.C2S_CustomCmd);
                    cmd.WriteString(szCmd);
                    net_client.SendCmd(cmd);

                    if (0 == m_arrayCustomCmds[m_nCmdIndex].CompareTo("FrustumQuery")) {
                        m_eShowType = ShowType.enum_FrustumObjs;
                    }
                }
            }
        }
        GUILayout.EndHorizontal();
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
                    m_eShowType = ShowType.enum_SceneObjs;
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
        GUILayout.BeginVertical("Box", GUILayout.Width(300));

        scroll_view_node_pos = GUILayout.BeginScrollView(scroll_view_node_pos);

        switch (m_eShowType){
            case ShowType.enum_SceneObjs:
                for (int i = 0; i < ShowPanelDataSet.ms_lstRootRDObjs.Count; ++i) {
                    RDGameObject rd = ShowPanelDataSet.ms_lstRootRDObjs[i];
                    GUIStyle uiStyle = rd.bActive ? m_LabelStyleActive : m_LabelStyleInActive;
                    if (string.IsNullOrEmpty(m_szObjName)) {
                        ShowNodeRecursion(m_eShowType, rd, 0, uiStyle);
                    }
                    else {
                        ShowNodeList(m_eShowType, rd, uiStyle);
                    }
                    
                }
                break;
            case ShowType.enum_FrustumObjs:
                for (int i = 0; i < ShowPanelDataSet.ms_lstFrustumRootRDObjs.Count; ++i) {
                    RDGameObject rd = ShowPanelDataSet.ms_lstFrustumRootRDObjs[i];
                    GUIStyle uiStyle = rd.bActive ? m_LabelStyleActive : m_LabelStyleInActive;
                    ShowNodeRecursion(m_eShowType, rd, 0, uiStyle);
                }
                break;
            default:
                break;
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

        GUILayout.BeginVertical("Box", GUILayout.Width(400));
        scroll_view_nodestatus_pos = GUILayout.BeginScrollView(scroll_view_nodestatus_pos);

        ShowGameObjectHeadInfo(select_obj);

        for (int i = 0; i < ShowPanelDataSet.ms_currentSelectComps.Length; ++i) {
            //GUIStyle uiStyle = m_uiStyleActive;
            RDComponent rdComp = ShowPanelDataSet.ms_currentSelectComps[i];

            GUILayout.BeginVertical("Box");

            GUILayout.BeginHorizontal();

            bool m_Expand = rdComp.bExpand;
            rdComp.bExpand = GUILayout.Toggle(rdComp.bExpand, "", EditorStyles.foldout, GUILayout.Width(15));

            if (!rdComp.bContainEnable || rdComp.szName.Equals("RemoteServer")) {
                GUILayout.Label("", GUILayout.Width(15));
            }
            else {
                bool bEnable = rdComp.bEnable;
//                 if (rdComp.bEnable == false) {
//                     uiStyle = m_uiStyleInActive;
//                 }
                rdComp.bEnable = GUILayout.Toggle(rdComp.bEnable, "", GUILayout.Width(15));
                if (bEnable != rdComp.bEnable) {
                    string data = RDDataBase.Serializer<RDComponent>(rdComp);

                    Cmd cmd = new Cmd(data.Length);
                    cmd.WriteNetCmd(NetCmd.C2S_EnableComponent);
                    cmd.WriteString(data);

                    net_client.SendCmd(cmd);
                }
            }
            
            //if (FilterList.HideComponent.Find(compType => compType.Equals(rdComp.szName)) == null) {
//             if (select_comp == rdComp) {
//                 uiStyle = m_uiStyleSelected;
//             }

            GUILayout.Label(rdComp.szName);
            GUILayout.EndHorizontal();

            Type PropertyType = Util.GetTypeByName(rdComp.szName);

            if (m_Expand == false && rdComp.bExpand == true) {
                select_comp = rdComp;

                //Type PropertyType = Util.GetTypeByName(rdComp.szName);
                ShowPanelDataSet.ms_remoteComponent = ShowPanelDataSet.ms_remoteGameObject.GetComponent(PropertyType);

                string data = RDDataBase.Serializer<RDComponent>(rdComp);

                Cmd cmd = new Cmd();
                cmd.WriteNetCmd(NetCmd.C2S_GetComponentProperty);
                cmd.WriteString(data);

                net_client.SendCmd(cmd);
            
            }

            if (rdComp.bExpand) {
                ShowPropertyPanel(ShowPanelDataSet.ms_remoteGameObject.GetComponent(PropertyType), rdComp.nInstanceID);
            }

            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        
        GUILayout.EndVertical();
    }


//     private void ShowPropertyPanel() {
//         if (select_comp == null || ShowPanelDataSet.ms_remoteComponent == null) {
//             return;
//         }
// 
//         GUILayout.BeginVertical("Box", GUILayout.Width(300));
// 
//         scroll_view_nodeComponentstatus_pos = GUILayout.BeginScrollView(scroll_view_nodeComponentstatus_pos);
// 
//         SerializedObject obj = new SerializedObject(ShowPanelDataSet.ms_remoteComponent);
// 
//         SerializedProperty property = obj.GetIterator();
// 
//         bool bRet = property.NextVisible(true);
// 
//         while (bRet) {
//             EditorGUILayout.PropertyField(property, true);
// 
//             if (obj.ApplyModifiedProperties()) {
// 
//                 RDProperty[] rdPropertys = ShowPanelDataSet.ms_remoteComponent.GetPropertys();
//                 
//                 for (int i = 0; i < rdPropertys.Length; ++i) {
//                     rdPropertys[i].nComponentID = select_comp.nInstanceID;
//                 }
//                 //rdPropertys[0].nComponentID = ShowPanelDataSet.ms_remoteRDComponent.nInstanceID;
//                 //string szSend = RDDataBase.Serializer<RDProperty[]>(rdPropertys);
//                 string szSend = RDDataBase.SerializerArray(rdPropertys);
// 
//                 Cmd Cmd = new Cmd(szSend.Length);
// 
//                 Cmd.WriteNetCmd(NetCmd.C2S_ModifyComponentProperty);
//                 Cmd.WriteString(szSend);
//                 net_client.SendCmd(Cmd);
//                 
//                 /*
//                 string data = RDDataBase.Serializer<RDComponent>(select_comp);
// 
//                 Cmd cmd = new Cmd(data.Length);
// 
//                 cmd.WriteNetCmd(NetCmd.C2S_GetComponentProperty);
//                 cmd.WriteString(data);
//                 net_client.SendCmd(cmd);
//                 */
//                 obj.Update();
//             }
// 
//             bRet = property.NextVisible(false);
//         }
//         
//         GUILayout.EndScrollView();
// 
//         GUILayout.EndVertical();
//     }

    private void ShowPropertyPanel(Component remoteComponent, int compID) {
        if (select_comp == null || remoteComponent == null) {
            return;
        }

        GUILayout.BeginVertical(GUILayout.Width(350));

        //scroll_view_nodeComponentstatus_pos = GUILayout.BeginScrollView(scroll_view_nodeComponentstatus_pos);

        SerializedObject obj = new SerializedObject(remoteComponent);

        SerializedProperty property = obj.GetIterator();

        bool bRet = property.NextVisible(true);

        while (bRet) {
            EditorGUILayout.PropertyField(property, true);

            if (obj.ApplyModifiedProperties()) {
                ShowPanelDataSet.ms_remoteComponent = remoteComponent;
                
                RDProperty[] rdPropertys = remoteComponent.GetPropertys();

                for (int i = 0; i < rdPropertys.Length; ++i) {
                    rdPropertys[i].nComponentID = compID;
                }
                //rdPropertys[0].nComponentID = ShowPanelDataSet.ms_remoteRDComponent.nInstanceID;
                //string szSend = RDDataBase.Serializer<RDProperty[]>(rdPropertys);
                string szSend = RDDataBase.SerializerArray(rdPropertys);

                Cmd Cmd = new Cmd(szSend.Length);

                Cmd.WriteNetCmd(NetCmd.C2S_ModifyComponentProperty);
                Cmd.WriteString(szSend);
                net_client.SendCmd(Cmd);

                /*
                string data = RDDataBase.Serializer<RDComponent>(select_comp);

                Cmd cmd = new Cmd(data.Length);

                cmd.WriteNetCmd(NetCmd.C2S_GetComponentProperty);
                cmd.WriteString(data);
                net_client.SendCmd(cmd);
                */
                obj.Update();
            }

            bRet = property.NextVisible(false);
        }

        //GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    void InitUIStyle() {
        if (m_uiStyleActive == null) {
            m_uiStyleActive = new GUIStyle(GUI.skin.button);
        }
        if (m_uiStyleInActive == null) {
            m_uiStyleInActive = new GUIStyle(GUI.skin.button);
        }

        if (m_uiStyleSelected == null) {
            m_uiStyleSelected = new GUIStyle(GUI.skin.button);
        }
        if (m_LabelStyleActive == null) {
            m_LabelStyleActive = new GUIStyle(GUI.skin.label);
        }

        if (m_LabelStyleInActive == null) {
            m_LabelStyleInActive = new GUIStyle(GUI.skin.label);
        }

        if (m_LabelStyleSelected == null) {
            m_LabelStyleSelected = new GUIStyle(GUI.skin.label);
        }

        m_uiStyleActive.normal.textColor = Color.green;
        m_uiStyleInActive.normal.textColor = Color.red;
        m_uiStyleSelected.normal.textColor = Color.blue;
        m_LabelStyleActive.normal.textColor = Color.green;
        m_LabelStyleInActive.normal.textColor = Color.red;

        GUIStyle selectStyle = new GUIStyle("CN EntryBackOdd");

        m_LabelStyleSelected.normal.background = selectStyle.onNormal.background;

        m_uiStyleActive.alignment = TextAnchor.MiddleLeft;
        m_uiStyleInActive.alignment = TextAnchor.MiddleLeft;
        m_uiStyleSelected.alignment = TextAnchor.MiddleLeft;
    }

    void OnGUI() {
        Listen();

        InitUIStyle();

        ShowConnectPanel();

        ShowCustomCmdPanel();

        ShowSearchPanel();

        GUILayout.BeginHorizontal();

        ShowGameObjectsPanel();

        ShowComponentPanel();

        //ShowPropertyPanel();
        
        GUILayout.EndHorizontal();
    }

    void Update() {
        if (net_client != null) {
            net_client.Update();
        }
    }

    bool Listen() {
        if (select_obj == null) {
            return false;
        }

        bool bRepaint = false;
        switch (Event.current.keyCode) {
            case KeyCode.RightArrow: {
                    if (select_obj.arrChildren.Length > 0) {
                        select_obj.bExpand = true;
                        Repaint();
                    }
                break;
                }

            case KeyCode.LeftArrow: {
                    if (select_obj.arrChildren.Length > 0) {
                        select_obj.bExpand = false;
                        Repaint();
                    }
                    break;
                }

            case KeyCode.DownArrow: {
                    if (Event.current.rawType == EventType.keyUp) {
                        RDGameObject nextObj = ShowPanelDataSet.GetNextRdGameObject(select_obj);
                        if (nextObj != null) {
                            select_obj = nextObj;
                            string data = RDDataBase.Serializer<RDGameObject>(select_obj);
                            Cmd cmd = new Cmd(data.Length);
                            cmd.WriteNetCmd(NetCmd.C2S_QueryComponent);
                            cmd.WriteString(data);
                            net_client.SendCmd(cmd);
                        }
                    }
                    break;
                }

            case KeyCode.UpArrow: {
                    if (Event.current.rawType == EventType.keyUp) {
                        RDGameObject nextObj = ShowPanelDataSet.GetPreRdGameObject(select_obj);
                        if (nextObj != null) {
                            select_obj = nextObj;
                            string data = RDDataBase.Serializer<RDGameObject>(select_obj);
                            Cmd cmd = new Cmd(data.Length);
                            cmd.WriteNetCmd(NetCmd.C2S_QueryComponent);
                            cmd.WriteString(data);
                            net_client.SendCmd(cmd);
                        }
                    }
                    break;
                }
        }

        return bRepaint;
    }

    private void ShowNodeList(ShowType eDataType, RDGameObject obj, GUIStyle labelStyle) {
        if (obj == null) {
            return;
        }

        if (!obj.bActive) {
            labelStyle = m_LabelStyleInActive;
        }

        GUIStyle selectedStyle = labelStyle;

        if (select_obj == obj) {
            selectedStyle = m_LabelStyleSelected;
        }

        if (obj.szName.ToLower().Contains(m_szObjName.ToLower())) {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(obj.szName, selectedStyle)) {
                select_obj = obj;
                SetParentNodeExpand(obj, true);
                string data = RDDataBase.Serializer<RDGameObject>(obj);

                Cmd cmd = new Cmd(data.Length);
                cmd.WriteNetCmd(NetCmd.C2S_QueryComponent);
                cmd.WriteString(data);

                net_client.SendCmd(cmd);

                OnUpdateUI();
            }
            GUILayout.EndHorizontal();
        }

        if (obj.arrChildren.Length > 0) {
            for (int i = 0; i < obj.arrChildren.Length; ++i) {
                RDGameObject rd = null;
                ShowPanelDataSet.TryGetRDGameObject(obj.arrChildren[i], out rd);
                if (rd != null) {
                    ShowNodeList(eDataType, rd, labelStyle);
                }
            }
        }
    }

    private void SetParentNodeExpand(RDGameObject rd, bool bExpand) {
        RDGameObject rdParent = null;
        ShowPanelDataSet.TryGetRDGameObject(rd.nParentID, out rdParent);
        if (rdParent != null) {
            rdParent.bExpand = true;
            SetParentNodeExpand(rdParent, bExpand);
        }
    }

    private void ShowNodeRecursion(ShowType eDataType, RDGameObject obj, int split, GUIStyle labelStyle) {
        if (obj == null) {
            return;
        }

        if (!obj.bActive) {
            labelStyle = m_LabelStyleInActive;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(split));

        GUIStyle selectedStyle = labelStyle;

        if (select_obj == obj) {
            selectedStyle = m_LabelStyleSelected;
        }

        if (obj.arrChildren.Length > 0) {
            obj.bExpand = GUILayout.Toggle(obj.bExpand, "", EditorStyles.foldout, GUILayout.Width(10));
        }
        else {
            GUILayout.Label("", GUILayout.Width(10));
        }

        if (GUILayout.Button(obj.szName, selectedStyle)) {
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
                if (eDataType == ShowType.enum_SceneObjs) {
                    ShowPanelDataSet.TryGetRDGameObject(obj.arrChildren[i], out rd);
                } else {
                    ShowPanelDataSet.TryGetFrustumRDGameObject(obj.arrChildren[i], out rd);
                }
                
                if (rd != null) {
                    ShowNodeRecursion(eDataType, rd, 15 + split, labelStyle);
                }
            }
        }
    }

    /*private void ShowNode(ShowType eDataType, RDGameObject obj, int split, GUIStyle btnStyle) {
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

        GUIStyle uiStyleSelected = btnStyle;
        if (select_obj == obj) {
            uiStyleSelected = m_uiStyleSelected;
        }
        if (GUILayout.Button(obj.szName, uiStyleSelected, GUILayout.Width(150))) {
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
                if (eDataType == ShowType.enum_SceneObjs) {
                    ShowPanelDataSet.TryGetRDGameObject(obj.arrChildren[i], out rd);
                }
                else {
                    ShowPanelDataSet.TryGetFrustumRDGameObject(obj.arrChildren[i], out rd);
                }

                if (rd != null) {
                    ShowNode(eDataType, rd, 25 + split, btnStyle);
                }
            }
        }
    }*/

    void OnDisable() {
        if (net_client != null) {
            net_client.Dispose();
            net_client = null;
        }

        ShowPanelDataSet.ClearAllData();
        CustomCmdExecutor.Instance.UnInit();
    }

    void SearchObjs(string strObjName) {
        int nIndex = 0;
        int nCount = ShowPanelDataSet.ms_lstRootRDObjs.Count;

        RDGameObject rd = null;

        ms_lstResultObjs.Clear();
        if (strObjName.Length > 0) {
            for (nIndex = 0; nIndex < nCount; ++nIndex) {
                rd = ShowPanelDataSet.ms_lstRootRDObjs[nIndex];
                SearchNode(rd, strObjName);
            }
        }
    }

    void SearchNode(RDGameObject obj, string strObjName) {
        if (obj.szName.IndexOf(strObjName) >= 0) {
            ms_lstResultObjs.Add(obj);
        }

        if (obj.arrChildren.Length > 0) {
            for (int i = 0; i < obj.arrChildren.Length; ++i) {
                RDGameObject rd = null;
                ShowPanelDataSet.TryGetRDGameObject(obj.arrChildren[i], out rd);
                if (rd != null) {
                    SearchNode(rd, strObjName);
                }
            }
        }
    }

    void ShowResult() {
        int nIndex = 0;
        int nCount = ms_lstResultObjs.Count;
        
        for (nIndex = 0; nIndex < nCount; ++nIndex) {
            RDGameObject obj = ms_lstResultObjs[nIndex];
            GUIStyle uiStyle = obj.bActive ? m_uiStyleActive : m_uiStyleInActive;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(obj.szName, uiStyle, GUILayout.Width(150))) {
                select_obj = obj;
                string data = RDDataBase.Serializer<RDGameObject>(obj);

                Cmd cmd = new Cmd(data.Length);
                cmd.WriteNetCmd(NetCmd.C2S_QueryComponent);
                cmd.WriteString(data);

                net_client.SendCmd(cmd);

                OnUpdateUI();
            }
            GUILayout.EndHorizontal();
        }
    }
}

#endif