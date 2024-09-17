using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AREffects : MonoBehaviour {
    
    [Header("References")]
    [SerializeField] private Transform cam;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private OpponentDetection opponentDetection;

    [Header("Objects")]
    [SerializeField] private GameObject basketball;
    [SerializeField] private GameObject soccerBall;
    [SerializeField] private GameObject volleyball;
    [SerializeField] private GameObject bowlingBall;
    [SerializeField] private GameObject rainBomb;
    [SerializeField] private GameObject rainEffect;
    [SerializeField] private GameObject shield;

    private bool readyToThrow;
    private Transform opponentTransform;

    private const float BASKETBALL_TIME = 1f;
    private const float SOCCER_BALL_TIME = 0.5f;
    private const float VOLLEYBALL_TIME = 1.5f;
    private const float BOWLING_BALL_TIME = 0.2f;
    private const float RAIN_BOMB_TIME = 1f;

    /* TESTING KEYS
    F - basketball
    G - soccer
    H - volleyball
    J - bowling
    K - rain bomb
    L - shield (to be changed to button) */


    private void Start() {
        readyToThrow = true;
        opponentTransform = opponentDetection.GetOpponentTransform();
        // GameObject test = Instantiate(shield, opponentTransform.position, cam.rotation);
        // test.SetActive(true);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.F) && readyToThrow) {
            opponentTransform = opponentDetection.GetOpponentTransform();
            Throw(opponentTransform, basketball, BASKETBALL_TIME);
        } else if (Input.GetKeyDown(KeyCode.G) && readyToThrow) {
            opponentTransform = opponentDetection.GetOpponentTransform();
            Throw(opponentTransform, soccerBall, SOCCER_BALL_TIME);
        } else if (Input.GetKeyDown(KeyCode.H) && readyToThrow) {
            opponentTransform = opponentDetection.GetOpponentTransform();
            Throw(opponentTransform, volleyball, VOLLEYBALL_TIME);
        } else if (Input.GetKeyDown(KeyCode.J) && readyToThrow) {
            opponentTransform = opponentDetection.GetOpponentTransform();
            Throw(opponentTransform, bowlingBall, BOWLING_BALL_TIME);
        } else if (Input.GetKeyDown(KeyCode.K) && readyToThrow) {
            opponentTransform = opponentDetection.GetOpponentTransform();
            Throw(opponentTransform, rainBomb, RAIN_BOMB_TIME);
            StartCoroutine(SpawnRainEffect(opponentTransform, 1.5f));
        } else if (Input.GetKeyDown(KeyCode.L)) {
            opponentTransform = opponentDetection.GetOpponentTransform();
            ShowOpponentShield(opponentTransform);
        }
    }


    public void Throw(Transform opponentTransform, GameObject objectToThrow, float timeToTarget) {
        if (opponentTransform != null) {
            readyToThrow = false;

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

            Destroy(projectile, timeToTarget + 0.5f);

            readyToThrow = true;
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
            GameObject shieldInstance = Instantiate(shield, opponentTransform.position, cam.rotation);
            shieldInstance.SetActive(true);
            shieldInstance.transform.SetParent(opponentTransform);
        }
    }

}