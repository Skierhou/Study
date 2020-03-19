using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEditor;
using System;
using System.IO;
using System.Xml;
using OfficeOpenXml;
using System.ComponentModel;

public class DataEditor{

    private static string XMLPATH = EditorConfig.GetEditorConfig().XmlPath;             //配置Xml文件根目录
    private static string BINARYPATH = EditorConfig.GetEditorConfig().BinaryPath;       //配置二进制文件根目录
    private static string SCRIPTPATH = EditorConfig.GetEditorConfig().ScriptPath;       //配置类文件根目录
    private static string REGXMLPATH = Application.dataPath + "/../Data/Reg/";          //xml转Excel的中间xml配置表
    private static string EXCELPATH = Application.dataPath + "/../Data/Excel/";         //Excel根目录

    #region 二进制与Xml互相转换
    [MenuItem("Assets/配置表转换/类转Xml")]
    public static void AssetsClassToXml()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayCancelableProgressBar("文件类转成Xml", "正在扫描:" + objs[i].name, i * 1.0f / objs.Length);
            ClassToXml(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

    }

    [MenuItem("Assets/配置表转换/Xml转Binary")]
    public static void AssetsXmlToBinary()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayCancelableProgressBar("Xml文件转成Binary文件", "正在扫描:" + objs[i].name, i * 1.0f / objs.Length);
            XmlToBinary(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    [MenuItem("Tools/配置表转换/类转Xml")]
    public static void ToolsClassToXml()
    {
        string path = Application.dataPath.Replace("Assets", "") + SCRIPTPATH;
        string[] fileNames = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        for (int i = 0; i < fileNames.Length; i++)
        {
            EditorUtility.DisplayProgressBar("查找路径：" + path + "下的Xml文件", "正在扫描" + fileNames[i] + "... ...", i * 1.0f / fileNames.Length);
            if (fileNames[i].EndsWith(".cs"))
            {
                string name = fileNames[i].Substring(fileNames[i].LastIndexOf("/") + 1).Replace(".cs", "");
                ClassToXml(name);
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    [MenuItem("Tools/配置表转换/Xml转Binary")]
    public static void ToolsXmlToBinary()
    {
        string path = Application.dataPath.Replace("Assets", "") + XMLPATH;
        string[] fileNames = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        for (int i = 0; i < fileNames.Length; i++)
        {
            EditorUtility.DisplayProgressBar("查找路径：" + path + "下的Xml文件", "正在扫描" + fileNames[i] + "... ...", i * 1.0f / fileNames.Length);
            if (fileNames[i].EndsWith(".xml"))
            {
                string name = fileNames[i].Substring(fileNames[i].LastIndexOf("/")+1).Replace(".xml","");
                XmlToBinary(name);
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    /// <summary>
    /// Xml转二进制实现
    /// </summary>
    private static void XmlToBinary(string name)
    {
        if (string.IsNullOrEmpty(name)) return;
        try
        {
            Type type = GetType(name);
            if (type != null)
            {
                string xmlPath = XMLPATH + name + ".xml";
                string binaryPath = BINARYPATH + name + ".bytes";
                System.Object obj = Serialize.XmlDeSerializeEditor(xmlPath, type);
                Serialize.BinarySerialize(binaryPath, obj);
                Debug.Log("Xml转二进制成功，路径为：" + binaryPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(name + " Xml转二进制出错，" + e);
        }
    }
    /// <summary>
    /// 实际类转XML
    /// </summary>
    private static void ClassToXml(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        try
        {
            Type type = GetType(name);
            if (type != null)
            {
                string xmlPath = XMLPATH + name + ".xml";
                var obj = Activator.CreateInstance(type);
                if (obj is BaseData)
                {
                    ((BaseData)obj).Construction();
                }
                Serialize.XmlSerialize(xmlPath, obj);
                Debug.Log("类转XML成功，路径为：" + xmlPath);
            }
        }
        catch(Exception e)
        {
            Debug.LogError(name+" 类转Xml出错，" +e);
        }
    }
    /// <summary>
    /// 获取类型
    /// </summary>
    public static Type GetType(string name)
    {
        Type type = null;
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();   //获取当前所有运行的程序集
        foreach (Assembly temp in assemblies)
        {
            //遍历每个程序集，尝试获取是否有name对应的类
            Type tempType = temp.GetType(name);
            if (tempType != null && tempType != tempType.BaseType)
            {
                type = tempType;
                break;
            }
        }
        return type;
    }

    #endregion

    #region 生成xml配置文件（用于转成excel的中间xml）
    
    //[MenuItem("Tools/配置表转换/生成xml转Excel的配置文件")]
    public static void ToolsCreateRegXml()
    {
        string path = Application.dataPath.Replace("Assets", "") + XMLPATH;
        string[] fileNames = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        for (int i = 0; i < fileNames.Length; i++)
        {
            EditorUtility.DisplayProgressBar("查找路径：" + path + "下的Xml文件", "正在扫描" + fileNames[i] + "... ...", i * 1.0f / fileNames.Length);
            if (fileNames[i].EndsWith(".xml"))
            {
                string name = fileNames[i].Substring(fileNames[i].LastIndexOf("/") + 1).Replace(".xml", "");
                CreateRegXmlFile(name);
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    [MenuItem("Assets/配置表转换/生成Xml转Excel的配置文件")]
    public static void AssetsCreateRegXml()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayCancelableProgressBar("文件类转成Xml", "正在扫描:" + objs[i].name, i * 1.0f / objs.Length);
            CreateRegXmlFile(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

    }
    /// <summary> 
    /// 创建Reg.xml的配置文件（用于excel配置）
    /// </summary> 
    private static void CreateRegXmlFile(string name)
    {
        XmlDocument xmldoc = new XmlDocument();
        XmlNode node;
        node = xmldoc.CreateXmlDeclaration("1.0", "utf-8", null);
        xmldoc.AppendChild(node);
        XmlElement root = xmldoc.CreateElement("data");
        xmldoc.AppendChild(root);
        root.SetAttribute("name", name);
        root.SetAttribute("from", name + ".xlsx");
        root.SetAttribute("to", name + ".xml");
        WriteToXmlElement(xmldoc, root, name);

        string path = REGXMLPATH + name + ".xml";
        try
        {
            if (File.Exists(path)) File.Delete(path);
            xmldoc.Save(path);
        }
        catch (Exception e)
        {
            Debug.LogError("无法生成reg.xml配置文件，路径为：" + path + "，" + e);
            return;
        }
        Debug.Log("成功生成配置文件："+path);
    }
    /// <summary>
    /// 循环写入xml元素
    /// </summary>
    /// <param name="xmldoc"></param>
    /// <param name="root"></param>
    /// <param name="name"></param>
    private static void WriteToXmlElement(XmlDocument xmldoc,XmlElement root,string name)
    {
        try
        {
            Type type = GetType(name);
            if (type != null)
            {
                System.Object obj = null;
                if (type == typeof(System.String))
                {
                    obj = "";
                }
                else
                {
                    obj = Activator.CreateInstance(type);
                }
                if (obj is BaseData)
                {
                    BaseData data = (BaseData)obj;
                    PropertyInfo[] infos = data.AllAttributes();
                    
                    foreach (PropertyInfo info in infos)
                    {
                        string propertyStr = info.PropertyType.ToString();
                        string listStr = typeof(List<>).ToString();

                        if (propertyStr.Contains("List") && !IsStandardList(propertyStr))
                        {
                            if (propertyStr.Substring(0, propertyStr.LastIndexOf('[')) == listStr.Substring(0, listStr.LastIndexOf('[')))
                            {
                                XmlElement tempElement = xmldoc.CreateElement("variable");
                                tempElement.SetAttribute("name", info.Name);
                                tempElement.SetAttribute("type", StandardType(info.PropertyType));
                                //tempElement.SetAttribute("defultvalue", "");
                                //tempElement.SetAttribute("foreign", "");
                                //tempElement.SetAttribute("split", "");

                                string typeName = propertyStr.Substring(propertyStr.LastIndexOf('[') + 1).Replace("]", "");
                                XmlElement ele = xmldoc.CreateElement("list");
                                ele.SetAttribute("name",typeName);
                                ele.SetAttribute("sheetname",typeName);
                                ele.SetAttribute("mainkey", "Id");
                                WriteToXmlElement(xmldoc, ele, typeName);
                                tempElement.AppendChild(ele);
                                root.AppendChild(tempElement);
                            }
                        }
                        else
                        {
                            XmlElement ele = xmldoc.CreateElement("variable");
                            ele.SetAttribute("name",info.Name);
                            ele.SetAttribute("col", info.Name);
                            ele.SetAttribute("type", StandardType(info.PropertyType));
                            //ele.SetAttribute("defultvalue", "");
                            //ele.SetAttribute("foreign", "");
                            if (IsStandardList(propertyStr))
                            {
                                ele.SetAttribute("split", ";");
                                string str = "list"+ propertyStr.Substring(propertyStr.LastIndexOf('['));
                                ele.SetAttribute("type", str);
                            }
                            root.AppendChild(ele);
                        }
                    }
                }
            }
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }
    private static bool IsStandardList(string typeStr)
    {
        if (typeStr.Contains(typeof(List<>).ToString().Substring(0, typeof(List<>).ToString().LastIndexOf('['))))
        {
            string str = typeStr.Substring(typeStr.LastIndexOf('[') + 1).Replace("]", "");
            if (str == "System.String" || str == "System.Single" || str == "System.Boolean" || str == "System.Int32")
            {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 标准化类型
    /// </summary>
    private static string StandardType(Type type)
    {
        string str = type.ToString();
        if (type == typeof(System.Int32))
        {
            str = "int";
        }
        else if (type == typeof(System.String))
        {
            str = "string";
        }
        else if (type == typeof(System.Single))
        {
            str = "float";
        }
        else if (type == typeof(System.Boolean))
        {
            str = "bool";
        }
        else if (type == typeof(System.Enum))
        {
            str = "enum";
        }
        else if (type.ToString().Contains("List"))
        {
            str = "list";
        }
        return str;
    }

    private static void SaveReflectionValue(PropertyInfo propertyInfo, object obj, string value, string type)
    {
        object val = (object)value;
        switch (type)
        {
            case "int":
                val = System.Convert.ToInt32(value);
                break;
            case "bool":
                val = System.Convert.ToBoolean(value);
                break;
            case "enum":
                val = TypeDescriptor.GetConverter(propertyInfo.PropertyType).ConvertFromInvariantString(value);
                break;
            case "float":
                val = System.Convert.ToSingle(value);
                break;
            default:
                break;
        }
        propertyInfo.SetValue(obj, val);
    }



    #endregion

    #region 由xml配置文件生成Excel
    [MenuItem("Tools/配置表转换/Xml转成Excel")]
    public static void ToolsCreateExcel()
    {
        string path = Application.dataPath.Replace("/Assets", "/Data/Reg/");
        string[] fileNames = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

        for (int i = 0; i < fileNames.Length; i++)
        {
            EditorUtility.DisplayProgressBar("查找路径：" + path + "下的Xml文件", "正在扫描" + fileNames[i] + "... ...", i * 1.0f / fileNames.Length);
            if (fileNames[i].EndsWith(".xml"))
            {
                string name = fileNames[i].Substring(fileNames[i].LastIndexOf("/") + 1).Replace(".xml", "");
                ReadRegXmlToExcel(name);
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    [MenuItem("Assets/配置表转换/Xml转Excel")]
    public static void AssetsXmlToExcel()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayCancelableProgressBar("文件类转成Xml", "正在扫描:" + objs[i].name, i * 1.0f / objs.Length);
            ReadRegXmlToExcel(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

    }
    /// <summary>
    /// 读取xml转excel的配置表
    /// </summary>
    private static void ReadRegXmlToExcel(string className)
    {
        string path = REGXMLPATH + className + ".xml";
        Dictionary<string, SheetClass> allSheetDict = ReadRegXml(path);
        Dictionary<string, SheetData> sheetDataDict = new Dictionary<string, SheetData>();

        object data = null;
        Type type = GetType(className);
        if (type != null)
        {
            data = Serialize.XmlDeSerializeEditor(XMLPATH + className + ".xml", type);
        }

        List<SheetClass> outSheetList = new List<SheetClass>();
        foreach (string key in allSheetDict.Keys)
        {
            if (allSheetDict[key].Depth == 1)
            {
                outSheetList.Add(allSheetDict[key]);
            }
        }
        for (int i = 0; i < outSheetList.Count; i++)
        {
            ReadSheetData(data, outSheetList[i], allSheetDict, sheetDataDict, string.Empty);
        }
        string excelPath = EXCELPATH + className + ".xlsx";

        try
        {
            if (IsFileUsed(excelPath))
            {
                Debug.LogError(excelPath + "文件正在使用，不能进行更改！");
                return;
            }
            using (FileStream fs = new FileStream(excelPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (ExcelPackage package = new ExcelPackage(fs))
                {
                    foreach (string str in sheetDataDict.Keys)
                    {
                        SheetData sheetData = sheetDataDict[str];
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(str);
                        worksheet.Cells.AutoFitColumns();
                        for (int i = 0; i < sheetData.AllName.Count; i++)
                        {
                            ExcelRange range = worksheet.Cells[1, i + 1];
                            range.Value = sheetData.AllName[i];
                            range.AutoFitColumns();
                        }
                        for (int i = 0; i < sheetData.AllRowData.Count; i++)
                        {
                            RowData rowData = sheetData.AllRowData[i];
                            for (int j = 0; j < rowData.RowDataDict.Count; j++)
                            {
                                ExcelRange range = worksheet.Cells[i + 2, j + 1];
                                range.Value = rowData.RowDataDict[sheetData.AllName[j]];
                                range.AutoFitColumns();
                            }
                        }
                    }
                    package.Save();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }
        Debug.Log("生成Excel成功：" + excelPath);
    }
    /// <summary>
    /// 读取xml配置文件
    /// </summary>
    private static Dictionary<string, SheetClass> ReadRegXml(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("该路径：" + path + "下不存在文件!");
            return null;
        }
        XmlReader reader = null;
        Dictionary<string, SheetClass> allSheetDict = new Dictionary<string, SheetClass>();
        try
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;   //忽略xml注释
            XmlDocument xmldoc = new XmlDocument();
            reader = XmlReader.Create(path, settings);
            xmldoc.Load(reader);
            XmlNode node = xmldoc.SelectSingleNode("data");
            XmlElement element = node as XmlElement;
            string name = element.GetAttribute("name");
            string from = element.GetAttribute("from");
            string to = element.GetAttribute("to");

            ReadXmlNode(element, allSheetDict, 0);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            if (reader != null)
            {
                reader.Close();
            }
        }
        return allSheetDict;
    }
    private static bool IsFileUsed(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 读取xml中所有表数据
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sheetClass"></param>
    /// <param name="allSheetDict"></param>
    /// <param name="sheetDataDict"></param>
    private static void ReadSheetData(object data, SheetClass sheetClass, Dictionary<string, SheetClass> allSheetDict, Dictionary<string, SheetData> sheetDataDict,string mainKey)
    {
        List<VarClass> varList = sheetClass.VarList;
        object dataList = GetMemberValue(data, sheetClass.ParentVar.Name);

        int listCount = System.Convert.ToInt32(dataList.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { }));

        SheetData sheetData = new SheetData();

        if (!string.IsNullOrEmpty(sheetClass.ParentVar.Foreign))
        {
            sheetData.AllName.Add(sheetClass.ParentVar.Foreign);
            sheetData.AllType.Add(sheetClass.ParentVar.Type);
        }

        for (int i = 0; i < varList.Count; i++)
        {
            if (!string.IsNullOrEmpty(varList[i].Col))
            {
                sheetData.AllName.Add(varList[i].Col);
                sheetData.AllType.Add(varList[i].Type);
            }
        }

        string tempKey = mainKey;
        for (int i = 0; i < listCount; i++)
        {
            object item = dataList.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { i });

            RowData rowData = new RowData();

            if (!string.IsNullOrEmpty(sheetClass.ParentVar.Foreign) && !string.IsNullOrEmpty(tempKey))
            {
                rowData.RowDataDict.Add(sheetClass.ParentVar.Foreign, tempKey);
            }
            if (!string.IsNullOrEmpty(sheetClass.MainKey))
            {
                mainKey = GetMemberValue(item, sheetClass.MainKey).ToString();
            }

            for (int j = 0; j < varList.Count; j++)
            {
                if (varList[j].Type == "list")
                {
                    SheetClass tempSheet = allSheetDict[varList[j].ListSheetName];
                    ReadSheetData(item, tempSheet, allSheetDict, sheetDataDict, mainKey);
                }
                else if (varList[j].Type.Contains("list") && varList[j].Type != "list")
                {
                    string value = GetListBaseStr(item, varList[j]);
                    rowData.RowDataDict.Add(varList[j].Col, value);
                }
                else
                {
                    object value = GetMemberValue(item, varList[j].Name);
                    if (value != null)
                    {
                        rowData.RowDataDict.Add(varList[j].Col, value.ToString());
                    }
                    else
                    {
                        Debug.LogError(varList[j].Name + "反射出来为空，请查看配置表！");
                    }
                }
            }
            if (sheetDataDict.ContainsKey(sheetClass.SheetName))
            {
                sheetDataDict[sheetClass.SheetName].AllRowData.Add(rowData);
            }
            else
            {
                sheetData.AllRowData.Add(rowData);
                sheetDataDict.Add(sheetClass.SheetName, sheetData);
            }
        }
    }
    /// <summary>
    /// 读取xml配置表中所有节点
    /// </summary>
    /// <param name="xmlElement"></param>
    /// <param name="allSheetDict"></param>
    /// <param name="depth"></param>
    private static void ReadXmlNode(XmlElement xmlElement, Dictionary<string, SheetClass> allSheetDict, int depth)
    {
        depth++;
        foreach (XmlNode tempNode in xmlElement.ChildNodes)
        {
            XmlElement tempElement = tempNode as XmlElement;
            string type = tempElement.GetAttribute("type");
            if (type == "list")
            {
                XmlElement listEle = tempElement.FirstChild as XmlElement;
                VarClass parentVar = new VarClass
                {
                    Name = tempElement.GetAttribute("name"),
                    Type = tempElement.GetAttribute("type"),
                    Col = tempElement.GetAttribute("col"),
                    DeafultValue = tempElement.GetAttribute("defultvalue"),
                    Foreign = tempElement.GetAttribute("foreign"),
                    SplitStr = tempElement.GetAttribute("split"),
                };
                if (parentVar.Type == "list")
                {
                    parentVar.ListName = listEle.GetAttribute("name");
                    parentVar.ListSheetName = listEle.GetAttribute("sheetname");
                }
                SheetClass sheet = new SheetClass
                {
                    ParentVar = parentVar,
                    Name = listEle.GetAttribute("name"),
                    SheetName = listEle.GetAttribute("sheetname"),
                    MainKey = listEle.GetAttribute("mainkey"),
                    SplitStr = listEle.GetAttribute("split"),
                    Depth = depth,
                };

                if (!string.IsNullOrEmpty(sheet.SheetName))
                {
                    if (!allSheetDict.ContainsKey(sheet.SheetName))
                    {
                        sheet.VarList = new List<VarClass>();
                        foreach (XmlNode node in listEle.ChildNodes)
                        {
                            XmlElement xe = node as XmlElement;
                            VarClass varClass = new VarClass
                            {
                                Name = xe.GetAttribute("name"),
                                Type = xe.GetAttribute("type"),
                                Col = xe.GetAttribute("col"),
                                DeafultValue = xe.GetAttribute("defultvalue"),
                                Foreign = xe.GetAttribute("foreign"),
                                SplitStr = xe.GetAttribute("split"),
                            };
                            if (varClass.Type == "list")
                            {
                                varClass.ListName = ((XmlElement)xe.FirstChild).GetAttribute("name");
                                varClass.ListSheetName = ((XmlElement)xe.FirstChild).GetAttribute("sheetname");
                            }
                            sheet.VarList.Add(varClass);
                        }
                        allSheetDict.Add(sheet.SheetName, sheet);
                    }
                }
                ReadXmlNode(listEle, allSheetDict, depth);
            }
            else
            {

            }
        }
    }
    /// <summary>
    /// 获取基础List的输出字符串
    /// </summary>
    /// <param name="data"></param>
    /// <param name="varClass"></param>
    /// <returns></returns>
    private static string GetListBaseStr(object data,VarClass varClass)
    {
        string str = "";
        if (string.IsNullOrEmpty(varClass.SplitStr))
        {
            Debug.LogError("基础List中Split不允许为空！！！");
            return str;
        }
        object dataList = GetMemberValue(data,varClass.Name);
        int listCount = System.Convert.ToInt32(dataList.GetType().InvokeMember("get_Count",BindingFlags.Default|BindingFlags.InvokeMethod,null, dataList, new object[] { }));
        for (int i = 0; i < listCount; i++)
        {
            str += dataList.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { i }).ToString();
            if (i != listCount - 1)
            {
                str += varClass.SplitStr;
            }
        }
        return str;
    }

    /// <summary>
    /// 获取对象成员名对应的值
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="memberName">成员名</param>
    /// <param name="bindingFlags"></param>
    private static object GetMemberValue(object obj, string memberName, BindingFlags bindingFlags =
        BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
    {
        Type type = obj.GetType();
        MemberInfo[] memberInfos = type.GetMember(memberName, bindingFlags);
        switch (memberInfos[0].MemberType)
        {
            case MemberTypes.Field:
                return type.GetField(memberName, bindingFlags).GetValue(obj);
            case MemberTypes.Property:
                return type.GetProperty(memberName, bindingFlags).GetValue(obj);
        }
        return null;
    }
    #endregion

    #region Excel转成Xml
    [MenuItem("Tools/配置表转换/Excel转Xml")]
    public static void ToolsExcelToXml()
    {
        string[] fileNames = Directory.GetFiles(EXCELPATH, "*.*", SearchOption.AllDirectories);

        for (int i = 0; i < fileNames.Length; i++)
        {
            EditorUtility.DisplayProgressBar("查找路径：" + EXCELPATH + "下的Xml文件", "正在扫描" + fileNames[i] + "... ...", i * 1.0f / fileNames.Length);
            if (fileNames[i].EndsWith(".xlsx"))
            {
                string name = fileNames[i].Substring(fileNames[i].LastIndexOf("/") + 1).Replace(".xlsx", "");
                ExcelToXml(name);
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
   
    private static void ExcelToXml(string className)
    {
        string path = REGXMLPATH + className + ".xml";
        Dictionary<string, SheetClass> allSheetDict = ReadRegXml(path);

        string excelPath = EXCELPATH + className + ".xlsx";
        Dictionary<string, SheetData> sheetDataDict = new Dictionary<string, SheetData>();

        try
        {
            using (FileStream fs = new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (ExcelPackage package = new ExcelPackage(fs))
                {
                    ExcelWorksheets excelWorksheets = package.Workbook.Worksheets;
                    for (int i = 0; i < excelWorksheets.Count; i++)
                    {
                        ExcelWorksheet worksheet = excelWorksheets[i + 1];
                        SheetData sheetData = new SheetData();
                        SheetClass sheetClass = allSheetDict[worksheet.Name];

                        for (int j = 0; j < sheetClass.VarList.Count; j++)
                        {
                            sheetData.AllName.Add(sheetClass.VarList[j].Name);
                            sheetData.AllType.Add(sheetClass.VarList[j].Type);
                        }

                        int rowCount = worksheet.Dimension.End.Row;
                        int colCount = worksheet.Dimension.End.Column;
                        for (int m = 1; m < rowCount; m++)
                        {
                            RowData rowData = new RowData();
                            int n = 0;
                            if (!string.IsNullOrEmpty(sheetClass.ParentVar.Foreign))
                            {
                                rowData.ForeignValue = worksheet.Cells[m + 1, 1].Value.ToString().Trim();
                                n = 1;
                            }
                            for (; n < colCount; n++)
                            {
                                ExcelRange range = worksheet.Cells[m + 1, n + 1];
                                string propertyName = GetNameByCol(sheetClass.VarList, worksheet.Cells[1, n + 1].Value.ToString().Trim());
                                string value = range.Value.ToString().Trim();
                                if (string.IsNullOrEmpty(value))
                                {
                                    Debug.LogError("Excel存在数据为空:[" + (m + 1) + "," + (n + 1) + "]");
                                    continue;
                                }
                                rowData.RowDataDict.Add(propertyName,value);
                            }
                            sheetData.AllRowData.Add(rowData);
                        }
                        sheetDataDict.Add(worksheet.Name, sheetData);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }

        Type type = GetType(className);
        object objClass = null;
        if (type != null)
        {
            objClass = System.Activator.CreateInstance(type);
        }

        List<string> outKeyList = new List<string>();
        foreach (string key in allSheetDict.Keys)
        {
            if (allSheetDict[key].Depth == 1)
            {
                outKeyList.Add(key);
            }
        }
        for (int i = 0; i < outKeyList.Count; i++)
        {
            string key = outKeyList[i];
            ReadDataToClass(objClass, allSheetDict[key], sheetDataDict[key], allSheetDict, sheetDataDict, null);
        }

        Serialize.XmlSerialize(XMLPATH + className + ".xml",objClass);
        Debug.Log(excelPath+"表导入Unity成功！");
        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 读取Excel信息到类
    /// </summary>
    /// <param name="objClass">类对象</param>
    /// <param name="sheetClass"></param>
    /// <param name="sheetData"></param>
    /// <param name="allSheetDict"></param>
    /// <param name="sheetDataDict"></param>
    private static void ReadDataToClass(object objClass,SheetClass sheetClass, SheetData sheetData,Dictionary<string,SheetClass> allSheetDict,
        Dictionary<string,SheetData> sheetDataDict,object keyValue)
    {
        object list = CreateList(sheetClass.Name);
        for (int i = 0; i < sheetData.AllRowData.Count; i++)
        {
            Type type = GetType(sheetClass.Name);
            object addItem = System.Activator.CreateInstance(type);

            if (keyValue != null && !string.IsNullOrEmpty(sheetData.AllRowData[i].ForeignValue))
            {
                if (sheetData.AllRowData[i].ForeignValue != keyValue.ToString())
                {
                    continue;
                }
            }
            for (int j = 0; j < sheetClass.VarList.Count; j++)
            {
                VarClass varClass = sheetClass.VarList[j];
                if (varClass.Type == "list")
                {
                    ReadDataToClass(addItem, allSheetDict[varClass.ListSheetName], sheetDataDict[varClass.ListSheetName], allSheetDict,
                        sheetDataDict, GetMemberValue(addItem, sheetClass.MainKey));
                }
                else if (varClass.Type.Contains("list"))
                {
                    string value = sheetData.AllRowData[i].RowDataDict[sheetData.AllName[j]];
                    SetBaseListValue(addItem, varClass, value);
                }
                else
                {
                    string value = sheetData.AllRowData[i].RowDataDict[sheetData.AllName[j]];
                    if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(varClass.DeafultValue))
                    {
                        value = varClass.DeafultValue;
                    }
                    SetValue(addItem.GetType().GetProperty(sheetData.AllName[j]), addItem, value, sheetData.AllType[j]);
                }
            }
            list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { addItem });
        }
        objClass.GetType().GetProperty(sheetClass.ParentVar.Name).SetValue(objClass, list);
    }
    /// <summary>
    /// 设置基础List的值
    /// </summary>
    /// <param name="item"></param>
    /// <param name="varClass"></param>
    /// <param name="value"></param>
    private static void SetBaseListValue(object item,VarClass varClass,string value)
    {
        string type = varClass.Type.Substring(varClass.Type.LastIndexOf("[") + 1).Replace("]", "");
        object list = CreateList(type);
        
        object[] values = value.Split(System.Convert.ToChar(varClass.SplitStr));
        for (int i = 0; i < values.Length; i++)
        {
            //list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { values[i] });
            Type classListType = list.GetType();
            BindingFlags flag = BindingFlags.Instance | BindingFlags.Public;
            MethodInfo methodInfo = classListType.GetMethod("Add", flag);
            methodInfo.Invoke(list, new object[] { ConvertType(values[i], type) });//相当于List<T>调用Add方法
        }
        item.GetType().GetProperty(varClass.Name).SetValue(item, list);
    }
    private static object ConvertType(object obj, String type)
    {
        object newObj = null;
        switch (type)
        {
            case "System.Int32":
                newObj = System.Convert.ToInt32(obj);
                break;
            case "System.Single":
                newObj = System.Convert.ToSingle(obj);
                break;
            case "System.String":
                newObj = System.Convert.ToString(obj);
                break;
        }
        return newObj;
    }
    /// <summary>
    /// 通过excel的列名获取对应属性名
    /// </summary>
    /// <param name="varList"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    private static string GetNameByCol(List<VarClass> varList,string col)
    {
        for (int i = 0; i < varList.Count; i++)
        {
            if (varList[i].Col == col)
            {
                return varList[i].Name;
            }
        }
        return "";
    }
    /// <summary>
    /// 通过反射设置对象的属性
    /// </summary>
    /// <param name="propertyInfo">属性信息</param>
    /// <param name="item">对象</param>
    /// <param name="value">值</param>
    /// <param name="type">属性类型</param>
    private static void SetValue(PropertyInfo propertyInfo,object item,string value,string type)
    {
        switch (type)
        {
            case "int":
                propertyInfo.SetValue(item, System.Convert.ToInt32(value));
                break;
            case "string":
                propertyInfo.SetValue(item, value);
                break;
            case "float":
                propertyInfo.SetValue(item, System.Convert.ToSingle(value));
                break;
            case "bool":
                propertyInfo.SetValue(item, System.Convert.ToBoolean(value));
                break;
            case "enum":
                object infoValue = TypeDescriptor.GetConverter(propertyInfo.PropertyType).ConvertFromInvariantString(value);  //通过这种方式创建枚举
                propertyInfo.SetValue(item, infoValue);
                break;
        }
    }
    #endregion

    #region 测试
    [MenuItem("Tools/测试/Reg.Xml读取测试")]
    public static void ToolsTest()
    {
        string path = REGXMLPATH + "EnemyData.xml";
        XmlReader reader = null;
        XmlDocument xmldoc = new XmlDocument();
        try
        {
            reader = XmlReader.Create(path);
            xmldoc.Load(reader);
            XmlNode node = xmldoc.SelectSingleNode("data");
            XmlElement element = node as XmlElement;
            string name = element.GetAttribute("name");
            string from = element.GetAttribute("from");
            string to = element.GetAttribute("to");
            foreach (XmlNode tempNode in element.ChildNodes)
            {
                XmlElement tempElement = tempNode as XmlElement;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            if (reader != null)
            {
                reader.Close();
            }
        }
    }
    [MenuItem("Tools/测试/测试创建Excel")]
    public static void ToolsTestExcel()
    {
        string path = EXCELPATH + "EnemyData.xlsx";
        try
        {
            if (File.Exists(path)) File.Delete(path);
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (ExcelPackage package = new ExcelPackage(fs))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("ggg");
                    package.Save();
                }
            }
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }
    [MenuItem("Tools/测试/测试已有对象进行反射")]
    public static void TestReflection1()
    {
        TestInfo testInfo = new TestInfo
        {
            Id = 2,
            Name = "213",
            IsSuccess = false,
            Strlist = new List<string>(),
            TestInfoList = new List<TestInfoTwo>()
        };
        testInfo.Strlist.Add("ff");
        testInfo.Strlist.Add("gg");
        testInfo.Strlist.Add("hh");
        for (int i = 0; i < 3; i++)
        {
            testInfo.TestInfoList.Add(new TestInfoTwo { Id = i });
        }


        //object list = GetMemberValue(testInfo, "Strlist");
        //int listCount = System.Convert.ToInt32(list.GetType().InvokeMember("get_Count",BindingFlags.Default|BindingFlags.InvokeMethod,null,list,new object[] { }));
        //for (int i = 0; i < listCount; i++)
        //{
        //    object item = list.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { i });
        //    Debug.Log(item);
        //}
        object list = GetMemberValue(testInfo, "TestInfoList");
        int listCount = System.Convert.ToInt32(list.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { }));
        for (int i = 0; i < listCount; i++)
        {
            object item = list.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { i });
            int id = System.Convert.ToInt32(GetMemberValue(item, "Id"));
            Debug.Log(id);
        }

    }
    [MenuItem("Tools/测试/测试已有数据进行反射")]
    public static void TestReflection2()
    {
        Type type = GetType("TestInfo");
        object obj = Activator.CreateInstance(type);
        //int
        PropertyInfo idInfo = obj.GetType().GetProperty("Id");
        idInfo.SetValue(obj,System.Convert.ToInt32("20"));
        //string
        PropertyInfo nameInfo = obj.GetType().GetProperty("Name");
        nameInfo.SetValue(obj, "zzxzc");
        //bool
        PropertyInfo boolInfo = obj.GetType().GetProperty("IsSuccess");
        boolInfo.SetValue(obj, System.Convert.ToBoolean("false"));
        //float
        PropertyInfo heightInfo = obj.GetType().GetProperty("Height");
        heightInfo.SetValue(obj, System.Convert.ToSingle("22.3"));
        //enum
        PropertyInfo enumInfo = obj.GetType().GetProperty("TestType");
        object infoValue = TypeDescriptor.GetConverter(enumInfo.PropertyType).ConvertFromInvariantString("HHHH");  //通过这种方式创建枚举
        enumInfo.SetValue(obj, infoValue);

        object list = CreateList("string");
        obj.GetType().GetProperty("Strlist").SetValue(obj,list);
        TestInfo testInfo = obj as TestInfo;
        Debug.Log(testInfo.Id + " " + testInfo.Name + " " + testInfo.IsSuccess + " " + testInfo.Height + " " + testInfo.TestType);
        foreach (string str in testInfo.Strlist)
        {
            Debug.Log(str);
        }
    }
    private static object CreateList(string type)
    {
        Type t = GetType(type);
        Type listType = typeof(List<>);
        Type newType = listType.MakeGenericType(new Type[] { t });
        object list = Activator.CreateInstance(newType, new object[] { });
        return list;
    }
    #endregion


}
/// <summary>
/// Excel表中所有的列名，列类型及列数据
/// </summary>
public class SheetData
{
    //列名
    public List<string> AllName = new List<string>();
    //列类型
    public List<string> AllType = new List<string>();
    //列数据
    public List<RowData> AllRowData = new List<RowData>();
}
/// <summary>
/// Excel表中每一列的所有数据
/// </summary>
public class RowData
{
    public string ForeignValue = "";
    public Dictionary<string, string> RowDataDict = new Dictionary<string, string>();
}
/// <summary>
/// Excel一个sheet对应一个类
/// 对应Reg文件中的一个list
/// </summary>
public class SheetClass
{
    //所属父亲变量
    public VarClass ParentVar { set; get; }
    //深度
    public int Depth { set; get; }
    //变量名
    public string Name { set; get; }
    //Excel中Sheet名
    public string SheetName { set; get; }
    //主键
    public string MainKey { set; get; }
    //分隔符
    public string SplitStr { set; get; }
    //所包含的变量
    public List<VarClass> VarList { set; get; }
}
/// <summary>
/// Excel中sheet中的一列对应一个VarClass
/// 对应Reg文件中的一个variable
/// </summary>
public class VarClass
{
    //变量名
    public string Name { set; get; }
    //类型
    public string Type { set; get; }
    //excel表中列名
    public string Col { set; get; }
    //默认值
    public string DeafultValue { set; get; }
    //外键：变量类型为List时外联部分列
    public string Foreign { set; get; }
    //分割符
    public string SplitStr { set; get; }
    //类型为List时的表名
    public string ListSheetName { set; get; }
    //类型为List时的List名
    public string ListName { set; get; }
}

public enum TestEnum
{
    None = 0,
    Var = 1,
    HHHH = 2,
}
public class TestInfo
{
    public int Id { set; get; }
    public string Name { set; get; }
    public bool IsSuccess { set; get; }
    public float Height { set; get; }
    public TestEnum TestType { set; get; }
    public List<string> Strlist { set; get; }
    public List<TestInfoTwo> TestInfoList { set; get; }
}
public class TestInfoTwo
{
    public int Id { set; get; }
}
