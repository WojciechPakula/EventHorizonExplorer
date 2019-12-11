using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuWlasneMapy : MonoBehaviour
{
    public Dropdown dd;
    // Start is called before the first frame update

    bool czyPuste = false;

    void Start()
    {
        dd.ClearOptions();
        var fls = Directory.GetFiles("SavedMaps");
        if (fls.Length == 0) czyPuste = true;
        List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
        foreach (var f in fls)
        {
            var f2 = (f.Split('\\'))[1];
            var f3 = (f2.Split('.'));
            var f4 = f3[f3.Length-2];

            Dropdown.OptionData od = new Dropdown.OptionData(f4);
            list.Add(od);
        }
        dd.AddOptions(list);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void click()
    {
        if (czyPuste==false)
        {
            var name = dd.options[dd.value].text;
            GameManager.lvlToLoad = name;
            GameManager.folderToLoad = "SavedMaps";
            GameManager.isFirst = false;
            SceneManager.LoadScene("Gra");
        }
    }

    public void back()
    {
        SceneManager.LoadScene("Menu0");
    }
}
