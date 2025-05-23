using UnityEngine;

public class PowerSource : MonoBehaviour
{
    public bool Drainable;

    public string PowerName;

    [SerializeField]
    private float _cooldownTime;
    private float _cooldownTimer;

    [SerializeField]
    private ParticleSystem _particleSystem;

    [Space(10)]
    [Header("Audio")]
    [SerializeField]
    private AudioSource _audioSource;
    [SerializeField]
    private AudioClip _drainEffect;

    private void Update()
    {
        // Check if powersource is drained and count it's cooldown to check when it can be activated again
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
    
    // Drain powersource
    public void DrainSource()
    {
        if(Drainable)
        {
            Drainable = false;
            _particleSystem.Stop();
            _audioSource.PlayOneShot(_drainEffect);
        }
    }

}
