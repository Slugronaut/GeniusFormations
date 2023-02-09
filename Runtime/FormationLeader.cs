using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;


namespace GeniusFormations
{
    /// <summary>
    /// 
    /// </summary>
    public class FormationLeader : MonoBehaviour
    {
        #region Fields and Properties
        [Tooltip("The agent that will act as the leader of the formation.")]
        public NavMeshAgent Agent;
        [Tooltip("The formation asset that defines the local-space positions of the formation followers, relative to the leader.")]
        [SerializeField]
        FormationAsset _Formation;
        public FormationAsset Formation
        {
            get => _Formation;
            set
            {
                _Formation = value;
                if (_Formation != null)
                    AssumeFormation();
            }
        }
        [Tooltip("The unmodified speed of the nav agent when not compensating.")]
        public float NavAgentDefaultSpeed = 7.0f;
        [Tooltip("If set, this leader will slow down until others are closer to formation. However, this can cause issues with jerky motion, slow down the movement greatly, and even occasionally dead-lock the nav mesh agent.")]
        public bool WaitForGroup = false;
        [Tooltip("How far this agent can drift from its formation position.")]
        public float MaxDrift = 3;
        [Tooltip("The maximum additional velocity can be applied to keep this agent in formation.")]
        public float MaxSpeedCompensaton = 10.0f;
        public UnityEvent OnAllAgentsInPosition;
        public UnityEvent OnLeaderInPosition;

        List<FormationFollower> Followers = new List<FormationFollower>();
        public bool HasFollowers => Followers.Count > 0;

        /// <summary>
        /// A copy of the list of followers.
        /// </summary>
        public FormationFollower[] FollowerList => Followers.ToArray();


        public bool HasPositionsAvailable => Formation.PositionCount > Followers.Count;

        public Vector3 CenterOfMass
        {
            get
            {
                Vector3 com = Vector3.zero;
                foreach (var fol in Followers)
                    com += fol.transform.position;
                com /= Followers.Count;
                return com;
            }
        }

        /// <summary>
        /// Is this leader stopped?
        /// </summary>
        public bool IsStopped =>
            Agent.isStopped ||
            (!Agent.pathPending && !Agent.hasPath) ||
            (Agent.velocity.sqrMagnitude < 0.1 &&
                Vector3.Distance(Agent.destination, Agent.transform.position) < Agent.stoppingDistance);

        /// <summary>
        /// Is this leader and all follower stopped?
        /// </summary>
        public bool IsFormationStopped
        {
            get
            {
                if (!IsStopped) return false;
                foreach(var fol in Followers)
                {
                    if (!fol.IsStopped) return false;
                }
                return true;
            }
        }
            

        float AgentGroupSpeedModifier
        {
            get
            {
                if (Followers.Count < 1) return 1.0f;

                var com = CenterOfMass;
                var trans = transform;
                var pos = trans.position;

                float distFromCom = (com - pos).magnitude;
                return (MaxDrift - distFromCom) / MaxDrift;
            }
        }
        #endregion


        private void Update()
        {
            if (!HasFollowers) return;
            if (!IsStopped)
                Agent.speed = NavAgentDefaultSpeed * (WaitForGroup ? AgentGroupSpeedModifier : 1.0f);
        }

        /// <summary>
        /// Gets the relative formation position as a worldspace position in relation to the leader.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector3 GetFormationPosition(int index, float lookAhead)
        {
            var trans = Agent.transform;
            var localPos = Formation.LocalFormation[index];
            return trans.TransformPoint(new Vector3(localPos.x, trans.position.y, localPos.y)) + (Agent.velocity * Time.deltaTime * lookAhead);
        }

        static List<FormationFollower> TempList = new List<FormationFollower>(4);
        /// <summary>
        /// Passes leadership to one of the followers. Optionally
        /// can include or remove this leader from the formation.
        /// </summary>
        /// <param name="newLeader">The follower that will now have leadership transferred to it.</param>
        /// <param name="includeMe">Should this leader become a follower of the new leader?</param>
        bool PassLeadership(FormationFollower newLeader, bool includeMe)
        {
            var leader = newLeader.GetComponent<FormationLeader>();
            if (leader == null)
                return false;

            TempList.Clear();
            TempList.AddRange(Followers);
            while (Followers.Count > 0)
                Followers[0].RemoveLeader(this);
            
            foreach(var follower in TempList)
            {
                if (follower != newLeader)
                    follower.AssignLeader(leader);
            }

            if(includeMe)
            {
                var follower = GetComponent<FormationFollower>();
                follower.AssignLeader(leader);
            }

            return true;
        }

        /// <summary>
        /// Passes leadership to one of the followers. Optionally
        /// can include or remove this leader from the formation.
        /// </summary>
        /// <param name="newLeader">A leader that will now have leadership transferred to it.</param>
        /// <param name="includeMe">Should this leader become a follower of the new leader?</param>
        bool PassLeadership(FormationLeader newLeader, bool includeMe)
        {
            var oldFollower = newLeader.GetComponent<FormationFollower>();

            TempList.Clear();
            TempList.AddRange(Followers);
            while (Followers.Count > 0)
                Followers[0].RemoveLeader(this);

            foreach (var follower in TempList)
            {
                if (follower != oldFollower)
                    follower.AssignLeader(newLeader);
            }

            if (includeMe)
            {
                var follower = GetComponent<FormationFollower>();
                follower.AssignLeader(newLeader);
            }

            return true;
        }

        /// <summary>
        /// Tells all followers to assume their formation positions based on this leader.
        /// Useful when you want them to take positions but the leader isn't moving.
        /// </summary>
        public void AssumeFormation()
        {
            foreach (var follower in Followers)
                follower.Agent.SetDestination(GetFormationPosition(follower.PositionIndex, 0));
        }

        /// <summary>
        /// Do not call this directly. It should be called internally by <see cref="FormationFollower.AssignLeader(FormationLeader)"/>
        /// </summary>
        /// <param name="follower"></param>
        /// <returns></returns>
        public int RegisterFollower(FormationFollower follower)
        {
            if (follower.Leader != this)
                throw new UnityException("Do not call RegisterFollower manually. Use FormationFollower.AssigLeader() instead.");

            if (Followers.Contains(follower))
                return Followers.IndexOf(follower);
            else if (!HasPositionsAvailable)
                return -1;

            if (follower.Agent.avoidancePriority < Agent.avoidancePriority)
                follower.Agent.avoidancePriority = Agent.avoidancePriority + 1;
            Followers.Add(follower);
            return Followers.IndexOf(follower);
        }

        /// <summary>
        /// Do no call this directly. It will be called internally by <see cref="FormationFollower.RemoveLeader(FormationLeader)"/>
        /// </summary>
        /// <param name="follower"></param>
        public bool UnregisterFollower(FormationFollower follower)
        {
            if (follower.Leader == null)
                throw new UnityException(follower.name + " has no leader");

            if (follower.Leader != this)
                throw new UnityException(follower.name + " is a follower of " + name);

            return Followers.Remove(follower);
        }

    }
}