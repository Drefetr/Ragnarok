# Ragnarok

Ragnarok, an Oxide mod. for the Rust Dedicated Server. A hellscape attempt: Shitty weather, and a constant barrage of meteors --

Ragnarok Hellscape -- Shitty weather, and a constant barrage of meteors -- Which can be configured to spawn loose resources, resource nodes, neither or both.

I highly recommend using the Cornucopia mod. to control clean-up of the spawned resource node(s).

An Example Ragnarok.json:

[CODE]{
  "MaxClusterSize": 5,
  "MaxLaunchAngle": 0.5,
  "MaxLaunchHeight": 0.5,
  "MaxLaunchVelocity": 75.0,
  "MeteorFrequency": 5,
  "MinLaunchAngle": 0.25,
  "MinLaunchHeight": 100.0,
  "MinLaunchVelocity": 25.0,
  "SpawnResourceNodePercent": 1.0,
  "SpawnResourcePercent": 0.05
}[/CODE]

[CODE]
    /**
      * Minimum clockwise angular deviation from the normal vector;
      * Where 0.0f is 0 rad, and 1.0f is 1/2 rad.
      * (0.0f, ..., maxLaunchAngle).
      */
     private float minLaunchAngle = 0.25f;
    
     /**
      * Maximum clockwise angular deviation from the normal vector;
      * Where 0.0f is 0 rad, and 1.0f is 1/2 rad.
      * (minLaunchAngle, ..., 1.0f).
      */
     private float maxLaunchAngle = 0.5f;
    
     /**
      * Minimum launch height (m); suggested sensible bounds:
      * x >= 1 * maxLaunchVelocity.
      */
     private float minLaunchHeight = 100.0f;
  
     /**
      * Maximum launch height (m); suggested sensible bounds:
      * x <= 10*minLaunchVelocity.
      */
     private float maxLaunchHeight = 250.0f;
  
     /**
      * Minimum launch velocity (m/s^-1).
      */
     private float minLaunchVelocity = 25.0f;
  
     /**
      * Maximum launch velocity (m/s^-1).
      * Suggested sensible maximum: 75.0f.
      */
     private float maxLaunchVelocity = 75.0f;

     /**
      * ServerTicks between Meteor(s).
      */
     private int meteorFrequency = 5;
    
     /**
      * Maximum number of Meteors per cluster.
      */
     private int maxClusterSize = 1;    
    
     /**
      * Percent chance of the Meteor dropping loose resources at the point of impact.
      */
     private float spawnResourcePercent = 0.05f;
  
     /**
      * Percent chance of the Meteor spawning a resource node at the point of impact.
      */
     private float spawnResourceNodePercent = 1.0f;
[/CODE]
