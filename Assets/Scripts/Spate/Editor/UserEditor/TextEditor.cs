using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Spate;
using System.Text.RegularExpressions;

public class TextEditor : BaseEditorWindow
{

    public enum TextStyle
    {
        Normal,
        /// <summary>
        /// 粗体[b][/b]
        /// </summary>
        Bold,
        /// <summary>
        /// 斜体[i][/i]
        /// </summary>
        Italic,
        /// <summary>
        /// 下划线[u][/u]
        /// </summary>
        Underline,
        /// <summary>
        /// 删除线[s][/s]
        /// </summary>
        Strikethrough,
        /// <summary>
        ///  让文字左下角显示[sub][/sub]
        /// </summary>
        Sub,
        /// <summary>
        ///  让文字左上角显示[sup][/sup] 
        /// </summary>
        Sup,
        /// <summary>
        /// 超链接 [url=http://www.tasharen.com/][u]clickable hyperlinks[/u][/url]
        /// </summary>
        Hyperlink,
        /// <summary>
        ///   颜色
        /// </summary>
        Color,
    }

    public enum Select
    {
        Key,
        Text,
    }
    public string[] savedCopies = new string[5];


    //Dictionary<string, LocalizeCsvData> dic_LanData;
    ////查找文本的内容
    //private string mText = string.Empty;
    ////默认中文搜索
    //private Select mSelect = Select.Text;
    ////需要编辑的编辑文本
    //private UILabel curLabel;
    ////绑定给编辑文本的数据
    //private LocalizeCsvData curLanguage;
    ////无Label文本时需要编辑数据
    //private LocalizeCsvData editLanguage;
    ////是否第一次编辑
    //bool IsFristEdit = false;
    ////是否第一次
    //bool isFrist = true;
    ////key的合法性校验
    //Regex regKey = new Regex("^[a-zA-Z]+_[a-zA-Z0-9]+$");
    ////是否能使用
    //bool isUse = false;
    //bool isAdd = false;
    ////编辑文本内容
    //private string mDesc = string.Empty;
    //private string mKey = string.Empty;
    ////Button样式
    //GUILayoutOption[] btnOption = new GUILayoutOption[]
    //{
    //    GUILayout.Height(20),
    //    GUILayout.Width(70)
    //};
    ////是否点击过样式
    //private bool isClickStyle = false;
    ////是否失去焦点
    //private bool isMissFoucs = false;

    //public TextStyle mTextStyle = TextStyle.Normal;

    //public override string Name
    //{
    //    get
    //    {
    //        return "文本编辑";
    //    }
    //}

    //public override void OnCreate()
    //{
    //    // AssetManager.LoadCsv();
    //    //填装数据
    //    dic_LanData = DataManager.GetTable<string, LocalizeCsvData>();
    //}

    //public override void OnOpen()
    //{
    //    isFrist = true;
    //}

    //public override void OnHide()
    //{
    //    SaveData();
    //}

    //public override void OnGUI()
    //{
    //    if (dic_LanData == null || dic_LanData.Count == 0)
    //    {
    //        UnityEngine.Debug.Log("没有language.csv的任何数据.");
    //        return;
    //    }
    //    if (isFrist)
    //    {
    //        if (Selection.activeGameObject)
    //        {
    //            curLabel = Selection.activeGameObject.GetComponent<UILabel>();
    //            if (curLabel != null && curLabel.trueTypeFont != null)
    //            {
    //                //if (dic_LanData.ContainsKey(curLabel.languageCsvKey))
    //                //{
    //                //    editLanguage = dic_LanData[curLabel.languageCsvKey];
    //                //    IsFristEdit = true;
    //                //}
    //            }
    //        }
    //        isFrist = false;
    //    }
    //    GraphicFrame();
    //}

    //public void GraphicFrame()
    //{
    //    DrawSearchLine();
    //    DrawDataTable();
    //    DrawEditFrame();
    //}

