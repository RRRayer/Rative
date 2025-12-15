using StarterAssets;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private GameObject beams;
    private bool isFiring;
#if ENABLE_INPUT_SYSTEM 
    private StarterAssetsInputs input;
#endif

    private void Awake()
    {
        input = GetComponent<StarterAssetsInputs>();
        
        if (beams == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> Beams Reference.", this);
        }
        else
        {
            beams.SetActive(false);
        }
    }

    private void Update()
    {
        ProcessInput();
        if (beams != null && isFiring != beams.activeInHierarchy)
        {
            beams.SetActive(isFiring);
        }
    }

    private void ProcessInput()
    {
        if (input.fire)
        {
            if (!isFiring)
            {
                isFiring = true;
            }
        }
        else if (!input.fire)
        {
            if (isFiring)
            {
                isFiring = false;
            }
        }
    }
}
