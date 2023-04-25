using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class EpisodeController : MonoBehaviour
{
    private List<Agent> agents;
    public bool mustEndEpisode = true;

    private void Awake() {
        agents = new List<Agent>();
    }

    public void AddAgent(Agent agent){
        agents.Add(agent);
    }

    public void EndEpisode(){
        mustEndEpisode = true;
    }

    public void EndEpisodeDoer(){
        foreach (Agent agent in agents)
        {
            agent.EndEpisode();
        }
    }

    private void LateUpdate() {
        if(mustEndEpisode){
            EndEpisodeDoer();
            mustEndEpisode = false;
        }
    }

}
