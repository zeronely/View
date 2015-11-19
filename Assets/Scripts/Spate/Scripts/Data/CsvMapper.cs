using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Spate
{
    /// <summary>
    /// Csv映射器，类似于Json映射器一样
    /// </summary>
    public sealed class CsvMapper
    {
        private int ColumnIndex = -1;

        public object ToObject(string line, BaseCsvData csvData)
        {
            if (string.IsNullOrEmpty(line))
                throw new ArgumentNullException("line不能为Null或者Empty");
            string[] allCells = line.Split(',');
            // 获取csvData的所有成员或者属性
            Type t = csvData.GetType();
            FieldInfo[] allFields = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (FieldInfo field in allFields)
            {
                // 获取索引
                ColumnIndex = GetOffset(field);
                // 取出对应在CSV中的值，offset是从1开始
                string cell = allCells[ColumnIndex - 1];
                Type fieldType = field.FieldType;
                object fieldValue = TranslatValueFromString(cell, fieldType);
                // 给成员赋值
                field.SetValue(csvData, fieldValue);
            }
            // 考虑到可能有安全类型，所以还要再检查一次属性
            PropertyInfo[] allProps = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (PropertyInfo prop in allProps)
            {
                if (IsDeclaringByType(t, prop))
                {
                    // 获取索引
                    ColumnIndex = GetOffset(prop);
                    // 取出对应在CSV中的值，offset是从1开始
                    string cell = allCells[ColumnIndex - 1];
                    // 转成对应的类型
                    object propValue = TranslatValueFromString(cell, prop.PropertyType);
                    // 给属性赋值,非索引器
                    prop.SetValue(csvData, propValue, null);
                }
            }
            return csvData.PrimaryKey;
        }

        private static object TranslatValueFromString(string orgValue, Type type)
        {
            object result = null;
            if (type.IsArray)
            {
                // 解析成数组
                result = ArrayFromString(orgValue, type.GetElementType());
            }
            else if (type.IsAssignableFrom(typeof(UnityEngine.Vector3)))
            {
                // 解析成Vector3
                result = VectorFromString(orgValue);
            }
            else if (type.IsAssignableFrom(typeof(bool)))
            {
                result = BoolFromString(orgValue);
            }
            else
            {
                // 转成对应的类型
                result = Convert.ChangeType(orgValue, type);
            }
            return result;
        }

        public string ToString(BaseCsvData csvData)
        {
            if (csvData == null)
                throw new ArgumentNullException("csvData不能为null");
            Type t = csvData.GetType();
            FieldInfo[] allFields = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            PropertyInfo[] allProps = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            int cellsCount = allFields.Length + allProps.Length;
            if (cellsCount == 0) return string.Empty;

            string[] allCells = new string[cellsCount];
            foreach (FieldInfo field in allFields)
            {
                ColumnIndex = GetOffset(field);
                string cell = null;
                if (field.FieldType.IsArray)
                {
                    Array arr = field.GetValue(csvData) as Array;
                    if (arr == null) cell = string.Empty;
                    else cell = ArrayToString(arr);
                }
                else if (field.FieldType.IsAssignableFrom(typeof(UnityEngine.Vector3)))
                {
                    cell = VectorToString((UnityEngine.Vector3)field.GetValue(csvData));
                }
                else if (field.FieldType.IsAssignableFrom(typeof(bool)))
                {
                    cell = BoolToString((bool)field.GetValue(csvData));
                }
                else
                {
                    object fieldValue = field.GetValue(csvData);
                    cell = fieldValue == null ? string.Empty : fieldValue.ToString();
                }
                allCells[ColumnIndex - 1] = cell;
            }
            foreach (PropertyInfo prop in allProps)
            {
                if (IsDeclaringByType(t, prop))
                {
                    ColumnIndex = GetOffset(prop);
                    object propValue = prop.GetValue(csvData, null);
                    string cell = "";
                    if (propValue != null)
                    {
                        if (prop.PropertyType.IsArray)
                        {
                            Array arr = propValue as Array;
                            if (arr == null) cell = string.Empty;
                            else cell = ArrayToString(arr);
                        }
                        else if (prop.PropertyType.IsAssignableFrom(typeof(UnityEngine.Vector3)))
                        {
                            cell = VectorToString((UnityEngine.Vector3)propValue);
                        }
                        else if (prop.PropertyType.IsAssignableFrom(typeof(bool)))
                        {
                            cell = BoolToString((bool)propValue);
                        }
                        else
                        {
                            cell = propValue.ToString();
                        }
                    }
                    allCells[ColumnIndex - 1] = cell;
                }
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < cellsCount; i++)
            {
                sb.Append(allCells[i]);
                if (i != (cellsCount - 1))
                {
                    sb.Append(",");
                }
            }
            return sb.ToString();
        }

        public string GetLastErrorColumn()
        {
            List<string> chars = new List<string>();
            ColumnIndex--;
            do
            {
                if (chars.Count > 0) ColumnIndex--;
                chars.Insert(0, ((char)(ColumnIndex % 26 + (int)'A')).ToString());
                ColumnIndex = (int)((ColumnIndex - ColumnIndex % 26) / 26);
            } while (ColumnIndex > 0);
            return String.Join(string.Empty, chars.ToArray());
        }

        private static int GetOffset(MemberInfo member)
        {
            FieldOffsetAttribute[] attrs = member.GetCustomAttributes(typeof(FieldOffsetAttribute), false) as FieldOffsetAttribute[];
            if (attrs == null)
                return -1;
            return attrs[0].Value;
        }

        private static Array ArrayFromString(string text, Type eleType)
        {
            string[] arr = text.Split(';');
            Array array = Array.CreateInstance(eleType, arr.Length);
            for (int i = 0; i < arr.Length; i++)
            {
                object eleValue = Convert.ChangeType(arr[i], eleType);
                array.SetValue(eleValue, i);
            }
            return array;
        }

        private static string ArrayToString(Array array)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                sb.Append(array.GetValue(i));
                if (i != (array.Length - 1))
                    sb.Append(";");
            }
            return sb.ToString();
        }

        private static UnityEngine.Vector3 VectorFromString(string text)
        {
            string[] arr = text.Split(';');
            UnityEngine.Vector3 vec = UnityEngine.Vector3.zero;
            vec.x = float.Parse(arr[0]);
            vec.y = float.Parse(arr[1]);
            if (arr.Length == 3)
                vec.z = float.Parse(arr[2]);
            return vec;
        }

        private static string VectorToString(UnityEngine.Vector3 vec)
        {
            return string.Format("{0};{1};{2}", vec.x, vec.y, vec.z);
        }

        private static bool BoolFromString(string text)
        {
            return "1".Equals(text);
        }

        private static string BoolToString(bool flag)
        {
            return flag ? "1" : "0";
        }

        private static bool IsDeclaringByType(Type t, PropertyInfo prop)
        {
            MethodInfo method = prop.GetGetMethod();
            if (method == null) method = prop.GetSetMethod();
            return IsDeclaringByType(t, method);
        }

        private static bool IsDeclaringByType(Type t, MethodInfo method)
        {
            // 仅仅在方法继承的时候DeclaringType != t会成立，重写和接口实现都不会成立
            bool ret = object.Equals(t, method.DeclaringType);
            if (ret)
            {
                // 对MethodAttributes进行再次判定,方法在重写或实现抽象、实现接口时都会带Virtual属性
                MethodAttributes attr = method.Attributes;
                if (attr == (attr | MethodAttributes.Virtual))
                {
                    ret = false;
                }
            }
            return ret;
        }
    }
}
