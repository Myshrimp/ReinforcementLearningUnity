using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop : MonoBehaviour
{
    [SerializeField] private Transform collect_water_pos;
    [SerializeField] private LineRenderer tube;
    [SerializeField] private GameObject collect_water;
    GameObject current_water_collect;
    // Start is called before the first frame update
    void Start()
    {
        collect_water.SetActive(false);
        tube.enabled = false ;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            if(!current_water_collect) current_water_collect = Instantiate(collect_water,collect_water_pos.position,collect_water_pos.rotation) ;
            current_water_collect.SetActive(true) ;
            tube.positionCount = 2;           
            tube.enabled = true;
        }
        tube.SetPosition(0, transform.position);
        tube.SetPosition(1, current_water_collect.transform.localPosition);
    }
    public void WithDraw()
    {
        tube.positionCount = 0;
        tube.enabled = false;
        collect_water.SetActive(false);
    }
}
