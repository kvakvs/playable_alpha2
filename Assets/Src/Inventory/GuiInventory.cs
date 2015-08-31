using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Assert = UnityEngine.Assertions.Assert;

public class GuiInventory : MonoBehaviour {

	public PlayerCtl player;
	public Inventory inventory;

	const int MAX_ITEMS = 40;

	// Use this for initialization
	void Start () {
		Assert.IsNotNull (player);

		inventory = new Inventory ();
		UpdateGuiElements ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// Read inventory contents and arrange screen elements to display that
	void UpdateGuiElements ()
	{
		int slot = 0;
		foreach (var item in inventory.items) {
			var itemBox = transform.FindChild (slot.ToString ());
			Assert.IsNotNull (itemBox);

			Image itemBoxImage = itemBox.GetComponent<Image> ();
			Assert.IsNotNull (itemBoxImage);
				
			Debug.Log ("set sprite from inv slot " + slot + ": " + item.name);
			itemBoxImage.sprite = item.GetSprite();
			slot++;
		}
		for (; slot < MAX_ITEMS; slot++) {
			var itemBox = transform.FindChild (slot.ToString ());
			Assert.IsNotNull (itemBox);

			Image itemBoxImage = itemBox.GetComponent<Image> ();
			Assert.IsNotNull (itemBoxImage);

			itemBoxImage.sprite = null;
		}
	}
}
