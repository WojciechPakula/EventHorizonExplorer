using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript0 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void start()
    {
        SceneManager.LoadScene("Menu");
    }

    public void edit()
    {
        SceneManager.LoadScene("EdytorMap");
    }

    public void exp()
    {
        SceneManager.LoadScene("Eksploracja");
    }

    public void quit()
    {
        if (GameManager._textureGravity != null)
        {
            GameManager._textureGravity.Dispose();
        }
        Application.Quit();
    }
}
