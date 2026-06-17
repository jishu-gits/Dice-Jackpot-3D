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

    // ADDED: The Particle System Reference
    [Header("Visual Effects")]
    public ParticleSystem confettiParticles;

    [Header("Pity System")]
    private int lossCount = 0; 

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

        foreach (DieController die in dice)
        {
            die.RollDie();
        }

        yield return new WaitUntil(() => AllDiceStopped());
        yield return new WaitForSeconds(0.1f);

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

    private void EvaluateResults()
    {
        if (lossCount >= 3)
        {
            Debug.Log("Pity System Activated! Forcing a Jackpot.");
            
            foreach (DieController die in dice)
            {
                Rigidbody rb = die.GetComponent<Rigidbody>();
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                die.transform.rotation = Quaternion.identity;
                die.DetectFaceValue();
            }
        }

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

            // ADDED: Play the Confetti explosion!
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