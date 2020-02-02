using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    public const int SPRITE_SIZE = 32;

    public static readonly Vector2 DEFAULT_PIVOT = new Vector2(0.5f, 0.5f);

    public const string LEG_SPRITES = "legs";
    public const string ROBOT_SPRITES = "robot";
    public const int NUM_ROBOTS = 3;
    public const string PART_SPRITES = "part";
    public const string WIRE_SPRITES = "wires";
    public const string LASER_SPRITES = "laser";
    public const string MELEE_SPRITES = "melee";
    public const string SHINY_SPRITES = "shiny";

    public static SpriteManager instance;

    public Sprite mainSpriteSheet;

    [System.Serializable]
    public struct SpriteVector
    {
        public string baseID;
        public Vector2Int startIndex;
        public int numFrames;
        public Vector2 pivot;

        public SpriteVector(string baseID, Vector2Int startIndex, int numFrames, Vector2 pivot)
        {
            this.baseID = baseID;
            this.startIndex = startIndex;
            this.numFrames = numFrames;
            this.pivot = pivot;
        }
    }

    List<SpriteVector> spriteVectors = new List<SpriteVector>()
    {
        new SpriteVector(LEG_SPRITES, Vector2Int.zero, 10, DEFAULT_PIVOT),
        new SpriteVector(ROBOT_SPRITES + "0", new Vector2Int(0, 1), 7, DEFAULT_PIVOT),
        new SpriteVector(ROBOT_SPRITES + "1", new Vector2Int(0, 2), 7, DEFAULT_PIVOT),
        new SpriteVector(ROBOT_SPRITES + "2", new Vector2Int(0, 3), 7, DEFAULT_PIVOT),
        new SpriteVector(PART_SPRITES, new Vector2Int(0, 4), 11, DEFAULT_PIVOT),
        new SpriteVector(LASER_SPRITES, new Vector2Int(0, 5), 7, new Vector2(0, 0.5f)),
        new SpriteVector(MELEE_SPRITES, new Vector2Int(0, 6), 6, DEFAULT_PIVOT),
        new SpriteVector(WIRE_SPRITES, new Vector2Int(0, 7), 4, DEFAULT_PIVOT),
        new SpriteVector(SHINY_SPRITES, new Vector2Int(0, 8), 11, DEFAULT_PIVOT)
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
                Sprite newSprite = Sprite.Create(mainSpriteSheet.texture, new Rect(x, y, SPRITE_SIZE, SPRITE_SIZE), v.pivot, SPRITE_SIZE);

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
    public static void InitColorSwapTex(out Texture2D colorSwapTex, out Color[] mSpriteColors, Texture2D refTexture = null)
    {
        colorSwapTex = new Texture2D(256, 1, TextureFormat.RGBA32, false, false);
        colorSwapTex.filterMode = FilterMode.Point;

        for (int i = 0; i < colorSwapTex.width; ++i)
            colorSwapTex.SetPixel(i, 0, new Color(0.0f, 0.0f, 0.0f, 0.0f));

        mSpriteColors = new Color[colorSwapTex.width];

        if (refTexture != null)
        {
            foreach(Color32 c in refTexture.GetPixels())
            {
                if (c.a != 0)
                {
                    colorSwapTex.SetPixel(c.r, 0, c);
                    mSpriteColors[c.r] = c;
                }
            }
        }

        colorSwapTex.Apply();
    }
}
