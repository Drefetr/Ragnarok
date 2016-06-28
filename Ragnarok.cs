using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using Rust;

using Oxide.Core;
using Oxide.Core.Plugins;

/**
 * Rust Ragnarok Plugin; Meteor shower(s).
 *
 * @author Drefetr
 * @author JShmitt
 * @version 0.7.0
 *
 * Todo: Min/max loot spawn variables; node & resource spawn types in pre-def arrays.
 */
namespace Oxide.Plugins
{
    [Info("Ragnarok", "Drefetr et Shmitt", "0.7.0", ResourceId = 31373)]
    public class Ragnarok : RustPlugin
    {			
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
		 */
		private float maxLaunchVelocity = 100.0f;

		/**
		 * ServerTicks between Meteor(s).
		 */
		private int meteorFrequency = 5;
		
		/**
		 * Percent chance of the Meteor dropping loose resources at the point of impact.
		 */
		private float spawnResourcePercent = 0.05f;
	
		/**
		 * Percent chance of the Meteor spawning a resource node at the point of impact.
		 */
		private float spawnResourceNodePercent = 1.0f;
		
		/**
		 * Maximum size of a Meteor cluster.
		 */
		private int maxClusterSize = 5;
	
		/**
		 * ServerTicks since OnServerInit().
		 */
		private int tickCounter = 0;
	
		/** 
		 * Server OnInit-bind; runs on server startup & mod. init.
		 */
		private void OnServerInitialized()
		{	
			// Ensure shitty weather; clouds & fog.
			ConsoleSystem.Run.Server.Normal("weather.clouds 1");			
			ConsoleSystem.Run.Server.Normal("weather.fog 1");
		}	
	
		/** 
		 * Server OnTick-bind; runs once per server tick --
		 * (An externally configurable frequency).
		 */
		void OnTick()
		{		
			// Spawn Meteors(s) Y/N:
			if (this.tickCounter % this.meteorFrequency == 0) {	
				// Fetch a random position, with an altitude of {0}.
				Vector3 location = this.getRandomMapPosition();
				int clusterSize = UnityEngine.Random.Range(1, this.maxClusterSize);
			
				for (int i = 0; i < clusterSize; i++) {
					float r = UnityEngine.Random.Range(0.0f, 100.0f);
				
					if (r < this.spawnResourcePercent)
						// Spawn a loose resource:
						this.spawnResource(location);
				
					if (r < this.spawnResourceNodePercent)
						// Spawn a resource node:			
						this.spawnResourceNode(location);
				
					this.spawnMeteor(location);
				}
			}
			
			this.tickCounter++;
		}
				
		/**
		 * Spawns a Meteor in the location specified by Vector3(location).
		 */			
		private void spawnMeteor(Vector3 origin)
		{
			float launchAngle = UnityEngine.Random.Range(this.minLaunchAngle, this.maxLaunchAngle);
			float launchHeight = UnityEngine.Random.Range(this.minLaunchHeight, this.maxLaunchHeight);
			
			Vector3 launchDirection = (Vector3.up * -launchAngle + Vector3.right).normalized;
			Vector3 launchPosition = origin - launchDirection * launchHeight;
			
			int r = UnityEngine.Random.Range(0, 3);			
			
			ItemDefinition projectileItem;			
			
			// Fetch rocket of type <x>:
			switch (r) {
				case 0:
					projectileItem = getBasicRocket();
					break;
					
				case 1:
					projectileItem = getHighVelocityRocket();
					break;
					
				case 2:
					projectileItem = getSmokeRocket();
					break;
				
				default: 
					projectileItem = getFireRocket();
					break;
			}
		
			// Create the in-game "Meteor" entity:
			ItemModProjectile component = projectileItem.GetComponent<ItemModProjectile>();
			BaseEntity entity = GameManager.server.CreateEntity(component.projectileObject.resourcePath, launchPosition, new Quaternion(), true);

			// Set Meteor speed:
			ServerProjectile serverProjectile = entity.GetComponent<ServerProjectile>();			
			serverProjectile.speed = UnityEngine.Random.Range(this.minLaunchVelocity, this.maxLaunchVelocity);
			
			entity.SendMessage("InitializeVelocity", (object) (launchDirection * 1.0f));
			entity.Spawn(true);								
		}
		
		/**
		 * Spawns a ResourceItem of a random type at the location specified by 
		 * Vector3(location).
		 */
		private void spawnResource(Vector3 location) {
			string resourceName = "";
			int resourceQuantity = 0;
			
			int r = UnityEngine.Random.Range(0, 3);
			
			switch (r) {
				case 1:
					resourceName = "hq.metal.ore";
					resourceQuantity = 100;
					break;
				
				case 2:
					resourceName = "metal.ore";
					resourceQuantity = 1000;
					break;
				
				case 3:
					resourceName = "stones";
					resourceQuantity = 2500;
					break;
				
				default:
					resourceName = "sulfur.ore";
					resourceQuantity = 1000;		
					break;
			}
			
			ItemManager.CreateByName(resourceName, resourceQuantity).Drop(location, Vector3.up);
		}
		
		/**
		 * Spawns a ResourceNode of a random type at the location specified by 
		 * Vector3(location).
		 */
		private void spawnResourceNode(Vector3 location) {
			string prefabName = "assets/bundled/prefabs/autospawn/resource/ores/";
			
			// Select a random ResourceNode type {Metal, Stone, Sulfur}.			
			int r = UnityEngine.Random.Range(0, 2);			
			
			switch (r) {
				case 1:
					prefabName += "metal-ore";
					break;
							
				case 2:
					prefabName += "stone-ore";
					break;
							
				default:
					prefabName += "sulfur-ore";						
					break;
			}			
			
			prefabName += ".prefab";
			
			// & spawn the ResourceNode at Vector3(location).
			BaseEntity resourceNode = GameManager.server.CreateEntity(prefabName, location, new Quaternion(0, 0, 0, 0));			
			resourceNode.Spawn(true);
		}
		
		/**
		 * Returns an Item of type "ammo.rocket.basic":
		 */
		private ItemDefinition getBasicRocket()
		{
			return ItemManager.FindItemDefinition("ammo.rocket.basic");
		}
		
		/**
		 * Returns an Item of type "ammo.rocket.fire":
		 */		
		private ItemDefinition getFireRocket()
		{
			return ItemManager.FindItemDefinition("ammo.rocket.fire");
		}
		
		/**
		 * Returns an Item of type "ammo.rocket.hv":
		 */
		private ItemDefinition getHighVelocityRocket() {
			return ItemManager.FindItemDefinition("ammo.rocket.hv");
		}
		
		/**
		 * Returns an Item of type "ammo.rocket.smoke":
		 */
		private ItemDefinition getSmokeRocket() {
			return ItemManager.FindItemDefinition("ammo.rocket.smoke");
		}	

		/**
		 * Returns a random Map position (x, y).
		 */
		private Vector3 getRandomMapPosition() {
			float mapsize = getMapSize() - 500f;			
			float randomX = UnityEngine.Random.Range(-mapsize, mapsize);
			float randomY = UnityEngine.Random.Range(-mapsize, mapsize);					
			return new Vector3(randomX, 0f, randomY);
		}
		
		/**
		 * Returns the current Map size, -assumed square- (x, y).
		 */
		private float getMapSize()
		{
			return TerrainMeta.Size.x / 2;
		}			
    }
}