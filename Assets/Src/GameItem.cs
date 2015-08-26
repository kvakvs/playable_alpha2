using UnityEngine;
using System.Collections;

public enum ItemGroup {
	Tool, Weapon, Armor,
	Potion, Elixir,
	Block,
	Resource,
	Junk
};

public enum ItemType {
	// Tool types
	Pick,

	// Weapon types
	Sword,

	// Armor types
	Helm, Chest, Gloves, Pants, Boots, Ring,

	// Block types
	Dirt, Stone, IronOre
};

//
// Represents an object picked up from the world or crafted. An item in inventory.
// A block of resource. A piece of equipment etc.
//
public class GameItem : MonoBehaviour {
	public ItemGroup group;
	public ItemType  type;

	// Use this for initialization
	void Start () {	
	}
	
	// Update is called once per frame
	//void Update () {}
}
