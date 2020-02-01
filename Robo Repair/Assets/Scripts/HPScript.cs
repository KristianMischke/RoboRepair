using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HPScript : MonoBehaviour
{
    [SerializeField] Sprite[] hpStages;
    SpriteRenderer mySR;

    // Testing health bar
    /*void Update() {
        if (Input.GetKey(KeyCode.K)) {
            if (test == 0) { upOrDown = false; }
            else if (test == 20) { upOrDown = true; }

            if (!upOrDown) { test++; }
            else { test--; }
            mySR.sprite = hpStages[test]; }}*/
    private void Awake()
    {
        mySR = GetComponent<SpriteRenderer>();
    }

    public void UpdateHP(int hp)
    {
        mySR.sprite = hpStages[hp];
    }
}