    ///// <summary>
    ///// 绘制搜索栏
    ///// </summary>
    //private void DrawSearchLine()
    //{
    //    GUILayout.Space(10f);
    //    //设置界面的整体布局方式
    //    GUILayout.BeginHorizontal();
    //    //选者搜索类型
    //    GUILayout.Label("搜索:");
    //    Select select = (Select)EditorGUILayout.EnumPopup("", mSelect, GUILayout.Width(80f));
    //    GUILayout.Space(20f);
    //    //切换搜索类型之后重置搜索内容
    //    if (mSelect != select)
    //        mText = string.Empty;
    //    mSelect = select;
    //    //绘制文本搜索框
    //    mText = EditorGUILayout.TextArea(mText, GUILayout.Width(455));
    //    GUILayout.Space(20f);
    //    string str = string.Empty;
    //    if (mSelect == Select.Key)
    //    {
    //        //如果没有搜索到任何内容增加添加按钮
    //        if (isAdd)
    //        {
    //            if (regKey.IsMatch(mText) && dic_LanData.ContainsKey(mKey.Trim()))
    //            {
    //                if (GUILayout.Button("添加"))
    //                    AddTableData();
    //            }
    //            else
    //            {
    //                str = "请输入合法字符";
    //            }
    //        }
    //        else
    //        {
    //            str = "请输入匹配的Key完成搜索";
    //        }
    //    }
    //    else
    //        str = "请输入匹配的文本内容完成搜索";
    //    GUILayout.Label(str);
    //    GUILayout.EndHorizontal();
    //}

    ///// <summary>
    ///// 绘制表格内容
    ///// </summary>
    //private void DrawDataTable()
    //{
    //    //绘制标题栏
    //    GUILayout.Space(5f);
    //    EditorGUILayout.BeginHorizontal();
    //    GUILayout.Space(5f);
    //    EditorGUILayout.LabelField("key", GUILayout.Width(70));
    //    GUILayout.Space(5f);
    //    EditorGUILayout.LabelField("Text", GUILayout.Width(500));
    //    EditorGUILayout.LabelField("");
    //    EditorGUILayout.EndHorizontal();
    //    //
    //    GUILayout.Space(5f);
    //    EditorGUILayout.BeginVertical(GUILayout.MaxHeight(200));
    //    isAdd = true;
    //    if (dic_LanData != null && dic_LanData.Count > 0)
    //    {
    //        foreach (LocalizeCsvData csv in dic_LanData.Values)
    //        {
    //            string sel = string.Empty;
    //            if (mSelect == Select.Key)
    //                sel = csv.key;
    //            else
    //                sel = csv.text;
    //            if (sel.Trim().Contains(mText.Trim()))
    //            {
    //                DrawTableLine(csv);
    //                isAdd = false;
    //            }
    //        }
    //    }
    //    EditorGUILayout.EndVertical();
    //}

    ///// <summary>
    ///// 绘制编辑框
    ///// </summary>
    //private void DrawEditFrame()
    //{
    //    if (editLanguage != null && !isAdd)
    //    {
    //        GUILayout.Space(20);
    //        GUILayout.BeginHorizontal();

    //        GUILayout.Label("主键:  ", GUILayout.Width(50));
    //        if (IsFristEdit)
    //        {
    //            mKey = editLanguage.key;
    //        }
    //        GUI.changed = false;
    //        string key = EditorGUILayout.TextArea(mKey, GUILayout.Width(90));
    //        if (GUI.changed)
    //            mKey = key;

    //        GUILayout.Label("选择样式:", GUILayout.Width(60));

