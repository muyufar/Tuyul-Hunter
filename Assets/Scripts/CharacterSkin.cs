using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSkin : MonoBehaviour
{
    public GameObject[] Characters;
    int current_char;
    // Start is called before the first frame update
    void Start()
    {
        current_char = PlayerPrefs.GetInt("CurrentChar", 0);
        Characters[current_char].SetActive(true);
        Debug.Log(current_char);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
