using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedEntity : MonoBehaviour
{
    public List<Sprite> DefaultAnimationCycle;
    public float Framerate = 12f;//frames per second
    public Image image;//image component to update

    // Give the animator the ability to split the default animation into an effective animation
    // This is mainly due to the player object having different animations based on the stress level
    public int sliceBy = 1;
    protected int effectiveSlicePortion = 0; // This represents the current slice of the animation being used
    private List<Sprite> EffectiveAnimationCycle;

    //private animation stuff
    private float animationTimer;//current number of seconds since last animation frame update
    private float animationTimerMax;//max number of seconds for each frame, defined by Framerate
    private int index;//current index in the DefaultAnimationCycle


    //interrupt animation info
    private bool interruptFlag;
    private List<Sprite> interruptAnimation;


    //Set up logic for animation stuff
    protected void AnimationSetup()
    {
        animationTimerMax = 1.0f / ((float)(Framerate));
        index = 0;

        // If the size of the default animation cycle is not divisible by the stress slice, log a warning
        if (DefaultAnimationCycle.Count % sliceBy != 0)
        {
            Debug.LogWarning("DefaultAnimationCycle size is not divisible by stressSliceBy. This may lead to unexpected behavior.");
            // Log the sizes for debugging
            Debug.LogWarning($"DefaultAnimationCycle size: {DefaultAnimationCycle.Count}, stressSliceBy: {sliceBy}");
        }

        // Get the effective animation cycle based on the stress slice
        EffectiveAnimationCycle = new List<Sprite>();
        int sliceSize = DefaultAnimationCycle.Count / sliceBy;

        for (int i = effectiveSlicePortion * sliceSize; i < (effectiveSlicePortion + 1) * sliceSize; i++)
        {
            EffectiveAnimationCycle.Add(DefaultAnimationCycle[i]);
        }
    }

    //Default animation update
    protected void AnimationUpdate()
    {
        int sliceSize = DefaultAnimationCycle.Count / sliceBy;
        //Print effective slice portion and slice size for debugging
        Debug.Log($"Effective Slice Portion: {effectiveSlicePortion}, Slice Size: {sliceSize}");
        EffectiveAnimationCycle.Clear();
        for (int i = effectiveSlicePortion * sliceSize; i < (effectiveSlicePortion + 1) * sliceSize; i++)
        {
            EffectiveAnimationCycle.Add(DefaultAnimationCycle[i]);
        }

        animationTimer += Time.deltaTime;

        if (animationTimer > animationTimerMax)
        {
            animationTimer = 0;
            index++;

            if (!interruptFlag)
            {
                if (EffectiveAnimationCycle.Count == 0 || index >= EffectiveAnimationCycle.Count)
                {
                    index = 0;
                }
                if (EffectiveAnimationCycle.Count > 0)
                {
                    image.sprite = EffectiveAnimationCycle[index];
                }
            }
            else
            {
                if (interruptAnimation == null || index >= interruptAnimation.Count)
                {
                    index = 0;
                    interruptFlag = false;
                    interruptAnimation = null;//clear interrupt animation
                }
                else
                {
                    image.sprite = interruptAnimation[index];
                }
            }
        }
    }

    //Interrupt animation
    protected void Interrupt(List<Sprite> _interruptAnimation)
    {
        interruptFlag = true;
        animationTimer = 0;
        index = 0;
        interruptAnimation = _interruptAnimation;
        image.sprite = interruptAnimation[index];
    }

}
