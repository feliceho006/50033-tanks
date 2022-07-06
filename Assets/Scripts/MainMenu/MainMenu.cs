using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public void PlayGame()
    {
        Debug.Log("Play");
        SceneManager.LoadScene(0);
    }

    public void OpenHelpMenu(){
        foreach (Transform child in transform){
            if(child.CompareTag("Help")){
                child.gameObject.SetActive(true);
            }
        }
    }

    public void CloseHelpMenu(){
        foreach (Transform child in transform){
            if(child.CompareTag("Help")){
                child.gameObject.SetActive(false);
            }
        }
    }

}
