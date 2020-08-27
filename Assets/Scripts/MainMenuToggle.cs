using UnityEngine;

public class MainMenuToggle : MonoBehaviour
{
    [SerializeField] MainMenuLogic mainMenuLogic = null;
    private HandManager handManager;

    private void Awake() {
        handManager = FindObjectOfType<HandManager>();
    }

    private void Update() {
        if (handManager.Left.buttonPrimary.isPressed() || handManager.Right.buttonPrimary.isPressed())
        {
            mainMenuLogic.ToggleMainMenu();
        }
    }
    
}
