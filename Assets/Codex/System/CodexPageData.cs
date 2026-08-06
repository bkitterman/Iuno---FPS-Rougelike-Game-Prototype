using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Page Data", menuName = "Codex/Page Data")]
public class CodexPageData : ScriptableObject
{
    [Header("Basics")]
    public string Title;
    public string Version = "1.0.0";

    public Sprite Icon;
    public string TagLine;
    public List<string> ContentHeaders = new List<string>();
    [TextArea(5, 20)] public List<string> Content = new List<string>();
    
}
