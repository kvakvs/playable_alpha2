using UnityEngine;
using System.Collections;
using Assert = UnityEngine.Assertions.Assert;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
	public ItemPreset preset;
	public int stack;

	static Sprite[] allSprites;

	public enum PresetId {
		OldStonePick,
		DirtBlock,
		StoneBlock,
		IronOreBlock,
		Void
	};
	static readonly IList<ItemPreset> itemPresets = new ReadOnlyCollection<ItemPreset>(
		new[] {
			new ItemPreset(ItemGroup.Tool, ItemType.Pick, "Old Stone Pick", ItemQuality.Poor, 0, 1),
			new ItemPreset(ItemGroup.Block, ItemType.Dirt, "Dirt Block", ItemQuality.Common, 1, 999),
			new ItemPreset(ItemGroup.Block, ItemType.Stone, "Stone Block", ItemQuality.Common, 2, 999),
			new ItemPreset(ItemGroup.Block, ItemType.IronOre, "Iron Ore", ItemQuality.Common, 3, 999)
		});

	public struct ItemPreset {
		public ItemGroup group;
		public ItemType type;
		public ItemQuality quality;
		public int spriteId;
		public string name;
		public int stackSize;

		public ItemPreset(ItemGroup g, ItemType t, string n, ItemQuality q, int sprId, int stk) {
			group = g;
			type = t;
			name = n;
			quality = q;
			spriteId = sprId;
			stackSize = stk;
		}
	};

	public static Item Create(PresetId id) {
		if (allSprites == null) {
			allSprites = Resources.LoadAll<Sprite> ("items");
		}
		var preset = itemPresets [(int)id];

		Item item = new Item();
		item.group = preset.group;
		item.type = preset.type;
		item.name = preset.name;
		item.quality = preset.quality;
		item.preset = preset;
		item.stack = 1;

		Assert.IsTrue (preset.spriteId < allSprites.Length);
		item.sprite = allSprites[preset.spriteId];
		Assert.IsNotNull (item.sprite);
		return item;
	}

	public Sprite GetSprite() {
		return sprite;
	}
}
