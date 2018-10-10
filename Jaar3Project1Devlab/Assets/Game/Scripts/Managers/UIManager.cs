﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public static UIManager instance;

    public Image crosshairImage;
    public GameObject sniperScope;

    public UI_SoldierStatus SoldierStatusOnUI;
    public List<Image> weaponIcons = new List<Image>();

    [Header("Windows")]
    public RectTransform soldierStatusWindow;
    public RectTransform weaponIconWindow;

    [Header("Prefabs")]
    public GameObject teamButtonPrefab;
    public GameObject soldierButtonPrefab;
    public GameObject weaponIconPrefab;

    [Header("Parents")]
    public RectTransform teamButtonsParent;
    public RectTransform soldierButtonsParent;
    public RectTransform weaponIconParent;

    private List<RectTransform> activeWindows = new List<RectTransform>();
    private List<Button> soldiersInTeamButtons = new List<Button>();
    public List<UI_SoldierStatus> worldSpaceStatuses = new List<UI_SoldierStatus>();

    [HideInInspector]
    internal bool showCroshair = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Toggles a window in the UI. 
    /// </summary>
    /// <param name="windowToShow"></param>
    public void ToggleWindow(RectTransform windowToShow, bool toggle)
    {
        if (toggle)
        {
            windowToShow.gameObject.SetActive(true);
            activeWindows.Add(windowToShow);
        }
        else
        {
            windowToShow.gameObject.SetActive(false);
            activeWindows.Remove(windowToShow);
        }
    }

    /// <summary>
    /// Hides all windows currently active on the UI.
    /// <para>Use the e.g. for opening a menu</para>
    /// </summary>
    public void HideAllWindows()
    {
        if (activeWindows.Count > 0)
        {
            foreach (RectTransform window in activeWindows)
            {
                window.gameObject.SetActive(false);
            }

            activeWindows.Clear();
        }
        else
        {
            Debug.LogWarning("You are trying to hide all windows, but there are no windows active");
        }
    }

    /// <summary>
    /// Lets the weapon Icons appear on the UI. Use this when changing the camera to Soldier View, or when the available weapons change.
    /// </summary>
    /// <param name="availableWeapons"></param>
    public void InstantiateWeaponIcons(List<GameObject> availableWeapons)
    {
        //Clears the existing weapon icons so that the new ones can be instantiated
        foreach(Image i in weaponIcons)
        {
            Destroy(i.gameObject);
        }

        weaponIcons.Clear();

        //New weapon Icons are instantiated
        for (int i = 0; i < availableWeapons.Count; i++)
        {
            GameObject newObject = Instantiate(weaponIconPrefab, weaponIconParent.transform.position, weaponIconPrefab.transform.rotation);
            newObject.transform.SetParent(weaponIconParent, false);
            Image weaponIcon = newObject.GetComponentInChildren<Image>();
            //weaponIcon.sprite = availableWeapons[i].icon;
            weaponIcons.Add(weaponIcon);
        }
    }

    /// <summary>
    /// Shows a buttons for every team.
    /// </summary>
    /// <param name="teams"></param>
    public void InstantiateStatusButtons(List<Team> teams)
    {
        for (int i = 0; i < teams.Count; i++)
        {
            int delegateIndex = 0;

            GameObject newObject = Instantiate(teamButtonPrefab, teamButtonsParent.position, teamButtonPrefab.transform.rotation);
            newObject.transform.SetParent(teamButtonsParent, false);

            Button b = newObject.GetComponent<Button>();
            Text text = b.transform.GetChild(0).GetComponent<Text>();
            text.text = "Team " + (i + 1);

            delegateIndex = i;
            b.onClick.AddListener(delegate { ShowSoldierButtons(teams[delegateIndex].allSoldiers, teams[delegateIndex].thisTeamColor); }); 
        }
    }

    /// <summary>
    /// Shows buttons for every soldier in the overload. 
    /// </summary>
    /// <param name="soldiersToShow"></param>
    /// <param name="teamColor"></param>
    public void ShowSoldierButtons(List<Soldier> soldiersToShow, Color teamColor)
    {
        if (soldiersInTeamButtons.Count > 0)
        {
            foreach(Button b in soldiersInTeamButtons)
            {
                Destroy(b.gameObject);
            }

            soldiersInTeamButtons.Clear();
        }

        for (int i = 0; i < soldiersToShow.Count; i++)
        {
            int delegateIndex = 0;

            GameObject newObject = Instantiate(soldierButtonPrefab, soldierButtonsParent.position, soldierButtonsParent.rotation);
            newObject.transform.SetParent(soldierButtonsParent, false);

            Button b = newObject.GetComponent<Button>();
            Text text = b.transform.GetChild(0).GetComponent<Text>();
            text.text = soldiersToShow[i].soldierName;

            delegateIndex = i;
            b.onClick.AddListener(delegate { SetActiveSoldierStatus(soldiersToShow[delegateIndex], teamColor); });
            soldiersInTeamButtons.Add(b);
        }
    }
    
    /// <summary>
    /// Updates the Soldier Icon in the solder status window
    /// </summary>
    /// <param name="toShow"></param>
    /// <param name="teamColor"></param>
    public void SetActiveSoldierStatus(Soldier toShow, Color teamColor)
    {
        SoldierStatusOnUI.UpdateStatus(toShow, teamColor);
    }

    /// <summary>
    /// Updates all the soldier statuses in the world space canvases on each soldier. This function will toggle the minimalism function on the status.
    /// </summary>
    /// <param name="teams"></param>
    public void UpdateWorldSpaceStatuses(List<Team> teams)
    {
        foreach(Team t in teams)
        {
            foreach(Soldier s in t.allSoldiers)
            {
                UI_SoldierStatus status = s.GetComponentInChildren<UI_SoldierStatus>(true);

                if (!status.minimal)
                {
                    status.ToggleMinimalism(true);
                }

                status.UpdateStatus(s, t.thisTeamColor);

                if (!worldSpaceStatuses.Contains(status))
                {
                    worldSpaceStatuses.Add(status);
                }
            }
        }
    }

    /// <summary>
    /// Toggles the world space statuses of every soldier.
    /// </summary>
    /// <param name="toggle"></param>
    public void ToggleWorldSpaceStatuses(bool toggle)
    {
        foreach(UI_SoldierStatus status in worldSpaceStatuses)
        {
            status.gameObject.SetActive(toggle);
        }
    }

    /// <summary>
    /// Shows the crosshair of the active player on screen. 
    /// </summary>
    /// <param name="toShow"></param>
    /// <param name="position"></param>
    public void ShowCrosshairOnScreen(Sprite toShow, Vector3 position)
    {
        if(showCroshair)
        {   
            crosshairImage.sprite = toShow;
            crosshairImage.enabled = true;
            crosshairImage.transform.position = Camera.main.WorldToScreenPoint(position);
        }
    }

    /// <summary>
    /// Hides the crosshair that was shown by ShowCrosshairOnScreen().
    /// <para>Use this e.g. for going to Top View</para>
    /// </summary>
    public void HideCrosshair()
    {
        crosshairImage.enabled = false;
    }

    public void ToggleScope()
    {
        if(sniperScope.activeSelf == true)
        {
            sniperScope.SetActive(false);
        }
        else
        {
            sniperScope.SetActive(true);
        }
    }
}
