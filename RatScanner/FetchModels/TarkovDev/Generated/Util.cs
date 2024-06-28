using System.Collections.Generic;
using Newtonsoft.Json;
using RatScanner.FetchModels.TarkovDev;

namespace RatScanner;


public enum HandbookCategoryName
{
	Ammo,
	AmmoPacks,
	AssaultCarbines,
	AssaultRifles,
	AssaultScopes,
	AuxiliaryParts,
	Backpacks,
	Barrels,
	BarterItems,
	Bipods,
	BodyArmor,
	BoltActionRifles,
	BuildingMaterials,
	ChargingHandles,
	Collimators,
	CompactCollimators,
	Drinks,
	ElectronicKeys,
	Electronics,
	EnergyElements,
	Eyewear,
	Facecovers,
	FlammableMaterials,
	FlashhidersBrakes,
	Flashlights,
	Food,
	Foregrips,
	FunctionalMods,
	GasBlocks,
	Gear,
	GearComponents,
	GearMods,
	GrenadeLaunchers,
	Handguards,
	Headgear,
	Headsets,
	HouseholdMaterials,
	InfoItems,
	Injectors,
	InjuryTreatment,
	IronSights,
	Keys,
	LaserTargetPointers,
	Launchers,
	LightLaserDevices,
	MachineGuns,
	Magazines,
	Maps,
	MarksmanRifles,
	MechanicalKeys,
	MedicalSupplies,
	Medication,
	Medkits,
	MeleeWeapons,
	Money,
	Mounts,
	MuzzleAdapters,
	MuzzleDevices,
	Optics,
	Others,
	Pills,
	PistolGrips,
	Pistols,
	Provisions,
	ReceiversSlides,
	Rounds,
	SmGs,
	SecureContainers,
	Shotguns,
	Sights,
	SpecialEquipment,
	SpecialPurposeSights,
	SpecialWeapons,
	StocksChassis,
	StorageContainers,
	Suppressors,
	TacticalComboDevices,
	TacticalRigs,
	Throwables,
	Tools,
	Valuables,
	VitalParts,
	WeaponPartsMods,
	Weapons,
}

public enum ItemCategoryName
{
	Ammo,
	AmmoContainer,
	ArmBand,
	Armor,
	ArmoredEquipment,
	AssaultCarbine,
	AssaultRifle,
	AssaultScope,
	AuxiliaryMod,
	Backpack,
	Barrel,
	BarterItem,
	Battery,
	Bipod,
	BuildingMaterial,
	ChargingHandle,
	ChestRig,
	CombMuzzleDevice,
	CombTactDevice,
	CommonContainer,
	CompactReflexSight,
	Compass,
	CompoundItem,
	CylinderMagazine,
	Drink,
	Drug,
	Electronics,
	Equipment,
	EssentialMod,
	FaceCover,
	Flashhider,
	Flashlight,
	Food,
	FoodAndDrink,
	Foregrip,
	Fuel,
	FunctionalMod,
	GasBlock,
	GearMod,
	GrenadeLauncher,
	Handguard,
	Handgun,
	Headphones,
	Headwear,
	HouseholdGoods,
	Info,
	Ironsight,
	Item,
	Jewelry,
	Key,
	KeyMechanical,
	Keycard,
	Knife,
	LockingContainer,
	Lubricant,
	Machinegun,
	Magazine,
	Map,
	MarksmanRifle,
	MedicalItem,
	MedicalSupplies,
	Medikit,
	Meds,
	Money,
	Mount,
	MuzzleDevice,
	NightVision,
	Other,
	PistolGrip,
	PortContainer,
	PortableRangeFinder,
	RadioTransmitter,
	RandomLootContainer,
	Receiver,
	ReflexSight,
	RepairKits,
	Revolver,
	Smg,
	Scope,
	SearchableItem,
	Shotgun,
	Sights,
	Silencer,
	SniperRifle,
	SpecialItem,
	SpecialScope,
	SpringDrivenCylinder,
	StackableItem,
	Stimulant,
	Stock,
	ThermalVision,
	ThrowableWeapon,
	Tool,
	Ubgl,
	VisObservDevice,
	Weapon,
	WeaponMod,
}


public enum ItemSourceName
{
	Prapor,
	Therapist,
	Fence,
	Skier,
	Peacekeeper,
	Mechanic,
	Ragman,
	Jaeger,
	FleaMarket,
}


public enum ItemType
{
	Ammo,
	AmmoBox,
	Any,
	Armor,
	Backpack,
	Barter,
	Container,
	Glasses,
	Grenade,
	Gun,
	Headphones,
	Helmet,
	Injectors,
	Keys,
	MarkedOnly,
	Meds,
	Mods,
	NoFlea,
	PistolGrip,
	Preset,
	Provisions,
	Rig,
	Suppressor,
	Wearable,
}

public enum LanguageCode
{
	Cs,
	De,
	En,
	Es,
	Fr,
	Hu,
	It,
	Ja,
	Ko,
	Pl,
	Pt,
	Ru,
	Sk,
	Tr,
	Zh,
}


public enum RequirementType
{
	PlayerLevel,
	LoyaltyLevel,
	QuestCompleted,
	StationLevel,
}


public enum StatusCode
{
	Ok,
	Updating,
	Unstable,
	Down,
}


public enum TraderName
{
	Prapor,
	Therapist,
	Fence,
	Skier,
	Peacekeeper,
	Mechanic,
	Ragman,
	Jaeger,
}


public class ItemProperties { }


public interface ITaskObjective
{
	[JsonProperty("id")]
	string Id { get; set; }

	[JsonProperty("type")]
	string Type { get; set; }

	[JsonProperty("description")]
	string Description { get; set; }

	[JsonProperty("maps")]
	List<Map> Maps { get; set; }

	[JsonProperty("optional")]
	bool Optional { get; set; }

	// Get needed items
	public List<NeededItem> GetNeededItems(string taskId);
}


public interface IVendor
{
	[JsonProperty("name")]
	string Name { get; set; }

	[JsonProperty("normalizedName")]
	string NormalizedName { get; set; }
}
