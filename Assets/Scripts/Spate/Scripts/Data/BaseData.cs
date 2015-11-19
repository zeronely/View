using System;
using System.Reflection;
using System.Collections.Generic;

namespace Spate
{
    /// <summary>
    /// Data的基类,自动映射,需要提供主键信息,如果想捕获字段被更新的消息,
    /// 请重写OnInitNotify并注册字段和消息码映射
    /// </summary>
    public abstract class BaseData
    {
        /// <summary>
        /// 获取主键值,便于DataManager的Table进行存储和索引,如果无主键,就继承BaseNoKeyData
        /// </summary>
        public abstract object PrimaryKey { get; }

        /// <summary>
        /// 数据发生改变时回调
        /// </summary>
        public virtual void OnDataUpdate(string name, object newValue, object oldValue)
        {

        }
        /// <summary>
        /// 数据发生改变时回调,每个对象仅会调用一次
        /// </summary>
        public virtual void OnDataUpdate(List<string> names, List<object> newValues, List<object> oldValues)
        {

        }

        public virtual void OnDataDelete()
        {

        }

        public virtual void OnDataAdd()
        {

        }


        public void CopyFrom(params BaseData[] newDatas)
        {
            foreach (BaseData newData in newDatas)
            {
                CopyFrom(newData);
            }
        }

        public void CopyFrom(BaseData newData)
        {
            if (newData == null)
                return;
            // 筛选出自身的有效字段集合
            Dictionary<string, FieldInfo> myFieldMap = new Dictionary<string, FieldInfo>();
            FieldInfo[] myfieldArray = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (FieldInfo f in myfieldArray)
            {
                myFieldMap.Add(f.Name, f);
            }
            // 获取newData所有有效的字段并取值
            if (newData == null)
                Logger.Log("new Data is Null");
            Dictionary<string, object> newDataFieldValueMap = new Dictionary<string, object>();
            FieldInfo[] newFieldArray = newData.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            bool redirect = !GetType().Equals(newData.GetType());
            foreach (FieldInfo f in newFieldArray)
            {
                string name = f.Name;
                if (redirect)
                    name = GetFieldName(f);
                // 这个地方的判定可有可无，加上只是为了用ContainsKey来降低GetValue的时间成本
                if (myFieldMap.ContainsKey(name))
                {
                    object value = f.GetValue(newData);
                    newDataFieldValueMap.Add(name, value);
                }
            }

            // 拷贝值
            foreach (KeyValuePair<string, FieldInfo> pair in myFieldMap)
            {
                string name = pair.Key;
                if (newDataFieldValueMap.ContainsKey(name))
                {
                    object value = newDataFieldValueMap[name];
                    // 覆盖值
                    try
                    {
                        if (value == null)
                            pair.Value.SetValue(this, null);
                        else
                        {
                            // 引用类型会直接拷贝引用过去，会在修改this的时候直接引发newData也被修改
                            Type t = value.GetType();
                            if (t.IsArray)
                            {
                                Type eType = t.GetElementType();
                                Array srcArray = (Array)value;
                                Array array = Array.CreateInstance(eType, srcArray.Length);
                                pair.Value.SetValue(this, array);
                                for (int i = 0, len = srcArray.Length; i < len; i++)
                                {
                                    array.SetValue(srcArray.GetValue(i), i);
                                }
                            }
                            else
                            {
                                pair.Value.SetValue(this, value);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex);
                        Logger.LogError(string.Format("{0}.{1}", this.GetType().Name, name));
                    }
                }
            }
        }

        private string GetFieldName(FieldInfo field)
        {
            string name = field.Name;
            // 属性判定
            FieldRedirectAttribute[] arr = field.GetCustomAttributes(typeof(FieldRedirectAttribute), false) as FieldRedirectAttribute[];
            if (arr != null && arr.Length > 0)
            {
                FieldRedirectAttribute attr = arr[0];
                name = attr.Value;
            }
            return name;
        }

        private FieldInfo mPrimaryKey;
        public FieldInfo GetPrimaryKey()
        {
            if (mPrimaryKey == null)
            {
                FieldInfo[] fieldArray = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (FieldInfo f in fieldArray)
                {
                    object[] attrs = f.GetCustomAttributes(typeof(PrimaryKeyAttribute), false);
                    if (attrs != null && attrs.Length > 0)
                    {
                        mPrimaryKey = f;
                    }
                }
            }
            return mPrimaryKey;
        }

        private FieldInfo mCsvForeginKey;
        public FieldInfo GetCsvForeginKey()
        {
            if (mCsvForeginKey == null)
            {
                FieldInfo[] fieldArray = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (FieldInfo f in fieldArray)
                {
                    object[] attrs = f.GetCustomAttributes(typeof(CsvForeginKeyAttribute), false);
                    if (attrs != null && attrs.Length > 0)
                    {
                        mCsvForeginKey = f;
                    }
                }
            }
            return mCsvForeginKey;
        }

    }
}
