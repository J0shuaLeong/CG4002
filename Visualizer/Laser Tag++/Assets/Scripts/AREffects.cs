using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AREffects : MonoBehaviour {
    
    [Header("References")]
    [SerializeField] private Transform cam;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private OpponentDetection opponentDetection;

    [Header("Objects")]
    [SerializeField] private GameObject rainEffect;
    [SerializeField] private GameObject shield;

    private Transform opponentTransform;
    private GameObject currentShield;


    private void Start() {
        opponentTransform = opponentDetection.GetOpponentTransform();
        // GameObject test = Instantiate(shield, opponentTransform.position, cam.rotation);
        // test.SetActive(true);
    }


    public void Throw(Transform opponentTransform, GameObject objectToThrow, float timeToTarget) {
        if (opponentTransform != null) {

            GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);
            projectile.SetActive(true);

            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

            Vector3 direction = opponentTransform.position - attackPoint.position;
            Vector3 horizontalDirection = new Vector3(direction.x, 0f, direction.z);

            float horizontalDistance = horizontalDirection.magnitude;
            float verticalDistance = direction.y;

            float horizontalVelocity = horizontalDistance / timeToTarget;
            float verticalVelocity = (verticalDistance + 0.5f * Mathf.Abs(Physics.gravity.y) * Mathf.Pow(timeToTarget, 2)) / timeToTarget;

            Vector3 forceToAdd = horizontalDirection.normalized * horizontalVelocity + transform.up * verticalVelocity;

            projectileRb.AddForce(forceToAdd, ForceMode.VelocityChange);

            Destroy(projectile, timeToTarget);
        }
    }


    public IEnumerator SpawnRainEffect(Transform opponentTransform, float delay) {
        yield return new WaitForSeconds(delay);

        if (opponentTransform != null) {
            Vector3 rainEffectPosition = new Vector3(opponentTransform.position.x, opponentTransform.position.y, opponentTransform.position.z - 1.5f);
            GameObject rainEffectInstance = Instantiate(rainEffect, rainEffectPosition, cam.rotation);
            rainEffectInstance.SetActive(true);
        }
    }

    public void ShowOpponentShield(Transform opponentTransform) {
        if (opponentTransform != null) {
            currentShield = Instantiate(shield, opponentTransform.position, cam.rotation);
            currentShield.SetActive(true);
            currentShield.transform.SetParent(opponentTransform);
        }
    }

    public void RemoveOpponentShield() {
        if (currentShield != null) {
            Destroy(currentShield);
            currentShield = null;
        }
    }

}