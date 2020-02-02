using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: ADD DIFFERENT SPAWN RATES FOR DIFFERENT TYPES OF TRASH
public class TrashGeneratorScript : MonoBehaviour
{
    [SerializeField] Transform top, bottom, left, right;
    [SerializeField] GameObject[] myPrefabs;
    [SerializeField] SpriteRenderer trashSpriteRenderer;
    [SerializeField] Sprite[] trashSprites;
    [SerializeField] SpriteRenderer canSpriteRenderer;
    [SerializeField] Sprite[] canSprites;
    [SerializeField] SpriteRenderer bananaSpriteRenderer;
    [SerializeField] Sprite[] bananaSprites;
    [SerializeField] SpriteRenderer smallTrashSpriteRenderer;
    [SerializeField] Sprite[] smallTrashSprites;
    [SerializeField] float edgeBoundaryModifier;
    // Ranges for # of clumps
    [SerializeField] Vector2Int[] spawnRanges;
    [SerializeField] Vector2 clumpingArea;
    [SerializeField] int numberClumped;
    float topY, bottomY, leftX, rightX;
    int trashPlaced = 0, cansPlaced = 0, bananasPlaced = 0, smallTrashPlaced = 0;

    // Start is called before the first frame update
    void Start()
    {
        bool xRight = false, yTop = false;
        topY = top.position.y - edgeBoundaryModifier;
        bottomY = bottom.position.y + edgeBoundaryModifier;
        leftX = left.position.x + edgeBoundaryModifier;
        rightX = right.position.x - edgeBoundaryModifier;

        // Set variables that will be used to determine the best random point
        float x = 0, y = 0, furthestMin = 0f;
        int rand = Random.Range(6, 12), placedPoints = 0, tRand, cRand, bRand, stRand;
        // Get all of the random numbers within the provided ranges
        tRand = Random.Range(spawnRanges[0].x, spawnRanges[0].y);
        cRand = Random.Range(spawnRanges[1].x, spawnRanges[1].y);
        bRand = Random.Range(spawnRanges[2].x, spawnRanges[2].y);
        stRand = Random.Range(spawnRanges[3].x, spawnRanges[3].y);
        // Experimental
        rand = tRand + cRand + bRand + stRand;
        Vector3 toReturn = new Vector3(Random.Range(leftX, rightX), Random.Range(bottomY, topY), 0f);
        Vector3[] placedObjects = new Vector3[rand];

        // Get random values
        while (x + edgeBoundaryModifier + 1 < right.position.x && x - edgeBoundaryModifier - 1 > left.position.x) { x = Random.Range(leftX, rightX); }
        while (y + edgeBoundaryModifier + 1 < top.position.y && x - edgeBoundaryModifier - 1 > bottom.position.y) { y = Random.Range(bottomY, topY); }

        //x = Random.Range(leftX, rightX);
        //y = Random.Range(bottomY, topY);

        // Insert the new point into the array of placed points and return
        placedObjects[0] = toReturn;
        // Increment n to decrease clumping
        placedPoints++;
        //int generateRandObj = Random.Range(0, myPrefabs.Length);
        int generateRandObj = 0;
        Instantiate(myPrefabs[generateRandObj], new Vector3(toReturn.x, toReturn.y, 0), Quaternion.identity);
        // Trash
        // Do stuff with sprites
        if (generateRandObj == 0) { trashSpriteRenderer.sprite = trashSprites[Random.Range(0, trashSprites.Length)]; trashPlaced++; }
        // Cans
        //else if (generateRandObj == 1) { canSpriteRenderer.sprite = canSprites[Random.Range(0, canSprites.Length)]; cansPlaced++; }
        // Banana
        //else if (generateRandObj == 2) { bananaSpriteRenderer.sprite = bananaSprites[Random.Range(0, bananaSprites.Length)]; bananasPlaced++; }
        // Small Trash
        //else if (generateRandObj == 3) { smallTrashSpriteRenderer.sprite = smallTrashSprites[Random.Range(0, smallTrashSprites.Length)]; smallTrashPlaced++; }

        Vector3 currVector;
        Vector3 diff;
        float dist;

        for (int l = 1; l < rand + 1; l++) {
            //Vector3 check = toReturn;
            //toReturn = new Vector3(0f, 0f, 0f);

            furthestMin = 0;
            // Loop and find n * 2 number of random points (n being the number of previously placed points)
            /*for (int i = 0; i < (2 * placedPoints); i++)
            {
                // Assign a value larger than any of the distances could possibly be
                float currMinDist = 1000f;
                // Get random values
                //Random.;
                x = Random.Range(leftX, rightX);
                y = Random.Range(bottomY, topY);
                currVector = new Vector3(x, y, 0f);

                // At each random point, loop through every previously placed point
                for (int j = 0; j < placedPoints; j++)
                {
                    // Find distance between the two vectors
                    diff = currVector - placedObjects[j];
                    //dist = Vector3.Distance(currVector, placedObjects[j]);
                    dist = diff.magnitude;
                    // Find the smallest distance between the current random point and each placed point
                    if (dist < currMinDist)
                    {
                        currMinDist = dist;
                    }
                }
                // Find the largest of the smallest distances gotten above
                if (currMinDist > furthestMin)
                {
                    furthestMin = currMinDist;
                    toReturn = currVector;
                }
            }*/
            // Insert the new point into the array of placed points and return
            //placedObjects[l] = toReturn;
            toReturn = BlueNoise(placedPoints, placedObjects, leftX, rightX, bottomY, topY);
            placedObjects[l] = toReturn;
            // Increment n to decrease clumping
            placedPoints++;
            //Instantiate(myPrefab, new Vector3(toReturn.x, toReturn.y, 0), Quaternion.identity);

            //generateRandObj = Random.Range(0, myPrefabs.Length);
            // Ensure spawning chances
            if (trashPlaced < tRand) { generateRandObj = 0; }
            else if (cansPlaced < cRand) { generateRandObj = 1; }
            else if (bananasPlaced < bRand) { generateRandObj = 2; }
            else if (smallTrashPlaced < stRand) { generateRandObj = 3; }

            //Instantiate(myPrefabs[generateRandObj], new Vector3(toReturn.x, toReturn.y, 0), Quaternion.identity);
            // Trash
            // Do stuff with sprites
            if (generateRandObj == 0) {

                /*Instantiate(myPrefabs[generateRandObj], new Vector3(
                    Random.Range(toReturn.x - clumpingArea.x, toReturn.x + clumpingArea.x),
                    Random.Range(toReturn.y - clumpingArea.y, toReturn.y + clumpingArea.y), 0), Quaternion.identity);
                trashSpriteRenderer.sprite = trashSprites[Random.Range(0, trashSprites.Length)]; trashPlaced++;*/
                
                int placedTrash = 1;
                Vector3[] placedTrashObjects = new Vector3[tRand];
                placedTrashObjects[0] = Instantiate(myPrefabs[generateRandObj], new Vector3(
                    Random.Range(toReturn.x - clumpingArea.x, toReturn.x + clumpingArea.x),
                    Random.Range(toReturn.y - clumpingArea.y, toReturn.y + clumpingArea.y), 0), Quaternion.identity).transform.position;
                trashSpriteRenderer.sprite = trashSprites[Random.Range(0, trashSprites.Length)]; trashPlaced++;

                for (int i = 1; i < numberClumped; i++)
                {
                    placedTrashObjects[i] = BlueNoise(placedTrash, placedTrashObjects, toReturn.x - clumpingArea.x,
                        toReturn.x + clumpingArea.x, toReturn.y - clumpingArea.y, toReturn.y + clumpingArea.y);
                    Instantiate(myPrefabs[generateRandObj], placedTrashObjects[i], Quaternion.identity);
                    placedTrash++;
                    trashSpriteRenderer.sprite = trashSprites[Random.Range(0, trashSprites.Length)]; //trashPlaced++;
                }
                trashPlaced++;
                //trashSpriteRenderer.sprite = trashSprites[Random.Range(0, trashSprites.Length)]; trashPlaced++;
            }
            // Cans
            else if (generateRandObj == 1) {
                int placedTrash = 1;
                Vector3[] placedTrashObjects = new Vector3[cRand];
                placedTrashObjects[0] = Instantiate(myPrefabs[generateRandObj], new Vector3(
                    Random.Range(toReturn.x - clumpingArea.x, toReturn.x + clumpingArea.x),
                    Random.Range(toReturn.y - clumpingArea.y, toReturn.y + clumpingArea.y), 0), Quaternion.identity).transform.position;
                canSpriteRenderer.sprite = canSprites[Random.Range(0, canSprites.Length)]; cansPlaced++;

                for (int i = 1; i < numberClumped; i++)
                {
                    placedTrashObjects[i] = BlueNoise(placedTrash, placedTrashObjects, toReturn.x - clumpingArea.x,
                        toReturn.x + clumpingArea.x, toReturn.y - clumpingArea.y, toReturn.y + clumpingArea.y);
                    Instantiate(myPrefabs[generateRandObj], placedTrashObjects[i], Quaternion.identity);
                    placedTrash++;
                    canSpriteRenderer.sprite = canSprites[Random.Range(0, canSprites.Length)]; //trashPlaced++;
                }
                cansPlaced++;
                /*for (int i = 0; i < numberClumped; i++)
                {
                    Instantiate(myPrefabs[generateRandObj], new Vector3(
                        Random.Range(toReturn.x - clumpingArea.x, toReturn.x + clumpingArea.x),
                        Random.Range(toReturn.y - clumpingArea.y, toReturn.y + clumpingArea.y), 0), Quaternion.identity);
                    canSpriteRenderer.sprite = canSprites[Random.Range(0, canSprites.Length)]; cansPlaced++;
                }*/
                //canSpriteRenderer.sprite = canSprites[Random.Range(0, canSprites.Length)]; cansPlaced++;
            }
            // Banana
            else if (generateRandObj == 2) {
                int placedTrash = 1;
                Vector3[] placedTrashObjects = new Vector3[bRand];
                placedTrashObjects[0] = Instantiate(myPrefabs[generateRandObj], new Vector3(
                    Random.Range(toReturn.x - clumpingArea.x, toReturn.x + clumpingArea.x),
                    Random.Range(toReturn.y - clumpingArea.y, toReturn.y + clumpingArea.y), 0), Quaternion.identity).transform.position;
                bananaSpriteRenderer.sprite = bananaSprites[Random.Range(0, bananaSprites.Length)]; bananasPlaced++;

                for (int i = 1; i < numberClumped; i++)
                {
                    placedTrashObjects[i] = BlueNoise(placedTrash, placedTrashObjects, toReturn.x - clumpingArea.x,
                        toReturn.x + clumpingArea.x, toReturn.y - clumpingArea.y, toReturn.y + clumpingArea.y);
                    Instantiate(myPrefabs[generateRandObj], placedTrashObjects[i], Quaternion.identity);
                    placedTrash++;
                    bananaSpriteRenderer.sprite = bananaSprites[Random.Range(0, bananaSprites.Length)]; //trashPlaced++;
                }
                bananasPlaced++;
                /*for (int i = 0; i < numberClumped; i++)
                {
                    Instantiate(myPrefabs[generateRandObj], new Vector3(
                        Random.Range(toReturn.x - clumpingArea.x, toReturn.x + clumpingArea.x),
                        Random.Range(toReturn.y - clumpingArea.y, toReturn.y + clumpingArea.y), 0), Quaternion.identity);
                    bananaSpriteRenderer.sprite = bananaSprites[Random.Range(0, bananaSprites.Length)]; bananasPlaced++;
                }*/
                //bananaSpriteRenderer.sprite = bananaSprites[Random.Range(0, bananaSprites.Length)]; bananasPlaced++;
            }
            // Small Trash
            else if (generateRandObj == 3) {
                int placedTrash = 1;
                Vector3[] placedTrashObjects = new Vector3[tRand];
                placedTrashObjects[0] = Instantiate(myPrefabs[generateRandObj], new Vector3(
                    Random.Range(toReturn.x - clumpingArea.x, toReturn.x + clumpingArea.x),
                    Random.Range(toReturn.y - clumpingArea.y, toReturn.y + clumpingArea.y), 0), Quaternion.identity).transform.position;
                smallTrashSpriteRenderer.sprite = smallTrashSprites[Random.Range(0, smallTrashSprites.Length)]; smallTrashPlaced++;

                for (int i = 1; i < numberClumped; i++)
                {
                    placedTrashObjects[i] = BlueNoise(placedTrash, placedTrashObjects, toReturn.x - clumpingArea.x,
                        toReturn.x + clumpingArea.x, toReturn.y - clumpingArea.y, toReturn.y + clumpingArea.y);
                    Instantiate(myPrefabs[generateRandObj], placedTrashObjects[i], Quaternion.identity);
                    placedTrash++;
                    smallTrashSpriteRenderer.sprite = smallTrashSprites[Random.Range(0, smallTrashSprites.Length)]; //trashPlaced++;
                }
                smallTrashPlaced++;
                /*for (int i = 0; i < numberClumped; i++)
                {
                    Instantiate(myPrefabs[generateRandObj], new Vector3(
                        Random.Range(toReturn.x - clumpingArea.x, toReturn.x + clumpingArea.x),
                        Random.Range(toReturn.y - clumpingArea.y, toReturn.y + clumpingArea.y), 0), Quaternion.identity);
                    smallTrashSpriteRenderer.sprite = smallTrashSprites[Random.Range(0, smallTrashSprites.Length)]; smallTrashPlaced++;
                }*/
                //smallTrashSpriteRenderer.sprite = smallTrashSprites[Random.Range(0, smallTrashSprites.Length)]; smallTrashPlaced++;
            }
        }
    }

