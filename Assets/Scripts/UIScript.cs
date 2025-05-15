using TMPro;
using UnityEngine;

public class UIScript : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _powerReservesText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnEnable()
    {
        PlayerCharacterControler.OnPowerReservesChange += ChangePowerReservesUI;
    }



    // Update is called once per frame
    void Update()
    {
        
    }

    private void ChangePowerReservesUI(PlayerCharacterControler playerCharacterControler)
    {
        _powerReservesText.text = $"Power reserves: {playerCharacterControler.CurrentPower.PowerReserves}";
    }
}
