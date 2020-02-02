using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [SerializeField]
    public int numTilesAcross;
    [SerializeField]
    public int numTilesDown;

    [SerializeField]
    public Vector2 offset;

    void Start()
    {

        List<Sprite> groundSprites = SpriteManager.instance.GetModifiedSprites(SpriteManager.GROUND_SPRITES);
        List<Sprite> dirtFleks = SpriteManager.instance.GetModifiedSprites(SpriteManager.DIRT_FLEKS);

        bool[,] edges = new bool[numTilesAcross, numTilesDown];
        for (int i = 1; i < numTilesAcross-1; i++)
        {
            for (int j = 1; j < numTilesDown-1; j++)
            {
                if (Random.Range(0, 1f) < 0.3f)
                {
                    for (int x = -1; x <= 1; x++)
                        for (int y = -1; y <= 1; y++)
                            edges[i + x, j + y] = true;
                }
            }
        }


        for (int i = 0; i < numTilesAcross; i++)
        {
            for (int j = 0; j < numTilesDown; j++)
            {
                GameObject newObj = new GameObject("Tile" + i + "," + j);
                newObj.transform.parent = this.transform;
                newObj.transform.position = new Vector3(i, j) + (Vector3)offset;
                SpriteRenderer sr = newObj.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;

                bool left = i == 0 || edges[i - 1, j];
                bool right = i == numTilesAcross-1 || edges[i + 1, j];
                bool up = j == numTilesDown-1 || edges[i, j + 1];
                bool down = j == 0 || edges[i, j - 1];

                int numEdges = (left?1:0) + (right?1:0) + (up?1:0) + (down?1:0);

                sr.sprite = groundSprites[0];

                if (!edges[i, j])
                {
                    if (numEdges == 1)
                    {
                        sr.sprite = groundSprites[1];
                        if (right)
                        {
                            newObj.transform.rotation = Quaternion.Euler(0, 0, 180);
                        }
                        if (up)
                        {
                            newObj.transform.rotation = Quaternion.Euler(0, 0, -90);
                        }
                        if (down)
                        {
                            newObj.transform.rotation = Quaternion.Euler(0, 0, 90);
                        }
                    }
                    else if (numEdges == 2 && left != right && up != down)
                    {
                        sr.sprite = groundSprites[2];
                        if (up && right)
                        {
                            newObj.transform.rotation = Quaternion.Euler(0, 0, -90);
                        }
                        if (down && left)
                        {
                            newObj.transform.rotation = Quaternion.Euler(0, 0, 90);
                        }
                        if (down && right)
                        {
                            newObj.transform.rotation = Quaternion.Euler(0, 0, 180);
                        }
                    }
                    else if (numEdges == 3)
                    {
                        sr.sprite = groundSprites[3];
                        if (!left)
                        {
                            newObj.transform.rotation = Quaternion.Euler(0, 0, -90);
                        }
                        if (!up)
                        {
                            newObj.transform.rotation = Quaternion.Euler(0, 0, 180);
                        }
                        if (!right)
                        {
                            newObj.transform.rotation = Quaternion.Euler(0, 0, 90);
                        }
                    }
                    else if (numEdges == 4)
                    {
                        sr.sprite = groundSprites[4];
                    }
                }

                if (Random.Range(0, 1f) < 0.2f)
                {
                    GameObject dustObj = new GameObject("Dust");
                    dustObj.transform.parent = newObj.transform;
                    dustObj.transform.localPosition = Vector3.zero;
                    SpriteRenderer sr2 = dustObj.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
                    sr2.sprite = dirtFleks[Random.Range(0, dirtFleks.Count)];
                    sr2.sortingOrder = 2;
                }
            }
        }
    }
}
