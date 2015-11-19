using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace MiniJson
{
    /// <summary>
    /// 提供对Json的序列化和反序列化，目前支持的数据类型如下：
    /// <para>简单类型：byte,sbyte,short,ushort,int,uint,long,ulong,float,double(目前不支持MaxValue和MinValue),decimal,bool,enum(Underlying),char,string</para>
    /// <para>复合类型:Array,IList泛型,IDictionary泛型</para>
    /// <para>复合类型:ArrayList，HashTable类仅支持的值默认采用int或者float</para>
    /// <para>对象类型</para>
    /// <para>仅仅是Public的Static或者Instance成员有效</para>
    /// <para>支持vector转成UnityEngine.Vector3</para>
    /// </summary>
    public static class Json
    {
        /// <summary>
        /// 反序列化
        /// <para>对象类型 -> Dictionary<string, object></para>
        /// </summary>
        public static object Deserialize(string json)
        {
            if (json == null) return null;
            return Parser.Parse(json);
        }

        public static T Deserialize<T>(string json)
        {
            object input = Deserialize(json);
            T output = Activator.CreateInstance<T>();
            if (ToObject(input, output)) return output;
            else return default(T);
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        public static string Serialize(object obj)
        {
            return Serializer.Serialize(obj);
        }

        /// <summary>
        /// 将Dictionary映射成对象
        /// </summary>
        public static bool ToObject(object input, object output, Action<string, object, object> valueUpdateHandle = null)
        {
            if (input == null) return false;
            Dictionary<string, object> inputDic = input as Dictionary<string, object>;
            if (inputDic == null) return false;

            bool flag = true;//操作是否成功
            FieldInfo[] fields = output.GetType().GetFields();// 所有的字段
            foreach (FieldInfo field in fields)
            {
                Thread.Sleep(0);//让系统重新进行一次CPU权重计算，防止循环中一直占用CPU资源
                string fieldName = field.Name;
                object fieldValue = null;
                object dicValue = null;
                if (inputDic.TryGetValue(fieldName, out dicValue) && dicValue != null)
                {
                    Type fieldType = field.FieldType;
                    if (ToField(fieldType, out fieldValue, dicValue))
                    {
                        if (valueUpdateHandle != null)
                        {
                            // 取得旧值和新值进行判定，一旦发生改变就进行通知
                            object oldFieldValue = field.GetValue(output);
                            field.SetValue(output, fieldValue);
                            if (!object.Equals(oldFieldValue, fieldValue))
                                valueUpdateHandle(fieldName, oldFieldValue, fieldValue);
                        }
                        else
                        {
                            field.SetValue(output, fieldValue);
                        }
                    }
                    else
                    {
                        flag = false;
                    }
                }
            }
            return flag;
        }

        /// <summary>
        /// 映射字段
        /// </summary>
        /// <param name="fieldType">字段类型</param>
        /// <param name="fieldValue">字段值,传出参数</param>
        /// <param name="jsonValue">字段的Json参照值</param>
        private static bool ToField(Type fieldType, out object fieldValue, object jsonValue)
        {
            bool flag = false;
            fieldValue = null;
            // 空的当做true
            if (jsonValue == null) return true;

            if (fieldType.IsEnum)
            {
                // 由于byte,sbyte,short,ushort,int,uint,long,ulong都有可能是enum，所以在前面处理
                Type underlyingType = Enum.GetUnderlyingType(fieldType);
                jsonValue = Convert.ChangeType(jsonValue, underlyingType);
                if (Enum.IsDefined(fieldType, jsonValue))
                {
                    flag = true;
                    fieldValue = Enum.Parse(fieldType, jsonValue.ToString());
                }
            }
            else
            {
                TypeCode fieldTypeCode = Type.GetTypeCode(fieldType);
                switch (fieldTypeCode)
                {
                    case TypeCode.String:
                        {
                            flag = true;
                            fieldValue = jsonValue.ToString();
                        }
                        break;
                    case TypeCode.Boolean:
                        {
                            flag = true;
                            fieldValue = bool.Parse(jsonValue.ToString());
                        }
                        break;
                    case TypeCode.Char:
                        {
                            flag = true;
                            fieldValue = char.Parse(jsonValue.ToString());
                        }
                        break;
                    case TypeCode.Byte:
                        {
                            flag = true;
                            fieldValue = byte.Parse(jsonValue.ToString());
                        }
                        break;
                    case TypeCode.SByte:
                        {
                            flag = true;
                            fieldValue = sbyte.Parse(jsonValue.ToString());
                        }
                        break;
                    case TypeCode.Int16:
                        {
                            flag = true;
                            fieldValue = short.Parse(jsonValue.ToString());
                        }
                        break;
                    case TypeCode.UInt16:
                        {
                            flag = true;
                            fieldValue = ushort.Parse(jsonValue.ToString());
                        }
                        break;
                    case TypeCode.Int32:
                        {
                            flag = true;
                            fieldValue = int.Parse(jsonValue.ToString());
                        }
                        break;
                    case TypeCode.UInt32:
                        {
                            flag = true;
                            fieldValue = uint.Parse(jsonValue.ToString());
                        }
                        break;
                    case TypeCode.Int64:
                        {
                            flag = true;
                            fieldValue = long.Parse(jsonValue.ToString());
                        }
                        break;
                    case TypeCode.UInt64:
                        {
                            flag = true;
                            fieldValue = ulong.Parse(jsonValue.ToString());
                        }
                        break;
                    case TypeCode.Single:
                        {
                            flag = true;
                            fieldValue = float.Parse(jsonValue.ToString());
                        }
                        break;
                    case TypeCode.Double:
                        {
                            flag = true;
                            fieldValue = double.Parse(jsonValue.ToString());
                        }
                        break;
                    case TypeCode.Decimal:
                        {
                            flag = true;
                            fieldValue = decimal.Parse(jsonValue.ToString());
                        }
                        break;
                    case TypeCode.Object:
                        {
                            if (jsonValue.GetType().IsValueType || (jsonValue is string))
                            {
                                // 逆向反序列化-根据数据来推导object的类型
                                flag = true;
                                fieldValue = jsonValue;
                            }
                            else
                            {
                                // 集合 列表用
                                bool isArray = fieldType.IsArray || (fieldType.GetInterface("IList") != null);
                                if (isArray)
                                {
                                    flag = true;
                                    List<object> list = jsonValue as List<object>;
                                    if (list != null)
                                    {
                                        int len = list.Count;
                                        fieldValue = Activator.CreateInstance(fieldType, new object[] { len });
                                        Type elementType = fieldType.IsGenericType ? fieldType.GetGenericArguments()[0] : fieldType.GetElementType();
                                        flag = (elementType != null);//非泛型IList非Array(例如ArrayList)不被支持
                                        if (flag)
                                        {
                                            for (int i = 0; i < len; i++)
                                            {
                                                object elementValue = null;
                                                object elementJsonValue = list[i];
                                                flag = ToField(elementType, out elementValue, elementJsonValue);
                                                if (flag)
                                                {
                                                    if (fieldType.IsArray) ((Array)fieldValue).SetValue(elementValue, i);
                                                    else ((IList)fieldValue).Add(elementValue);
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (fieldType.GetInterface("IDictionary") != null)
                                {
                                    // 对于IDictionary，目前仅支持泛型类，非泛型类无法精准还原
                                    flag = fieldType.IsGenericType;
                                    if (flag)
                                    {
                                        Dictionary<string, object> dic = jsonValue as Dictionary<string, object>;
                                        if (dic != null)
                                        {
                                            int len = dic.Count;
                                            // 这个地方的capacity可能不能完整还原，但是没关系，Count会一样就行
                                            fieldValue = Activator.CreateInstance(fieldType, new object[] { len });
                                            // 这两个类型非常重要
                                            Type keyType = fieldType.GetGenericArguments()[0];
                                            Type valueType = fieldType.GetGenericArguments()[1];
                                            foreach (KeyValuePair<string, object> pair in dic)
                                            {
                                                object key = null;
                                                object val = null;
                                                if (ToField(keyType, out key, pair.Key) && ToField(valueType, out val, pair.Value))
                                                {
                                                    ((IDictionary)fieldValue).Add(key, val);
                                                }
                                                else
                                                {
                                                    flag = false;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // 纯对象处理
                                    fieldValue = Activator.CreateInstance(fieldType);
                                    flag = ToObject(jsonValue, fieldValue);
                                }
                            }
                        }
                        break;
                }
            }
            return flag;
        }

        /// <summary>
        /// 解析器
        /// </summary>
        private sealed class Parser : IDisposable
        {
            private static Dictionary<string, int> switchMap;
            private StringReader jsonReader;
            private const string WHITE_SPACE = " \t\n\r";
            private const string WORD_BREAK = " \t\n\r{}[],:\"";

            private Parser(string jsonString)
            {
                jsonReader = new StringReader(jsonString);
            }

            public void Dispose()
            {
                jsonReader.Dispose();
                jsonReader = null;
            }



            public static object Parse(string jsonString)
            {
                using (Json.Parser parser = new Json.Parser(jsonString))
                {
                    return parser.ParseValue();
                }
            }

            private List<object> ParseList()
            {
                List<object> list = new List<object>();
                this.jsonReader.Read();
                bool flag = true;
                while (flag)
                {
                    TOKEN nextToken = this.NextToken;
                    switch (nextToken)
                    {
                        case TOKEN.SQUARED_CLOSE:
                            {
                                flag = false;
                                continue;
                            }
                        case TOKEN.COMMA:
                            {
                                continue;
                            }
                        case TOKEN.NONE:
                            return null;
                    }
                    object item = this.ParseByToken(nextToken);
                    list.Add(item);
                }
                return list;
            }

            private object ParseByToken(TOKEN token)
            {
                switch (token)
                {
                    case TOKEN.CURLY_OPEN:
                        return ParseObject();

                    case TOKEN.SQUARED_OPEN:
                        return ParseList();

                    case TOKEN.STRING:
                        return ParseString();

                    case TOKEN.NUMBER:
                        return ParseNumber();

                    case TOKEN.TRUE:
                        return true;

                    case TOKEN.FALSE:
                        return false;

                    case TOKEN.NULL:
                        return null;
                }
                return null;
            }

            private object ParseNumber()
            {
                string nextWord = NextWord;
                // 判定是浮点数还是整数
                if (nextWord.IndexOf('.') == -1)
                {
                    // int
                    {
                        int intNum = 0;
                        if (int.TryParse(nextWord, out intNum)) return intNum;
                    }
                    // uint
                    {
                        uint uintNum = 0U;
                        if (uint.TryParse(nextWord, out uintNum)) return uintNum;
                    }
                    // long
                    {
                        long longNum = 0L;
                        if (long.TryParse(nextWord, out longNum)) return longNum;
                    }
                    // ulong
                    {
                        ulong ulongNum = 0UL;
                        if (ulong.TryParse(nextWord, out ulongNum)) return ulongNum;
                    }
                    return 0;
                }
                else
                {
                    // float
                    {
                        float floatNum = 0.0F;
                        if (float.TryParse(nextWord, out floatNum)) return floatNum;
                    }
                    // double
                    {
                        double doubleNum = 0.0D;
                        if (double.TryParse(nextWord, out doubleNum)) return doubleNum;
                    }
                    // dicimal
                    {
                        decimal decimalNum = 0M;
                        if (decimal.TryParse(nextWord, out decimalNum)) return decimalNum;
                    }
                    return 0f;
                }
            }

            private Dictionary<string, object> ParseObject()
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                jsonReader.Read();
                while (true)
                {
                    Thread.Sleep(0);
                    TOKEN nextToken = NextToken;
                    switch (nextToken)
                    {
                        case TOKEN.NONE:
                            return null;

                        case TOKEN.CURLY_CLOSE:
                            return dictionary;
                    }
                    if (nextToken != TOKEN.COMMA)
                    {
                        string str = ParseString();
                        if (str == null)
                        {
                            return null;
                        }
                        if (NextToken != TOKEN.COLON)
                        {
                            return null;
                        }
                        jsonReader.Read();
                        dictionary[str] = ParseValue();
                    }
                }
            }

            private string ParseString()
            {
                StringBuilder builder = new StringBuilder();
                jsonReader.Read();
                while (true)
                {
                    if (jsonReader.Peek() == -1) break;
                    char ch = NextChar;
                    // 字符串结尾符
                    if (ch == '"') break;
                    // 如果没有转义符，就直接添加
                    if (ch != '\\')
                    {
                        builder.Append(ch);
                        continue;
                    }
                    // 对转义符之后的内容进行处理
                    if (jsonReader.Peek() == -1) break;
                    ch = NextChar;
                    switch (ch)
                    {
                        case 'b':
                            builder.Append('\b');
                            break;
                        case 't':
                            builder.Append('\t');
                            break;
                        case 'n':
                            builder.Append('\n');
                            break;
                        case 'f':
                            builder.Append('\f');
                            break;
                        case 'r':
                            builder.Append('\r');
                            break;
                        case 'u':
                            {
                                // 读取之后的4个Unicode字符并还原
                                StringBuilder tmpBuilder = new StringBuilder();
                                int num = 0;
                                while (num < 4)
                                {
                                    tmpBuilder.Append(NextChar);
                                    num++;
                                }
                                builder.Append((char)Convert.ToInt32(tmpBuilder.ToString(), 0x10));
                                break;
                            }
                        default:
                            builder.Append(ch);
                            break;
                    }
                }
                return builder.ToString();
            }

            private object ParseValue()
            {
                TOKEN nextToken = NextToken;
                return ParseByToken(nextToken);
            }

            private TOKEN NextToken
            {
                get
                {
                    EatWhitespace();
                    if (jsonReader.Peek() != -1)
                    {
                        switch (PeekChar)
                        {
                            case '"':
                                return TOKEN.STRING;
                            case ',':
                                jsonReader.Read();
                                return TOKEN.COMMA;
                            case '-':
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                return TOKEN.NUMBER;
                            case ':':
                                return TOKEN.COLON;
                            case '[':
                                return TOKEN.SQUARED_OPEN;
                            case ']':
                                jsonReader.Read();
                                return TOKEN.SQUARED_CLOSE;
                            case '{':
                                return TOKEN.CURLY_OPEN;
                            case '}':
                                jsonReader.Read();
                                return TOKEN.CURLY_CLOSE;
                        }
                        string nextWord = NextWord;
                        if (nextWord != null)
                        {
                            if (switchMap == null)
                            {
                                // 序列化时的约定
                                Dictionary<string, int> dictionary = new Dictionary<string, int>(3);
                                dictionary.Add("false", 0);
                                dictionary.Add("true", 1);
                                dictionary.Add("null", 2);
                                switchMap = dictionary;
                            }
                            int num = 0;
                            if (switchMap.TryGetValue(nextWord, out num))
                            {
                                switch (num)
                                {
                                    case 0:
                                        return TOKEN.FALSE;
                                    case 1:
                                        return TOKEN.TRUE;
                                    case 2:
                                        return TOKEN.NULL;
                                }
                            }
                        }
                    }
                    return TOKEN.NONE;
                }
            }

            /// <summary>
            /// 跳过WHITE_SPACE中指定的字符，不进行处理
            /// </summary>
            private void EatWhitespace()
            {
                while (WHITE_SPACE.IndexOf(PeekChar) != -1)
                {
                    jsonReader.Read();
                    if (jsonReader.Peek() == -1) break;
                }
            }

            private string NextWord
            {
                get
                {
                    StringBuilder builder = new StringBuilder();
                    while (WORD_BREAK.IndexOf(PeekChar) == -1)
                    {
                        builder.Append(NextChar);
                        if (jsonReader.Peek() == -1)
                        {
                            break;
                        }
                    }
                    return builder.ToString();
                }
            }

            /// <summary>
            /// 获取当前游标处的字符
            /// </summary>
            private char PeekChar
            {
                get
                {
                    return Convert.ToChar(jsonReader.Peek());
                }
            }

            /// <summary>
            /// 获得下一处的字符
            /// </summary>
            private char NextChar
            {
                get
                {
                    return Convert.ToChar(jsonReader.Read());
                }
            }

            private enum TOKEN
            {
                NONE,
                /// <summary>
                /// {
                /// </summary>
                CURLY_OPEN,
                /// <summary>
                /// }
                /// </summary>
                CURLY_CLOSE,
                /// <summary>
                /// [
                /// </summary>
                SQUARED_OPEN,
                /// <summary>
                /// ]
                /// </summary>
                SQUARED_CLOSE,
                /// <summary>
                /// :
                /// </summary>
                COLON,
                /// <summary>
                /// ,
                /// </summary>
                COMMA,
                STRING,
                NUMBER,
                TRUE,
                FALSE,
                NULL
            }
        }

        /// <summary>
        /// 序列化器，负责将对象序列化成JSon字符串
        /// <para>空对象 -> null</para>
        /// <para>string|char -> "string"</para>
        /// <para>bool -> true|false</para>
        /// <para>enum,int,uint,long,byte,sbyte,short,ushort,long,ulong,float,double,decimal -> 值</para>
        /// </summary>
        private sealed class Serializer
        {
            private StringBuilder builder = new StringBuilder();

            private Serializer() { }

            public static string Serialize(object obj)
            {
                Json.Serializer serializer = new Json.Serializer();
                serializer.SerializeValue(obj);
                return serializer.builder.ToString();
            }

            private void SerializeValue(object value)
            {
                if (value == null)
                {
                    builder.Append("null");
                    return;
                }
                if (value is string)
                {
                    // 优先判断是否为字符串
                    SerializeString(value as string);
                }
                else if (value is char)
                {
                    SerializeString(value.ToString());
                }
                else if (value is bool)
                {
                    // 为什么不转成0和1来表达呢？
                    builder.Append(value.ToString().ToLower());
                }
                else if (value is IList)
                {
                    IList list = value as IList;
                    SerializeList(list);
                }
                else if (value is IDictionary)
                {
                    IDictionary dictionary = value as IDictionary;
                    SerializeDic(dictionary);
                }
                else if (value is Enum)
                {
                    // 枚举值特殊处理
                    object obj = Convert.ChangeType(Enum.Parse(value.GetType(), value.ToString()), Enum.GetUnderlyingType(value.GetType()));
                    builder.Append(obj);
                }
                else
                {
                    SerializeOther(value);
                }
            }

            private void SerializeList(IList anArray)
            {
                builder.Append('[');
                int index = anArray.Count - 1;
                // 遍历集合，对集合中的元素进行序列化
                AotSafe.ForEach<object>(anArray, (object obj) =>
                {
                    SerializeValue(obj);
                    // 最后一个不给,
                    if (index-- > 0) builder.Append(',');
                });
                builder.Append(']');
            }

            private void SerializeDic(IDictionary obj)
            {
                bool first = true;
                builder.Append('{');
                IEnumerator enumerator = obj.Keys.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        object key = enumerator.Current;
                        if (!first) builder.Append(',');
                        // 暂不支持复杂的Key类型，目前只支持简单类型
                        SerializeString(key.ToString());
                        builder.Append(':');
                        SerializeValue(obj[key]);
                        first = false;
                    }
                }
                finally
                {
                    IDisposable disposable = enumerator as IDisposable;
                    if (disposable != null) disposable.Dispose();
                }
                builder.Append('}');
            }

            private void SerializeOther(object value)
            {
                if ((((value is float) || (value is int)) || ((value is uint) || (value is long))) || ((((value is double) || (value is sbyte)) || ((value is byte) || (value is short))) || (((value is ushort) || (value is ulong)) || (value is decimal))))
                {
                    builder.Append(value.ToString());
                }
                else
                {
                    SerializeClass(value);
                }
            }

            private void SerializeClass(object value)
            {
                bool first = true;
                builder.Append('{');
                // 只获取public的Static或Instance成员
                foreach (FieldInfo info in value.GetType().GetFields())
                {
                    if (!first) builder.Append(',');
                    SerializeString(info.Name);//不管如何，都把Key翻译成string的Json字符串
                    builder.Append(':');
                    SerializeValue(info.GetValue(value));
                    first = false;
                }
                builder.Append('}');
            }

            private void SerializeString(string str)
            {
                builder.Append('"');
                char[] arr = str.ToCharArray();//逐字符处理
                for (int i = 0, len = arr.Length; i < len; i++)
                {
                    char ch = arr[i];
                    switch (ch)
                    {
                        case '\b':
                            builder.Append(@"\b");
                            break;
                        case '\t':
                            builder.Append(@"\t");
                            break;
                        case '\n':
                            builder.Append(@"\n");
                            break;
                        case '\f':
                            builder.Append(@"\f");
                            break;
                        case '\r':
                            builder.Append(@"\r");
                            break;
                        case '\\':
                            builder.Append(@"\\");
                            break;
                        case '"':
                            // 不能直接给builder.Append('"');，否则无法解析
                            builder.Append("\\\"");
                            break;
                        default:
                            int num = Convert.ToInt32(ch);
                            // ASCII是7位的单字节编码，其中0x20-0x7E的可见字符
                            if (num >= 0x20 && num <= 0x7e)
                            {
                                builder.Append(ch);
                            }
                            else
                            {
                                // 转成Unicode码
                                builder.Append(@"\u" + Convert.ToString(num, 0x10).PadLeft(4, '0'));
                            }
                            break;
                    }
                }
                builder.Append('"');
            }
        }
    }
}