    //        GUI.backgroundColor = Color.white;
    //        //mTextStyle = (TextStyle)EditorGUILayout.EnumPopup(mTextStyle, GUILayout.Width(510));
    //        //
    //        if (GUILayout.Button("粗体", btnOption))
    //        {
    //            mTextStyle = TextStyle.Bold;
    //            isClickStyle = true;
    //        }
    //        if (GUILayout.Button("斜体", btnOption))
    //        {
    //            mTextStyle = TextStyle.Italic;
    //            isClickStyle = true;
    //        }
    //        if (GUILayout.Button("删除线", btnOption))
    //        {
    //            mTextStyle = TextStyle.Strikethrough;
    //            isClickStyle = true;
    //        }
    //        if (GUILayout.Button("下划线", btnOption))
    //        {
    //            mTextStyle = TextStyle.Underline;
    //            isClickStyle = true;
    //        }
    //        if (GUILayout.Button("左下角", btnOption))
    //        {
    //            mTextStyle = TextStyle.Sub;
    //            isClickStyle = true;
    //        }
    //        if (GUILayout.Button("左上角", btnOption))
    //        {
    //            mTextStyle = TextStyle.Sup;
    //            isClickStyle = true;
    //        }
    //        if (GUILayout.Button("超链接", btnOption))
    //        {
    //            mTextStyle = TextStyle.Hyperlink;
    //            isClickStyle = true;
    //        }
    //        if (GUILayout.Button("颜色", btnOption))
    //        {
    //            mTextStyle = TextStyle.Color;
    //            isClickStyle = true;
    //        }
    //        GUILayout.EndHorizontal();
    //        GUILayout.Space(8);
    //        GUILayout.BeginHorizontal();
    //        GUILayout.Label("内容：", GUILayout.Width(50f));
    //        GUI.backgroundColor = Color.white;
    //        GUILayoutOption[] option = new GUILayoutOption[]
    //    {
    //        GUILayout.MinHeight(20),
    //        GUILayout.MaxHeight(100),
    //        GUILayout.Width(745)
    //    };
    //        //如果是刚刚开始
    //        if (IsFristEdit)
    //        {
    //            mDesc = editLanguage.text;
    //            IsFristEdit = false;
    //        }
    //        if (isClickStyle)
    //        {
    //            mDesc += InsertEffect();
    //            //失去焦点
    //            GUI.FocusControl(mDesc);
    //            isClickStyle = false;
    //        }
    //        if (isMissFoucs)
    //        {
    //            GUI.FocusControl(mDesc);
    //            isMissFoucs = false;
    //        }

    //        string text = EditorGUILayout.TextArea(mDesc, option);
    //        if (text != mDesc)
    //        {
    //            mDesc = text;
    //        }
    //        //Debug.Log("keyCode" + Event.current.keyCode + "||character = " + Event.current.character);

    //        GUILayout.EndHorizontal();
    //        GUILayout.Space(20);
    //        GUILayout.BeginHorizontal();
    //        GUILayout.Label("", GUILayout.Width(200));

    //        if ((dic_LanData[editLanguage.key].key != mKey.Trim()
    //             && !dic_LanData.ContainsKey(mKey.Trim())
    //             && regKey.IsMatch(mKey.Trim()))
    //            || dic_LanData[editLanguage.key].text != mDesc.Trim())
    //        {
    //            if (GUILayout.Button("保存", btnOption))
    //            {
    //                dic_LanData[editLanguage.key].key = mKey.Trim();
    //                dic_LanData[editLanguage.key].text = mDesc.Trim();
    //                IsFristEdit = true;
    //                isMissFoucs = true;
    //            }
    //        }
    //        else
    //        {
    //            GUILayout.Label("", btnOption);
    //        }
    //        if (dic_LanData[editLanguage.key].key != mKey.Trim()
    //            || dic_LanData[editLanguage.key].text != mDesc.Trim())
    //        {
    //            GUILayout.Label("", GUILayout.Width(200));
    //            if (GUILayout.Button("取消", btnOption))
    //            {
    //                //editLanguage = null;
    //                //mText = string.Empty;
    //                mDesc = editLanguage.text;
    //                mKey = editLanguage.key;
    //                IsFristEdit = true;
    //                isMissFoucs = true;
    //            }
    //        }
    //        GUILayout.EndHorizontal();
    //    }
    //}

