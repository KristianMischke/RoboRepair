using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashGeneratorScript : MonoBehaviour
{
    [SerializeField] Transform top, bottom, left, right;
    [SerializeField] GameObject myPrefab;

    // Start is called before the first frame update
    void Start()
    {
        // Set variables that will be used to determine the best random point
        float x, y, furthestMin = 0f;
        int rand = Random.Range(6, 12), placedPoints = 0;
        Vector3 toReturn = new Vector3(Random.Range(left.position.x, right.position.x), Random.Range(bottom.position.y, top.position.y), 0f);
        Vector3[] placedObjects = new Vector3[rand];

        // Get random values
        x = Random.Range(left.position.x, right.position.x);
        y = Random.Range(bottom.position.y, top.position.y);

        // Insert the new point into the array of placed points and return
        placedObjects[0] = toReturn;
        // Increment n to decrease clumping
        placedPoints++;
        Instantiate(myPrefab, new Vector3(toReturn.x, toReturn.y, 0), Quaternion.identity);

        Vector3 currVector;
        Vector3 diff;
        float dist;

        for (int l = 1; l < rand + 1; l++) {
            //Vector3 check = toReturn;
            //toReturn = new Vector3(0f, 0f, 0f);
            furthestMin = 0;
            // Loop and find n * 5 number of random points (n being the number of previously placed points)
            for (int i = 0; i < (5 * placedPoints); i++)
            {
                // Assign a value larger than any of the distances could possibly be
                float currMinDist = 1000f;
                // Get random values
                //Random.;
                x = Random.Range(left.position.x, right.position.x);
                y = Random.Range(bottom.position.y, top.position.y);
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
            // Insert the new point into the array of placed points and return
            placedObjects[l] = toReturn;
            // Increment n to decrease clumping
            placedPoints++;
            Instantiate(myPrefab, new Vector3(toReturn.x, toReturn.y, 0), Quaternion.identity);
        }

        //for (int i = 0; i < (Random.Range(20, 40)); i++)
        //{
            //Instantiate(myPrefab, new Vector3(Random.Range(left.position.x, right.position.x), Random.Range(bottom.position.y, top.position.y), 0), Quaternion.identity);
        //}
    }
}
