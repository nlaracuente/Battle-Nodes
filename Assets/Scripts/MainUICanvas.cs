using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main UI Canvas holds the messages and screens that sit
/// ontop of the entire game
/// </summary>
public class MainUICanvas : MonoBehaviour
{
    /// <summary>
    /// A reference to the title screen
    /// </summary>
    [SerializeField]
    GameObject titleScreen;

    /// <summary>
    /// A reference to the victory screen
    /// </summary>
    [SerializeField]
    GameObject victoryScreen;

    /// <summary>
    /// A reference to the defeated screen
    /// </summary>
    [SerializeField]
    GameObject defeatedScreen;

    /// <summary>
    /// Disables the title screen
    /// </summary>
	public void HideTitleScreen()
    {
        if (this.titleScreen != null) {
            this.titleScreen.SetActive(false);
        }
    }

    /// <summary>
    /// Displays the victory screen
    /// </summary>
    public void ShowVictoryScreen()
    {
        if (this.victoryScreen != null) {
            this.victoryScreen.SetActive(true);
        }
    }

    /// <summary>
    /// Displays the victory screen
    /// </summary>
    public void ShowDefeatedScreen()
    {
        if (this.defeatedScreen != null) {
            this.defeatedScreen.SetActive(true);
        }
    }
}
