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

    private const float BASKETBALL_THROW_FORCE = 10;
    private const float BASKETBALL_THROW_UPWARD_FORCE = 10;
    private const float SOCCERBALL_THROW_FORCE = 20;
    private const float SOCCERBALL_THROW_UPWARD_FORCE = 5;
    private const float VOLLEYBALL_THROW_FORCE = 15;
    private const float VOLLEYBALL_THROW_UPWARD_FORCE = 13;
    private const float BOWLINGBALL_THROW_FORCE = 25;
    private const float BOWLINGBALL_THROW_UPWARD_FORCE = 0;
    private const float RAINBOMB_THROW_FORCE = 10;
    private const float RAINBOMB_THROW_UPWARD_FORCE = 10;

    /* TESTING KEYS
    F - basketball
    G - soccer
    H - volleyball
    J - bowling
    K - rain bomb
    L - shield (to be changed to button) */


    private void Start() {
        readyToThrow = true;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.F) && readyToThrow) {
            opponentTransform = opponentDetection.GetOpponentTransform();
            Throw(opponentTransform, BASKETBALL_THROW_FORCE, BASKETBALL_THROW_UPWARD_FORCE, basketball);
        } else if (Input.GetKeyDown(KeyCode.G) && readyToThrow) {
            opponentTransform = opponentDetection.GetOpponentTransform();
            Throw(opponentTransform, SOCCERBALL_THROW_FORCE, SOCCERBALL_THROW_UPWARD_FORCE, soccerBall);
        } else if (Input.GetKeyDown(KeyCode.H) && readyToThrow) {
            opponentTransform = opponentDetection.GetOpponentTransform();
            Throw(opponentTransform, VOLLEYBALL_THROW_FORCE, VOLLEYBALL_THROW_UPWARD_FORCE, volleyball);
        } else if (Input.GetKeyDown(KeyCode.J) && readyToThrow) {
            opponentTransform = opponentDetection.GetOpponentTransform();
            Throw(opponentTransform, BOWLINGBALL_THROW_FORCE, BOWLINGBALL_THROW_UPWARD_FORCE, bowlingBall);
        } else if (Input.GetKeyDown(KeyCode.K) && readyToThrow) {
            opponentTransform = opponentDetection.GetOpponentTransform();
            Throw(opponentTransform, RAINBOMB_THROW_FORCE, RAINBOMB_THROW_UPWARD_FORCE, rainBomb);
            StartCoroutine(SpawnRainEffect(opponentTransform, 3f));
        } else if (Input.GetKeyDown(KeyCode.L)) {
            opponentTransform = opponentDetection.GetOpponentTransform();
            ShowOpponentShield(opponentTransform);
        }
    }


    public void Throw(Transform opponentTransform, float throwForce, float throwUpwardForce, GameObject objectToThrow) {
        if (opponentTransform != null) {
            readyToThrow = false;

            GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);
            projectile.SetActive(true);

            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

            // cam.transform.forward = (-0.93, -0.28, 0.23)
            Vector3 forceDirection = opponentTransform.position;

            Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;
            projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

            Debug.Log("attack point position: " + attackPoint.position);
            Debug.Log("opponent position: " + opponentTransform.position);

            readyToThrow = true;
        }
    }

    public IEnumerator SpawnRainEffect(Transform opponentTransform, float delay) {
        yield return new WaitForSeconds(delay);

        if (opponentTransform != null) {
            GameObject rainEffectInstance = Instantiate(rainEffect, opponentTransform.position, cam.rotation);
            rainEffectInstance.SetActive(true);
            Debug.Log("opponent position: " + opponentTransform.position);
            Debug.Log("rain cloud position: " + rainEffectInstance.transform.position);
        }
    }

    public void ShowOpponentShield(Transform opponentTransform) {
        if (opponentTransform != null) {
            GameObject shieldInstance = Instantiate(shield, opponentTransform.position, cam.rotation);
            shieldInstance.SetActive(true);
            shieldInstance.transform.SetParent(opponentTransform);
            Debug.Log("opponent position: " + opponentTransform.position);
            Debug.Log("shield position: " + shieldInstance.transform.position);
        }
    }

}