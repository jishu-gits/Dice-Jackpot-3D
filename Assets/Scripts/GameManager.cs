using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class GameManager : MonoBehaviour
{
    [Header("Game References")]
    public DieController[] dice; 
    public Button rollButton;
    public TextMeshProUGUI resultText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip rollSound;
    public AudioClip winSound;

    [Header("Visual Effects")]
    public ParticleSystem confettiParticles;

    [Header("Pity System")]
    private int lossCount = 0; 

    // The 6 perfect mathematical flat rotations for each face of the die
    private Quaternion[] perfectRotations = new Quaternion[]
    {
        Quaternion.Euler(0, 0, 0),    // Index 0: Faces '6' Up
        Quaternion.Euler(180, 0, 0),  // Index 1: Faces '1' Up
        Quaternion.Euler(0, 0, 90),   // Index 2: Faces '5' Up
        Quaternion.Euler(0, 0, -90),  // Index 3: Faces '3' Up
        Quaternion.Euler(-90, 0, 0),  // Index 4: Faces '2' Up
        Quaternion.Euler(90, 0, 0)    // Index 5: Faces '4' Up
    };

    void Start()
    {
        resultText.text = "Ready to Roll!";
        rollButton.onClick.AddListener(OnRollButtonClicked);
    }

    public void OnRollButtonClicked()
    {
        StartCoroutine(RollRoutine());
    }

    private IEnumerator RollRoutine()
    {
        rollButton.interactable = false;
        resultText.text = "Rolling...";

        if (rollSound != null)
        {
            audioSource.PlayOneShot(rollSound);
        }

        // DECISION BRANCH: Is this the guaranteed Pity Win?
        if (lossCount >= 3)
        {
            Debug.Log("Pity System Activated! Triggering the Magic Toss...");
            // Run the seamless animated toss instead of physics
            yield return StartCoroutine(SeamlessPityRoll());
        }
        else
        {
            // Standard random physics toss
            foreach (DieController die in dice)
            {
                die.RollDie();
            }

            // Wait for normal physics to finish
            yield return new WaitUntil(() => AllDiceStopped());
            yield return new WaitForSeconds(0.1f);
        }

        // Evaluate the results regardless of which toss was used
        EvaluateResults();
        rollButton.interactable = true;
    }

    private bool AllDiceStopped()
    {
        foreach (DieController die in dice)
        {
            if (die.isRolling) return false;
        }
        return true;
    }

    // The new, fluid animation sequence
    private IEnumerator SeamlessPityRoll()
    {
        // Pick a completely random face for all 3 dice to land on
        int randomFaceIndex = Random.Range(0, perfectRotations.Length);
        Quaternion targetRotation = perfectRotations[randomFaceIndex];

        Vector3[] startPositions = new Vector3[3];
        Vector3[] peakPositions = new Vector3[3];
        Vector3[] randomSpinAxes = new Vector3[3];

        for (int i = 0; i < 3; i++)
        {
            Rigidbody rb = dice[i].GetComponent<Rigidbody>();
            rb.isKinematic = true; // Temporarily disable gravity so we can animate
            
            startPositions[i] = dice[i].transform.position;
            peakPositions[i] = startPositions[i] + (Vector3.up * 3f); // Jump up 3 units
            randomSpinAxes[i] = new Vector3(Random.value, Random.value, Random.value).normalized;
        }

        // PHASE 1: Toss up and spin wildly (1 second)
        float duration = 1.0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            float arcCurve = Mathf.Sin(percent * Mathf.PI * 0.5f); // Math to make a smooth upward arc
            
            for (int i = 0; i < 3; i++)
            {
                dice[i].transform.position = Vector3.Lerp(startPositions[i], peakPositions[i], arcCurve);
                dice[i].transform.Rotate(randomSpinAxes[i] * 1000f * Time.deltaTime); // Spin fast
            }
            yield return null;
        }

        // Save their exact rotation at the highest point of the jump
        Quaternion[] peakRotations = new Quaternion[3];
        for (int i = 0; i < 3; i++)
        {
            peakRotations[i] = dice[i].transform.rotation;
        }

        // PHASE 2: Float down and magically align perfectly (1 second)
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            float smoothPercent = Mathf.SmoothStep(0f, 1f, percent); // The "Easing" bonus point!

            for (int i = 0; i < 3; i++)
            {
                dice[i].transform.position = Vector3.Lerp(peakPositions[i], startPositions[i], smoothPercent);
                dice[i].transform.rotation = Quaternion.Slerp(peakRotations[i], targetRotation, smoothPercent);
            }
            yield return null;
        }

        // Final cleanup after the animation
        for (int i = 0; i < 3; i++)
        {
            dice[i].transform.rotation = targetRotation;
            dice[i].transform.position = startPositions[i];
            
            Rigidbody rb = dice[i].GetComponent<Rigidbody>();
            rb.isKinematic = false; // Turn physics back on for the next normal round

            // Force the die to update its face value so EvaluateResults() sees the match
            dice[i].DetectFaceValue();
        }
    }

    private void EvaluateResults()
    {
        bool isJackpot = (dice[0].currentFaceValue == dice[1].currentFaceValue) && 
                         (dice[1].currentFaceValue == dice[2].currentFaceValue);

        if (isJackpot)
        {
            resultText.text = "JACKPOT!";
            lossCount = 0; 
            
            if (winSound != null)
            {
                audioSource.PlayOneShot(winSound);
            }

            if (confettiParticles != null)
            {
                confettiParticles.Play();
            }
        }
        else
        {
            resultText.text = "Try Again";
            lossCount++; 
            Debug.Log("Loss Count is now: " + lossCount);
        }
    }
}