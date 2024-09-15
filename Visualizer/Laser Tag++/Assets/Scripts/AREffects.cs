using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AREffects : MonoBehaviour {
    
    [Header("References")]
    [SerializeField] private Transform cam;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GameObject objectToThrow;

    [Header("Throwing")]
    [SerializeField] private KeyCode testThrowKey = KeyCode.F;
    [SerializeField] private float throwForce;
    [SerializeField] private float throwUpwardForce;

    bool readyToThrow;


    private void Start() {
        readyToThrow = true;
    }

    private void Update() {
        if (Input.GetKeyDown(testThrowKey) && readyToThrow) {
            Throw();
        }
    }


    private void Throw() {
        readyToThrow = false;

        GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);

        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        Vector3 forceDirection = cam.transform.forward;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, 500f)) {
            forceDirection = (hit.point - attackPoint.position).normalized;
        }

        Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;
        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

        readyToThrow = true;
    }

}