    ///// <summary>
    ///// 创建一行数据
    ///// </summary>
    //public void DrawTableLine(LocalizeCsvData data)
    //{
    //    EditorGUILayout.BeginHorizontal();
    //    GUI.backgroundColor = Color.white;
    //    //
    //    GUILayout.Space(5f);
    //    EditorGUILayout.LabelField(data.key, GUILayout.Width(70));
    //    GUILayout.Space(5f);
    //    EditorGUILayout.LabelField(data.text, GUILayout.Width(500f));
    //    if (curLabel != null)
    //    {
    //        if (GUILayout.Button("使用", btnOption))
    //        {
    //            editLanguage = data;
    //            //curLabel.languageCsvKey = editLanguage.key;
    //            IsFristEdit = true;
    //            isMissFoucs = true;
    //        }
    //    }
    //    if (GUILayout.Button("修改", btnOption))
    //    {
    //        mSelect = Select.Text;
    //        editLanguage = data;
    //        IsFristEdit = true;
    //        isMissFoucs = true;
    //    }
    //    if (GUILayout.Button("删除", btnOption))
    //    {
    //        if (dic_LanData[data.key] != null)
    //        {
    //            dic_LanData.Remove(data.key);
    //        }
    //        if (editLanguage != null)
    //        {
    //            if (!dic_LanData.ContainsKey(editLanguage.key))
    //            {
    //                editLanguage = null;
    //            }
    //        }
    //        isMissFoucs = true;
    //    }
    //    EditorGUILayout.EndHorizontal();

    //}

    ///// <summary>
    ///// 添加数据
    ///// </summary>
    //public void AddTableData()
    //{
    //    string content = mText;

    //    LocalizeCsvData newLanguage = new LocalizeCsvData();
    //    newLanguage.key = mText;
    //    newLanguage.text = string.Empty;
    //    //添加数据
    //    dic_LanData.Add(newLanguage.key, newLanguage);
    //    editLanguage = newLanguage;
    //    mText = string.Empty;
    //    //进入编辑文本状态
    //    IsFristEdit = true;
    //}

    ///// <summary>
    ///// 插入效果  
    ///// </summary>
    //public string InsertEffect()
    //{
    //    string str = string.Empty;
    //    switch (mTextStyle)
    //    {
    //        case TextStyle.Normal:
    //            break;
    //        case TextStyle.Bold:
    //            str += "[b]";
    //            str += "[/b]";
    //            break;
    //        case TextStyle.Italic:  //[b][i][/b][/i][s][/u] [sub][/sup] [/url] [url]
    //            str += "[i]";
    //            str += "[/i]";
    //            break;
    //        case TextStyle.Strikethrough:
    //            str += "[s]";
    //            str += "[/s]";
    //            break;
    //        case TextStyle.Underline:
    //            str += "[u]";
    //            str += "[/u]";
    //            break;
    //        case TextStyle.Hyperlink:
    //            str += "[url= [ip Address]][u]";
    //            str += "[/u][/url]";
    //            break;
    //        case TextStyle.Sub:
    //            str += "[sub]";
    //            str += "[/sub]";
    //            break;
    //        case TextStyle.Sup:
    //            str += "[sup]";
    //            str += "[/sup]";
    //            break;
    //        case TextStyle.Color:
    //            break;
    //    }
    //    return str;
    //}

    //public void SaveData()
    //{
    //    DataManager.SaveCsvData<LocalizeCsvData>();
    //}

    //public override void OnDestroy()
    //{
    //    SaveData();
    //    mText = string.Empty;
    //    curLabel = null;
    //    editLanguage = null;
    //}

    public override string Name
    {
        get { throw new System.NotImplementedException(); }
    }

    public override void OnCreate()
    {
        throw new System.NotImplementedException();
    }

    public override void OnGUI()
    {
        throw new System.NotImplementedException();
    }

    public override void OnDestroy()
    {
        throw new System.NotImplementedException();
    }
}

