using UnityEngine;

public class MainMenuButtonManagement : MonoBehaviour
{
    [System.Serializable]
    public class Panel
    {
        public string panelName;
        public GameObject panelObject;
    }

    public Panel[] panels;

    public void OpenPanel(string panelName)
    {
        foreach (Panel panel in panels)
        {
            if (panel.panelName == panelName)
            {
                panel.panelObject.SetActive(true);
                return;
            }
        }
    }

    public void ClosePanel(string panelName)
    {
        foreach (Panel panel in panels)
        {
            if (panel.panelName == panelName)
            {
                panel.panelObject.SetActive(false);
                return;
            }
        }
    }

    public void CloseAllPanels()
    {
        foreach (Panel panel in panels)
        {
            panel.panelObject.SetActive(false);
        }
    }
}