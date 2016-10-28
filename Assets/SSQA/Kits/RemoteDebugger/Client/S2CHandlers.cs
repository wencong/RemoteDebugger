using System;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using LitJsonEx;

namespace RemoteDebugger {
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
                net_client.RegisterHandler(NetCmd.S2C_GetComponentProperty, S2C_GetComponentProperty);
                net_client.RegisterHandler(NetCmd.S2C_EnableComponent, S2CEnableComponent);
                net_client.RegisterHandler(NetCmd.S2C_ModifyComponentProperty, S2C_ModifyComponentProperty);
                net_client.RegisterHandler(NetCmd.S2C_Log, S2CDebugLog);
                net_client.RegisterHandler(NetCmd.S2C_FinishWait, S2CFinishWait);
                net_client.RegisterHandler(NetCmd.S2C_QueryFrustumObjs, S2C_QueryFrustumObjs);
            }
        }

        public bool S2CDebugLog(NetCmd cmd, Cmd c) {
            string szLog = c.ReadString();

            Debug.Log(szLog);

            return true;
        }

        public bool S2C_QueryFrustumObjs(NetCmd cmd, Cmd c) {
            string rdGameObjs = c.ReadString();

            try {
                ShowPanelDataSet.InitDataSet();
                GameObj[] arrRdObjs = IObject.DeSerializerArray<GameObj>(rdGameObjs);

                for (int i = 0; i < arrRdObjs.Length; ++i) {
                    ShowPanelDataSet.AddFrustumRDObject(arrRdObjs[i]);
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

        public bool S2C_QueryAllObjs(NetCmd cmd, Cmd c) {
            string rdGameObjs = c.ReadString();

            try {
                ShowPanelDataSet.InitDataSet();
                GameObj[] arrRdObjs = IObject.DeSerializerArray<GameObj>(rdGameObjs);

                for (int i = 0; i < arrRdObjs.Length; ++i) {
                    ShowPanelDataSet.AddGameObj(arrRdObjs[i]);
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
            GameObj rdGameObj = IObject.DeSerializer<GameObj>(szRecv);

            GameObj cacheRDGameObj = null;
            ShowPanelDataSet.ms_GameObjDict.TryGetValue(rdGameObj.m_nInstanceID, out cacheRDGameObj);

            if (cacheRDGameObj != null) {
                cacheRDGameObj.m_bActive = rdGameObj.m_bActive;
            }

            if (OnUpdateData != null) {
                OnUpdateData();
            }

            return true;
        }

        /*
        public bool S2C_SetObjStatic(NetCmd cmd, Cmd c) {
            string szRecv = c.ReadString();

            List<GameObj> rdGameObjs = RDDataBase.Deserializer<List<GameObj>>(szRecv);
            GameObj cacheRDGameObj = null;

            foreach (GameObj rdGameObj in rdGameObjs) {

                ShowPanelDataSet.ms_GameObjDict.TryGetValue(rdGameObj.nInstanceID, out cacheRDGameObj);
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

            List<GameObj> rdGameObjs = RDDataBase.Deserializer<List<GameObj>>(szRecv);
            GameObj cacheRDGameObj = null;

            foreach (GameObj rdGameObj in rdGameObjs) {
                ShowPanelDataSet.ms_GameObjDict.TryGetValue(rdGameObj.nInstanceID, out cacheRDGameObj);
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

            GameObj[] rdGameObjs = IObject.DeSerializerArray<GameObj>(szRecv);
            GameObj cacheRDGameObj = null;

            foreach (GameObj rdGameObj in rdGameObjs) {
                ShowPanelDataSet.ms_GameObjDict.TryGetValue(rdGameObj.m_nInstanceID, out cacheRDGameObj);
                if (cacheRDGameObj != null) {
                    cacheRDGameObj.m_nLayer = rdGameObj.m_nLayer;
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
                CompObj[] rdComps = IObject.DeSerializerArray<CompObj>(data);

                if (ShowPanelDataSet.ms_remoteGameObject != null) {
                    UnityEngine.Object.DestroyImmediate(ShowPanelDataSet.ms_remoteGameObject);
                }

                ShowPanelDataSet.ms_remoteGameObject = new GameObject("_RemoteDebugger");
                ShowPanelDataSet.ms_remoteGameObject.SetActive(false);

                for (int i = 0; i < rdComps.Length; ++i) {
                    if (ShowPanelDataSet.AddRemoteComponent(rdComps[i].m_szName)) {
                        ShowPanelDataSet.AddCompObj(rdComps[i]);
                    }
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

        public bool S2C_GetComponentProperty(NetCmd cmd, Cmd c) {
            try {
                string szRecv = c.ReadString();

                PropertyObj[] PropertyObjs = IObject.DeSerializerArray<PropertyObj>(szRecv);

                ShowPanelDataSet.ms_remoteComponent.SetPropertys(PropertyObjs);

                if (OnUpdateData != null) {
                    OnUpdateData();
                }

            }
            catch (Exception ex) {
                Debug.Log(ex.ToString());
                return false;
            }

            return true;
        }

        public bool S2CEnableComponent(NetCmd cmd, Cmd c) {
            return true;
        }

        public bool S2C_ModifyComponentProperty(NetCmd cmd, Cmd c) {
            return true;
        }
        public bool S2CFinishWait(NetCmd cmd, Cmd c) {
            return true;
        }
    }
}