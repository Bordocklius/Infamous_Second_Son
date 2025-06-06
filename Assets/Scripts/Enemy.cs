using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audiosource;
    [SerializeField]
    private AudioClip _audioclip;

    // Destroy enemy and play death sound
    public void DestroyEnemy(bool playAudio)
    {
        if (playAudio)
        {
            PlayDeathAudio();
        }

        Destroy(this.gameObject);
    }

    public void PlayDeathAudio()
    {
        _audiosource.PlayOneShot(_audioclip);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject obj = collision.gameObject;
        if (obj.layer == 7)
        {
            DestroyEnemy(true);
        }

    }


}
