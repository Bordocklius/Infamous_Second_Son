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
    public AudioClip ShootEffect;

    [Space(10)]
    public float PowerReserves;
    public float HeavyPowerReserves;
    public float MaxPowerReserves;
    public float MaxHeavyPowerReserves;
    public Material PowerMaterial;
    public string PowerName;

    [Space(10)]
    public float GlideSlow;

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ METHODS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public virtual void FireLightAttack(Vector3 shootpoint, Vector3 targetDirection)
    {
        PowerReserves--;
        GameObject lightAttack = Instantiate(_lightRangedPrefab, shootpoint, Quaternion.identity);
        lightAttack.GetComponent<Rigidbody>().AddForce(targetDirection * _lightRangedSpeed, ForceMode.Impulse);
    }

    public virtual void FireHeavyAttack(Vector3 shootpoint, Vector3 targetDirection)
    {
        HeavyPowerReserves--;
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

    public bool CheckHeavyPowerReserves()
    {
        if (HeavyPowerReserves <= 0)
        {
            Debug.Log("No heavy power reserves left");
            return false;
        }
        return true;
    }

}
