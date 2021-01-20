using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

using NetMQ;
using NetMQ.Sockets;


public class Responder : MonoBehaviour {
	private enum State
    {
		Waiting,
		Processing,
		Sending
    }

	public GameObject agentPrefab;
	private Dictionary<int, Agent> agents = new Dictionary<int, Agent>();
	private int agentIdGenerator = 0;
	private bool responderIsRunning = false;
	private string request;
	private State state = State.Waiting;
	private ResponseSocket socket;

	private void ProcessRequest()
    {
		Debug.Log($"Processing request: {request}");
		if(request.Equals("reset"))
        {
			OnEpisodeBegin();
			state = State.Waiting;
        }
        else
		{
			var actionPerAgent = GetActionPerAgent(request);
			foreach (var action in actionPerAgent)
			{
				agents[action.Key].PerformAction(action.Value);
			}
			state = State.Sending;
		}
    }

	private Dictionary<int, string> GetActionPerAgent(string request)
    {
		Dictionary<int, string> result = new Dictionary<int, string>();
		foreach(var agent in agents)
        {
			result.Add(agent.Key, Constants.AGENT_ACTION_MOVE_FORWARD);
        }
		return result;
    }

	private string CollectObervations()
	{
		var stringBuilder = new StringBuilder();
		foreach(var agent in agents)
        {
			stringBuilder.Append(agent.Key);
			stringBuilder.Append(Constants.KEY_VALUE_SEPERATOR);
			stringBuilder.Append(agent.Value.GetObservation().Substring(0,10));
			stringBuilder.Append(Constants.AGENT_OBSERVATION_DELIMITER);
        }
		return stringBuilder.ToString();
	}

	private void SendResult()
    {
		Debug.Log($"Sending result for request: {request}");
		var observation = CollectObervations();
		socket.SendFrame($"observation:{observation}");
		state = State.Waiting;
    }

	private void OnEpisodeBegin()
	{
		agentIdGenerator = 0;
		DestroyAllAgents();
		CreateAgents(1);
		Debug.Log("finished resetting");
		socket.SendFrame("request done: reset environment");
		Debug.Log("will this print?");
    }


	void Respond(){
		AsyncIO.ForceDotNet.Force();

		socket = new ResponseSocket("tcp://*:5558");

		try{
			while(responderIsRunning)
			{
				if(state == State.Waiting)
                {
					if(socket.TryReceiveFrameString(out request))
					{
						Debug.Log($"Received: {request}");
						state = State.Processing;
					}
                }
			}
			}finally{
				if (socket != null)
				{
					socket.Close();
					((IDisposable)socket).Dispose();
					NetMQConfig.Cleanup(true);
				}
			}
		}

	void DestroyAllAgents()
    {
		foreach(var agent in agents)
        {
			Destroy(agent.Value.gameObject);
		}

		agents.Clear();
	}

	void CreateAgents(int numAgents)
    {
		for(int i=0;i<numAgents;i++)
		{
            try
            {
				var agent = Instantiate(agentPrefab);
				var randomOffset = UnityEngine.Random.insideUnitSphere * 1.0f;
				randomOffset.y = 0;
				agent.transform.position += randomOffset;
				agents.Add(agentIdGenerator++, agent.GetComponent<Agent>());
			}
			catch(Exception e)
            {
				Debug.Log(e.Message);
            }
		}
		Debug.Log("creating done");
    }

	void Start () {
		UnityEngine.Random.InitState(Constants.RANDOM_SEED);
		responderIsRunning = true;
		Task.Run(Respond);
	}

	void FixedUpdate () {
		switch(state)
        {
			case State.Processing:
				ProcessRequest();
				break;

			case State.Sending:
				SendResult();
				break;
        }
	}

	void OnDestroy(){
		responderIsRunning = false;
	}
}
