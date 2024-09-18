using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class AREffects : MonoBehaviour {
    
    [Header("References")]
    [SerializeField] private Transform cam;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private OpponentDetection opponentDetection;

    [Header("Objects")]
    [SerializeField] private GameObject rainEffect;
    [SerializeField] private GameObject playerShield;
    [SerializeField] private GameObject opponentShield;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject test; // FOR TESTING

    private GameObject currentPlayerShield;
    private GameObject currentOpponentShield;


    private void Start() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();
        GameObject testObject = Instantiate(test, opponentTransform.position, cam.rotation);
        testObject.SetActive(true);
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

            StartCoroutine(SpawnHitEffect(projectile, opponentTransform.position, timeToTarget));
        }
    }

    private IEnumerator SpawnHitEffect(GameObject projectile, Vector3 targetPosition, float delay) {
        yield return new WaitForSeconds(delay);

        GameObject hit = Instantiate(hitEffect, targetPosition, cam.rotation);
        hit.SetActive(true);

        Destroy(projectile);
    }

    public IEnumerator SpawnRainEffect(Transform opponentTransform, float delay) {
        yield return new WaitForSeconds(delay);

        if (opponentTransform != null) {
            Vector3 rainEffectPosition = new Vector3(opponentTransform.position.x, opponentTransform.position.y, opponentTransform.position.z - 1.5f);
            GameObject rainEffectInstance = Instantiate(rainEffect, rainEffectPosition, cam.rotation);
            rainEffectInstance.SetActive(true);
        }
    }

    public void ShowPlayerShield() {
        currentPlayerShield = Instantiate(playerShield, new Vector3(0f, 0f, 0f), cam.rotation);
        currentPlayerShield.SetActive(true);
    }

    public void RemovePlayerShield() {
        if (currentPlayerShield != null) {
            Destroy(currentPlayerShield);
            currentPlayerShield = null;
        }
    }

    public void ShowOpponentShield(Transform opponentTransform) {
        if (opponentTransform != null) {
            currentOpponentShield = Instantiate(opponentShield, opponentTransform.position, cam.rotation);
            currentOpponentShield.SetActive(true);
            currentOpponentShield.transform.SetParent(opponentTransform);
            currentOpponentShield.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            currentOpponentShield.transform.localPosition = new Vector3(0f, -1f, 0f);
        }
    }

    public void RemoveOpponentShield() {
        if (currentOpponentShield != null) {
            Destroy(currentOpponentShield);
            currentOpponentShield = null;
        }
    }

}