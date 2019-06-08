using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.Events;


namespace GeniusFormations
{
    /// <summary>
    /// 
    /// </summary>
    public class FormationFollower : MonoBehaviour
    {
        #region Fields and Properties
        [Tooltip("The NavAgent that will act as a follower in a formation.")]
        public NavMeshAgent Agent;
        [Tooltip("The unmodified speed of the nav agent when not compensating.")]
        public float NavAgentDefaultSpeed = 7.0f;
        [Tooltip("How far ahead will followers try to predict their leader's position so as to assume the correct formation position while moving?")]
        public float LookAhead= 10.0f;
        [Tooltip("How far this agent can drift from its formation position.")]
        public float MaxDrift = 1;
        [Tooltip("The maximum additional velocity can be applied to keep this agent in formation.")]
        public float MaxSpeedCompensaton = 1.5f;
        public UnityEvent OnStopped;

        public int PositionIndex { get; private set; }
        public FormationLeader Leader { get; private set; }
        public bool HasLeader => Leader != null;

        bool StoppedThisFrame;



        public bool IsStopped =>
            Agent.isStopped ||
            (!Agent.pathPending && !Agent.hasPath) ||
            (Leader.IsStopped &&
                Agent.velocity.sqrMagnitude < 0.1 &&
                Vector3.Distance(Agent.destination, Agent.transform.position) < Agent.stoppingDistance);

        /// <summary>
        /// Provides a scaling value for the agent's speed so as to ensure it maintains a tighter formation.
        /// </summary>
        float AgentGroupSpeedModifier
        {
            get
            {
                var formationPos = Leader.GetFormationPosition(PositionIndex, LookAhead);
                var trans = transform;
                var pos = trans.position;

                //this puts the desired formation position in 'aim space' relative to this agent.
                //That way we can see how far 'behind' we are on the z-axis and how far laterally we are
                //on the x-axis.
                var localFormPos = trans.InverseTransformPoint(formationPos);
                float distZ = localFormPos.z;
                float distX = localFormPos.y;
                float speedScale = 1.0f;

                if (distZ > 0 && distZ > MaxDrift)
                    speedScale = distZ / MaxDrift;
                else if (distZ < 0 && Mathf.Abs(distZ) > MaxDrift && Agent.remainingDistance < MaxDrift * 4.0f)
                    speedScale = MaxDrift / Mathf.Abs(distZ);

                if (distX > MaxDrift)
                    speedScale = 1.0f;

                return speedScale > MaxSpeedCompensaton ? MaxSpeedCompensaton : speedScale;
            }
        }
        #endregion


        /// <summary>
        /// 
        /// </summary>
        public void LateUpdate()
        {
            if (!HasLeader) return;
            if (!Leader.IsStopped)
            {
                StoppedThisFrame = false;
                Agent.SetDestination(Leader.GetFormationPosition(PositionIndex, LookAhead));
                Agent.speed = NavAgentDefaultSpeed * AgentGroupSpeedModifier;
            }
            else
            {
                //one last destination set, this time without compensation
                if (!StoppedThisFrame)
                {
                    Agent.SetDestination(Leader.GetFormationPosition(PositionIndex, 0));
                    StoppedThisFrame = true;
                }
                
                //TODO:
                //we need to do a sample here and see if it's possible to even get to our destination
                //without fighting over it with another agent.
            }
        }

        /// <summary>
        /// Use this to assign a leader to this follower.
        /// </summary>
        /// <param name="leader"></param>
        /// <returns></returns>
        public bool AssignLeader(FormationLeader leader)
        {
            Assert.IsNotNull(leader);

            Leader = leader;
            var i = Leader.RegisterFollower(this);
            if (i >= 0)
            {
                PositionIndex = i;
                Leader = leader;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="leader"></param>
        /// <returns></returns>
        public void RemoveLeader(FormationLeader leader)
        {
            Assert.IsNotNull(leader);
            if (Leader != leader)
                throw new UnityException(leader.name + " is not the current FormationLeader of " + name);

            if (Leader.UnregisterFollower(this))
            {
                Leader = null;
                PositionIndex = -1;
            }
            else throw new UnityException("There was an error removing " + name + " as a follower of " + leader.name);
        }
    }
}
