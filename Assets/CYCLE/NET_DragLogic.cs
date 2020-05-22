using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NET_DragLogic : MonoBehaviour
{

    public struct userAttributes { }
    public struct appAttributes { }
    public PlantsType currentPlant;
    public PlantsTime currentPlantTime;
    public bool needWater = false;
    public Sprite[] listPlantsTime;
    public GameObject prefabsType;

    [Serializable]
    public enum PlantsType
    {
        NONE,
        FRUITS,
        FLOWERS,
        VEGETABLES,

    }

    public enum PlantsTime
    {
        NONE,
        SEED,
        PETIT,
        READY,
        DEAD,

    }

    public static NET_DragLogic Instance { set; get; }
    void Start()
    {
        Instance = this;
        //currentPlant = PlantsType.NONE;
        currentPlantTime = PlantsTime.NONE;
        
    }
    
    void Update()
    {
        
    }

    public void SetDrag(GameObject oben, GameObject unten )
    {
        //GameObject.Destroy(oben);
        // if != zero, keep seed in hand, if zero destroy
        prefabsType = GameObject.Find("FlowerPrefab");
        Vector3 newPos = new Vector3(unten.transform.position.x, unten.transform.position.y +2, unten.transform.position.z);
        var go = Instantiate(prefabsType, newPos, Quaternion.identity); // 0 = FlowerPrefab
        go.transform.localScale = new Vector3(1f, 1f, go.transform.localScale.z);
        go.transform.parent = unten.transform;

        currentPlantTime = PlantsTime.SEED;
        StartCoroutine(GrowSystem(currentPlantTime));
    }


    IEnumerator GrowSystem(PlantsTime time)
    {
        switch (time.ToString())
        {
            case "SEED":
                this.gameObject.GetComponent<SVGImage>().sprite = listPlantsTime[0];
                yield return new WaitForSeconds(2);
                currentPlantTime = PlantsTime.PETIT;
                this.gameObject.GetComponent<SVGImage>().sprite = listPlantsTime[1];
                this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y +5, this.transform.position.z);
                yield return new WaitForSeconds(2);
                currentPlantTime = PlantsTime.READY;
                this.gameObject.GetComponent<SVGImage>().sprite = listPlantsTime[2];
                this.transform.localScale = new Vector3(this.transform.localScale.x + 0.3f, this.transform.localScale.y + 0.3f, this.transform.localScale.z);
                this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 10, this.transform.position.z);
                yield return new WaitForSeconds(2);
                currentPlantTime = PlantsTime.DEAD;
                this.gameObject.GetComponent<SVGImage>().sprite = listPlantsTime[3];
                break;


            default:
                break;
        }
        //Debug.Log("Started Coroutine at timestamp : " + Time.time);

        
        //yield return new WaitForSeconds(5);

        
       // Debug.Log("Finished Coroutine at timestamp : " + Time.time);
    }
}
