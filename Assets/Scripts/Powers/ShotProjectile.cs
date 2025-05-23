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
        if (HeavyShot && !_triggered)
        {
            _triggered = true;
            Debug.Log("Draw");
            GameObject explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            Debug.Log("Destroy");
            Destroy(explosion, _explosionTime);
            Destroy(this.gameObject, _explosionTime);
        }
        else
        {
            Destroy(this.gameObject);
        }

    }

    private IEnumerator ExplosionRoutine()
    {

        //Explode();

        // Draw explosion effect, persist for time, then destroy it
        Debug.Log("Draw");
        GameObject explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(_explosionTime);
        Debug.Log("Destroy");
        Destroy(explosion);
        Destroy(this.gameObject);
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRadius);
        foreach (Collider collider in colliders)
        {

        }
    }
}
