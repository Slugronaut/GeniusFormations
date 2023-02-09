using UnityEngine;


namespace GeniusFormations
{
    /// <summary>
    /// Used to define the local-space positions of a formation, relative to a leader's position.
    /// Note that (X,Y) actually translates to (X,Z) in 3D space.
    /// </summary>
    [CreateAssetMenu(fileName = "New Formation", menuName = "Formations/New Formation")]
    public class FormationAsset : ScriptableObject
    {
        [Tooltip("The local (x,z) positions that each formation follower will take, relative to their leader.")]
        public Vector2[] LocalFormation;
        public int PositionCount => LocalFormation == null ? 0 : LocalFormation.Length;
    }
}
