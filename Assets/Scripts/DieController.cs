using UnityEngine;

public class DieController : MonoBehaviour
{
    [Header("Die Status")]
    public int currentFaceValue; // The final result of the roll
    private Rigidbody rb;
    public bool isRolling = false;
    
    // ADDED: The grace period timer
    private float rollTimer = 0f; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // UPDATED: Only run this check if the dice are supposed to be rolling
        if (isRolling)
        {
            // Count down the timer
            rollTimer -= Time.deltaTime;

            // Wait until the timer finishes AND the die completely stops moving to check the value
            if (rollTimer <= 0f && rb.linearVelocity.magnitude < 0.01f && rb.angularVelocity.magnitude < 0.01f)
            {
                isRolling = false;
                DetectFaceValue();
            }
        }
    }

    // Call this from your GameManager when the "Roll" button is clicked
    public void RollDie()
    {
        isRolling = true;
        
        // ADDED: Give the physics engine 0.5 seconds to launch the dice before checking if they stopped
        rollTimer = 0.5f; 
        
        // Push the die up and spin it randomly
        float force = Random.Range(2f, 4f);
        float spinX = Random.Range(0f, 500f);
        float spinY = Random.Range(0f, 500f);
        float spinZ = Random.Range(0f, 500f);

        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
        rb.AddTorque(new Vector3(spinX, spinY, spinZ));
    }

    public void DetectFaceValue()
    {
        // The world's ceiling direction
        Vector3 ceiling = Vector3.up;

        float bestMatchScore = -Mathf.Infinity;
        int bestFaceIndex = 0;

        // List all 6 local directions of the die
        Vector3[] directions = new Vector3[]
        {
            transform.up,         // Index 0
            -transform.up,        // Index 1
            transform.right,      // Index 2
            -transform.right,     // Index 3
            transform.forward,    // Index 4
            -transform.forward    // Index 5
        };

        // MAP YOUR VALUES HERE! 
        // You MUST change these numbers to match where the numbers are painted on YOUR specific 3D model.
        // MAP YOUR VALUES HERE! 
        int[] faceValues = new int[] 
        { 
            6, // The value on the transform.up face
            1, // The value on the -transform.up face
            5, // The value on the transform.right face
            3, // The value on the -transform.right face
            2, // The value on the transform.forward face
            4  // The value on the -transform.forward face
        };
        

        // TEMPORARY CHEAT FOR TESTING: Every side is a 6!
        //int[] faceValues = new int[] { 6, 6, 6, 6, 6, 6 };

        // Loop through all 6 directions to find which one points closest to the ceiling
        for (int i = 0; i < directions.Length; i++)
        {
            // The Dot product gives a score of how well the vectors align
            float matchScore = Vector3.Dot(ceiling, directions[i]);

            // If this direction is the best match so far, save it
            if (matchScore > bestMatchScore)
            {
                bestMatchScore = matchScore;
                bestFaceIndex = i;
            }
        }

        // Set the final result!
        currentFaceValue = faceValues[bestFaceIndex];
        Debug.Log(gameObject.name + " stopped on: " + currentFaceValue);
    }
}