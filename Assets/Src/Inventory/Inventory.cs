using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory {

	public List<Item> items;

	// Use this for initialization
	public Inventory() {
		items = new List<Item> ();
	}

	// Adds item without trying to stack
	public void AddNoStack(Item i) {
		items.Add (i);
	}

	// Adds item and tries to stack if it existed in inventory
	public void AddAndStack(Item i) {
		var pos = items.FindIndex (delegate(Item other) {
			return other.type == i.type 
				&& other.group == i.group 
				&& other.quality == i.quality
				&& other.preset.stackSize > 1;
		});
		if (pos == -1) {
			AddNoStack (i);
		} else {
			items[pos].stack++;
		}
	}
}
