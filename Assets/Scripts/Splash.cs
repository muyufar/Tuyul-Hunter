using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Splash : MonoBehaviour
{

    public int delayTime;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame() {

        yield return new WaitForSeconds(delayTime);
        SceneManager.LoadScene("Menu");
    }
}
