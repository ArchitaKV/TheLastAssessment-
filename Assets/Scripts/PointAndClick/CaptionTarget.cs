using TMPro;
using UnityEngine;

// An element that appears when a specific Script caption line is reached (e.g. Q18's
// "SUBJECT INSTANCE: ACTIVE." field). With applyText, the caption's exact Script text is shown.
public class CaptionTarget : MonoBehaviour
{
    public string caption;
    public TMP_Text text;

    public void Show(string scriptCaption)
    {
        if (text) text.text = scriptCaption;
        gameObject.SetActive(true);
    }
}
