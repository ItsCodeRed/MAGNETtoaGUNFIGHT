using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject HitParticles;
    [SerializeField] private float disabledTime = 0.2f;
    [SerializeField] private float gravityScale = 0f;

    private bool currentlyDisabled = true;
    private float disabledTimer = 0;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Shoot()
    {
        disabledTimer = 0;
        currentlyDisabled = true;
    }

    public void Update()
    {
        if (!currentlyDisabled)
            return;

        disabledTimer += Time.deltaTime;

        if (disabledTimer > disabledTime)
        {
            currentlyDisabled = false;
            GetComponent<Collider>().enabled = true;
        }
    }

    private void FixedUpdate()
    {
        rb.velocity += Physics.gravity * Time.fixedDeltaTime * gravityScale;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (currentlyDisabled || GameManager.instance.player.magnet.grabbedBodies.Contains(rb))
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.instance.Die();
        }

        GameObject particles = Instantiate(HitParticles, transform.position, Quaternion.identity);
        particles.transform.LookAt(particles.transform.position + collision.contacts[0].normal);
        if (collision.transform.GetComponent<Renderer>() != null)
            particles.GetComponent<ParticleSystemRenderer>().material.color = collision.transform.GetComponent<Renderer>().material.color;
        Destroy(gameObject);
    }
}
