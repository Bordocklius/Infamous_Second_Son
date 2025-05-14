using UnityEngine;

public class PowerSource : MonoBehaviour
{
    [SerializeField]
    public bool Drainable;

    [SerializeField]
    public string PowerName;

    [SerializeField]
    private float _cooldownTime;
    private float _cooldownTimer;

    private void Update()
    {
        if(!Drainable)
        {
            _cooldownTimer += Time.deltaTime;
        }
        if(_cooldownTimer >= _cooldownTime)
        {
            Drainable = true;
            _cooldownTimer = 0;
        }
    }

}
