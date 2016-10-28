using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LitJsonEx;

namespace RemoteDebugger {
    public abstract class IObject {
        protected abstract bool SupportSerializer();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected abstract bool Serializer();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected abstract bool DeSerializer();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract string GetTypeName();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract string GetValueName();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serializer<T>(T obj) where T : IObject {
            string szRet = null;

            if (!obj.SupportSerializer()) {
                Debug.LogWarningFormat("[Serializer] {0} : {1}  NotSupport", obj.GetValueName(), obj.GetTypeName());
                goto Exit0;
            }

            if (!obj.Serializer()) {
                Debug.LogWarningFormat("[Serializer] {0} : {1} Failed", obj.GetValueName(), obj.GetTypeName());
                goto Exit0;
            }

            try {
                szRet = JsonMapper.ToJson(obj);
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                szRet = null;
            }

        Exit0:
            return szRet;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public static T DeSerializer<T>(string jsonData) where T : IObject {
            T ret = null;

            try {
                ret = JsonMapper.ToObject<T>(jsonData);
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                goto Exit0;
            }

            if (ret == null) {
                goto Exit0;
            }

            if (!ret.DeSerializer()) {
                Debug.LogWarningFormat("[DeSerializer] {0} : {1} Failed", ret.GetValueName(), ret.GetTypeName());
                ret = null;
            }

        Exit0:
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static string SerializerArray<T>(T[] objs) where T : IObject {
            List<string> lstJson = new List<string>();

            for (int i = 0; i < objs.Length; ++i) {
                T obj = objs[i];

                string szJson = Serializer<T>(obj);
                if (szJson == null) {
                    continue;
                }

                lstJson.Add(szJson);
            }
            return JsonMapper.ToJson(lstJson);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="szJsonData"></param>
        /// <returns></returns>
        public static T[] DeSerializerArray<T>(string szJsonData) where T : IObject, new() {
            List<string> lstJson = JsonMapper.ToObject<List<string>>(szJsonData);
            List<T> lstObj = new List<T>();

            for (int i = 0; i < lstJson.Count; ++i) {
                string szJson = lstJson[i];

                T obj = DeSerializer<T>(szJson);
                if (obj == null) {
                    continue;
                }

                lstObj.Add(obj);
            }
            return lstObj.ToArray();
        }
    }

    /// <summary>
    /// Game object.
    /// </summary>
    public class GameObj : IObject {
        public string m_szName;
        public int m_nInstanceID;
        public int m_nParentID;
        public int[] m_childrenID;

        public int m_nLayer;
        public string m_szTag;
        public bool m_bActive;
        public bool m_bStatic;

        public bool m_bExpand;
        public GameObj(GameObject gameObject) {
            Transform trans = gameObject.transform;

            this.m_szName = gameObject.name;
            this.m_nInstanceID = gameObject.GetInstanceID();

            if (trans.parent != null) {
                this.m_nParentID = trans.parent.gameObject.GetInstanceID();
            }
            else {
                this.m_nParentID = -1;
            }

            this.m_childrenID = new int[trans.childCount];
            for (int i = 0, count = trans.childCount; i < count; ++i) {
                this.m_childrenID[i] = trans.GetChild(i).gameObject.GetInstanceID();
            }

            this.m_nLayer = gameObject.layer;
            this.m_szTag = gameObject.tag;
            this.m_bActive = gameObject.activeSelf;
            this.m_bStatic = gameObject.isStatic;
        }

        public GameObj() {

        }

        public override string GetTypeName() {
            return "GameObject";
        }

        public override string GetValueName() {
            return m_szName;
        }

        protected override bool SupportSerializer() {
            return true;
        }

        protected override bool Serializer() {
            return true;
        }

        protected override bool DeSerializer() {
            return true;
        }
    }

    /// <summary>
    /// Comp object.
    /// </summary>
    public class CompObj : IObject {
        public string m_szName;
        public int m_nInstanceID;

        public bool m_bEnable;
        public bool m_bContainEnable;

        public bool m_bExpand;
        public CompObj() {

        }

        public CompObj(Component comp) {
            this.m_szName = comp.GetType().ToString();
            this.m_nInstanceID = comp.GetInstanceID();

            if (comp.ContainProperty("enabled")) {
                m_bContainEnable = true;
                m_bEnable = comp.GetValue<bool>("enabled");
            }
            else {
                m_bContainEnable = m_bEnable = false;
            }
        }

        public override string GetTypeName() {
            return "Component";
        }

        public override string GetValueName() {
            return m_szName;
        }

        protected override bool SupportSerializer() {
            return true;
        }

        protected override bool Serializer() {
            return true;
        }

        protected override bool DeSerializer() {
            return true;
        }
    }

    /// <summary>
    /// Property object.
    /// </summary>
    public class PropertyObj : IObject {
        public string m_szTypeName;
        public string m_szValueName;
        public System.Object m_value;

        public int m_nComponentID;

        public PropertyObj(Component comp, MemberInfo memInfo) {
            Type type = null;
            if (memInfo.MemberType.Equals(MemberTypes.Property)) {
                type = ((PropertyInfo)memInfo).PropertyType;
                this.m_value = ((PropertyInfo)memInfo).GetValue(comp, null);
            }
            else if (memInfo.MemberType.Equals(MemberTypes.Field)) {
                type = ((FieldInfo)memInfo).FieldType;
                this.m_value = ((FieldInfo)memInfo).GetValue(comp);
            }

            this.m_szTypeName = type.ToString();
            this.m_szValueName = memInfo.Name;
            this.m_nComponentID = comp.GetInstanceID();
        }

        public PropertyObj() {

        }

        public override string GetTypeName() {
            return m_szTypeName;
        }

        public override string GetValueName() {
            return m_szValueName;
        }

        protected override bool SupportSerializer() {
            if (IsAsset()) {
                return false;
            }

            if (IsCollectionType()) {
                return false;
            }

            if (IsDelegate()) {
                return false;
            }

            return true;
        }

        protected override bool Serializer() {
            bool bRet = false;
            Type type = Util.GetTypeByName(m_szTypeName);

            try {
                if (type.IsEnum) {
                    this.m_value = this.m_value.ToString();
                }
                else if (!type.IsPrimitive && !type.Equals(typeof(string))) {
                    this.m_value = JsonMapper.ToJson(this.m_value);
                }
            }
            catch {
                goto Exit0;
            }

            bRet = true;
        Exit0:
            return bRet;
        }

        protected override bool DeSerializer() {
            bool bRet = false;
            Type type = Util.GetTypeByName(m_szTypeName);

            try {
                if (type.IsEnum) {
                    this.m_value = (Enum)Enum.Parse(type, m_value.ToString());
                }
                else if (!type.IsPrimitive && !type.Equals(typeof(string))) {
                    MethodInfo[] mis = typeof(JsonMapper).GetMethods();
                    for (int i = 0; i < mis.Length; ++i) {
                        MethodInfo mi = mis[i];
                        if (mi.Name == "ToObject" &&
                           mi.IsGenericMethod &&
                           mi.GetParameters().Length == 1 &&
                           mi.GetParameters()[0].ParameterType.Equals(typeof(string))) {
                            mi = mi.MakeGenericMethod(type);
                            this.m_value = mi.Invoke(null, new System.Object[] { m_value });
                            break;
                        }
                    }
                }
            }
            catch {
                goto Exit0;
            }

            bRet = true;
        Exit0:
            return bRet;
        }

        public bool IsAsset() {
            return Util.IsAsset(m_szTypeName);
        }

        public bool IsCollectionType() {
            return m_value is System.Collections.ICollection;
        }

        public bool IsDelegate() {
            return m_value is Delegate || m_value is System.Action;
        }
    }
}