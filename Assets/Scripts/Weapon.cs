using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private Collider col;
    [SerializeField] private float shakeAmount = 1f;
    [SerializeField] private float holdTime = 1f;
    [SerializeField] private float safeDisarmTime = 0.3f;

    public bool IsHeld => transform.parent != null;
    public int weightValue = 1;

    public Enemy enemy;
    public bool isMagnitized;
    public bool safeToTouch = true;

    private float disarmTimer = 0f;

    private float magnitizedTimer = 0f;

    private void Start()
    {
        col.enabled = false;
    }

    public virtual void Update()
    {
        if (!IsHeld)
        {
            if (disarmTimer > safeDisarmTime)
            {
                safeToTouch = false;
            }
            else
            {
                safeToTouch = true;
                disarmTimer += Time.deltaTime;
            }    

            return;
        }

        if (isMagnitized)
        {
            transform.localPosition = Random.insideUnitSphere * shakeAmount;
            magnitizedTimer += Time.deltaTime;
            if (magnitizedTimer > holdTime)
            {
                GameManager.instance.throwables.Add(gameObject);
                transform.parent = null;
                GetComponent<Collider>().enabled = true;
                magnitizedTimer = 0;
                isMagnitized = false;
            }
        }
        else
        {
            magnitizedTimer = 0;
        }

        isMagnitized = false;
    }
}