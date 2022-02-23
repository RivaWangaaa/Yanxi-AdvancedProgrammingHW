using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
public class CubeAgent : Agent
{
   //Ball on the top of the cube's head
   public GameObject ball;
   private Rigidbody m_BallRb;
   public override void Initialize()
   {
      //Execute only once when game scene initialized
      base.Initialize();
      m_BallRb = ball.GetComponent<Rigidbody>();
      SetResetParamters();
   }

   void SetResetParamters()
   {
      var scale = 1;
      m_BallRb.mass = scale;

      ball.transform.localScale = new Vector3(scale, scale, scale);
   }
   public override void OnEpisodeBegin()
   {
      //Execute before each game cycle / round
      base.OnEpisodeBegin();
      
      SetResetParamters();
      
      SetBallSize();

      //Reset cuble's rotation
      gameObject.transform.rotation = new Quaternion(0, 0, 0, 0);
      
      //Add dynamic random rotation to the cube
      gameObject.transform.Rotate(new Vector3(1,0,0), Random.Range(-10,10f));
      gameObject.transform.Rotate(new Vector3(0,0,1), Random.Range(-10,10f));

      //Reset velocity of the cube
      m_BallRb.velocity = new Vector3(0f, 0f, 0f);

      ball.transform.position = new Vector3(Random.Range(-1.2f, 1.2f), 4f, Random.Range(-1.2f, 1.2f))
                                + gameObject.transform.position;
   }
   
   public override void CollectObservations(VectorSensor sensor)
   {
      // Add customized data for observations
      base.CollectObservations(sensor);
      
      //speed & direction of the ball
      //(x,y,z) = 3
      sensor.AddObservation(m_BallRb.velocity);
      //rotation z of the cube
      //float = 1
      sensor.AddObservation(gameObject.transform.rotation.z);
      //rotation x of the cube
      //float = 1
      sensor.AddObservation(gameObject.transform.rotation.x);
      //rotation position between ball and cube
      //(x,y,z) = 3
      sensor.AddObservation(ball.transform.position - gameObject.transform.position);
      
      //observe ball mass/scale
      sensor.AddObservation(m_BallRb.mass);
      
   }
   
   public override void OnActionReceived(ActionBuffers actions)
   {
      //Apply actions from neural network
      base.OnActionReceived(actions);

      //Control action z
      var actionZ = 2f * Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
      //Control action x
      var actionX = 2f * Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
      
      //Rotate cube around axis Z
      //constraint of cube rotation to make the training faster
      if((gameObject.transform.rotation.z < 0.25f && actionZ > 0 ) || (gameObject. transform.rotation.z > 0f))
      gameObject.transform.Rotate(new Vector3(0,0,1), actionZ);
      //Rotate cube around axis X
      if((gameObject.transform.rotation.x < 0.25f && actionX > 0 ) || (gameObject. transform.rotation.x > 0f))
      gameObject.transform.Rotate(new Vector3(1,0,0), actionX);

      if ((ball.transform.position.y - gameObject.transform.position.y) < -3f 
          || Mathf.Abs(ball.transform.position.x - gameObject.transform.position.x) > 5f
          || Mathf.Abs(ball.transform.position.z - gameObject.transform.position.z) > 5f)
      {
         SetReward(-1);
         EndEpisode();
      }
      else
      {
         SetReward(0.1f);
      }
   }

   public override void Heuristic(in ActionBuffers actionsOut)
   {
      //For human player
      base.Heuristic(in actionsOut);

      //Replace actions by keyboard
      var continuousActionsOut = actionsOut.ContinuousActions;
      continuousActionsOut[0] = -Input.GetAxis("Horizontal");
      continuousActionsOut[1] = Input.GetAxis("Vertical");
   }

   public void SetBallSize()
   {
      var ballScale = Random.Range(1f, 5f);
      
      m_BallRb.mass = ballScale;

      ball.transform.localScale = new Vector3(ballScale, ballScale, ballScale);
   }
}
