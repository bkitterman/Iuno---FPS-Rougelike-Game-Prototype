using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Section Data", menuName = "Codex/Section Data")]
public class CodexSectionData : ScriptableObject
{
    [Header("Basics")]
    public string Title;
    public string Version = "1.0.0";
    public List<CodexPageData> Pages = new List<CodexPageData>();

    public CodexPageData Page;
}
