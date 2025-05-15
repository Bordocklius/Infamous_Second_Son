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

    [SerializeField]
    private ParticleSystem _particleSystem;

    private void Update()
    {
        if(!Drainable)
        {
            _cooldownTimer += Time.deltaTime;
        }
        if(!Drainable && _cooldownTimer >= _cooldownTime)
        {
            Drainable = true;
            _cooldownTimer = 0;
            _particleSystem.Play();
        }
    }

    public void DrainSource()
    {
        if(Drainable)
        {
            Drainable = false;
            _particleSystem.Stop();
        }
    }

}
