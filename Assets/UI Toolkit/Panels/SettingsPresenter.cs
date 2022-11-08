using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsPresenter : MonoBehaviour
{
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject playPanel;

    private void OnEnable()
    {
        VisualElement root=GetComponent<UIDocument>().rootVisualElement;
        root.Q<Button>("BackButton").clicked+=()=>{
                root.style.display=DisplayStyle.None;
                gameObject.SetActive(false);
                
                mainPanel.SetActive(true);
                mainPanel.GetComponent<UIDocument>().rootVisualElement.style.display=DisplayStyle.Flex;
            };
    }
}