using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Spate
{
    public sealed class DataManager : BaseManager
    {
        private const string OPT_ADD = "add";
        private const string OPT_UPDATE = "update";
        private const string OPT_DELETE = "delete";
        private const string OPT_OTHER = "other";

        // 类型名-类型映射表(服务器数据)
        private static Dictionary<string, Type> mNameDataTypeMap = new Dictionary<string, Type>();
        // 类型名-类型映射表(CSV数据)
        private static Dictionary<string, Type> mNameCSVTypeMap = new Dictionary<string, Type>();
        // 类型名-用户数据类型的映射(自定义数据)
        private static Dictionary<string, Type> mNameUserTypeMap = new Dictionary<string, Type>();
        // SvrData类型->(CsvData类型,用户自定义Data类型)
        private static Dictionary<Type, AutoEntity> mAutoDataMap = new Dictionary<Type, AutoEntity>();
        // 文件名-csv头部，便于对BaseCSV数据进行写文件,在读取的时候要填充该数据
        // private static Dictionary<Type, StringBuilder> mTypeCsvHeadMap = new Dictionary<Type, StringBuilder>();
        // 数据库-表-主键-数据体
        private static Dictionary<Type, Dictionary<object, BaseData>> mDataBase = new Dictionary<Type, Dictionary<object, BaseData>>();

        //private static Dictionary<Type, Dictionary<object, BaseData>> mOtherBase = new Dictionary<Type, Dictionary<object, BaseData>>();

        private static Dictionary<Type, Dictionary<object, BaseData>> mCacheUpdateDatas = new Dictionary<Type, Dictionary<object, BaseData>>();
        // private static Dictionary<Type, Dictionary<string, List<Action<BaseData>>>> mHandles = new Dictionary<Type, Dictionary<string, List<Action<BaseData>>>>(); 

        public static List<object> UpdatesCache = null;

        public static List<BaseData> DeletesCache = new List<BaseData>();

        public static List<BaseData> AddsCache = new List<BaseData>();

        internal DataManager()
        {

        }

        /// <summary>
        /// 填充网络模块返回的数据(该方法是在子线程中被调度的)
        /// </summary>
        internal static void FillNetData(Dictionary<string, object> data)
        {
            object dataDic = null;
            if (data.TryGetValue(OPT_ADD, out dataDic))
            {
                FillNetData(dataDic as Dictionary<string, object>, AddHandle);
                //data.Remove(OPT_ADD);
            }
            if (data.TryGetValue(OPT_UPDATE, out dataDic))
            {
                FillNetData(dataDic as Dictionary<string, object>, UpdateHandle);
                //data.Remove(OPT_UPDATE);
            }
            if (data.TryGetValue(OPT_DELETE, out dataDic))
            {
                FillNetData(dataDic as Dictionary<string, object>, DeleteHandle);
                //data.Remove(OPT_DELETE);
            }
            FillOtherData();
        }

        /// <summary>
        /// 线程中被调度
        /// </summary>
        private static void FillNetData(Dictionary<string, object> dataDic, Action<Type, Dictionary<string, object>> handle)
        {
            using (Dictionary<string, object>.Enumerator erator = dataDic.GetEnumerator())
            {
                while (erator.MoveNext())
                {
                    KeyValuePair<string, object> pair = erator.Current;
                    // 获取类型
                    Type dataType = TryFindSvrDataType(pair.Key);
                    object pairValue = pair.Value;
                    Type pairValueType = pairValue.GetType();
                    if (pairValueType.GetInterface("IList") != null)
                    {
                        List<object> listJsonData = pair.Value as List<object>;
                        for (int i = 0; i != listJsonData.Count; i++)
                        {
                            handle(dataType, listJsonData[i] as Dictionary<string, object>);
                        }
                    }
                    else
                    {
                        handle(dataType, pairValue as Dictionary<string, object>);
                    }
                }
            }
        }

        private static void FillNetOtherData(Dictionary<string, object> dataDic, out Dictionary<Type, Dictionary<string, object>> handle)
        {
            handle = new Dictionary<Type, Dictionary<string, object>>();
            using (Dictionary<string, object>.Enumerator erator = dataDic.GetEnumerator())
            {
                while (erator.MoveNext())
                {
                    KeyValuePair<string, object> pair = erator.Current;

                    // 获取类型
                    Type dataType = TryFindSvrDataType(pair.Key);
                    object pairValue = pair.Value;
                    Type pairValueType = pairValue.GetType();
                    if (pairValueType.GetInterface("IList") != null)
                    {
                        List<object> listJsonData = pair.Value as List<object>;
                        for (int i = 0; i != listJsonData.Count; i++)
                        {
                            handle.Add(dataType, listJsonData[i] as Dictionary<string, object>);
                        }
                    }
                    else
                    {
                        handle.Add(dataType, pairValue as Dictionary<string, object>);
                    }
                }
            }
        }

        // 线程中被调度
        private static void AddHandle(Type t, Dictionary<string, object> jsonData)
        {
            if (jsonData == null || jsonData.Count == 0)
                return;
            FillUpdateData(t, jsonData);
            // 检查Table是否存在
            Dictionary<object, BaseData> table = null;
            if (!mDataBase.TryGetValue(t, out table))
            {
                // 新增表
                table = new Dictionary<object, BaseData>();
                mDataBase.Add(t, table);
            }
            // 创建t的实例
            BaseData data = TryCreateBaseData(t);
            // 初始化数据
            MiniJson.Json.ToObject(jsonData, data);
            AddsCache.Add(data);
            // 添加到Table中
            table.Add(data.PrimaryKey, data);

            TryAutoUserData(data);
        }

        //  线程中被调度
        private static void UpdateHandle(Type t, Dictionary<string, object> jsonData)
        {
            if (jsonData == null || jsonData.Count == 0)
                return;

            bool exists = false;
            if (mDataBase.ContainsKey(t))
            {
                // 我们可以确保table如果存在则一定有数据(add和delete后记得处理table)
                // 大胆猜想jsonData中第一个Pair就是主键信息,因此可以利用值转换成类型得到真实的主键值
                Dictionary<object, BaseData> table = mDataBase[t];
                //找到Data主键字段
                BaseData baseData = TryCreateBaseData(t) as BaseData;
                FieldInfo primaryKeyFiled = baseData.GetPrimaryKey();
                //找到jsonData对应主键字段的值
                object jsonValue = null;
                if (primaryKeyFiled != null)
                    jsonData.TryGetValue(primaryKeyFiled.Name, out jsonValue);
                else
                {
                    BaseNoKeyData noKeyData = baseData as BaseNoKeyData;
                    if (!noKeyData.NoKeyButList)
                        jsonValue = Get(t).PrimaryKey;
                }
                BaseData data = null;
                if (jsonValue != null && table.TryGetValue(jsonValue, out data))
                {
                    FillUpdateData(t, jsonData);
                    exists = true;
                    // 执行更新操作
                    MiniJson.Json.ToObject(jsonData, data,
                        (string fieldName, object oldValue, object newValue) =>
                        {
                            if (UpdatesCache == null)
                                UpdatesCache = new List<object>(100);
                            UpdatesCache.Add(new object[] { data, fieldName, newValue, oldValue });
                        });
                    // 更新AutoData
                    TryAutoUserData(data);
                }
            }
            if (!exists)
                AddHandle(t, jsonData);
        }

        public static void FillUpdateData(Type t, Dictionary<string, object> jsonData)
        {
            if (jsonData == null || jsonData.Count == 0)
                return;
            if (!mCacheUpdateDatas.ContainsKey(t))
                mCacheUpdateDatas.Add(t, new Dictionary<object, BaseData>());
            Dictionary<object, BaseData> table = mCacheUpdateDatas[t];
            //找到jsonData对应主键字段的值
            BaseData baseData = TryCreateBaseData(t) as BaseData;
            FieldInfo primaryKeyFiled = baseData.GetPrimaryKey();
            if (primaryKeyFiled == null) return;
            object primaryValue = null;
            jsonData.TryGetValue(primaryKeyFiled.Name, out primaryValue);

            if (!mDataBase.ContainsKey(t) || (mDataBase.ContainsKey(t) && !mDataBase[t].ContainsKey(primaryValue)))
            {
                if (!table.ContainsKey(primaryValue))
                    table.Add(primaryValue, baseData);
                baseData = table[primaryValue];
                MiniJson.Json.ToObject(jsonData, baseData);
            }
            else
            {
                //取出数据库的数据并拷贝
                BaseData oldData = mDataBase[t][primaryValue];
                //if (!(baseData is RoleSvrData))
                //{
                //    FieldInfo curFiled = baseData.GetCsvForeginKey();
                //    if (curFiled != null)
                //        curFiled.SetValue(baseData, oldData.GetCsvForeginKey().GetValue(oldData));
                //}
                //取出JsonData数据
                bool isChange = false;
                using (Dictionary<string, object>.Enumerator erator = jsonData.GetEnumerator())
                {
                    while (erator.MoveNext())
                    {
                        KeyValuePair<string, object> pair = erator.Current;

                        FieldInfo oldFiled = oldData.GetType().GetField(pair.Key);
                        if (oldFiled == null)
                            continue;
                        if (!pair.Value.Equals(oldFiled.GetValue(oldData)))
                        {
                            object addValue = null;
                            if (oldFiled.FieldType.IsValueType)
                            {
                                switch (Type.GetTypeCode(oldFiled.FieldType))
                                {
                                    case TypeCode.Int32:
                                        {
                                            int newValue = (int)pair.Value;
                                            int oldValue = (int)oldFiled.GetValue(oldData);
                                            if (newValue - oldValue > 0)
                                                addValue = newValue - oldValue;
                                            //if (Settings.Debug && newValue <= oldValue && (oldData is GoodsSvrData))
                                            //{
                                            //    GoodsSvrData svrData = oldData as GoodsSvrData;
                                            //    GoodsCsvData csvData = DataManager.Get<GoodsCsvData>(svrData.goods);
                                            //    Logger.LogError("GoodsID={0},Name={1},newValue={2},oldValue={3}", svrData.goods, csvData.name, newValue, oldValue);
                                            //}
                                        }
                                        break;
                                    //case TypeCode.Int64:
                                    //    addValue = (long)jsonData[filedName] - (long)oldFiled.GetValue(baseData);
                                    //    break;
                                    case TypeCode.Single:
                                        addValue = (float)pair.Value - (float)oldFiled.GetValue(oldData);
                                        break;
                                    case TypeCode.Double:
                                        addValue = (double)pair.Value - (double)oldFiled.GetValue(oldData);
                                        break;
                                }
                            }
                            if (addValue != null)
                            {
                                isChange = true;
                                oldFiled.SetValue(baseData, addValue);
                            }
                        }
                    }
                }
                if (isChange)
                {
                    if (!table.ContainsKey(primaryValue))
                        table.Add(primaryValue, baseData);
                    else
                    {
                        table[primaryValue].CopyFrom(baseData);
                    }
                }
            }
        }

        public static void ClearUpdateCacheData()
        {
            if (mCacheUpdateDatas != null)
                mCacheUpdateDatas.Clear();
        }
        // 线程中被调度
        private static void DeleteHandle(Type t, Dictionary<string, object> jsonData)
        {
            if (jsonData == null || jsonData.Count == 0)
                return;
            // 查找到主键对应的数据
            if (mDataBase.ContainsKey(t))
            {
                // 方案同UpdateHandle
                Dictionary<object, BaseData> table = mDataBase[t];
                //找到Data主键字段
                BaseData baseData = TryCreateBaseData(t) as BaseData;
                FieldInfo primaryKeyFiled = baseData.GetPrimaryKey();
                //找到jsonData对应主键字段的值
                object jsonValue = null;
                if (primaryKeyFiled != null)
                    jsonData.TryGetValue(primaryKeyFiled.Name, out jsonValue);
                else
                {
                    jsonValue = Get(t).PrimaryKey;
                }
                // 从表中移除
                if (table.ContainsKey(jsonValue))
                    DeletesCache.Add(table[jsonValue]);
                table.Remove(jsonValue);
                if (mAutoDataMap.ContainsKey(t))
                {
                    Type autoType = mAutoDataMap[t].autoType;
                    if (mDataBase.ContainsKey(autoType))
                    {
                        Dictionary<object, BaseData> autoTable = mDataBase[autoType];
                        autoTable.Remove(jsonValue);
                    }
                }
            }
        }

        /// <summary>
        /// 尝试根据json中的类名推导出真实的Type 
        /// </summary>
        private static Type TryFindSvrDataType(string name)
        {
            Type t = null;
            if (!mNameDataTypeMap.TryGetValue(name, out t))
            {
                // notice -> NoticeData
                // 1,首字母大写
                string inferName = name.ToUpper(0, 1);
                // 2,尾缀添加SvrData,去掉服务器返回的Data尾缀然后再添加SvrData尾缀
                if (inferName.EndsWith("Data"))
                    inferName = inferName.Substring(0, inferName.Length - 4);
                inferName = inferName + "SvrData";
                // 根据名称查找类型(考虑垮程序集查找的问题)
                t = Type.GetType(inferName, false, false);
                if (t == null)
                {
                    string assemblyQualifiedName = string.Format("{0}, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", inferName);
                    t = Type.GetType(assemblyQualifiedName);
                }
                if (t == null)
                    throw new Exception(string.Format("推导出的类型{0}不存在", inferName));
                // 将类型缓存起来,检测t是否实现了BaseData接口
                if (!t.IsSubclassOf(typeof(BaseData)))
                    throw new Exception(string.Format("请确保类型{0}继承了BaseData类", t));
                mNameDataTypeMap.Add(name, t);

                // 尝试根据SvrType推导出CsvDataType和UserDataType
                if (t != null && !mAutoDataMap.ContainsKey(t))
                {
                    BaseData baseData = TryCreateBaseData(t);
                    FieldInfo primaryKey = baseData.GetPrimaryKey();
                    FieldInfo csvForeginKey = baseData.GetCsvForeginKey();

                    if (csvForeginKey != null)
                    {
                        //
                        string csvDataTypeName = t.Name.Replace("SvrData", "CsvData");
                        string userDataTypeName = t.Name.Replace("SvrData", "Data");
                        //
                        AutoEntity entity = new AutoEntity();
                        entity.csvType = TryFindCsvDataType(csvDataTypeName);
                        entity.autoType = TryFindUserDataType(userDataTypeName);
                        entity.svrPrimaryKey = primaryKey;
                        entity.svrCsvForeginKey = csvForeginKey;
                        mAutoDataMap.Add(t, entity);
                    }
                }
            }
            return t;
        }

        private static void TryAutoUserData(BaseData svrData)
        {
            Type svrDataType = svrData.GetType();
            if (mAutoDataMap.ContainsKey(svrDataType))
            {
                AutoEntity entity = mAutoDataMap[svrDataType];
                // 检测制定id的数据是否在DataBase中存在
                object primaryKey = entity.svrPrimaryKey.GetValue(svrData);
                BaseData autoData = Get(entity.autoType, primaryKey);
                if (autoData != null)
                {
                    autoData.CopyFrom(svrData);
                }
                else
                {
                    autoData = Activator.CreateInstance(entity.autoType) as BaseData;
                    // 找到csvData记录
                    object foreginKeyValue = entity.svrCsvForeginKey.GetValue(svrData);
                    BaseData csvData = Get(entity.csvType, foreginKeyValue) as BaseData;
                    autoData.CopyFrom(csvData, svrData);
                    if (!mDataBase.ContainsKey(entity.autoType))
                    {
                        mDataBase.Add(entity.autoType, new Dictionary<object, BaseData>());
                    }
                    mDataBase[entity.autoType].Add(primaryKey, autoData);
                }
            }
        }


        /// <summary>
        /// 尝试根据Csv中的表名名推导出真实的Type
        /// </summary>
        private static Type TryFindUserDataType(string name)
        {
            Type t = null;
            if (!mNameUserTypeMap.TryGetValue(name, out t))
            {
                // 根据名称查找类型(考虑垮程序集查找的问题)
                t = Type.GetType(name, false, false);
                if (t == null)
                {
                    string assemblyQualifiedName = string.Format("{0}, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", name);
                    t = Type.GetType(assemblyQualifiedName);
                }
                if (t == null)
                    throw new Exception(string.Format("推导出的用户自定义数据类型{0}不存在", name));
                // 将类型缓存起来,检测t是否实现了BaseCsvData接口
                if (!t.IsSubclassOf(typeof(BaseData)))
                    throw new Exception(string.Format("请确保类型{0}继承了BaseData类", t));
                mNameUserTypeMap.Add(name, t);
            }
            return t;
        }


        public static Dictionary<Type, Dictionary<object, BaseData>> OtherHandle(Dictionary<string, object> jsonData)
        {
            if (jsonData == null || jsonData.Count == 0)
                return null;
            Dictionary<Type, Dictionary<string, object>> typeTable = null;
            //
            FillNetOtherData(jsonData, out typeTable);
            Dictionary<Type, Dictionary<object, BaseData>> otherTable = new Dictionary<Type, Dictionary<object, BaseData>>();

            using (Dictionary<Type, Dictionary<string, object>>.Enumerator erator = typeTable.GetEnumerator())
            {
                while (erator.MoveNext())
                {
                    KeyValuePair<Type, Dictionary<string, object>> pair = erator.Current;
                    BaseData baseData = TryCreateBaseData(pair.Key) as BaseData;
                    FieldInfo primaryKeyFiled = baseData.GetPrimaryKey();
                    //找到jsonData对应主键字段的值
                    object primaryKey = null;
                    pair.Value.TryGetValue(primaryKeyFiled.Name, out primaryKey);
                    Dictionary<object, BaseData> table = null;

                    if (!otherTable.ContainsKey(pair.Key))
                    {
                        table = new Dictionary<object, BaseData>();
                        otherTable.Add(pair.Key, table);
                    }
                    table = otherTable[pair.Key];
                    MiniJson.Json.ToObject(pair.Value, baseData);
                    table.Add(primaryKey, baseData);
                }
            }
            return otherTable;
        }

        public static Dictionary<object, BaseData> GetOtherData<T>() where T : BaseData
        {
            Dictionary<object, BaseData> table = null;
            mCacheUpdateDatas.TryGetValue(typeof(T), out table);
            return table;
        }

        public static Dictionary<Type, Dictionary<object, BaseData>> GetOtherBase()
        {
            return mCacheUpdateDatas;
        }

        public static List<BaseData> GetOtherList()
        {
            List<BaseData> datas = new List<BaseData>();
            using (Dictionary<Type, Dictionary<object, BaseData>>.Enumerator erator = mCacheUpdateDatas.GetEnumerator())
            {
                while (erator.MoveNext())
                {
                    using (Dictionary<object, BaseData>.ValueCollection.Enumerator x = erator.Current.Value.Values.GetEnumerator())
                    {
                        while (x.MoveNext())
                        {
                            datas.Add(x.Current);
                        }
                    }
                }
            }
            return datas;
        }

        private static void FillOtherData()
        {
            /*
            mOtherBase.Clear();
            if (AddsCache != null)
                mOtherBase.Clear();
            if (UpdatesCache != null && UpdatesCache.Count > 0)
            {
                foreach (object o in DataManager.UpdatesCache)
                {
                    object[] arr = o as object[];
                    if (arr == null)
                        continue;
                    BaseData data = (BaseData)arr[0];
                    string fieldName = arr[1].ToString();
                    object newValue = arr[2];
                    object oldValue = arr[3];

                    Type t = data.GetType();
                    FieldInfo filed = t.GetField(fieldName);
                    object addValue = null;
                    bool isAdd = false;
                    if (filed.FieldType.IsValueType)
                    {
                        switch (Type.GetTypeCode(filed.FieldType))
                        {
                            case TypeCode.Int32:
                                addValue = (int)newValue - (int)oldValue;
                                isAdd = (int)addValue > 0;
                                break;
                            case TypeCode.Int64:
                                addValue = (long)newValue - (long)oldValue;
                                isAdd = (long)addValue > 0;
                                break;
                            case TypeCode.Single:
                                addValue = (float)newValue - (float)oldValue;
                                isAdd = (float)addValue > 0;
                                break;
                            case TypeCode.Double:
                                addValue = (double)newValue - (double)oldValue;
                                isAdd = (double)addValue > 0;
                                break;
                        }
                    }
                    //Logger.Log("isAdd" + isAdd + "||addValue = " + addValue);
                    if (isAdd)
                    {
                        BaseData baseData = null;

                        Dictionary<object, BaseData> map = null;
                        if (!mOtherBase.TryGetValue(data.GetType(), out map))
                        {
                            map = new Dictionary<object, BaseData>();
                            mOtherBase.Add(data.GetType(), map);
                        }

                        FieldInfo priFiled = data.GetPrimaryKey();
                        if (priFiled != null)
                        {
                            if (!map.ContainsKey(data.PrimaryKey))
                            {
                                baseData = TryCreateBaseData(data.GetType());
                                if (!data.GetType().Equals(typeof(RoleSvrData)))
                                    baseData.CopyFrom(data);
                                else
                                {
                                    priFiled.SetValue(baseData, data.PrimaryKey);
                                }
                                map.Add(baseData.PrimaryKey, baseData);
                            }
                            baseData = map[data.PrimaryKey];
                        }
                        else
                        {
                            if (mOtherBase[data.GetType()].Count == 0)
                            {
                                baseData = TryCreateBaseData(data.GetType());
                                baseData.CopyFrom(data);
                                mOtherBase[data.GetType()].Add(baseData.PrimaryKey, baseData);
                            }
                            baseData = mOtherBase[data.GetType()].FirstValue() as BaseData;
                        }
                        filed.SetValue(baseData, addValue);
                    }
                }
            }

            if (AddsCache != null && AddsCache.Count > 0)
            {
                foreach (BaseData d in DataManager.AddsCache)
                {
                    BaseData data = null;
                    if (!mOtherBase.ContainsKey(d.GetType()))
                    {
                        mOtherBase.Add(d.GetType(), new Dictionary<object, BaseData>());
                    }
                    if (!mOtherBase[d.GetType()].ContainsKey(d.PrimaryKey))
                    {
                        data = TryCreateBaseData(d.GetType());
                        data.CopyFrom(d);
                        mOtherBase[d.GetType()].Add(data.PrimaryKey, data);
                    }
                    else
                    {
                        Logger.Log("数据库已经存在的数据");
                    }
                }
            }
             */
        }

        /// <summary>
        /// 保存所有的CSV数据，写入到原始csv文件中去(如果不存在就新建)，通常该方法用于编辑器
        /// </summary>
        public static void SaveAllCsvData()
        {
            foreach (Type t in mDataBase.Keys)
            {
                // 所有的Csv数据都继承了BaseCSV
                if (t.IsSubclassOf(typeof(BaseCsvData)))
                {
                    SaveCsvData(t);
                }
            }
        }

        /// <summary>
        /// 保存指定的CSV数据
        /// </summary>
        public static void SaveCsvData<T>() where T : BaseCsvData
        {
            SaveCsvData(typeof(T));
        }

        public static void SaveCsvData(Type t)
        {
            //if (!ResHost.UseCsv) return;
            //// 所有的Csv数据都继承了BaseCSV
            //if (!t.IsSubclassOf(typeof(BaseCsvData)))
            //    throw new ArgumentException(string.Format("类型{0}必须要是BaseCsv的子类", t));
            //// 根据t找到原始的csv文件名
            //string csvFileName = FindCsvNameByType(t);
            //if (string.IsNullOrEmpty(csvFileName))
            //    throw new Exception(string.Format("未知异常:类型{0}没有经过读取", t));
            //// 找到Csv源文件的路径
            //string csvPath = Path.Combine(Settings.CSV_FOLDER, csvFileName);

            //// 找到Type的头
            //StringBuilder sb = null;
            //if (!mTypeCsvHeadMap.TryGetValue(t, out sb))
            //    throw new Exception(string.Format("未知异常:类型{0}并没有保存CSV头信息", t));
            //// 找到所有的行记录
            //Dictionary<object, BaseData> allRowData = null;
            //mDataBase.TryGetValue(t, out allRowData);
            //if (allRowData != null)
            //{
            //    CsvMapper mapper = new CsvMapper();
            //    foreach (BaseData bd in allRowData.Values)
            //    {
            //        BaseCsvData rowData = bd as BaseCsvData;
            //        sb.AppendLine(mapper.ToString(rowData));
            //    }
            //}
            //// 写入文件
            //StreamWriter writer = new StreamWriter(csvPath, false, Encoding.Default, 4096);
            //writer.AutoFlush = true;
            //writer.Write(sb.ToString());
            //try
            //{
            //    writer.Close();
            //}
            //catch { }
        }

        /// <summary>
        /// 填充CSV数据，暂不支持无类型数据，强制要求前三行分别是中文，类型、英文
        /// </summary>
        public static void FillCsvData(string name, string[] allLines)
        {
            Type t = TryFindCsvDataType(name);
            mDataBase.Add(t, FillCsvData(t, allLines));
        }

        private static Dictionary<object, BaseData> FillCsvData(Type t, string[] allLines)
        {
            // 表头占3行
            int rowCount = allLines.Length - 3;
            Dictionary<object, BaseData> tableData = new Dictionary<object, BaseData>(rowCount);
            // 逐行进行解析
            //CsvMapper mapper = new CsvMapper();
            for (int i = 3; i < allLines.Length; i++)
            {
                if (string.IsNullOrEmpty(allLines[i]))
                    continue;
                string[] cells = allLines[i].Split(',');
                BaseCsvData rowData = Activator.CreateInstance(t, new object[] { cells }) as BaseCsvData;
                tableData.Add(rowData.PrimaryKey, rowData);
                //try
                //{
                //    object primaryKey = mapper.ToObject(allLines[i], rowData);
                //    tableData.Add(primaryKey, rowData);
                //}
                //catch (Exception ex)
                //{
                //    string message = string.Format("解析数据出错:文件={0}.csv,行={1},列={2},详细错误={3}", FindCsvNameByType(t), i + 1, mapper.GetLastErrorColumn(), ex.Message);
                //    Logger.LogAsset(LogType.Error, message);
                //    break;
                //}
            }
            return tableData;
        }

        /// <summary>
        /// 尝试根据Csv中的表名名推导出真实的Type
        /// </summary>
        private static Type TryFindCsvDataType(string name)
        {
            Type t = null;
            if (!mNameCSVTypeMap.TryGetValue(name, out t))
            {
                // notice -> NoticeData
                // 1,首字母大写
                string inferName = name.ToUpper(0, 1);
                // 2,尾缀添加Csv
                if (!inferName.EndsWith("CsvData"))
                    inferName = inferName + "CsvData";
                // 根据名称查找类型(考虑垮程序集查找的问题)
                t = Type.GetType(inferName, false, false);
                if (t == null)
                {
                    string assemblyQualifiedName = string.Format("{0}, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", inferName);
                    t = Type.GetType(assemblyQualifiedName);
                }
                if (t == null)
                    throw new Exception(string.Format("推导出的类型{0}不存在", inferName));
                // 将类型缓存起来,检测t是否实现了BaseCsvData接口
                if (!t.IsSubclassOf(typeof(BaseCsvData)))
                    throw new Exception(string.Format("请确保类型{0}继承了BaseCsvData类", t));
                mNameCSVTypeMap.Add(name, t);
            }
            return t;
        }

        /// <summary>
        /// 根据CsvData的类型找到原始csv文件名
        /// </summary>
        private static string FindCsvNameByType(Type t)
        {
            string name = null;
            using (Dictionary<string, Type>.Enumerator erator = mNameCSVTypeMap.GetEnumerator())
            {
                while (erator.MoveNext())
                {
                    if (erator.Current.Value == t)
                    {
                        name = erator.Current.Key;
                        break;
                    }
                }
            }
            return name;
        }

        /// <summary>
        /// 根据类型创建BaseData实例
        /// </summary>
        private static BaseData TryCreateBaseData(Type t)
        {
            if (t == null)
                throw new ArgumentNullException("参数t不能为null");
            BaseData data = Activator.CreateInstance(t, true) as BaseData;
            if (data == null)
                throw new Exception(string.Format("为类型{0}创建实例失败,请确保属于BaseData的子类", t));
            return data;
        }

        public override void OnStart()
        {
            RemoveAll();
        }

        public override void OnReset()
        {
            // 重置数据
            RemoveAll();
        }

        public override void OnDestroy()
        {
            RemoveAll();
        }



        /// <summary>
        /// 获取指定类型的第一条数据，常用于获取只有单条数据,例如RoleData
        /// </summary>
        public static T Get<T>() where T : BaseData
        {
            return Get<T>(null);
        }

        /// <summary>
        /// 从数据库中检索数据,T为数据表
        /// <param name="primaryKeyValue">索引值,如果为null则尝试检索T表中第一条数据</param>
        /// </summary>
        public static T Get<T>(object primaryKeyValue) where T : BaseData
        {
            T data = default(T);
            if (primaryKeyValue == null)
            {
                List<T> list = GetList<T>();
                if (list != null && list.Count > 0)
                    data = list[0];
            }
            else
            {
                Dictionary<object, BaseData> table = GetTable<T>();
                if (table != null)
                {
                    BaseData tmpData = null;
                    table.TryGetValue(primaryKeyValue, out tmpData);
                    if (tmpData != null)
                        data = (T)tmpData;
                }
            }
            return data;
        }

        public static BaseData Get(Type t)
        {
            return Get(t, null);
        }

        public static BaseData Get(Type t, object primaryKeyValue)
        {
            BaseData data = null;
            Dictionary<object, BaseData> tableDatas = null;
            mDataBase.TryGetValue(t, out tableDatas);
            if (tableDatas != null && tableDatas.Count > 0)
            {
                if (primaryKeyValue != null)
                    tableDatas.TryGetValue(primaryKeyValue, out data);
                else
                    data = (BaseData)tableDatas.FirstValue();
            }
            return data;
        }

        /// <summary>
        /// 获取整张表的数据
        /// </summary>
        public static Dictionary<object, BaseData> GetTable<T>() where T : BaseData
        {
            Dictionary<object, BaseData> table = null;
            mDataBase.TryGetValue(typeof(T), out table);
            return table;
        }

        /// <summary>
        /// 获取整张表的数据，并智能转换
        /// </summary>
        public static Dictionary<K, V> GetTable<K, V>() where V : BaseData
        {
            if (typeof(V).IsSubclassOf(typeof(BaseNoKeyData)))
                throw new Exception("不支持对无主键数据进行主键类型指定");

            Dictionary<object, BaseData> table = null;
            mDataBase.TryGetValue(typeof(V), out table);

            Dictionary<K, V> vTable = null;
            if (table != null)
            {
                vTable = new Dictionary<K, V>(table.Count);
                // 类型检测
                bool needCheck = true;
                using (Dictionary<object, BaseData>.Enumerator erator = table.GetEnumerator())
                {
                    KeyValuePair<object, BaseData> pair = erator.Current;
                    while (erator.MoveNext())
                    {
                        if (needCheck)
                        {
                            if (!object.Equals(typeof(K), pair.Key.GetType()))
                            {
                                throw new Exception("检索类型不匹配,该为:" + pair.Key.GetType());
                            }
                            needCheck = false;
                        }
                        K newKey = (K)pair.Key;
                        V newValue = (V)pair.Value;
                        vTable.Add(newKey, newValue);
                    }
                }
            }
            return vTable;
        }

        /// <summary>
        /// 获取整张表的数据，用List表示
        /// </summary>
        public static List<T> GetList<T>() where T : BaseData
        {
            Dictionary<object, BaseData> table = GetTable<T>();
            if (table == null)
                return null;
            List<T> list = new List<T>(table.Count);
            // 转成List<T>
            using (Dictionary<object, BaseData>.ValueCollection.Enumerator erator = table.Values.GetEnumerator())
            {
                while (erator.MoveNext())
                {
                    BaseData data = erator.Current;
                    if (data != null)
                        list.Add((T)data);
                }
            }
            return list;
        }

        public static List<BaseData> GetListBaseData<T>() where T : BaseData
        {
            List<T> list = new List<T>();
            list = GetList<T>();
            if (list.Count == 0) return null;
            List<BaseData> listbasedata = new List<BaseData>();
            for (int i = 0; i != list.Count; i++)
            {
                listbasedata.Add(list[i] as BaseData);
            }
            return listbasedata;
        }


        public static void RemoveAll()
        {
            mDataBase.Clear();
            mNameDataTypeMap.Clear();
            mNameCSVTypeMap.Clear();
            mNameUserTypeMap.Clear();
            mAutoDataMap.Clear();
            mCacheUpdateDatas.Clear();
            DeletesCache.Clear();
            AddsCache.Clear();
            if (UpdatesCache != null)
                UpdatesCache.Clear();
        }

        /// <summary>
        /// 删除整张数据表
        /// </summary>
        public static bool RemoveTable<T>()
        {
            Dictionary<object, BaseData> table = null;
            if (mDataBase.TryGetValue(typeof(T), out table))
            {
                table.Clear();
            }
            return mDataBase.Remove(typeof(T));
        }

        /// <summary>
        /// 删除指定表的指定记录
        /// </summary>
        public static bool Remove<T>(object primaryKeyValue)
        {
            bool flag = false;
            Dictionary<object, BaseData> table = null;
            if (mDataBase.TryGetValue(typeof(T), out table))
            {
                flag = table.Remove(primaryKeyValue);
            }
            return flag;
        }

        /// <summary>
        /// 从数据库中移除指定数据对象
        /// </summary>
        public static bool Remove(BaseData data)
        {
            bool flag = false;
            Dictionary<object, BaseData> table = null;
            if (mDataBase.TryGetValue(data.GetType(), out table))
            {
                flag = table.Remove(data.PrimaryKey);
            }
            return flag;
        }

        /// <summary>
        /// 从数据库中移除指定数据对象集合
        /// </summary>
        public static bool Remove(List<BaseData> list)
        {
            bool flag = true;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (!Remove(list[i]))
                    flag = false;
            }
            return flag;
        }

        public static void Add(BaseData data)
        {
            Dictionary<object, BaseData> table = null;
            if (mDataBase.TryGetValue(data.GetType(), out table))
            {
                if (!table.ContainsKey(data.PrimaryKey))
                {
                    table.Add(data.PrimaryKey, data);
                }
            }
        }

        private class AutoEntity
        {
            public Type csvType;
            public Type autoType;
            public FieldInfo svrPrimaryKey;
            public FieldInfo svrCsvForeginKey;
        }
    }
}
