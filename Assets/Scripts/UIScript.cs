using TMPro;
using UnityEngine;

public class UIScript : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _powerReservesText;
    [SerializeField]
    private TextMeshProUGUI _heavyPowerReservesText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnEnable()
    {
        PlayerCharacterControler.OnPowerReservesChange += ChangePowerReservesUI;
        PlayerCharacterControler.OnHeavyPowerReservesChange += ChangeHeavyPowerReservesUI;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ChangePowerReservesUI(PlayerCharacterControler playerCharacterControler)
    {
        _powerReservesText.text = $"Power reserves: {playerCharacterControler.CurrentPower.PowerReserves}";        
    }

    private void ChangeHeavyPowerReservesUI(PlayerCharacterControler playerCharacterControler)
    {
        _heavyPowerReservesText.text = $"Heavy reserves: {playerCharacterControler.CurrentPower.HeavyPowerReserves}";
    }
}
