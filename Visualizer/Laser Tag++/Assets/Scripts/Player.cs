using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This code will be moved to the Ultra96 which will host the game engine
// TODO: rethink 2 player logic (this version is only for testing UI on unity)
public class Player : MonoBehaviour {

    public int HP = 100;
    public int ShieldHP = 0;
    public int ShieldCount = 3;
    public int Score = 0;
    public int Ammo = 6;
    public int RainBombCount = 2;


    public GameUI gameUI;


    /* reloads the player's magazine */
    public void Reload() {
        if (Ammo == 0) {
            Ammo = 6;
            gameUI.UpdateAmmoCount();
        }
    }

}