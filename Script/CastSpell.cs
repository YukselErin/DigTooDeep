using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CastSpell : NetworkBehaviour
{
    PlayerControls playerControls;

    public PlayerSpell[] playerSpells;
    public int selectedSpell;
    public ParticleSystem particleSystem;

    InputAction useSpell;
    InputAction scrollNext;
    public UnityEvent OnChannellingSpell;
    void Awake()
    {
        if (OnChannellingSpell == null)
        {
            OnChannellingSpell = new UnityEvent();
        }
        playerControls = new PlayerControls();
        useSpell = playerControls.Player.UseSpell;
        scrollNext = playerControls.Player.NextSpell;
        int itrateLen = Mathf.Min(spellIcons.Length, playerSpells.Length);
        for (int i = 0; i < itrateLen; i++)
        {
            spellIcons[i].setChargeOreType(playerSpells[i].getOreType());
        }

    }
    void OnEnable() { playerControls.Enable(); }
    void OnDisable() { playerControls.Disable(); }
    bool casting = false;
    public SpellIcon[] spellIcons;
    void selectedUIUpdate()
    {
        foreach (SpellIcon spellIcon in spellIcons)
        {
            spellIcon.deselect();
        }
        spellIcons[selectedSpell].select();
    }
    void Update()
    {
        if (!IsOwner) { return; }
        if (scrollNext.ReadValue<float>() > 0)
        {
            if (casting)
            {
                EndCastSpellRpc(selectedSpell);
                casting = false;
            }
            selectedSpell += 1;
            selectedSpell = selectedSpell % playerSpells.Length;
            selectedUIUpdate();
        }
        else if (scrollNext.ReadValue<float>() < 0)
        {
            if (casting)
            {
                EndCastSpellRpc(selectedSpell);
                casting = false;
            }
            selectedSpell -= 1;
            if (selectedSpell < 0) selectedSpell = playerSpells.Length - 1;
            selectedUIUpdate();

        }
        alphanumChangeSpell();
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (playerSpells[selectedSpell].checkCost())
            {
                CastSpellRpc(selectedSpell);
                OnChannellingSpell.Invoke();

            }


        }
        else if (Input.GetKeyUp(KeyCode.E) && casting)
        {
            EndCastSpellRpc(selectedSpell);


        }
        if (casting)
        {
            if (!playerSpells[selectedSpell].ChannelingCost()) { EndCastSpellRpc(selectedSpell); }
        }


    }
    void alphanumChangeSpell()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (casting)
            {
                EndCastSpellRpc(selectedSpell);
                casting = false;
            }
            selectedSpell = 0;
            selectedSpell = selectedSpell % playerSpells.Length;
            selectedUIUpdate();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (casting)
            {
                EndCastSpellRpc(selectedSpell);
                casting = false;
            }
            selectedSpell = 1;
            selectedSpell = selectedSpell % playerSpells.Length;
            selectedUIUpdate();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (casting)
            {
                EndCastSpellRpc(selectedSpell);
                casting = false;
            }
            selectedSpell = 2;
            selectedSpell = selectedSpell % playerSpells.Length;
            selectedUIUpdate();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (casting)
            {
                EndCastSpellRpc(selectedSpell);
                casting = false;
            }
            selectedSpell = 3;
            selectedSpell = selectedSpell % playerSpells.Length;
            selectedUIUpdate();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            if (casting)
            {
                EndCastSpellRpc(selectedSpell);
                casting = false;
            }
            selectedSpell = 4;
            selectedSpell = selectedSpell % playerSpells.Length;
            selectedUIUpdate();
        }
    }
    [Rpc(SendTo.Everyone)]
    public void CastSpellRpc(int spellIndex)
    {

        playerSpells[spellIndex].StartCast();
        casting = true;
    }

    [Rpc(SendTo.Everyone)]
    public void EndCastSpellRpc(int spellIndex)
    {
        playerSpells[spellIndex].EndCast();
        casting = false;
    }

    [Rpc(SendTo.Everyone)]
    public void ChannelingCostRpc(int spellIndex)
    {

    }
}
