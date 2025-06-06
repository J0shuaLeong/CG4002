using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class AREffects : MonoBehaviour {

    // Serialized Fields
    [Header("References")]
    [SerializeField] private Transform cam;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private OpponentDetection opponentDetection;
    [SerializeField] private GameEngine gameEngine;

    [Header("Objects")]
    [SerializeField] private GameObject bullets;
    [SerializeField] private GameObject rainCloud;
    [SerializeField] private GameObject playerRainEffect;
    [SerializeField] private GameObject playerShield;
    [SerializeField] private GameObject opponentShield;
    [SerializeField] private GameObject playerHitEffect;
    [SerializeField] private GameObject opponentThrowHitEffect;
    [SerializeField] private GameObject opponentBulletHitEffect;

    // Game Objects
    private GameObject currentPlayerShield;
    private GameObject currentOpponentShield;
    private GameObject currentPlayerRainEffect;

    // Variables
    private Transform opponentTransform;
    private bool opponentHasShield;


    private void Start() {
        opponentTransform = opponentDetection.GetOpponentTransform();

        opponentHasShield = false;
    }

    private void Update() {
        // get opponent transform
        opponentTransform = opponentDetection.GetOpponentTransform();


        // check for shields
        if (opponentHasShield && opponentTransform != null)
        {
            if (currentOpponentShield == null)
            {
                currentOpponentShield = Instantiate(opponentShield, opponentTransform.position, Quaternion.identity);
                currentOpponentShield.SetActive(true);
                currentOpponentShield.transform.SetParent(opponentTransform);
            }

            currentOpponentShield.transform.position = opponentTransform.position;
        }
        else
        {
            if (currentOpponentShield != null)
            {
                Destroy(currentOpponentShield);
            }
        }
    }


    // -------------------- Throw Projectile --------------------

    public void Throw(GameObject objectToThrow, float timeToTarget) {
        Vector3 targetPosition;

        if (opponentTransform != null) {
            targetPosition = opponentTransform.position;
        } else {
            float distanceFromCamera = 2f;
            targetPosition = cam.transform.position + cam.transform.forward * distanceFromCamera;
        }

        GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);
        projectile.SetActive(true);

        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        Vector3 direction = targetPosition - attackPoint.position;
        Vector3 horizontalDirection = new Vector3(direction.x, 0f, direction.z);

        float horizontalDistance = horizontalDirection.magnitude;
        float verticalDistance = direction.y;

        float horizontalVelocity = horizontalDistance / timeToTarget;
        float verticalVelocity = (verticalDistance + 0.5f * Mathf.Abs(Physics.gravity.y) * Mathf.Pow(timeToTarget, 2)) / timeToTarget;

        Vector3 forceToAdd = horizontalDirection.normalized * horizontalVelocity + transform.up * verticalVelocity;

        projectileRb.AddForce(forceToAdd, ForceMode.VelocityChange);

        StartCoroutine(ShowOpponentThrowHitEffect(projectile, targetPosition, timeToTarget));
    }


    // -------------------- Rain Bomb --------------------

    public IEnumerator SpawnOpponentRainCloud(float delay, Transform currentOpponentTransform) {
        yield return new WaitForSeconds(delay);

        if (currentOpponentTransform != null) {
            Vector3 rainCloudPosition = new Vector3(currentOpponentTransform.position.x, currentOpponentTransform.position.y, currentOpponentTransform.position.z);

            GameObject cloud = Instantiate(rainCloud, rainCloudPosition, cam.rotation);
            cloud.SetActive(true);

            Debug.Log("Rain Bomb instantiated at: " + rainCloudPosition);
        }
    }

    public void ShowPlayerRainEffect() {
        currentPlayerRainEffect = Instantiate(playerRainEffect, cam.position, cam.rotation);

        currentPlayerRainEffect.transform.SetParent(cam);
        currentPlayerRainEffect.transform.localPosition = Vector3.zero;
        currentPlayerRainEffect.transform.localRotation = Quaternion.identity;

        currentPlayerRainEffect.SetActive(true);
    }

    public void RemovePlayerRainEffect() {
        if (currentPlayerRainEffect != null) {
            Destroy(currentPlayerRainEffect);
            currentPlayerRainEffect = null;
        }
    }


    // -------------------- Shield --------------------

    public void ShowPlayerShield() {
        currentPlayerShield = Instantiate(playerShield, cam.position, cam.rotation);

        currentPlayerShield.transform.SetParent(cam);
        currentPlayerShield.transform.localPosition = Vector3.zero;
        currentPlayerShield.transform.localRotation = Quaternion.identity;

        currentPlayerShield.SetActive(true);
    }

    public void RemovePlayerShield() {
        if (currentPlayerShield != null) {
            Destroy(currentPlayerShield);
            currentPlayerShield = null;
        }
    }

    public void ShowOpponentShield() {
        opponentHasShield = true;
    }

    public void RemoveOpponentShield() {
        if (currentOpponentShield != null) {
            Destroy(currentOpponentShield);
            currentOpponentShield = null;
            opponentHasShield = false;
        }
    }


    // -------------------- Hit Effects --------------------

    public void ShowPlayerHitEffect() {
        GameObject hit = Instantiate(playerHitEffect, cam.position, cam.rotation);

        hit.transform.SetParent(cam);
        hit.transform.localPosition = new Vector3(0f, -2f, 0f);
        hit.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        hit.SetActive(true);
    }

    public void ShowOpponentBulletHitEffect() {
        GameObject hit = Instantiate(opponentBulletHitEffect, opponentTransform.position, cam.rotation);

        hit.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

        hit.SetActive(true);
    }

    private IEnumerator ShowOpponentThrowHitEffect(GameObject projectile, Vector3 targetPosition, float delay) {
        yield return new WaitForSeconds(delay);

        GameObject hit = Instantiate(opponentThrowHitEffect, targetPosition, cam.rotation);
        hit.SetActive(true);

        Destroy(projectile);
    }


    // -------------------- Reload --------------------

    public void ShowReloadAnimation() {
        StartCoroutine(FlyBulletsOnReload());
    }

    private IEnumerator FlyBulletsOnReload() {
        for (int i = 0; i < 6; i++) {
            GameObject bullet = Instantiate(bullets, attackPoint.position, cam.rotation);
            bullet.SetActive(true);

            bullet.transform.SetParent(cam);

            Vector3 endPoint = new Vector3(attackPoint.position.x + 3f, attackPoint.position.y, attackPoint.position.z);
            StartCoroutine(MoveBullet(bullet, endPoint));

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator MoveBullet(GameObject bullet, Vector3 targetPosition) {
        float time = 0;

        Vector3 startPosition = bullet.transform.position;

        while (time < 1f) {
            time += Time.deltaTime * 2f;
            bullet.transform.position = Vector3.Lerp(startPosition, targetPosition, time);
            yield return null;
        }

        Destroy(bullet);
    }

}