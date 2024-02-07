using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEnemy : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void DestroySelf() {

        StartCoroutine(DieWait());
    }
    IEnumerator DieWait() {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);

    }
}
