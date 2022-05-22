using System;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    [SerializeField] GameObject deathParticles;
    [SerializeField] private Weapon weapon;
    [SerializeField] private LayerMask metalLayer;

    [SerializeField] private float flankForwardSpeed = 1;
    [SerializeField] private float flankSpeed = 2;
    [SerializeField] private float chaseSpeed = 3;
    [SerializeField] private float runSpeed = 3;

    [SerializeField] private float decisionTime = 1f;
    [SerializeField] private float decisionVariance = 0.4f;
    [SerializeField] private int decisionNum = 4;

    [SerializeField] private float shockTime = 1f;

    public EnemyState state;

    public bool weaponMagnitized = false;

    private Vector3 targetPosition;

    private float decisionTimer = 1f;
    private float shockTimer = 0f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        decisionTimer = 5;
        state = EnemyState.Chasing;
    }

    private void Update()
    {
        if (GameManager.instance.player != null)
            targetPosition = GameManager.instance.player.transform.position;

        if (weapon == null || !weapon.IsHeld)
        {
            Run();
            return;
        }

        transform.LookAt(targetPosition);

        if (GameManager.instance.player == null)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        decisionTimer -= Time.deltaTime;
        if (decisionTimer < 0)
        {
            decisionTimer = decisionTime + Random.Range(-decisionVariance, decisionVariance);
            Array states = Enum.GetValues(typeof(EnemyState));
            state = (EnemyState)states.GetValue(Random.Range(0, decisionNum));
        }

        switch (state)
        {
            case EnemyState.Chasing:
                Chase();
                break;
            case EnemyState.FlankRight:
                Flank(true);
                break;
            case EnemyState.FlankLeft:
                Flank(false);
                break;
            case EnemyState.StopAndShoot:
                rb.velocity = Vector3.zero;
                break;
        }
    }

    private void Run()
    {
        shockTimer += Time.deltaTime;
        rb.velocity = Vector3.zero;
        if (shockTimer > shockTime)
        {
            transform.LookAt(transform.position + (transform.position - targetPosition));
            rb.velocity = transform.forward * runSpeed;
        }
    }

    private void Chase()
    {
        rb.velocity = transform.forward * chaseSpeed;
    }

    private void Flank(bool goRight)
    {
        rb.velocity = transform.forward * flankForwardSpeed + transform.right * (goRight ? 1 : -1) * flankSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (1 << collision.gameObject.layer == metalLayer.value && collision.collider.transform.parent == null && (weapon == null || collision.gameObject != weapon.gameObject || !weapon.safeToTouch))
        {
            Instantiate(deathParticles, transform.position, Quaternion.identity);
            GameManager.instance.enemies.Remove(this);
            Destroy(gameObject);
            GameManager.instance.AddScore(transform, 100);
            GameManager.instance.SpawnEnemy();
        }
    }
}

public enum EnemyState
{
    Chasing = 0,
    FlankRight,
    FlankLeft,
    StopAndShoot,
}
