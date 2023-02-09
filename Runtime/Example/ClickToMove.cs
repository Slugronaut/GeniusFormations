using UnityEngine;
using UnityEngine.AI;


namespace GeniusFormations
{
    /// <summary>
    /// Utility for testing nav agents by allow controll of one agent using the pointer.
    /// </summary>
    public class ClickToMove : MonoBehaviour
    {
        public LayerMask Layers;
        public float RayDistance = 200;
        public NavMeshAgent agent;


        void Update()
        {
            if (Input.GetMouseButton(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, RayDistance, Layers))
                agent.destination = hit.point;
        }
    }
}
