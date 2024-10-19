using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class AREffects : MonoBehaviour {
    
    [Header("References")]
    [SerializeField] private Transform cam;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private OpponentDetection opponentDetection;
    [SerializeField] private GameEngine gameEngine;

    [Header("Objects")]
    [SerializeField] private GameObject bullets;
    [SerializeField] private GameObject rainCloud;
    [SerializeField] private GameObject rainEffect;
    [SerializeField] private GameObject playerShield;
    [SerializeField] private GameObject opponentShield;
    [SerializeField] private GameObject playerHitEffect;
    [SerializeField] private GameObject opponentThrowHitEffect;
    [SerializeField] private GameObject opponentBulletHitEffect;
    [SerializeField] private GameObject test; // FOR TESTING

    private GameObject currentPlayerShield;
    private GameObject currentOpponentShield;
    private GameObject rain;
    private Transform opponentTransform;
    private List<Vector3> rainEffectPositions = new List<Vector3>();

    private bool hasTakenDamageForFirstBomb = false;
    private bool hasTakenDamageForSecondBomb = false;


    private void Start() {
        opponentTransform = opponentDetection.GetOpponentTransform();
    }

    private void Update() {
        opponentTransform = opponentDetection.GetOpponentTransform();
        // CheckIfOpponentStepsInRainBomb();
    }


    // -------------------- Throw Projectile --------------------

    public void Throw(GameObject objectToThrow, float timeToTarget) {
        // TODO: add throw case where opponent transform is null - throw to center
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

            StartCoroutine(SpawnOpponentThrowHitEffect(projectile, opponentTransform.position, timeToTarget));
        }
    }


    // -------------------- Rain Bomb --------------------

    public IEnumerator SpawnRainCloud(float delay) {
        // TODO: add spawn case where opponent transform is null - spawn at center
        // TODO: fix rain cloud positioning
        yield return new WaitForSeconds(delay);

        Transform fixedTransform = opponentTransform;

        if (opponentTransform != null) {
            Vector3 rainEffectPosition = new Vector3(opponentTransform.position.x - 0.7f, opponentTransform.position.y, opponentTransform.position.z);

            rainEffectPositions.Add(rainEffectPosition);

            GameObject rainEffectInstance = Instantiate(rainCloud, rainEffectPosition, cam.rotation);
            rainEffectInstance.SetActive(true);
            rainEffectInstance.transform.SetParent(fixedTransform);

            hasTakenDamageForFirstBomb = false;
            hasTakenDamageForSecondBomb = false;
        }
    }

    private void CheckIfOpponentStepsInRainBomb() {
        // TODO: update logic to accommodate for unlimited rain bombs on arena not max 2
        // TODO: use collider instead to check for collisions ?
        if (opponentTransform != null) {
            for (int i = 0; i < rainEffectPositions.Count; i++) {
                float distance = Vector3.Distance(opponentTransform.position, rainEffectPositions[i]);

                if (i == 0 && distance <= 1f && !hasTakenDamageForFirstBomb) {
                    hasTakenDamageForFirstBomb = true;
                    gameEngine.OpponentRainEffect();
                }

                if (i == 1 && distance <= 1f && !hasTakenDamageForSecondBomb) {
                    hasTakenDamageForSecondBomb = true;
                    gameEngine.OpponentRainEffect();
                }

                if (distance > 1f) {
                    if (i == 0) {
                        hasTakenDamageForFirstBomb = false;
                        RemoveRainEffect();
                    }
                    if (i == 1) {
                        hasTakenDamageForSecondBomb = false;
                        RemoveRainEffect();
                    }
                }
            }
        }
    }

    public void SpawnRainEffect() {
        Transform fixedTransform = opponentTransform;

        rain = Instantiate(rainEffect, fixedTransform.position, cam.rotation);
        rain.SetActive(true);

        rain.transform.SetParent(fixedTransform);

        rain.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
        rain.transform.localPosition = new Vector3(0f, 0f, 1f);
        rain.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
    }

    public void RemoveRainEffect() {
        if (rain != null) {
            Destroy(rain);
            rain = null;
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


    // -------------------- Hit Effects --------------------

    public void SpawnPlayerHitEffect() {
        GameObject hit = Instantiate(playerHitEffect, cam.position, cam.rotation);

        hit.transform.SetParent(cam);
        hit.transform.localPosition = new Vector3(0f, -2f, 0f);
        hit.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        hit.SetActive(true);
    }

    public void SpawnOpponentBulletHitEffect() {
        GameObject hit = Instantiate(opponentBulletHitEffect, opponentTransform.position, cam.rotation);

        hit.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

        hit.SetActive(true);
    }

    private IEnumerator SpawnOpponentThrowHitEffect(GameObject projectile, Vector3 targetPosition, float delay) {
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