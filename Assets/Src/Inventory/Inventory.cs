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
	public void AddAndStack(Item newItem) {
		var pos = items.FindIndex (delegate(Item haveItem) {
			return haveItem.type == newItem.type 
				&& haveItem.group == newItem.group 
				&& haveItem.quality == newItem.quality
				&& haveItem.stack + newItem.stack < haveItem.preset.stackSize;
		});
		// TODO: remove stack size check and split stack on overflow
		if (pos == -1) {
			AddNoStack (newItem);
		} else {
			items[pos].stack++;
		}
	}
}
