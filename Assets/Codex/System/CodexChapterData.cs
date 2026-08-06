using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Chapter Data", menuName = "Codex/Chapter Data")]
public class CodexChapterData : ScriptableObject
{
    [Header("Basics")]
    public string Title;
    public string Version = "1.0.0";
    public List<CodexSectionData> Sections = new List<CodexSectionData>();

    public CodexPageData Page;
}
