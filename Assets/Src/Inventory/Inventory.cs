using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory {

	public List<Item> items;

	// Use this for initialization
	public Inventory() {
		items = new List<Item> ();
		items.Add (Item.Create (ItemGroup.Tool, ItemType.Pick, "Old Stone Pick", ItemQuality.Poor));
	}
}
