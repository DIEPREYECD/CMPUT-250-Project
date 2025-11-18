using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Portal : AnimatedSprite
{
    public Speeds Speed;
    public bool gravity;
    public int State;
    private Movement movement;


    public void initiatePortal(Movement movement) {
        movement.ChangeThroughPortal(Speed, gravity ? 1 : -1, State, transform.position.y);
    }

}
