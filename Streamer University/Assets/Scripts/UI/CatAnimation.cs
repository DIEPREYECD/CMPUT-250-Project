using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatAnimation : AnimatedEntity
{
    // Timer before the user can pet the cat again
    public float petCooldown = 5.0f;
    private float petTimer = 0.0f;
    public Button catButton;
    public List<Sprite> catInterruptAnimation;

    // Start is called before the first frame update
    void Start()
    {
        AnimationSetup();
    }

    // Update is called once per frame
    void Update()
    {
        AnimationUpdate();

        // Update the pet timer
        if (petTimer < petCooldown)
        {
            petTimer += Time.deltaTime;
            catButton.enabled = false;
        }
        else
        {
            catButton.enabled = true;
        }
    }

    public void OnCatButtonClick()
    {
        if (petTimer >= petCooldown)
        {
            AudioController.Instance.PlayCatMeow();

            this.Interrupt(catInterruptAnimation);
            petTimer = 0.0f; // Reset the pet timer
        }
    }
}
