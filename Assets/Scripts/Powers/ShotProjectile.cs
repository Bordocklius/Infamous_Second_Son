using UnityEngine;

public class ShotProjectile : MonoBehaviour
{
    [SerializeField]
    private float _lifeTime;
    private float _timer;

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;
        if(_timer >= _lifeTime)
        {
            Destroy(this.gameObject);
        }
    }

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ METHODS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(this.gameObject);
    }
}
