using System.Collections;
using UnityEngine;

public abstract class PowerBase : MonoBehaviour
{
    [Header("Ranged attacks prefabs")]
    [SerializeField]
    private GameObject _lightRangedPrefab;
    [SerializeField]
    private GameObject _heavyRangedPrefab;
    [SerializeField]
    private float _lightRangedSpeed;
    [SerializeField]
    private float _heavyRangedSpeed;

    [Space(10)]
    [SerializeField]
    public float PowerReserves;

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ METHODS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public virtual void FireLightAttack(Vector3 shootpoint, Vector3 targetDirection)
    {
        PowerReserves--;
        GameObject lightAttack = Instantiate(_lightRangedPrefab, shootpoint, Quaternion.identity);
        lightAttack.GetComponent<Rigidbody>().AddForce(targetDirection * _lightRangedSpeed, ForceMode.Impulse);
    }

    public virtual void FireHeavyAttack(Vector3 shootpoint, Vector3 targetDirection)
    {
        PowerReserves-= 5;
        GameObject heavyAttack = Instantiate(_heavyRangedPrefab, transform.position, Quaternion.identity);
        heavyAttack.GetComponent<Rigidbody>().AddForce(targetDirection * _heavyRangedSpeed, ForceMode.Impulse);
    }

    public bool CheckPowerReserves()
    {
        if (PowerReserves <= 0)
        {
            Debug.Log("No power reserves left");
            return false;
        }
        return true;
    }

}
