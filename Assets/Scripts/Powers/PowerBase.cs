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

    [SerializeField]
    private float _powerReserves;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ METHODS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public virtual void FireLightAttack(Vector3 shootpoint, Vector3 targetDirection)
    {
        Debug.DrawRay(shootpoint, targetDirection, Color.red, 2f, true);
        GameObject lightAttack = Instantiate(_lightRangedPrefab, shootpoint, Quaternion.identity);
        lightAttack.GetComponent<Rigidbody>().AddForce(targetDirection * _lightRangedSpeed, ForceMode.Impulse);
    }

    public virtual void FireHeavyAttack(Vector3 target)
    {
        return;
    }

    public abstract void MovementAbility();
}
