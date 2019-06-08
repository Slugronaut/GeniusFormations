A simple formation system for Unity's NavMeshAgents.

You may attach a FormationLeader to one agent and FormationFollowers to any number of others and allow them to follow their assigned leader with a defined formation.

Define formation assets by right-clicking in the project window and choosing Create->Formations->New Formation. You can then assign a formation in the Leader's inspector window. Follower are positions are defined in aim-space relative to the leader such that z = forward/back, x = lefr/right, and y = up/down all relative to the direction the leader is facing. Not that the editor only displays (X,Y) coordinates but they are translated to (X,Z) internally so that you can only define positions on a flat plane.

Followers may be assigned a leader through script by calling FormationFollower.AssignLeader(FormationLeader). There is also a simple utility script called AutoRegisterLeader.cs that can be attached to a follower and used to set their leader via the inspector.

IMPORTANT: Due to limitations in Unity's NavAgent system it is advised that leader and follower NavAgents should have their Obstacle Avoidance Quality set to 'None' to ensure the smoothest formation transitions possible. Followers are automatically set to a lower-priority than leaders so that the leader will always push past them when Obstacle Avoidance is on.

FormationLeader.WaitForGroup is a fickle beast. It can help keep a formation together better but will reduce the speed of the group on the whole as well as run the risk of locking up the entire formation by slowing the leader down to a crawl. Best to leave this off unless it is absolutely needed.

Leadership can be passed to another leader or a follower that has a FormationLeader component attached using FormationLeader.PassLeadership().


