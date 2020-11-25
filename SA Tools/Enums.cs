using System;

namespace SASplit
{
	public enum SA1LevelIDs : byte
	{
		HedgehogHammer = 0,
		EmeraldCoast = 1,
		WindyValley = 2,
		TwinklePark = 3,
		SpeedHighway = 4,
		RedMountain = 5,
		SkyDeck = 6,
		LostWorld = 7,
		IceCap = 8,
		Casinopolis = 9,
		FinalEgg = 0xA,
		HotShelter = 0xC,
		Chaos0 = 0xF,
		Chaos2 = 0x10,
		Chaos4 = 0x11,
		Chaos6 = 0x12,
		PerfectChaos = 0x13,
		Chaos7 = 0x13,
		EggHornet = 0x14,
		EggWalker = 0x15,
		EggViper = 0x16,
		Zero = 0x17,
		E101 = 0x18,
		E101R = 0x19,
		StationSquare = 0x1A,
		EggCarrierOutside = 0x1D,
		EggCarrierInside = 0x20,
		MysticRuins = 0x21,
		Past = 0x22,
		TwinkleCircuit = 0x23,
		SkyChase1 = 0x24,
		SkyChase2 = 0x25,
		SandHill = 0x26,
		SSGarden = 0x27,
		ECGarden = 0x28,
		MRGarden = 0x29,
		ChaoRace = 0x2A,
		Invalid = 0x2B
	}

	public enum SA1Characters : byte
	{
		Sonic = 0,
		Eggman = 1,
		Tails = 2,
		Knuckles = 3,
		Tikal = 4,
		Amy = 5,
		Gamma = 6,
		Big = 7,
		MetalSonic = 8
	}

	[Flags()]
	public enum SA1CharacterFlags
	{
		Sonic = 1 << SA1Characters.Sonic,
		Eggman = 1 << SA1Characters.Eggman,
		Tails = 1 << SA1Characters.Tails,
		Knuckles = 1 << SA1Characters.Knuckles,
		Tikal = 1 << SA1Characters.Tikal,
		Amy = 1 << SA1Characters.Amy,
		Gamma = 1 << SA1Characters.Gamma,
		Big = 1 << SA1Characters.Big
	}

	public enum SA2LevelIDs : byte
	{
		BasicTest = 0,
		KnucklesTest = 1,
		SonicTest = 2,
		GreenForest = 3,
		WhiteJungle = 4,
		PumpkinHill = 5,
		SkyRail = 6,
		AquaticMine = 7,
		SecurityHall = 8,
		PrisonLane = 9,
		MetalHarbor = 0xA,
		IronGate = 0xB,
		WeaponsBed = 0xC,
		CityEscape = 0xD,
		RadicalHighway = 0xE,
		WeaponsBed2P = 0xF,
		WildCanyon = 0x10,
		MissionStreet = 0x11,
		DryLagoon = 0x12,
		SonicVsShadow1 = 0x13,
		TailsVsEggman1 = 0x14,
		SandOcean = 0x15,
		CrazyGadget = 0x16,
		HiddenBase = 0x17,
		EternalEngine = 0x18,
		DeathChamber = 0x19,
		EggQuarters = 0x1A,
		LostColony = 0x1B,
		PyramidCave = 0x1C,
		TailsVsEggman2 = 0x1D,
		FinalRush = 0x1E,
		GreenHill = 0x1F,
		MeteorHerd = 0x20,
		KnucklesVsRouge = 0x21,
		CannonsCoreS = 0x22,
		CannonsCoreE = 0x23,
		CannonsCoreT = 0x24,
		CannonsCoreR = 0x25,
		CannonsCoreK = 0x26,
		MissionStreet2P = 0x27,
		FinalChase = 0x28,
		WildCanyon2P = 0x29,
		SonicVsShadow2 = 0x2A,
		CosmicWall = 0x2B,
		MadSpace = 0x2C,
		SandOcean2P = 0x2D,
		DryLagoon2P = 0x2E,
		PyramidRace = 0x2F,
		HiddenBase2P = 0x30,
		PoolQuest = 0x31,
		PlanetQuest = 0x32,
		DeckRace = 0x33,
		DowntownRace = 0x34,
		CosmicWall2P = 0x35,
		GrindRace = 0x36,
		LostColony2P = 0x37,
		EternalEngine2P = 0x38,
		MetalHarbor2P = 0x39,
		IronGate2P = 0x3A,
		DeathChamber2P = 0x3B,
		BigFoot = 0x3C,
		HotShot = 0x3D,
		FlyingDog = 0x3E,
		KingBoomBoo = 0x3F,
		EggGolemS = 0x40,
		Biolizard = 0x41,
		FinalHazard = 0x42,
		EggGolemE = 0x43,
		Route101280 = 70,
		KartRace = 71,
		ChaoWorld = 90,
		Invalid = 91
	}

	public enum SA2Characters
	{
		Sonic = 0,
		Shadow = 1,
		Tails = 2,
		Eggman = 3,
		Knuckles = 4,
		Rouge = 5,
		MechTails = 6,
		MechEggman = 7
	}

	public enum Languages
	{
		Japanese = 0,
		English = 1,
		French = 2,
		Spanish = 3,
		German = 4
	}

	public enum ChaoItemCategory
	{
		ChaoItemCategory_Egg = 1,
		ChaoItemCategory_Fruit = 3,
		ChaoItemCategory_Seed = 7,
		ChaoItemCategory_Hat = 9,
		ChaoItemCategory_MenuTheme = 0x10
	}

}
