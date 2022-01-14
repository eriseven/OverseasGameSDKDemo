using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XLua;

[LuaCallCSharp()]
public class DemoUI : MonoBehaviour
{
    [SerializeField]
    private Transform contents;

    [SerializeField]
    private GameObject buttonTemplate;
    
    public void AddTestAction(string title, Action callback)
    {
        if (contents == null) return;
        if (buttonTemplate == null) return;

        var button = GameObject.Instantiate(buttonTemplate, contents);
       
        button.SetActive(true);
        button.GetComponent<Button>().onClick.AddListener(() => callback?.Invoke());
        button.GetComponentInChildren<TMP_Text>().text = title;
    }
}
