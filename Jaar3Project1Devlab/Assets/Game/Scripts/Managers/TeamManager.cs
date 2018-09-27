﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamManager : Photon.PunBehaviour {

    public static TeamManager instance;
    private bool runningIenumerator;

    [Header("Camera proporties")]
    public CameraMovement mainCamera;
    public Transform cameraPositionSky;
    public float movementSpeed;

    [Header("Team Proporties")]
    public int teamIndex;
    public List<Team> allTeams = new List<Team>();

    [Header("Networking")]
     public PhotonPlayer currentPlayer;

    public List<Color> playerColors = new List<Color>();
    public Image playerAvatar; //Temporary

    [Header("Turn Properties")]
    public float maxTurnTime = 60;
    private float turnTime;
    private float tempFloat;
    public Text timeText;
    public Image turnTimerCircle;
    private bool countingDown;

    public Color circleStartColor;
    public Color circleEndColor;

    private void Awake()
    {
        //Makes a Singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        mainCamera = FindObjectOfType<CameraMovement>();

        mainCamera.transform.root.position = cameraPositionSky.position;
        mainCamera.transform.root.rotation = Quaternion.LookRotation(cameraPositionSky.forward);
    }

    private void Start()
    {
        if(currentPlayer == null)
        {
            currentPlayer = PhotonNetwork.masterClient;
        }

        if(playerAvatar != null)
        {
            playerAvatar.color = new Color(playerColors[PhotonNetwork.player.ID - 1].r, playerColors[PhotonNetwork.player.ID - 1].g, playerColors[PhotonNetwork.player.ID - 1].b, 1); //Temporary client identification
        }

        turnTime = maxTurnTime;
        timeText.text = "" + maxTurnTime;
        turnTimerCircle.fillAmount = 1;
        turnTimerCircle.color = circleStartColor;

        tempFloat = maxTurnTime;
    }

    void Update()
    {
        if(mainCamera.cameraState == CameraMovement.CameraStates.Topview)
        {
            if(NWManager.instance.playingMultiplayer)
            {
                if(Input.GetButtonDown("Enter") && currentPlayer == PhotonNetwork.player)
                {
                    Debug.Log("TeamIndex = " + teamIndex);

                    photonView.RPC("ToSoldier", PhotonTargets.All);

                    
                }
            }
            else
            {
                if(Input.GetButtonDown("Enter"))
                {
                    ToSoldier();

                }
            }
        }
        if(mainCamera.cameraState == CameraMovement.CameraStates.ThirdPerson)
        {
            if(NWManager.instance.playingMultiplayer)
            {
                if (Input.GetKeyDown("n") && currentPlayer == PhotonNetwork.player)
                {
                    CallNextTurn();
                    mainCamera.transform.parent.GetComponent<PhotonView>().TransferOwnership(currentPlayer);
                    mainCamera.transform.GetComponent<PhotonView>().TransferOwnership(currentPlayer);
                    photonView.RPC("NextTeam", PhotonTargets.All);
                }
            }
            else if(Input.GetKeyDown("n"))
            {
                NextTeam();
            }
        }

        if(currentPlayer == PhotonNetwork.player)
        {
            TurnTimerCircle();
        }

        if (!NWManager.instance.playingMultiplayer)
        {
            TurnTimerCircleHotseat();
        }

      
    }

    [PunRPC]
    void InvokeRepeat()
    {
        Debug.Log("Invoked");
        if (!countingDown)
        {
            Debug.Log("InvokedAfterIfBool");
            InvokeRepeating("TurnTimer", 1, 1);
        }
    }

    [PunRPC]
    void CancelRepeat()
    {
        CancelInvoke();
    }

    [PunRPC]
    void ResetTimer()
    {
        turnTime = maxTurnTime;
        tempFloat = maxTurnTime;
        turnTimerCircle.fillAmount = 1;

        timeText.text = "" + maxTurnTime;
        turnTimerCircle.color = circleStartColor;
    }

    [PunRPC]
    void ResetCountingDown()
    {
        countingDown = false;
    }

    void TurnTimer()
    {
        countingDown = true;
        if(turnTime > 0)
        {
            turnTime--;

            if (NWManager.instance.playingMultiplayer)
            {
                photonView.RPC("TempFloatRPC", PhotonTargets.Others);
            }

        }
        if(turnTime <= 0)
        {
            if (NWManager.instance.playingMultiplayer)
            {
                photonView.RPC("CancelRepeat", PhotonTargets.All);
                photonView.RPC("ResetTimer", PhotonTargets.All);

            }
            else
            {
                CancelInvoke();
            }
            turnTime = maxTurnTime;
            if (NWManager.instance.playingMultiplayer)
            {
                CallNextTurn();
                mainCamera.transform.parent.GetComponent<PhotonView>().TransferOwnership(currentPlayer);
                mainCamera.transform.GetComponent<PhotonView>().TransferOwnership(currentPlayer);
                photonView.RPC("NextTeam", PhotonTargets.All);
            }
            else
            {
                NextTeam();
            }
         
        }
        int seconds = Mathf.FloorToInt(turnTime);

        if (!NWManager.instance.playingMultiplayer)
        {
           // turnTimerCircle.fillAmount = 1;
            timeText.text = "" + seconds;
            //turnTimerCircle.color = circleStartColor;
        }
        
    }

    [PunRPC]
    void TempFloatRPC()
    {
        if(tempFloat > 0)
        {
            tempFloat--;
            turnTimerCircle.fillAmount = tempFloat / maxTurnTime;
            timeText.text = "" + tempFloat;
        }
        if(tempFloat <= 0)
        {
            tempFloat = maxTurnTime;
            turnTimerCircle.fillAmount = 1;
            timeText.text = "" + maxTurnTime;
            turnTimerCircle.color = circleStartColor;
        }

    }

    void TurnTimerCircleHotseat()
    {

        turnTimerCircle.fillAmount = turnTime / maxTurnTime;

        float t = turnTime / maxTurnTime;

        Color color = Color.Lerp(circleEndColor, circleStartColor, t);
        turnTimerCircle.color = color;

        timeText.text = "" + turnTime;
    }

    void TurnTimerCircle()
    {

        turnTimerCircle.fillAmount = tempFloat / maxTurnTime;

        float t = tempFloat / maxTurnTime;

        Color color = Color.Lerp(circleEndColor, circleStartColor, t);
        turnTimerCircle.color = color;

        timeText.text = "" + tempFloat;
    }

    /// <summary>
    /// Sets the active team to the next in the list.
    /// <para>Call NextSoldier() in the active team to set the next soldier in that team </para>
    /// </summary>
    [PunRPC]
    public void NextTeam()
    {
        if(NWManager.instance.playingMultiplayer)
        {
            photonView.RPC("ToTopView", PhotonTargets.All);
            photonView.RPC("ResetCountingDown", PhotonTargets.All);
        }
        else
        {
            ToTopView();
            ResetCountingDown();
        } 
    }

    /// <summary>
    /// Call this to move the MainCamera to the current soldier of the current team.
    /// </summary>
    [PunRPC]
    public void ToSoldier()
    {
        if(NWManager.instance.playingMultiplayer)
        {
            if(currentPlayer == PhotonNetwork.player)
            {
                mainCamera.transform.parent.GetComponent<PhotonView>().TransferOwnership(currentPlayer);
                mainCamera.transform.GetComponent<PhotonView>().TransferOwnership(currentPlayer);
            }
        }
        
        //pakt de positie waar de camera heen moet gaan
        int soldierIndex = allTeams[teamIndex].soldierIndex;
        Transform playerCamPos = allTeams[teamIndex].allSoldiers[soldierIndex].thirdPersonCamPos;
        mainCamera.transform.parent.SetParent(playerCamPos);

        StartCoroutine(MoveCam(playerCamPos.position, playerCamPos.rotation, CameraMovement.CameraStates.ThirdPerson));

         if(NWManager.instance.playingMultiplayer)
        {
            if (currentPlayer == PhotonNetwork.player)
            {
                allTeams[teamIndex].allSoldiers[soldierIndex].GetComponent<Movement>().canMove = true;
                allTeams[teamIndex].allSoldiers[soldierIndex].GetComponent<PhotonView>().RequestOwnership();


            }
        }
    }

    /// <summary>
    /// Call this to move the MainCamera to the TopView.
    /// </summary>
    [PunRPC]
    public void ToTopView()
    {
        mainCamera.transform.parent.SetParent(null);

       if(NWManager.instance.playingMultiplayer)
       {
           photonView.RPC("MoveCam", PhotonTargets.All, cameraPositionSky.position,cameraPositionSky.rotation, CameraMovement.CameraStates.Topview);
       }
       else
       {
           StartCoroutine(MoveCam(cameraPositionSky.position,cameraPositionSky.rotation,CameraMovement.CameraStates.Topview));
       }
    }

    /// <summary>
    /// Moves the main camera to the overloaded position. Set camState to the state the camera should be in when it arrives.
    /// </summary>
    /// <param name="moveTo"></param>
    /// <param name="camState"></param>
    /// <returns></returns>
    [PunRPC]
    public IEnumerator MoveCam(Vector3 moveTo,Quaternion rotateTo, CameraMovement.CameraStates camState)
    {
        if(!runningIenumerator)
        {
            runningIenumerator = true;
            mainCamera.cameraState = CameraMovement.CameraStates.Idle;

            if(camState == CameraMovement.CameraStates.Topview)
            {
                //mainCamera.transform.parent.rotation = Quaternion.identity;
                Soldier soldier = allTeams[teamIndex].allSoldiers[allTeams[teamIndex].soldierIndex];
                soldier.soldierMovement.canMove = false;
                soldier.isActive = false;

                if (teamIndex + 1 < allTeams.Count  )
                {
                    teamIndex += 1;
                }
                else
                {
                    teamIndex = 0;
                    allTeams[teamIndex].NextSoldier();
                }
                if(allTeams[teamIndex].teamAlive == false)
                {
                    NextTeam();
                }   
            }
            print(mainCamera.transform.parent.rotation);
            while (mainCamera.transform.parent.position != moveTo && mainCamera.transform.rotation != rotateTo)
            {
                mainCamera.transform.parent.position = Vector3.MoveTowards(mainCamera.transform.parent.position, moveTo, movementSpeed * 0.01f);
                mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, rotateTo, movementSpeed / 5 * 0.01f);

                if(camState == CameraMovement.CameraStates.ThirdPerson)
                {
                    Quaternion test = Quaternion.Euler(mainCamera.transform.parent.eulerAngles.x,rotateTo.eulerAngles.y,rotateTo.eulerAngles.z);
                    mainCamera.transform.parent.rotation = Quaternion.Lerp(mainCamera.transform.parent.rotation, test, movementSpeed / 3 * 0.01f);
                }
                if(camState == CameraMovement.CameraStates.Topview)
                {
                    mainCamera.transform.parent.rotation = Quaternion.Lerp(mainCamera.transform.parent.rotation, Quaternion.identity,movementSpeed / 3 * 0.01f);
                }
                yield return null;
                
            }

            if(camState == CameraMovement.CameraStates.ThirdPerson)
            {
                Soldier soldier = allTeams[teamIndex].allSoldiers[allTeams[teamIndex].soldierIndex];
                soldier.soldierMovement.canMove = true;
                soldier.isActive = true;
                mainCamera.xRotInput = mainCamera.baseXRotInput;

                Debug.Log("IEnumerator");


                InvokeRepeat();

                //Start the turn timmy
            }

            mainCamera.cameraState = camState;
            
            runningIenumerator = false;
        }

        yield return null;
    }

    public void CallNextTurn()
    {
        photonView.RPC("NextTurn", PhotonTargets.All);
    }

    [PunRPC]
    void NextTurn()
    {
            Debug.Log("StartOfNexTurn()");


            currentPlayer = PhotonNetwork.player.GetNextFor(currentPlayer);
            Debug.Log(currentPlayer.NickName + currentPlayer.ID);

            Debug.Log("EndOfNexTurn()");
    }

  
}
