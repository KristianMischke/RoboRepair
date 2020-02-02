using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] TMP_Text leftText, rightText;

    // Start is called before the first frame update
    void Start()
    {
        //leftText = GetComponentInChildren(TextMeshPro, false);    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeLeftText(string text) { leftText.SetText(text); }
    public void ChangeRightText(string text) { rightText.SetText(text); }
}
