using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    public const int SPRITE_SIZE = 32;

    public const string LEG_SPRITES = "legs";
    public const string ROBOT_SPRITES = "robot";
    public const int NUM_ROBOTS = 3;
    public const string PART_SPRITES = "part";

    public static SpriteManager instance;

    public Sprite mainSpriteSheet;

    [System.Serializable]
    public struct SpriteVector
    {
        public string baseID;
        public Vector2Int startIndex;
        public int numFrames;

        public SpriteVector(string baseID, Vector2Int startIndex, int numFrames)
        {
            this.baseID = baseID;
            this.startIndex = startIndex;
            this.numFrames = numFrames;
        }
    }

    List<SpriteVector> spriteVectors = new List<SpriteVector>()
    {
        new SpriteVector(LEG_SPRITES, Vector2Int.zero, 10),
        new SpriteVector(ROBOT_SPRITES + "0", new Vector2Int(0, 1), 6),
        new SpriteVector(ROBOT_SPRITES + "1", new Vector2Int(0, 2), 6),
        new SpriteVector(ROBOT_SPRITES + "2", new Vector2Int(0, 3), 6),
        new SpriteVector(PART_SPRITES, new Vector2Int(0, 4), 4)
    };

    private Dictionary<string, List<Sprite>> spriteDictionary = null;

    private void OnEnable()
    {
        if (instance != null && instance != this)
            Debug.LogError("Singleton SpriteManager should not be instantiated more than once!");
        instance = this;
    }

    void Start()
    {
        if (spriteDictionary == null)
            LoadSpriteDictionary();
    }

    public void LoadSpriteDictionary()
    {
        if (spriteDictionary != null)
            return;

        spriteDictionary = new Dictionary<string, List<Sprite>>();
        foreach (SpriteVector v in spriteVectors)
        {
            spriteDictionary.Add(v.baseID, new List<Sprite>());
            for (int i = 0; i < v.numFrames; i++)
            {
                /*Color[] pixels = mainSpriteSheet.texture.GetPixels((v.startIndex.x + i) * SPRITE_SIZE, v.startIndex.y * SPRITE_SIZE, SPRITE_SIZE, SPRITE_SIZE);

                Texture2D tex = new Texture2D(SPRITE_SIZE, SPRITE_SIZE);
                tex.SetPixels(pixels);*/

                float x = (v.startIndex.x + i) * SPRITE_SIZE;
                float y = mainSpriteSheet.texture.height - SPRITE_SIZE - v.startIndex.y * SPRITE_SIZE;
                Sprite newSprite = Sprite.Create(mainSpriteSheet.texture, new Rect(x, y, SPRITE_SIZE, SPRITE_SIZE), new Vector2(0.5f, 0.5f), SPRITE_SIZE);

                spriteDictionary[v.baseID].Add(newSprite);
            }
        }
    }


    public List<Sprite> GetModifiedSprites(string spriteID)
    {
        if (spriteDictionary == null)
            LoadSpriteDictionary();

        return spriteDictionary[spriteID];
    }

    // Thanks to https://gamedevelopment.tutsplus.com/tutorials/how-to-use-a-shader-to-dynamically-swap-a-sprites-colors--cms-25129
    public static void InitColorSwapTex(out Texture2D mColorSwapTex, out Color[] mSpriteColors, Texture2D refTexture = null)
    {
        Texture2D colorSwapTex = new Texture2D(256, 1, TextureFormat.RGBA32, false, false);
        colorSwapTex.filterMode = FilterMode.Point;

        for (int i = 0; i < colorSwapTex.width; ++i)
            colorSwapTex.SetPixel(i, 0, new Color(0.0f, 0.0f, 0.0f, 0.0f));

        colorSwapTex.Apply();

        mSpriteColors = new Color[colorSwapTex.width];
        mColorSwapTex = colorSwapTex;

        if (refTexture != null)
        {
            foreach(Color32 c in refTexture.GetPixels())
            {
                colorSwapTex.SetPixel(c.r, 0, c);
                mSpriteColors[c.r] = c;
            }
        }
    }
}
