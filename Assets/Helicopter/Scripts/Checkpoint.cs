using System;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
	public bool is_delay;
	public bool is_terminal = false;
	public bool end_here = false;
	[SerializeField] private float time=0;
	public Action CheckpointActivated;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Activate");
		if(!is_delay)
		{
			if (end_here) other.gameObject.GetComponent<Train>().shouldEnd = true;
            gameObject.SetActive(false);
        }
		
    }
    private void OnTriggerStay(Collider other)
    {
		if (!is_delay) return;
        time+= Time.deltaTime;
		if(time>=3)
		{
			other.gameObject.GetComponent<Train>().checkpoint_award.Invoke();
            time = 0;
            if (is_terminal)
            {
                other.gameObject.GetComponent<Train>().shouldEnd = true;
            }
            this.gameObject.SetActive(false);
			
			
		}
    }
    private void OnTriggerExit(Collider other)
    {
        if (!is_delay)
        {
			return;
        }
        other.gameObject.GetComponent<Train>().obstacle_punish.Invoke();
    }
}
