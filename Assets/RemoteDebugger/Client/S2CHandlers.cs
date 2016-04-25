using System;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class S2CHandlers {
    private static S2CHandlers inst = null;
    public static S2CHandlers Instance {
        get {
            if (inst == null) {
                inst = new S2CHandlers();
            }
            return inst;
        }
    }

    public delegate void DelegateMethod();
    public DelegateMethod OnUpdateData = null;

    public void Init(NetClient net_client) {
        if (net_client != null) {
            net_client.RegisterHandler(NetCmd.S2C_CmdQueryAllObjs, S2C_QueryAllObjs);
            net_client.RegisterHandler(NetCmd.S2C_CmdSetObjActive, S2C_SetObjActive);
            net_client.RegisterHandler(NetCmd.S2C_QueryComponent, S2C_QueryComponent);
            net_client.RegisterHandler(NetCmd.S2C_GetComponentProperty, S2CGetComponentProperty);
            net_client.RegisterHandler(NetCmd.S2C_EnableComponent, S2CEnableComponent);
            net_client.RegisterHandler(NetCmd.S2C_CustomComponent, S2CCustomComponent);
            net_client.RegisterHandler(NetCmd.S2C_Log, S2CDebugLog);
            net_client.RegisterHandler(NetCmd.S2C_FinishWait, S2CFinishWait);
        }
    }

    public bool S2CDebugLog(NetCmd cmd, Cmd c) {
        string szLog = c.ReadString();

        Debug.Log(szLog);

        return true;
    }

    public bool S2C_QueryAllObjs(NetCmd cmd, Cmd c) {
        string data = c.ReadString();

        try {
			ShowPanelDataSet.InitDataSet();

            RDGameObject[] arrRdObjs = RDDataBase.Deserializer<RDGameObject[]>(data);
            for (int i = 0; i < arrRdObjs.Length; ++i) {
                ShowPanelDataSet.AddRdGameObject(arrRdObjs[i]);
            }

            if (OnUpdateData != null) {
                OnUpdateData();
            }
        }

        catch (Exception ex) {
            Debug.LogException(ex);
        }

        return true;
    }

    public bool S2C_SetObjActive(NetCmd cmd, Cmd c) {
        int id = c.ReadInt32();
        bool active = c.ReadInt32() == 1 ? true : false;
        ObjNode cacheNode = null;

        ShowPanelDataSet.ms_objNodeDict.TryGetValue(id, out cacheNode);
        if (cacheNode != null) {
            cacheNode.active = active;
        }
        return true;
    }

    public bool S2C_QueryComponent(NetCmd cmd, Cmd c) {
        string data = c.ReadString();
        try {
            RDComponent[] rdComps = RDDataBase.Deserializer<RDComponent[]>(data);

			for (int i = 0; i < rdComps.Length; ++i) {
				ShowPanelDataSet.AddRdComponent(rdComps[i]);
			}

			ShowPanelDataSet.ms_currentSelectComps = rdComps;
        }
        catch (Exception ex) {
            Debug.LogException(ex);
        }
        //ShowPanelDataSet.select_node_components.Add(JsonMapper.ToObject<CompNode>(data));
        if (OnUpdateData != null) {
            OnUpdateData();
        }

        return true;
    }

    public bool S2CGetComponentProperty(NetCmd cmd, Cmd c) {
        string data = c.ReadString();
        CompProperty compProperty = JsonMapper.ToObject<CompProperty>(data);
            if (compProperty.isUnityBaseType) {
                string value = c.ReadString();
                compProperty.UnityBaseTypeDeserialize(value);
            }
        Type T = null;
        T = Type.GetType(compProperty.compType + ",UnityEngine");
        if (T == null)
            T = Type.GetType(compProperty.compType + ",UnityEngine.UI");
        if (T == null)
            T = Type.GetType(compProperty.compType + ",UnityEngine.Networking");
        if (T == null)
            T = Type.GetType(compProperty.compType);
        if(T != null)
            if (ShowPanelDataSet.ms_component == null || ShowPanelDataSet.ms_component.GetType().ToString() != compProperty.compType) {
                ShowPanelDataSet.ms_component = ShowPanelDataSet.ms_gameObj.GetComponent(T);
                if (ShowPanelDataSet.ms_component == null) {
                    ShowPanelDataSet.ms_component = ShowPanelDataSet.ms_gameObj.AddComponent(T);
                }
            }
        try {
            HandleData.SetComponentProperty(compProperty, ShowPanelDataSet.ms_component);
        }
        catch(Exception ex) {
            Debug.Log(ex);
        }
        if (OnUpdateData != null) {
            OnUpdateData();
        }
        return true;
    }

    public bool S2CEnableComponent(NetCmd cmd, Cmd c) {
        return true;
    }

    public bool S2CCustomComponent(NetCmd cmd, Cmd c){
        return true;
    }
    public bool S2CFinishWait(NetCmd cmd, Cmd c) {
        ShowPanelDataSet.hasCustomProperty = true;
        return true;
    }
}
