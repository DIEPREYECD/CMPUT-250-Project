using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct EndingDisplay
{
    public GameEndings ending;   // Enum dropdown
    public Sprite imageToShow;    // Drag Image reference
}

public class GameEndController : MonoBehaviour
{

    public Image gameEndingPanel;
    public List<EndingDisplay> endingsToShow;
    public Button playAgainButton; // Button reference

    // Start is called before the first frame update
    void Start()
    {
        // Check which ending to show based on the GameFlowController's current ending
        GameEndings currentEnding = GameFlowController.Instance.GetEnding();
        foreach (EndingDisplay endingDisplay in endingsToShow)
        {
            if (endingDisplay.ending == currentEnding)
            {
                gameEndingPanel.sprite = endingDisplay.imageToShow;
                break;
            }
        }

        // Make the panel invisible at start
        Color panelColor = gameEndingPanel.color;
        panelColor.a = 0;
        gameEndingPanel.color = panelColor;
        // Hide the button at start
        if (playAgainButton != null)
            playAgainButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Fade in the ending panel using alpha channel also fade from black to the image
        Color panelColor = gameEndingPanel.color;
        float fadeSpeed = 0.2f;
        if (panelColor.a < 1f)
        {
            panelColor.a += Time.deltaTime * fadeSpeed; // Adjust the speed of fade-in here
            gameEndingPanel.color = panelColor;
        }

        // Show the button after the panel is fully visible
        if (panelColor.a >= 1f)
        {
            if (playAgainButton != null)
                playAgainButton.gameObject.SetActive(true);
        }
    }

    public void PlayAgain()
    {
        // Reload the main menu scene or title scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
