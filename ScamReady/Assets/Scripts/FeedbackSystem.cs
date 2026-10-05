using UnityEngine;
using TMPro;

public class FeedbackSystem : MonoBehaviour
{
    public GameObject feedbackPanel;
    public TMP_Text feedbackTitle;
    public TMP_Text feedbackMessage;

    public void ShowSafeFeedback(string message)
    {
        ShowFeedback("SAFE DECISION", message);
        Debug.Log("SAFE: " + message);
    }

    public void ShowUnsafeFeedback(string message)
    {
        ShowFeedback("UNSAFE DECISION", message);
        Debug.Log("UNSAFE: " + message);
    }

    public void ShowNeutralFeedback(string message)
    {
        ShowFeedback("INFORMATION", message);
        Debug.Log("INFO: " + message);
    }

    private void ShowFeedback(string title, string message)
    {
        feedbackPanel.SetActive(true);
        feedbackTitle.text = title;
        feedbackMessage.text = message;
    }

    public void CloseFeedback()
    {
        feedbackPanel.SetActive(false);
    }
}