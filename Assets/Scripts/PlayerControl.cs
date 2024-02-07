using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SWS;
using MoreMountains.NiceVibrations;
using SgLib;

public class PlayerControl : MonoBehaviour
{
    NavMeshAgent navMeshAgent;
    Animator animator;
    LineRenderer lineRenderer;
    public Transform marker;
    public GameObject UI;
    bool isEnded;
    public GameObject blast;
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
     //   lineRenderer = GetComponent<LineRenderer>();
     //   lineRenderer.positionCount = 0;
     //   lineRenderer.startWidth = 0.1f;
     //   lineRenderer.endWidth = 0.1f;
        isEnded = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isEnded) {

            ClickToMove();
        }
        if (Vector3.Distance(navMeshAgent.destination, transform.position) <= navMeshAgent.stoppingDistance)
        {
            animator.SetBool("isRunning", false);
            marker.gameObject.SetActive(false);
        }
        else if (navMeshAgent.hasPath) {
            DrawPath();
        }
    }

    public void ClickToMove() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool hasHit = Physics.Raycast(ray, out hit);
        if (hasHit) {
            SetDirection(hit.point);
        }

    }

    public void SetDirection(Vector3 position) {
        marker.gameObject.SetActive(true);
        navMeshAgent.isStopped = false;
        marker.position = position;
        navMeshAgent.SetDestination(position);
        animator.SetBool("isRunning", true);
    }

    void DrawPath() {

     //   lineRenderer.positionCount = navMeshAgent.path.corners.Length;
    //    lineRenderer.SetPosition(0, transform.position);

        if (navMeshAgent.path.corners.Length < 2) {
            return;
        }

        for (int i = 1; i < navMeshAgent.path.corners.Length; i++) {
            Vector3 pointPosition = new Vector3(navMeshAgent.path.corners[i].x, navMeshAgent.path.corners[i].y, navMeshAgent.path.corners[i].z);
        //    lineRenderer.SetPosition(i, pointPosition);
        }
    }


    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Hunter")) {
            HunterSpooked(other.gameObject);

        }
        if (other.gameObject.CompareTag("Radar")) {
            GhostSeen(other.gameObject);
            //UI.SetActive(true);

        }

        if (other.gameObject.CompareTag("Tripod")) {
            navMeshAgent.isStopped = true;
            animator.SetBool("isDead", true);
            marker.gameObject.SetActive(false);
         //   lineRenderer.positionCount = 0;
            MMVibrationManager.Haptic(HapticTypes.HeavyImpact);
            GameManager.instance.Die();
            isEnded = true;
            GameManager.instance.Die();

        }

        if (other.gameObject.CompareTag("Coin"))
        {

            CoinManager.Instance.AddCoins(10);
            GameManager.instance.CollectCoin();
            Destroy(other.gameObject);
        }




        }

    public void HunterSpooked(GameObject target) {
        animator.SetTrigger("kill");
        GameObject a = Instantiate(blast, target.transform)as GameObject;
        a.transform.SetParent(null);
        a.transform.localScale = new Vector3(2, 2, 2);
        target.GetComponent<Animator>().SetBool("Death",true);
        GameManager.instance.spooked();
        navMeshAgent.isStopped = true;
        marker.gameObject.SetActive(false);
        target.GetComponent<navMove>().Stop();
        target.transform.GetChild(0).gameObject.SetActive(false);
        target.GetComponent<DestroyEnemy>().DestroySelf();
        MMVibrationManager.Haptic(HapticTypes.RigidImpact);

    }

    public void GhostSeen(GameObject hunter) {
        navMeshAgent.isStopped = true;
        animator.SetBool("isDead", true);
        marker.gameObject.SetActive(false);
       // lineRenderer.positionCount = 0;
        hunter.transform.parent.GetComponent<navMove>().Stop();
        hunter.transform.parent.GetComponent<Animator>().SetBool("isSeen", true);
        MMVibrationManager.Haptic(HapticTypes.HeavyImpact);
        GameManager.instance.Die();
        isEnded = true;
    }

}
