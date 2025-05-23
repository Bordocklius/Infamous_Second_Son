using TMPro;
using UnityEngine;

public class UIScript : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _powerReservesText;
    [SerializeField]
    private TextMeshProUGUI _heavyPowerReservesText;
    [SerializeField]
    private TextMeshProUGUI _powerText;

    private void OnEnable()
    {
        // Hook in to power reserves change events
        PlayerCharacterControler.OnPowerReservesChange += ChangePowerReservesUI;
        PlayerCharacterControler.OnHeavyPowerReservesChange += ChangeHeavyPowerReservesUI;
        PlayerCharacterControler.OnPowerChange += ChangePowerTextUI;
    }

    // I think these events speak for themselves
    private void ChangePowerReservesUI(PlayerCharacterControler playerCharacterControler)
    {
        _powerReservesText.text = $"Power reserves: {playerCharacterControler.CurrentPower.PowerReserves}";        
    }

    private void ChangeHeavyPowerReservesUI(PlayerCharacterControler playerCharacterControler)
    {
        _heavyPowerReservesText.text = $"Heavy reserves: {playerCharacterControler.CurrentPower.HeavyPowerReserves}";
    }

    private void ChangePowerTextUI(PlayerCharacterControler playerCharacterControler)
    {
        _powerText.text = $"Current power: {playerCharacterControler.CurrentPower.PowerName}";
    }

    public void OnStartButtonClick()
    {
        // Load sandbox scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Sandbox");
    }
}
