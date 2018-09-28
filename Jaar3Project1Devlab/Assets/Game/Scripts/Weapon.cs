﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Photon.PunBehaviour {

    [Header("Gun proporties")]
    public int gunID;
    public Transform barrelExit;
    public GameObject bulletPrefab;
    public Sprite gunCrosshair;
    public LayerMask crosshairRayMask;
    public float bulletVelocity;
    public Vector2 bulletSpread;
    public float recoil;

    [Header("Gun Sound Effects")]
    public CustomAudioClip shotSound;
    public CustomAudioClip[] reloadSounds;

    [Header("Clip proporties")]
    public int currentClip;
    public int clipMax;

    private RaycastHit hit;
    private GameObject newGameObject;

    [PunRPC]

    void Start() 
    {
        FillClip();
    }

    public virtual void ShootBullet()
    {
        if (NWManager.instance.playingMultiplayer)
        {
            if (TeamManager.instance.currentPlayer == PhotonNetwork.player)
            {
                newGameObject = PhotonNetwork.Instantiate("Bullet", barrelExit.position, bulletPrefab.transform.rotation, 0);
            }
        }
        else
        {
            newGameObject = Instantiate(bulletPrefab, barrelExit.position, bulletPrefab.transform.rotation);
        }
                
        Rigidbody rb = newGameObject.GetComponent<Rigidbody>();
        Vector2 spread = CalculatedBulletSpread();

        Vector3 bulletDirection = barrelExit.forward * bulletVelocity;
        bulletDirection.x += spread.x;
        bulletDirection.y += spread.y;
        rb.velocity = bulletDirection;
    }

    private Vector2 CalculatedBulletSpread()
    {
        float x = Random.Range(bulletSpread.x, -bulletSpread.x);
        float y = Random.Range(bulletSpread.y, -bulletSpread.y);

        Vector2 ToReturn = new Vector2(x, y);

        return ToReturn;
    }

    public void ShowCrosshair()
    {
        Debug.DrawRay(barrelExit.position, barrelExit.forward * 20, Color.red);
        Physics.Raycast(barrelExit.position, barrelExit.forward, out hit, 20, crosshairRayMask);

        if (hit.transform != null)
        {
            UIManager.instance.ShowCrosshairOnScreen(gunCrosshair, hit.point);
        }
        else
        {
            UIManager.instance.HideCrosshair();
        }
    }

    public virtual void Reload(int reloadSoundIndex)
    {
        if (currentClip != clipMax)
        {
            AudioManager.instance.PlayAudio2D(reloadSounds[reloadSoundIndex]);
            FillClip();
            TeamManager.instance.lastTeamIndex = TeamManager.instance.teamIndex;
            TeamManager.instance.NextTeam();
        }
        else
        {
            //whatever de fuck we willen als reloaden waardenloos is aka max ammo in geweer
        }
    }

    private void FillClip()
    {
        currentClip = clipMax;
    }
}
