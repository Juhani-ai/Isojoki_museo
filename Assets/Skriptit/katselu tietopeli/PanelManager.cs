using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject[] allPanels;

    private GameObject currentPanel;
    private GameObject lastToggledPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ShowPanel(GameObject panel)
    {
        if (panel == null) return;

        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }

        panel.SetActive(true);
        currentPanel = panel;
    }

    public void HidePanel(GameObject panel)
    {
        if (panel == null) return;

        panel.SetActive(false);

        if (currentPanel == panel)
            currentPanel = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
