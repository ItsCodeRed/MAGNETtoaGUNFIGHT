using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knife : Weapon
{
    [SerializeField] private float throwVerticalStrength = 2f;
    [SerializeField] private float throwStrength = 10f;
    [SerializeField] private float stabAttemptDist = 1f;

    [SerializeField] private Animation anim;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void Update()
    {
        base.Update();

        if (enemy.state == EnemyState.StopAndShoot && IsHeld)
        {
            transform.parent = null;
            rb.velocity = transform.forward * throwStrength + Vector3.up * throwVerticalStrength;
        }

        if (!IsHeld)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            return;
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.instance.player != null && IsHeld && (GameManager.instance.player.transform.position - enemy.transform.position).magnitude < stabAttemptDist)
        {
            if (!anim.isPlaying)
                anim.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.instance.player == null || GameManager.instance.player.magnet.grabbedBodies.Contains(rb))
            return;

        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.instance.Die();
        }
    }
}
