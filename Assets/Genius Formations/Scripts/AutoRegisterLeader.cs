using UnityEngine;


namespace GeniusFormations
{
    /// <summary>
    /// Handy little component for assigning a leader to a follower when the follower is enabled.
    /// </summary>
    [RequireComponent(typeof(FormationFollower))]
    public class AutoRegisterLeader : MonoBehaviour
    {
        public FormationLeader DesiredLeader;


        private void OnEnable()
        {
            GetComponent<FormationFollower>().AssignLeader(DesiredLeader);
        }
    }
}
