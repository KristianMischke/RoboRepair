using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    public SpriteManager instance;

    public Sprite mainSpriteSheet;

    void Start()
    {
        if (instance != null && instance != this)
            Debug.LogError("Singleton SpriteManager should not be instantiated more than once!");
        instance = this;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
