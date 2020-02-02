using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DespawnAnimation : MonoBehaviour
{
    private SpriteRenderer myRenderer;

    public List<Sprite> spriteList;
    public int spriteIndex;
    public bool incrementIndex = true;
    
    private float frameTime = 0.1f;
    private float timer = 0;

    public float FrameTimer { set { timer = frameTime = value; } }
    void Start()
    {
        myRenderer = GetComponent<SpriteRenderer>();

        if (first)
        {
            myRenderer.sprite = spriteList[spriteIndex];
            first = false;
        }
    }

    bool first = true;
    // Update is called once per frame
    void Update()
    {
        if (first)
        {
            myRenderer.sprite = spriteList[spriteIndex];
            first = false;
        }

        timer -= Time.deltaTime;

        if (spriteList == null)
        {
            Destroy(gameObject);
        }
        else
        {
            if (timer <= 0)
            {
                timer += frameTime;

                spriteIndex = spriteIndex + (incrementIndex ? 1 : -1);

                if ((incrementIndex && spriteIndex >= spriteList.Count) || (!incrementIndex && spriteIndex < 0))
                {
                    Destroy(gameObject);
                }
                else
                {
                    myRenderer.sprite = spriteList[spriteIndex];
                }
            }
        }
    }
}
