using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum ObjType {
    enum_gameobject,
    enum_component
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
    public List<ObjNode> self_childrens = new List<ObjNode>();
    public CompNode[] self_componets = null;

    public ObjNode() {

    }

    public ObjNode(int id, string name, bool active) {
        this.instance_id = id;
        this.name = name;
        this.active = active;
        expand = false;
    }

    public void AddChild(ObjNode objNode) {
        objNode.parent_id = instance_id;
        self_childrens.Add(objNode);
    }

    public List<ObjNode> Children() {
        return self_childrens;
    }

    public void Clear() {
        for (int i = 0; i < self_childrens.Count; ++i) {
            self_childrens[i].Clear();
        }
        self_childrens.Clear();
    }
}

public class Util {
    private static StringBuilder sb = new StringBuilder(4096 * 4);

    public static string ObjNode2String(ObjNode node) {
        sb.Remove(0, sb.Length);

        _AddNodeToStringBuilder(node);

        return sb.ToString();
    }

    private static void _AddNodeToStringBuilder(ObjNode node) {
        int id = node.instance_id;
        string name = node.name;
        int act = node.active == true ? 1 : 0;
        int id_parent = node.parent_id;
        

        sb.AppendFormat("{0},{1},{2},{3};", id, name, act, id_parent);

        for (int i = 0; i < node.self_childrens.Count; ++i) {
            _AddNodeToStringBuilder(node.self_childrens[i]);
        }
    }

    public static ObjNode[] String2NodeArray(string s) {
        char[] splits = { ';' };
        char[] sp = { ',' };

        string[] str_nodes = s.Split(splits);

        ObjNode[] arr_nodes = new ObjNode[str_nodes.Length - 1];

        for (int i = 0; i < str_nodes.Length; ++i) {
            string str_node = str_nodes[i];
            try {
                if (str_node == "") {
                    continue;
                }

                string[] str_nodeinfo = str_node.Split(sp);

                ObjNode node = new ObjNode();
                node.instance_id = int.Parse(str_nodeinfo[0]);
                node.name = str_nodeinfo[1];
                node.active = str_nodeinfo[2] == "1" ? true : false;
                node.expand = false;
                node.parent_id = int.Parse(str_nodeinfo[3]);

                arr_nodes[i] = node;
            }
            catch (Exception e) {
                Debug.LogException(e);
                Debug.Log(string.Format("str_node: {0}, index: {1}, total length: {2}", str_node, i, str_nodes.Length));
            }
            
        }
        return arr_nodes;
    }

    //public static Dictionary<int, ObjNode> cacheNodes = new Dictionary<int, ObjNode>();

    public static ObjNode String2ObjNode(string s) {
        //cacheNodes.Clear();
        if (ShowPanelDataSet.ms_objNodeDict == null) {
            ShowPanelDataSet.ms_objNodeDict = new Dictionary<int, ObjNode>();
        }
        else {
            ShowPanelDataSet.ms_objNodeDict.Clear();
        }

        ObjNode[] arr_nodes = String2NodeArray(s);

        for (int i = 0; i < arr_nodes.Length; ++i) {
            ObjNode node = arr_nodes[i];

            ShowPanelDataSet.ms_objNodeDict.Add(node.instance_id, node);
           
            ObjNode node_parent = null;
            ShowPanelDataSet.ms_objNodeDict.TryGetValue(node.parent_id, out node_parent);
            if (node_parent != null) {
                node_parent.AddChild(node);
            }
        }

        return ShowPanelDataSet.ms_objNodeDict[0];
    }


    public static string Comps2String(Component[] comps) {
        sb.Remove(0, sb.Length);

        for (int i = 0; i < comps.Length; ++i) {
			Component comp = comps[i];

			int contain_enable = 1;
			int enabled = 1;
			if (!comp.ContainProperty("enabled")) {
				contain_enable = 0;
				enabled = 0;
			}
			else if (!comp.GetValue<bool>("enabled")){
				enabled = 0;
			}

			int id = comp.GetInstanceID();
			string name = comp.GetType().ToString();
			
			sb.AppendFormat("{0},{1},{2},{3};", id, name, contain_enable, enabled);
        }

        return sb.ToString();
    }

    public static CompNode[] String2Comps(string data) {
        char[] splits = { ';' };
        char[] sp = { ',' };

        string[] str_nodes = data.Split(splits);

        CompNode[] arr_nodes = new CompNode[str_nodes.Length - 1];

        for (int i = 0; i < str_nodes.Length; ++i) {
            string str_node = str_nodes[i];
            if (str_node == "") {
                continue;
            }

            string[] str_nodeinfo = str_node.Split(sp);

            CompNode node = new CompNode();
            node.instance_id = int.Parse(str_nodeinfo[0]);
            node.name = str_nodeinfo[1];
			node.contain_enable = str_nodeinfo[2] == "1" ? true : false;
            node.enabled = str_nodeinfo[3] == "1" ? true : false;
            arr_nodes[i] = node;
        }

        return arr_nodes;
    }
}


public static class GameRunTimeDataSet {
    public static Dictionary<int, GameObject> ms_gameObjectDict = null;
    public static Dictionary<int, ObjNode> ms_nodeDict = null;
    public static ObjNode ms_rootObj = null;
}

public static class ShowPanelDataSet {
    public static Dictionary<int, ObjNode> ms_objNodeDict = null;
    public static ObjNode ms_rootObj = null;

	public static void ClearAllData() {
		if (ms_objNodeDict != null) {
			ms_objNodeDict.Clear();
			ms_objNodeDict = null;
		}
		if (ms_rootObj != null) {
			ms_rootObj.Clear();
			ms_rootObj = null;
		}
	}
}