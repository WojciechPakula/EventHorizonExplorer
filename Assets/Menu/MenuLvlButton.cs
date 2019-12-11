using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class MenuLvlButton : MonoBehaviour
{
    public string levelName;
    Button btn;

    public static string loader = "";

    // Start is called before the first frame update
    void Start()
    {
        btn = this.GetComponent<Button>();
        btn.onClick.AddListener(click);

        if (loader == "") {
            GameManager.unlockedLevels = Int32.Parse(Parser.stringFromFile("config.dat"));
            loader = GameManager.unlockedLevels.ToString();
        }
    }

    public void click()
    {
        if (Int32.Parse(levelName) == GameManager.unlockedLevels)
        {
            GameManager.isFirst = true;
        } else
        {
            GameManager.isFirst = false;
        }
        GameManager.lvlToLoad = levelName;
        GameManager.folderToLoad = "BuiltInMaps";
        SceneManager.LoadScene("Gra");
    }

    // Update is called once per frame
    void Update()
    {
        if (Int32.Parse(levelName) <= GameManager.unlockedLevels)
        {
            btn.interactable = true;
        }
        else
        {
            btn.interactable = false;
        }

    }
}
