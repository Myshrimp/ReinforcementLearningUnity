using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System;
public class Train : Agent
{
    [SerializeField]GoalManager goalManager;
   [SerializeField] private bool phase1 = true;
    private HelicopterController controller;
    public Action checkpoint_award;
    public Action obstacle_punish;
    public bool shouldEnd = false;
    public Vector3 SpawnPos = Vector3.zero;
    [SerializeField] private Transform target_height;
    public override void Initialize()
    {
        checkpoint_award += CheckpointReward;
        obstacle_punish += CollisionWithObstacle;
        controller = GetComponent<HelicopterController>();
       
    }
    public override void OnEpisodeBegin()
    {
        goalManager.OnBegin();
        if (phase1) SpawnPos = new Vector3(UnityEngine.Random.Range(-460, -500), UnityEngine.Random.Range(71, 73), UnityEngine.Random.Range(88,130));
        controller.EngineForce = 0;
        shouldEnd = false;
        transform.localPosition = new Vector3(SpawnPos.x,SpawnPos.y,SpawnPos.z);
        transform.localRotation = Quaternion.identity;
        transform.Rotate(Vector3.up, UnityEngine.Random.Range(0, 360));
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        int op = actions.DiscreteActions[0];
        float engineforce = actions.ContinuousActions[0];
        Debug.Log("Engineforce=" + engineforce.ToString());
        controller.ReceiveDecision(op);
        controller.EngineForce += engineforce;
        if(controller.EngineForce < 0) { controller.EngineForce = 0; }
        
        Debug.Log(op + 1);
        if(shouldEnd)
        {
            EndEpisode();
        }
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        Collider[] near = Physics.OverlapSphere(transform.position, 1000,3);
        GameObject nearest=new GameObject();
        float distance = 99999;
        for (int i = 0; i < near.Length; i++)
        {
            if (Vector3.Distance(transform.position, near[i].gameObject.transform.position)<distance && near[i].gameObject.CompareTag("Checkpoint"))
            {
                distance = Vector3.Distance(transform.position, near[i].gameObject.transform.position);
                nearest = near[i].gameObject;
            }
        }
        Vector3 dir = nearest.transform.position - transform.position;
        sensor.AddObservation(transform.position.y - nearest.transform.position.y);//1
        sensor.AddObservation(transform.position-nearest.transform.position);//3
        sensor.AddObservation(Vector3.Dot(transform.forward,dir.normalized));//1
        for(int i = 0;i<5;i++)
        {
            sensor.AddObservation(0);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Checkpoint"))
        {
            checkpoint_award.Invoke();
            //shouldEnd = true;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Obstacle"))
        {
            obstacle_punish.Invoke();
            shouldEnd=true;
        }
    }
    private void CheckpointReward()
    {
        AddReward(+0.3f);
        Debug.Log("Awarded");
    }
    private void CollisionWithObstacle()
    {
        AddReward(-0.4f);
        Debug.Log("punished");
    }
}
