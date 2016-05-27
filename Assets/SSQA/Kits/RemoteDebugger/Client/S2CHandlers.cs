using System;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using LitJsonEx;

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
            
            //net_client.RegisterHandler(NetCmd.S2C_CmdSetObjStatic, S2C_SetObjStatic);
            //net_client.RegisterHandler(NetCmd.S2C_CmdSetObjTag, S2C_SetObjTag);
            net_client.RegisterHandler(NetCmd.S2C_CmdSetObjLayer, S2C_SetObjLayer);

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
        string rdGameObjs = c.ReadString();

        try {
            ShowPanelDataSet.InitDataSet();
            RDGameObject[] arrRdObjs = RDDataBase.Deserializer<RDGameObject[]>(rdGameObjs);

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
        string szRecv = c.ReadString();
        RDGameObject rdGameObj = RDDataBase.Deserializer<RDGameObject>(szRecv);

        RDGameObject cacheRDGameObj = null;
        ShowPanelDataSet.ms_rdgameobjectDict.TryGetValue(rdGameObj.nInstanceID, out cacheRDGameObj);

        if (cacheRDGameObj != null) {
            cacheRDGameObj.bActive = rdGameObj.bActive;
        }
        
        if (OnUpdateData != null) {
            OnUpdateData();
        }

        return true;
    }

    /*
    public bool S2C_SetObjStatic(NetCmd cmd, Cmd c) {
        string szRecv = c.ReadString();

        List<RDGameObject> rdGameObjs = RDDataBase.Deserializer<List<RDGameObject>>(szRecv);
        RDGameObject cacheRDGameObj = null;

        foreach (RDGameObject rdGameObj in rdGameObjs) {

            ShowPanelDataSet.ms_rdgameobjectDict.TryGetValue(rdGameObj.nInstanceID, out cacheRDGameObj);
            if (cacheRDGameObj != null) {
                cacheRDGameObj.bStatic = rdGameObj.bStatic;
            }
        }
        if (OnUpdateData != null) {
            OnUpdateData();
        }

        return true;
    }

    public bool S2C_SetObjTag(NetCmd cmd, Cmd c) {
        string szRecv = c.ReadString();

        List<RDGameObject> rdGameObjs = RDDataBase.Deserializer<List<RDGameObject>>(szRecv);
        RDGameObject cacheRDGameObj = null;

        foreach (RDGameObject rdGameObj in rdGameObjs) {
            ShowPanelDataSet.ms_rdgameobjectDict.TryGetValue(rdGameObj.nInstanceID, out cacheRDGameObj);
            if (cacheRDGameObj != null) {
                cacheRDGameObj.szTag = rdGameObj.szTag;
            }
        }
        if (OnUpdateData != null) {
            OnUpdateData();
        }

        return true;
    }
    */

    public bool S2C_SetObjLayer(NetCmd cmd, Cmd c) {
        string szRecv = c.ReadString();

        RDGameObject[] rdGameObjs = RDDataBase.Deserializer<RDGameObject[]>(szRecv);
        RDGameObject cacheRDGameObj = null;

        foreach (RDGameObject rdGameObj in rdGameObjs) {
            ShowPanelDataSet.ms_rdgameobjectDict.TryGetValue(rdGameObj.nInstanceID, out cacheRDGameObj);
            if (cacheRDGameObj != null) {
                cacheRDGameObj.nLayer = rdGameObj.nLayer;
            }
        }
        if (OnUpdateData != null) {
            OnUpdateData();

        }
        return true;
    }
    public bool S2C_QueryComponent(NetCmd cmd, Cmd c) {
        string data = c.ReadString();
        try {
            RDComponent[] rdComps = RDDataBase.Deserializer<RDComponent[]>(data);

            MonoBehaviour.DestroyImmediate(ShowPanelDataSet.ms_remoteGameObject);

            ShowPanelDataSet.ms_remoteGameObject = new GameObject("_RemoteDebugger");
            ShowPanelDataSet.ms_remoteGameObject.SetActive(false);

            for (int i = 0; i < rdComps.Length; ++i) {
                ShowPanelDataSet.AddRdComponent(rdComps[i]);

                ShowPanelDataSet.AddRemoteComponent(rdComps[i].szName);
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
        try {
            string szRecv = c.ReadString();

            RDProperty[] rdPropertys = RDDataBase.Deserializer<RDProperty[]>(szRecv);

            for (int i = 0; i < rdPropertys.Length; ++i) {
                rdPropertys[i].Deserializer();
            }

            ShowPanelDataSet.ms_remoteComponent.SetPropertys(rdPropertys); 

            if (OnUpdateData != null) {
                OnUpdateData();
            }

        }
        catch (Exception ex) {
            Debug.Log(ex);
            return false;
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
        return true;
    }
}
