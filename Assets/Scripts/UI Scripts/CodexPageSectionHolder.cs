using UnityEngine;

using TMPro;

using System.Text;

public class CodexPageSectionHolder : MonoBehaviour
{
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Content;
    

    /// <summary>
    /// Set the text of the section, and includes markdown post processing.
    /// </summary>
    /// <param name="text"></param>
    public void SetText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        string processedText = text;

        // Handle escaped stars first
        processedText = processedText.Replace("\\*", "[[STAR]]");

        // Replace Bold + Italic (***)
        while (processedText.Contains("***"))
        {
            // Replace the first for opening
            int first = processedText.IndexOf("***");
            processedText = processedText.Remove(first, 3).Insert(first, "<b><i>");

            // Replace the next for closing (if it exists)
            if (processedText.Contains("***"))
            {
                int next = processedText.IndexOf("***");
                processedText = processedText.Remove(next, 3).Insert(next, "</i></b>");
            }
        }

        // Replace Bold (**)
        while (processedText.Contains("**"))
        {
            int first = processedText.IndexOf("**");
            processedText = processedText.Remove(first, 2).Insert(first, "<b>");
            if (processedText.Contains("**"))
            {
                int next = processedText.IndexOf("**");
                processedText = processedText.Remove(next, 2).Insert(next, "</b>");
            }
        }

        // Replace Italic (*)
        while (processedText.Contains("*"))
        {
            int first = processedText.IndexOf("*");
            processedText = processedText.Remove(first, 1).Insert(first, "<i>");
            if (processedText.Contains("*"))
            {
                int next = processedText.IndexOf("*");
                processedText = processedText.Remove(next, 1).Insert(next, "</i>");
            }
        }

        // Put back the escaped stars
        processedText = processedText.Replace("[[STAR]]", "*");

        this.Content.text = processedText;
    }
}
