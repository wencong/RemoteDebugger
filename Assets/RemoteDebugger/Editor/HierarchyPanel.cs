#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class HierarchyPanel : EditorWindow{
	// Use this for initialization
	void Start () {
	}



    [MenuItem("Tools/RemoteDebugger/Hierarchy")]
    public static void OnShowWindow() {
        GetWindow<HierarchyPanel>().Show();
    }

    private string str_ip_addr = "127.0.0.1";
    private string str_port = "4996";

    private NetClient net_client = new NetClient();

    public void OnUpdateUI() {
        Repaint();
    }

    GUIStyle activeStyle = null;
    GUIStyle unActiveStyle = null;

    public void Awake() {
        S2CHandlers.Instance.OnUpdateData = OnUpdateUI;
    }

    void OnGUI() {
        activeStyle = new GUIStyle(GUI.skin.button);
        unActiveStyle = new GUIStyle(GUI.skin.button);

        activeStyle.normal.textColor = Color.green;
        unActiveStyle.normal.textColor = Color.red;

        activeStyle.alignment = TextAnchor.MiddleLeft;
        unActiveStyle.alignment = TextAnchor.MiddleLeft;

        GUILayout.BeginHorizontal();
            GUILayout.Label("IP:", GUILayout.Width(40));
            str_ip_addr = GUILayout.TextField(str_ip_addr, GUILayout.Width(100));
            str_port = GUILayout.TextField(str_port, GUILayout.Width(40));
            if (GUILayout.Button("Connect", GUILayout.Width(100))) {
                net_client.Connect(str_ip_addr, int.Parse(str_port));
            }

            if (net_client.IsConnected) {
                if (GUILayout.Button("Query", GUILayout.Width(100))) {
                    Cmd cmd = new Cmd(); ;
                    cmd.WriteNetCmd(NetCmd.C2S_CmdQueryAllObjs);
                    net_client.SendCmd(cmd);
                }
            }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical("Box", GUILayout.Width(250));

		scroll_view_node_pos = GUILayout.BeginScrollView(scroll_view_node_pos);
        ShowNodes(ShowPanelDataSet.ms_rootObj, 0, activeStyle);
		GUILayout.EndScrollView();

        GUILayout.EndVertical();

        GUILayout.BeginVertical("Box", GUILayout.Width(200));
		scroll_view_nodestatus_pos = GUILayout.BeginScrollView(scroll_view_nodestatus_pos);
        ShowNodeStatus();
		GUILayout.EndScrollView();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    void Update() {
        if (net_client != null) {
            net_client.Update();
        }
    }

    //public static ObjNode root_node = null;
    //public static Dictionary<int, ObjNode> node_dict = new Dictionary<int, ObjNode>();

    private ObjNode select_node = null;
	private Vector2 scroll_view_node_pos = Vector2.zero;
	private Vector2 scroll_view_nodestatus_pos = Vector2.zero;

    private void ShowNodes(ObjNode node, int split, GUIStyle btnStyle) {
        if (node == null) {
            return;
        }

        if (node.active == false) {
            btnStyle = unActiveStyle;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(split));
            if (node.self_childrens.Count > 0) {
                string str_expand = node.expand ? "-" : "+";
                if (GUILayout.Button(str_expand, GUILayout.Width(25))) {
                    node.expand = !node.expand;
                }
            }
            else {
                GUILayout.Label("", GUILayout.Width(25));
            }

            if (GUILayout.Button(node.name, btnStyle, GUILayout.Width(150))) {
                Cmd cmd = new Cmd();
                cmd.WriteNetCmd(NetCmd.C2S_QueryComponent);
                cmd.WriteInt32(node.instance_id);

                net_client.SendCmd(cmd);

                select_node = node;
            }
        GUILayout.EndHorizontal();

        if (node.expand && node.self_childrens.Count > 0) {
            for (int i = 0; i < node.self_childrens.Count; ++i) {
                ShowNodes(node.self_childrens[i], 25 + split, btnStyle);
            }
        }
    }

    private void ShowNodeStatus() {
        if (select_node == null || select_node.instance_id == 0) {
            return;
        }
        GUILayout.BeginHorizontal();
        bool active = select_node.active;
        select_node.active = GUILayout.Toggle(select_node.active, select_node.name, GUILayout.Width(200));
        GUILayout.EndHorizontal();

        if (select_node.active != active && select_node.instance_id != 0) {
            int nActive = select_node.active ? 1 : 0;
            string data = string.Format("{0},{1},{2}", select_node.instance_id, select_node.name, nActive);

            Cmd cmd = new Cmd();
            cmd.WriteNetCmd(NetCmd.C2S_CmdSetObjActive);
            cmd.WriteString(data);
            net_client.SendCmd(cmd);
        }

        if (select_node.self_componets != null) {
            for (int i = 0; i < select_node.self_componets.Length; ++i) {
                CompNode compNode = select_node.self_componets[i];
                active = compNode.enabled;
                GUILayout.BeginHorizontal();
				GUILayout.Label("");
				if (compNode.contain_enable) {
					compNode.enabled = GUILayout.Toggle(compNode.enabled, compNode.name, GUILayout.Width(200));
				}
				else {
					GUILayout.Label(compNode.name, GUILayout.Width(200));
				}
                
                GUILayout.EndHorizontal();

                if (active != compNode.enabled) {
                    Cmd cmd = new Cmd();
                    cmd.WriteNetCmd(NetCmd.C2S_EnableComponent);

                    int nActive = compNode.enabled ? 1 : 0;
                    //string data = string.Format("{0},{1},{2}", select_node.instance_id, compNode.instance_id, nActive);
                    //cmd.WriteString(data);
                    cmd.WriteInt32(select_node.instance_id);
                    cmd.WriteInt32(compNode.instance_id);
                    cmd.WriteInt32(nActive);
                    net_client.SendCmd(cmd);
                }
            }
        }
        
    }

    void OnDisable() {
        if (net_client != null) {
            net_client.Dispose();
            net_client = null;
        }
        select_node = null;
		ShowPanelDataSet.ClearAllData();
    }
}

#endif