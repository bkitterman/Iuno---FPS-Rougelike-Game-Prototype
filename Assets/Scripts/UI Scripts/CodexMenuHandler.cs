using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using TMPro;

public class CodexMenuHandler : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject codexHeader;
    [SerializeField] private GameObject codexPageHeader;
    [SerializeField] private GameObject codexPageSection;

    [Header("Codex Files")]
    [SerializeField] private List<CodexChapterData> chapterList;

    [Header("Codes Panes")]
    [SerializeField] private Transform chapterPanel;
    [SerializeField] private Transform sectionPanel;
    [SerializeField] private Transform pagePanel;

    [SerializeField] private GameObject pageDisplayPanel;

    [SerializeField] private Color highlightColor;
    [SerializeField] private Color deactivatedHighlightColor = new Color(0, 0, 0, 0);

    // Current Stuff
    private List<CodexHeaderHolder> chapterHolders = new List<CodexHeaderHolder>();
    private List<CodexHeaderHolder> sectionHolders = new List<CodexHeaderHolder>();
    private List<CodexHeaderHolder> pageHolders = new List<CodexHeaderHolder>();

    // Current Selections;
    private CodexHeaderHolder currentChapterHolder;
    private CodexHeaderHolder currentSectionHolder;
    private CodexHeaderHolder currentPageHolder;

    void Start()
    {
        foreach (var chapter in chapterList)
        {
            GameObject chapterHeader = Instantiate(codexHeader, chapterPanel);
            CodexHeaderHolder holderUI = chapterHeader.GetComponent<CodexHeaderHolder>();
            holderUI.Border.color = deactivatedHighlightColor;
            holderUI.Text.text = chapter.Title;
            holderUI.Arrow.color = deactivatedHighlightColor;
            holderUI.Icon.sprite = chapter.Page.Icon;         

            Button holderButton = chapterHeader.GetComponent<Button>();
            if (holderButton == null) holderButton = chapterHeader.gameObject.AddComponent<Button>();
            holderButton.onClick.AddListener(() => {
                OnChapterButtonClicked(chapter, holderUI); 
            });

            chapterHolders.Add(holderUI);
        }
        
    }

    void OnChapterButtonClicked(CodexChapterData data, CodexHeaderHolder newHolder)
    {
        pageHolders.Clear();
        sectionHolders.Clear();

        // Destroy old sections
        foreach (Transform child in sectionPanel)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in pagePanel)
        {
            Destroy(child.gameObject);
        }

        // Update chapter buttons
        if (currentChapterHolder != null)
        {
            currentChapterHolder.Arrow.color = deactivatedHighlightColor;
            currentChapterHolder.Border.color = deactivatedHighlightColor;
            currentChapterHolder.IsSelected = false;
        }

        currentChapterHolder = newHolder;
        currentChapterHolder.IsSelected = true;
        currentChapterHolder.Arrow.color = highlightColor;
        currentChapterHolder.Border.color = highlightColor;

        // Build Section holders
        foreach (var section in data.Sections)
        {
            GameObject sectionHeader = Instantiate(codexHeader, sectionPanel);
            CodexHeaderHolder holderUI = sectionHeader.GetComponent<CodexHeaderHolder>();
            holderUI.Border.color = deactivatedHighlightColor;
            holderUI.Text.text = section.Title;
            holderUI.Arrow.color = deactivatedHighlightColor;
            holderUI.Icon.sprite = section.Page.Icon;

            Button holderButton = sectionHeader.GetComponent<Button>();
            if (holderButton == null) holderButton = sectionHeader.gameObject.AddComponent<Button>();
            holderButton.onClick.AddListener(() => {
                OnSectionButtonClicked(section, holderUI);
            });

            sectionHolders.Add(holderUI);
        }

        DisplayPage(data.Page, null);
    }

    void OnSectionButtonClicked(CodexSectionData data, CodexHeaderHolder newHolder)
    {
        pageHolders.Clear();

        // Destroy old Pages
        foreach (Transform child in pagePanel)
        {
            Destroy(child.gameObject);
        }

        // Update Section buttons
        if (currentSectionHolder != null)
        {
            currentSectionHolder.Arrow.color = deactivatedHighlightColor;
            currentSectionHolder.Border.color = deactivatedHighlightColor;
            currentSectionHolder.IsSelected = false;
        }

        currentSectionHolder = newHolder;
        currentSectionHolder.IsSelected = true;
        currentSectionHolder.Arrow.color = highlightColor;
        currentSectionHolder.Border.color = highlightColor;

        // Build Page holders
        foreach (var page in data.Pages)
        {
            GameObject pageHeader = Instantiate(codexHeader, pagePanel);
            CodexHeaderHolder holderUI = pageHeader.GetComponent<CodexHeaderHolder>();
            holderUI.Border.color = deactivatedHighlightColor;
            holderUI.Text.text = page.Title;
            holderUI.Arrow.color = deactivatedHighlightColor;
            holderUI.Icon.sprite = page.Icon;

            Button holderButton = pageHeader.GetComponent<Button>();
            if (holderButton == null) holderButton = pageHeader.gameObject.AddComponent<Button>();
            holderButton.onClick.AddListener(() => {
                DisplayPage(page, holderUI);
            });

            pageHolders.Add(holderUI);
        }

        DisplayPage(data.Page, null);
    }

    void DisplayPage(CodexPageData data, CodexHeaderHolder newHolder)
    {
        // Clear old page
        foreach (Transform child in pageDisplayPanel.transform)
        {
            Destroy(child.gameObject);
        }

        GameObject pageHeader = Instantiate(codexPageHeader, pageDisplayPanel.transform);
        CodexPageHeaderHolder headerHolder = pageHeader.GetComponent<CodexPageHeaderHolder>();

        // Set header info
        headerHolder.Title.text = data.Title;
        headerHolder.Icon.sprite = data.Icon;
        headerHolder.Subtitle.text = data.TagLine;
        headerHolder.Version.text = $"Version: { data.Version}";

        if (data.Content.Count == 0) return;
        if (data.Content.Count != data.ContentHeaders.Count)
        {
            Debug.LogError($"Content and Content Headers count mismatch on page {data.Title}");
            return;
        }

        // Build Page Sections
        for (int i = 0; i < data.Content.Count; i++)
        {
            GameObject section = Instantiate(codexPageSection, pageDisplayPanel.transform);
            CodexPageSectionHolder sectionHolder = section.GetComponent<CodexPageSectionHolder>();

            sectionHolder.SetText(data.Content[i]);
            sectionHolder.Title.text = data.ContentHeaders[i];
        }

        // Return if something other than page was clicked
        if (newHolder == null)
            return;

        // Update Page buttons
        if (currentPageHolder != null)
        {
            currentPageHolder.Arrow.color = deactivatedHighlightColor;
            currentPageHolder.Border.color = deactivatedHighlightColor;
            currentPageHolder.IsSelected = false;
        }

        currentPageHolder = newHolder;
        currentPageHolder.IsSelected = true;
        currentPageHolder.Arrow.color = highlightColor;
        currentPageHolder.Border.color = highlightColor;
    }
}