    Vector3 BlueNoise(int placedPoints, Vector3[] placedObjects, float lX, float rX, float bY, float tY)
    {
        Vector3 currVector, toReturn = new Vector3(0, 0, 0);
        Vector3 diff;
        float dist, furthestMin = 0f;
        float x, y;
        // Loop and find n * 5 number of random points (n being the number of previously placed points)
        for (int i = 0; i < (5 * placedPoints); i++)
        {
            // Assign a value larger than any of the distances could possibly be
            float currMinDist = 1000f;
            // Get random values
            //Random.;
            x = Random.Range(lX, rX);
            y = Random.Range(bY, tY);
            currVector = new Vector3(x, y, 0f);

            // At each random point, loop through every previously placed point
            for (int j = 0; j < placedPoints; j++)
            {
                // Find distance between the two vectors
                diff = currVector - placedObjects[j];
                //dist = Vector3.Distance(currVector, placedObjects[j]);
                dist = diff.magnitude;
                // Find the smallest distance between the current random point and each placed point
                if (dist < currMinDist)
                {
                    currMinDist = dist;
                }
            }
            // Find the largest of the smallest distances gotten above
            if (currMinDist > furthestMin)
            {
                furthestMin = currMinDist;
                toReturn = currVector;
            }
        }
        return toReturn;
    }
}
