using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpell : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }
    public virtual int getOreType()
    {
        Debug.Log("Returning base path");
        return -1;
    }
    // Update is called once per frame
    public virtual void StartCast()
    {

    }
    public virtual void EndCast()
    {

    }
    public virtual bool ChannelingCost()
    {
        return false;
    }
    public virtual bool checkCost()
    {
        return false;
    }
}
