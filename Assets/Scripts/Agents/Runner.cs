using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Runner : Agent
{
    [SerializeField] private Transform target; //Target is enemy
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;
    [SerializeField] private SpriteRenderer floor;

    [Header("Spawning")]
    public float minSpawnDist = 5f;
    public Transform bottomLeftBound;
    public Transform topRightBound;
    public LayerMask wallMask;
    public EpisodeController epCntl;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer sprite;
    private bool facingRight = true;

    [Header("Reward")]
    public float maxBaseReward = 100f;
    public float timeSinceGoal = 0f;

    [Header("Senses")]
    public LayerMask rayMask;
    public int numRays = 8;
    public float rayDist = 20f;
    public float rayTotalDegrees = 360f;
    public Vector2 rayLeft = Vector2.up;

    [Header("Goal")]
    public Transform goal;
    public int goalsNeeded = 2;
    public float timeSinceMyGoal = 0f;
    public int goalsReached = 0;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = FindSpawnLocation(target, minSpawnDist);

        timeSinceGoal = 0f;
        timeSinceMyGoal = 0f;
        goalsReached = 0;

        MoveGoal();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(target.transform.position);
        sensor.AddObservation(goal.position);
        sensor.AddObservation(GetRayHits());
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];
        
        transform.position += new Vector3(moveX, moveY, 0) * Time.deltaTime * moveSpeed;

        // If the input is moving the player right and the player is facing left...
        if (moveX > 0 && !facingRight)
        {
            // ... flip the player.
            Flip();
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if (moveX < 0 && facingRight)
        {
            // ... flip the player.
            Flip();
        } 
        timeSinceGoal += Time.deltaTime;
        timeSinceMyGoal += Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if( other.gameObject.tag == "Tagger"){
            float reward = -100f - (maxBaseReward/timeSinceGoal);
            AddReward(reward);
            Debug.Log(string.Format("Runner: Reward {0}",reward));
            //floor.color = winColor;
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    public Vector3 FindSpawnLocation(Transform objectToAvoid, float minDist){
        while(true){
            Vector3 newPos = new Vector3(Random.Range(bottomLeftBound.localPosition.x,topRightBound.localPosition.x),Random.Range(bottomLeftBound.localPosition.y,topRightBound.localPosition.y),0);
            if( Vector3.Distance(objectToAvoid.localPosition,newPos) >= minDist && Physics2D.OverlapCircle(newPos, 0.8f,wallMask)==null){
                return newPos;
            }
        }
    }

    private void Flip()
	{
        facingRight = !facingRight;
		sprite.flipX = !sprite.flipX;
	}

        private List<float> GetRayHits(){
        List<float> distances = new List<float>();

        Vector2 currentDirection = rayLeft;

        float rotation = rayTotalDegrees/numRays;

        
        // before our check set it to true
        for (int x = 0; x < numRays; x++)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, currentDirection, rayDist ,rayMask);
            Debug.DrawRay(transform.position, currentDirection* rayDist, Color.red);
            distances.Add(hit.distance);

            currentDirection = rotate(currentDirection, rotation * Mathf.Deg2Rad);
        }
        return distances;
    }

    public static Vector2 rotate(Vector2 v, float delta) {
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
    }

    private void OnCollisionStay2D(Collision2D other) {
        if(other.gameObject.tag == "Wall"){
            float reward = -0.1f;
            AddReward(reward);
            Debug.Log(string.Format("Runner: Reward {0}",reward));
        }   
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.tag == "Goal"){
            goalsReached+=1;
            if(goalsReached >= goalsNeeded){
                floor.color = winColor;
            }
            AddReward(50f + (maxBaseReward/timeSinceMyGoal));
            MoveGoal();
        }
    }

    private void MoveGoal(){
        goal.transform.localPosition = FindSpawnLocation(transform, minSpawnDist);
        //closestDistance = Vector2.Distance(transform.position,goal.transform.position);
        timeSinceMyGoal = 0f;
    }


}
