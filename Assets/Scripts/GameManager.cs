using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using SgLib;
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    int CurrentLevel;

    public GameObject GameOverUI, GameWinUI;
    public bool finished;
    public Text currentText;
    public GameObject[] Level;
    public AudioSource BackgroundSound;
    public AudioClip boo, Scream,OhNo,CollectSound;
    public AudioSource Fx;
    public NavMeshSurface meshSurface;
    public Text TotalCoin;
    int TotalEnemy, EnemyLeft;
    public void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
       CurrentLevel = PlayerPrefs.GetInt("Level", 0);
        Instantiate(Level[CurrentLevel]);

        currentText.text = "DAY " + (CurrentLevel);
    }

    void Start()
    {
        TotalEnemy = GameObject.FindGameObjectsWithTag("Hunter").Length;
        meshSurface.BuildNavMesh();
    }

    
    void Update()
    {
      EnemyLeft = GameObject.FindGameObjectsWithTag("Hunter").Length;
        if (EnemyLeft <= 0 && !finished) {

            GameWin();
        }
        TotalCoin.text = ""+(CoinManager.Instance.Coins);

    }

    IEnumerator End() {
        yield return new WaitForSeconds(2f);
        GameOverUI.SetActive(true);

    }

    public void GameEnd() {
        finished = true;
         Adcontrol.instance.ShowInterstitial();
        GameOverUI.SetActive(true);

    }

    public void GameWin() {
        finished = true;
        Adcontrol.instance.ShowInterstitial();
        PlayerPrefs.SetInt("Level", (CurrentLevel + 1));
        GameWinUI.SetActive(true);


    }





    public void Restart() {
        SceneManager.LoadScene("Game");
    }

    public void Menu()
    {
        SceneManager.LoadScene("Menu");
    }

   


    public void spooked() {

        Fx.PlayOneShot(boo);
    }

    public void CollectCoin() {
        Fx.PlayOneShot(CollectSound);
    }

    public void Die() {
        Fx.PlayOneShot(OhNo);
        GameEnd();

    }


    public void Shop()
    {
        SceneManager.LoadScene("Shop");

    }
}
