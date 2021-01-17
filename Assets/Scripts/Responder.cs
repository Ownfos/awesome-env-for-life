using System;
using System.Collections;
using System.Threading;
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

	public Agent agent;

	private bool responderIsRunning = false;
	private string request;
	private State state = State.Waiting;
	private ResponseSocket socket;

	void ProcessRequest()
    {
		Debug.Log($"Processing request: {request}");
		agent.Move(agent.transform.forward * 10.0f);
    }

	void SendResult()
    {
		Debug.Log($"Sending result for request: {request}");
		socket.SendFrame($"request done: {agent.transform.position}");
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
						state = State.Processing;

						Debug.Log($"Received: {request}");
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

	void Start () {
		responderIsRunning = true;
		Task.Run(Respond);
	}

	void FixedUpdate () {
		switch(state)
        {
			case State.Processing:
				ProcessRequest();
				state = State.Sending;
				break;

			case State.Sending:
				SendResult();
				state = State.Waiting;
				break;
        }
	}

	void OnDestroy(){
		responderIsRunning = false;
	}
}
