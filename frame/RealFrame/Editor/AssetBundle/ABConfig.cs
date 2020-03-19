using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Assets/RealFrame/Editor/ABConfig", menuName = "CreateABConfig",order = 0)]
public class ABConfig : ScriptableObject {

    public List<string> m_AllPrefabPathList = new List<string>();

    public List<FileDirABName> m_AllFileDirABList = new List<FileDirABName>();

    [System.Serializable]
    public struct FileDirABName
    {
        public string AbName;
        public string FilePath;
    }
	
}
