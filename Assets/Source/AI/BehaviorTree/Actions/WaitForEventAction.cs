using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Wait for Event", story: "Wait for Event [Name]", category: "Action", id: "43515d4cf79fdaaf531774562b4fc7df")]
    public partial class WaitForEventAction : Action
    {
        [SerializeReference]
        public BlackboardVariable<string> Name;

		[SerializeReference]
		public BlackboardVariable<float> TimeoutDuration = new(2f);

		private AgentAI _agent;
        private float _timeoutTime;

        protected override Status OnStart()
        {
            _agent = GameObject.GetComponent<AgentAI>();
            _timeoutTime = Time.time + TimeoutDuration.Value;

            return Status.Running;
        }

		protected override Status OnUpdate()
		{
            if (Time.time > _timeoutTime)
            {
                return Status.Failure;
            }

            return _agent.ReadEvent(Name.Value) ? Status.Success : Status.Running;
		}
    }
}
