using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon
{
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject shootParticles;
    [SerializeField] private float shootSpeed = 10f;
    [SerializeField] private float shootInterval = 1f;
    [SerializeField] private float fastShootInterval = 0.6f;

    private float shootTimer = 0;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void Update()
    {
        base.Update();

        if (!IsHeld)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            return;
        }

        shootTimer += Time.deltaTime;
        
        if (enemy.state == EnemyState.StopAndShoot ? shootTimer > fastShootInterval : shootTimer > shootInterval)
        {
            shootTimer = 0;
            Shoot();
        }
    }

    private void Shoot()
    {
        Instantiate(shootParticles, shootPoint);
        GameObject shotBullet = Instantiate(bullet, shootPoint.position, shootPoint.rotation);
        GameManager.instance.throwables.Add(shotBullet);

        shotBullet.GetComponent<Rigidbody>().velocity = transform.forward * shootSpeed;
    }
}
