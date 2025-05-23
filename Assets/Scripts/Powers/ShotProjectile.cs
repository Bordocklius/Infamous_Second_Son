using System.Collections;
using UnityEngine;

public class ShotProjectile : MonoBehaviour
{
    [SerializeField]
    private float _lifeTime;
    private float _timer;

    public bool HeavyShot;
    [SerializeField]
    private float _explosionRadius;
    [SerializeField]
    private GameObject _explosionPrefab;
    [SerializeField]
    private float _explosionTime;

    private bool _triggered = false;

    // Update is called once per frame
    void Update()
    {
        // Destroy shot is time to live has expired
        _timer += Time.deltaTime;
        if (_timer >= _lifeTime)
        {
            Destroy(this.gameObject);
        }
    }

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ METHODS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    private void OnCollisionEnter(Collision collision)
    {
        // If shot is heavy shot, trigger explosion
        if (HeavyShot && !_triggered)
        {
            _triggered = true;
            Explode();
            GameObject explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, _explosionTime);
            
        }

        Destroy(this.gameObject, _explosionTime);

    }

    // Check if enemies are in explosion range and destroy them
    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRadius);
        foreach (Collider collider in colliders)
        {
            GameObject obj = collider.gameObject;
            if (obj.layer == 10)
            {
                Enemy enemy = obj.GetComponent<Enemy>();
                enemy.DestroyEnemy();
            }
        }
    }
}
