using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Assert = UnityEngine.Assertions.Assert;

public class GuiInventory : MonoBehaviour {

//	public PlayerCtl player;
	public GameObject inventoryStackLabel;

	const int MAX_ITEMS = 40;

	// Use this for initialization
	void Start () {
//		Assert.IsNotNull (player);
//		UpdateGuiElements ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// Read inventory contents and arrange screen elements to display that
	public void UpdateGuiElements (Inventory inv)
	{
		int slot = 0;
		foreach (var item in inv.items) {
			var itemBox = transform.FindChild (slot.ToString ());
			Assert.IsNotNull (itemBox);

			Image itemBoxImage = itemBox.GetComponent<Image> ();
			Assert.IsNotNull (itemBoxImage);
				
			itemBoxImage.sprite = item.GetSprite();
			UpdateCount (itemBox, item);

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

	void UpdateCount(Transform tr, Item item) {
		if (item.stack == 1) {
			// do nothing or delete count text
			Transform txt2 = tr.FindChild ("Stack");
			if (txt2 != null) {
				txt2.transform.parent = null;
				DestroyImmediate (txt2);
			}
			return;
		}
		// update text or create new text
		Transform txt1 = tr.FindChild ("Stack");
		GameObject label = txt1 != null ? txt1.gameObject : null;
		if (txt1 == null) {
			label = Instantiate(inventoryStackLabel);
			label.transform.SetParent(tr);
			label.transform.localPosition = new Vector3 ();
			label.name = "Stack";
		}
		var comp = label.GetComponent<Text> ();
		comp.text = item.stack.ToString ();
	}
}
