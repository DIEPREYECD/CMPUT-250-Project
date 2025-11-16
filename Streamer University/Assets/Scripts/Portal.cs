using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Portal : AnimatedEntity
{
    public Speeds Speed;
    public bool gravity;
    public int State;
    private Movement movement;

    // private AnimatedEntity anim;

    void Awake()
    {
        GetComponent<AnimatedEntity>();
    }

    void Start()
    {
        AnimationSetup();
    }

    void Update()
    {
        AnimationUpdate();
    }

    public void initiatePortal(Movement movement) {
        movement.ChangeThroughPortal(Speed, gravity ? 1 : -1, State, transform.position.y);
    }

}
