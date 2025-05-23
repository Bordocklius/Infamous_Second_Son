using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audiosource;
    [SerializeField]
    private AudioClip _audioclip;

    // Destroy enemy and play death sound
    public void DestroyEnemy()
    {
        _audiosource.PlayOneShot(_audioclip);
        Destroy(this.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject obj = collision.gameObject;
        if(obj.layer == 7)
        {
            DestroyEnemy();
        }

    }


}
