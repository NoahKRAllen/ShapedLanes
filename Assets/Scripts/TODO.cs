/*  TODO:
    Single source of truth/Keep things simple stupid
    Monetary system
    -Earn cash by killing units/towers
    -Earn cash slowly over time
    -Spend cash to spawn units
    --Potentially change the waves to a cash system as well?
    --Test
    Testing script
    -Button press simulated
    
    
    What have we tried and moved on from:
    -Individual waves being a class
    --Using scriptable objects now
    -Waypoints and physics based movement
    --Changing to navmesh and nav agents due to trouble with tracking if unit has moved past the next waypoint in list
    -Splitting up the different control scripts for the units
    --If I was going to have a more complex unit, it would make sense. However for this, I feel that the amount of code is safe for one UnitController
    
    Useful things learned:
    The Physics.OverlapSphereNonAlloc() function is perfect for checking for nearby GameObjects, saves from having to check all GameObjects in the scene
    Doing if(x!=null)return; is cleaner than if(x == null) { Do something else }, as long as there is nothing beyond that check
    Having DrawGizmo checking for a variable that isn't set except on runtime leads to constant Null Reference Exceptions anything that GameObject is selected in editor outside of runtime
    There is no way to automatically set a delay on button clickable, using a script instead.
    Making it work first is the way to do it, even if its dirty and Unity/Rider is telling me that there are better ways to do things
    
    
    Future Plans:
    Button animation for cooldown, can tying into existing script that does the disable/re-enable of the button on click
    Projectiles for towers and ranged units
    Animation for death of units/towers
    Win condition(Destroy opposing base)
    Animation for OnHit, probably some kind of pixel particle system of the same color as the unit being hit along with maybe a flinch on the unit
    
    
    Finished TODOs:
    
    -Change of plans, keeping Waypoint movement with physics control, with a trigger box before each waypoint to act as the swap to the next waypoint
    -Ensure collision works fine as well, turn off targeting to do this best
    ----Maybe do a hybrid? Navmesh agent for me to learn to use it, with waypoints that the navmesh agents are sent towards instead of just setting their target to the end goal
    ----Add back navmesh and navmesh agents, use agent.destination linked to the waypoints, with the waypointArriveDistance that is currently in use being the determining factor of we are close enough to swap to next waypoint
    -Make sure that the towers and base are all still hittable by the 1 range melee units, the fix in place should still work?
*/


