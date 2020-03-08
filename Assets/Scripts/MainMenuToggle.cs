using UnityEngine;
using UnityEngine.XR;

public class MainMenuToggle : MonoBehaviour
{
    [SerializeField] MainMenuLogic mainMenuLogic = null;
    private HandManager handManager;

    private void Awake() {
        handManager = FindObjectOfType<HandManager>();
    }

    private void Update() {
        if (handManager.buttonLeftPrimary.isPressed())
        {
            mainMenuLogic.ToggleMainMenu();
        }
    }
    
}
