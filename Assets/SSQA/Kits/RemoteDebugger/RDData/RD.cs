using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;

namespace RemoteDebugger {
    public enum BatchOption {
        eContainChildren,
        eOnlySelf,
        eCancle
    }

    public class ExLoad : System.Exception {
        public ExLoad()
            : base() {
        }

        public ExLoad(string msg)
            : base(msg) {
        }
    }

    public class RDLoader {
        public virtual T Load<T>(string szPath) where T : UnityEngine.Object {
            return Resources.Load<T>(szPath);
        }
    }

    public class RD {
        private static RD m_inst = null;
        public static RD Instance {
            get {
                if (m_inst == null) {
                    m_inst = new RD();
                }
                return m_inst;
            }
        }

        private RDLoader m_loader = null;

        public void SetAssetLoader(RDLoader loader) {
            //m_loader = loader;
            m_loader = new RDLoader();
        }

        public T Load<T>(string szPath) where T : UnityEngine.Object {
            return m_loader.Load<T>(szPath);
        }

        public Material LoadMaterial(string szPath) {
            Material ret = null;

            try {
                ret = m_loader.Load<Material>(szPath);
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }

            return ret;
        }

        public UnityEngine.Object LoadShader() {
            return null;
        }

        public UnityEngine.Object LoadMesh() {
            return null;
        }
    }

    public static class ShowPanelDataSet {
        public static void InitDataSet() {
            ms_GameObjDict.Clear();
            ms_lstRootRDObjs.Clear();

            ms_rdFrustumObjDict.Clear();
            ms_lstFrustumRootRDObjs.Clear();

            ms_CompObjDict.Clear();
            ms_currentSelectComps = null;

            if (ms_remoteGameObject == null) {
                ms_remoteGameObject = new GameObject("_RemoteDebugger");
                ms_remoteGameObject.SetActive(false);
            }

            if (ms_remoteComponent != null) {
                UnityEngine.Object.DestroyImmediate(ms_remoteComponent);
            }
        }

        public static void AddGameObj(GameObj rd) {
            int nInstanceID = rd.m_nInstanceID;
            if (!ms_GameObjDict.ContainsKey(nInstanceID)) {
                ms_GameObjDict.Add(nInstanceID, rd);
            }
            if (rd.m_nParentID == -1) {
                ms_lstRootRDObjs.Add(rd);
            }
        }

        public static void AddFrustumRDObject(GameObj rd) {
            int nInstanceID = rd.m_nInstanceID;
            if (!ms_rdFrustumObjDict.ContainsKey(nInstanceID)) {
                ms_rdFrustumObjDict.Add(nInstanceID, rd);
            }
            if (rd.m_nParentID == -1) {
                ms_lstFrustumRootRDObjs.Add(rd);
            }
        }

        public static bool TryGetGameObj(int nInstanceID, out GameObj rd) {
            return ms_GameObjDict.TryGetValue(nInstanceID, out rd);
        }

        public static bool TryGetFrustumGameObj(int nInstanceID, out GameObj rd) {
            return ms_rdFrustumObjDict.TryGetValue(nInstanceID, out rd);
        }

        public static void AddCompObj(CompObj rdComp) {
            int nInstanceID = rdComp.m_nInstanceID;
            if (ms_CompObjDict.ContainsKey(nInstanceID)) {
                ms_CompObjDict[nInstanceID] = rdComp;
            }
            else {
                ms_CompObjDict.Add(nInstanceID, rdComp);
            }
        }

        public static bool AddRemoteComponent(string szComponentType) {
            Type t = Util.GetTypeByName(szComponentType);

            if (t == null || ms_remoteGameObject == null) {
                return false;
            }

            if (ms_remoteGameObject.GetComponent(t) == null) {
                ms_remoteGameObject.AddComponent(t);
            }

            return true;
        }


        private static bool bFind = false;
        private static GameObj preObj = null;

        public static GameObj GetPreGameObj(GameObj GameObj) {
            if (GameObj == null) {
                return null;
            }

            GameObj rdRet = null;
            preObj = null;

            for (int i = 0; i < ms_lstRootRDObjs.Count; ++i) {
                rdRet = _GetPreGameObj(ms_lstRootRDObjs[i], GameObj);
                if (rdRet != null) {
                    break;
                }
            }


            return rdRet;
        }

        private static GameObj _GetPreGameObj(GameObj root, GameObj GameObj) {
            GameObj ret = null;

            if (root == GameObj) {
                return preObj;
            }
            else {
                preObj = root;
            }

            if (root.m_bExpand == true) {
                for (int i = 0; i < root.m_childrenID.Length; ++i) {
                    GameObj rd = null;
                    if (TryGetGameObj(root.m_childrenID[i], out rd)) {
                        ret = _GetPreGameObj(rd, GameObj);
                        if (ret != null) {
                            break;
                        }
                    }
                }
            }

            return ret;
        }

        public static GameObj GetNextGameObj(GameObj GameObj) {
            if (GameObj == null) {
                return null;
            }

            GameObj rdRet = null;
            bFind = false;

            for (int i = 0; i < ms_lstRootRDObjs.Count; ++i) {
                rdRet = _GetNextGameObj(ms_lstRootRDObjs[i], GameObj);
                if (rdRet != null) {
                    break;
                }
            }
            return rdRet;
        }

        private static GameObj _GetNextGameObj(GameObj root, GameObj GameObj) {
            if (bFind == true) {
                return root;
            }

            if (root == GameObj) {
                bFind = true;
            }

            GameObj ret = null;
            if (root.m_bExpand == true) {
                for (int i = 0; i < root.m_childrenID.Length; ++i) {
                    GameObj rd = null;
                    if (TryGetGameObj(root.m_childrenID[i], out rd)) {
                        ret = _GetNextGameObj(rd, GameObj);
                        if (ret != null) {
                            break;
                        }
                    }
                }
            }

            return ret;
        }


        public static Dictionary<int, GameObj> ms_GameObjDict = new Dictionary<int, GameObj>();
        public static List<GameObj> ms_lstRootRDObjs = new List<GameObj>();

        public static Dictionary<int, CompObj> ms_CompObjDict = new Dictionary<int, CompObj>();
        public static CompObj[] ms_currentSelectComps = null;

        public static Dictionary<int, GameObj> ms_rdFrustumObjDict = new Dictionary<int, GameObj>();
        public static List<GameObj> ms_lstFrustumRootRDObjs = new List<GameObj>();

        public static GameObject ms_remoteGameObject = null;
        public static Component ms_remoteComponent = null;

        public static void ClearAllData() {
            if (ms_GameObjDict.Count > 0) {
                ms_GameObjDict.Clear();
            }
            if (ms_lstRootRDObjs.Count > 0) {
                ms_lstRootRDObjs.Clear();
            }
            if (ms_CompObjDict.Count > 0) {
                ms_CompObjDict.Clear();
            }
            if (ms_rdFrustumObjDict.Count > 0) {
                ms_rdFrustumObjDict.Clear();
            }
            if (ms_lstFrustumRootRDObjs.Count > 0) {
                ms_lstFrustumRootRDObjs.Clear();
            }
            if (ms_remoteGameObject != null) {
                MonoBehaviour.DestroyImmediate(ms_remoteGameObject);
            }
        }
    }

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
    }
}