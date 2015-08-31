using UnityEngine;
using System.Collections;
using Assert = UnityEngine.Assertions.Assert;

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

public enum ItemQuality {
	Poor, Common, Uncommon, Rare
};

public class Item {
	public ItemGroup group;
	public ItemType type;
	public ItemQuality quality;
	public Sprite sprite;
	public string name;

	static Sprite[] allSprites;

	public static Item Create(ItemGroup g, ItemType t, string n, ItemQuality q) {
		if (allSprites == null) {
			allSprites = Resources.LoadAll<Sprite> ("items");
		}

		Item item = new Item();
		item.group = g;
		item.type = t;
		item.name = n;
		item.quality = q;
		item.sprite = allSprites[0];
		Assert.IsNotNull (item.sprite);
		return item;
	}

	public Sprite GetSprite() {
		return sprite;
	}
}
