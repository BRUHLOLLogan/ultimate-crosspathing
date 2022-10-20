using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using BTD_Mod_Helper.Extensions;
using BTD_Mod_Helper.Api;

namespace UltimateCrosspathing.Loaders;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Runtime.Serialization;
using Il2CppSystem.Reflection;
using Il2CppSystem;
using Assets.Scripts.Simulation.SMath;
using System.IO;

public class SuperMonkeyLoader : ModByteLoader<Assets.Scripts.Models.Towers.TowerModel> {
	
	BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static; 
	BinaryReader br = null;
	
	// NOTE: was a collection per type but it prevented inheriance e.g list of Products would required class type id
	protected override string BytesFileName => "SuperMonkeys.bytes";
	int mIndex = 1; // first element is null
	#region Read array
	
	private void LinkArray<T>() where T : Il2CppObjectBase {
		var setCount = br.ReadInt32();
		for (var i = 0; i < setCount; i++) {
			var arrIndex = br.ReadInt32();
			var arr = (Il2CppReferenceArray<T>)m[arrIndex];
			for (var j = 0; j < arr.Length; j++) {
				arr[j] = (T) m[br.ReadInt32()];
			}
		}
	}
	private void LinkList<T>() where T : Il2CppObjectBase {
		var setCount = br.ReadInt32();
		for (var i = 0; i < setCount; i++) {
			var arrIndex = br.ReadInt32();
			var arr = (List<T>)m[arrIndex];
			for (var j = 0; j < arr.Capacity; j++) {
				arr.Add( (T) m[br.ReadInt32()] );
			}
		}
	}
	private void LinkDictionary<T>() where T : Il2CppObjectBase {
		var setCount = br.ReadInt32();
		for (var i = 0; i < setCount; i++) {
			var arrIndex = br.ReadInt32();
			var arr = (Dictionary<string, T>)m[arrIndex];
			var arrCount = br.ReadInt32();
			for (var j = 0; j < arrCount; j++) {
				var key = br.ReadString();
				var valueIndex = br.ReadInt32();
				arr[key] = (T) m[valueIndex];
			}
		}
	}
	private void LinkModelDictionary<T>() where T : Assets.Scripts.Models.Model {
		var setCount = br.ReadInt32();
		for (var i = 0; i < setCount; i++) {
			var arrIndex = br.ReadInt32();
			var arr = (Dictionary<string, T>)m[arrIndex];
			var arrCount = br.ReadInt32();
			for (var j = 0; j < arrCount; j++) {
				var valueIndex = br.ReadInt32();
				var obj = (T)m[valueIndex];
				arr[obj.name] = obj;
			}
		}
	}
	private void Read_a_Int32_Array() {
		var arrSetCount = br.ReadInt32();
		var count = arrSetCount;
		for (var i = 0; i < count; i++) {
			var arrCount = br.ReadInt32();
			var arr = new Il2CppStructArray<int>(arrCount);
			for (var j = 0; j < arr.Length; j++) {
				arr[j] = br.ReadInt32();
			}
			m[mIndex++] = arr;
		}
	}
	private void Read_a_String_Array() {
		var arrSetCount = br.ReadInt32();
		var count = arrSetCount;
		for (var i = 0; i < count; i++) {
			var arrCount = br.ReadInt32();
			var arr = new Il2CppStringArray(arrCount);
			for (var j = 0; j < arr.Length; j++) {
				arr[j] = br.ReadBoolean() ? null : br.ReadString();
			}
			m[mIndex++] = arr;
		}
	}
	private void Read_a_Vector3_Array() {
		var arrSetCount = br.ReadInt32();
		var count = arrSetCount;
		for (var i = 0; i < count; i++) {
			var arrCount = br.ReadInt32();
			var arr = new Il2CppStructArray<Assets.Scripts.Simulation.SMath.Vector3>(arrCount);
			for (var j = 0; j < arr.Length; j++) {
				arr[j] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
			}
			m[mIndex++] = arr;
		}
	}
	private void Read_a_TargetType_Array() {
		var arrSetCount = br.ReadInt32();
		var count = arrSetCount;
		for (var i = 0; i < count; i++) {
			var arrCount = br.ReadInt32();
			var arr = new Il2CppReferenceArray<Assets.Scripts.Models.Towers.TargetType>(arrCount);
			for (var j = 0; j < arr.Length; j++) {
				arr[j] = new Assets.Scripts.Models.Towers.TargetType {id = br.ReadString(), isActionable = br.ReadBoolean()};
			}
			m[mIndex++] = arr;
		}
	}
	private void Read_a_AreaType_Array() {
		var arrSetCount = br.ReadInt32();
		var count = arrSetCount;
		for (var i = 0; i < count; i++) {
			var arrCount = br.ReadInt32();
			var arr = new Il2CppStructArray<Assets.Scripts.Models.Map.AreaType>(arrCount);
			for (var j = 0; j < arr.Length; j++) {
				arr[j] = (Assets.Scripts.Models.Map.AreaType)br.ReadInt32();
			}
			m[mIndex++] = arr;
		}
	}
	#endregion
	
	#region Read object records
	
	private void CreateArraySet<T>() where T : Il2CppObjectBase {
		var arrCount = br.ReadInt32();
		for(var i = 0; i < arrCount; i++) {
			m[mIndex++] = new Il2CppReferenceArray<T>(br.ReadInt32());;
		}
	}
	
	private void CreateListSet<T>() where T : Il2CppObjectBase {
		var arrCount = br.ReadInt32();
		for (var i = 0; i < arrCount; i++) {
			m[mIndex++] = new List<T>(br.ReadInt32()); // set capactity
		}
	}
	
	private void CreateDictionarySet<K, T>() {
		var arrCount = br.ReadInt32();
		for (var i = 0; i < arrCount; i++) {
			m[mIndex++] = new Dictionary<K, T>(br.ReadInt32());// set capactity
		}
	}
	
	private void Create_Records<T>() where T : Il2CppObjectBase {
		var count = br.ReadInt32();
		var t = Il2CppType.Of<T>();
		for (var i = 0; i < count; i++) {
			m[mIndex++] = FormatterServices.GetUninitializedObject(t).Cast<T>();
		}
	}
	#endregion
	
	#region Link object records
	
	private void Set_v_Model_Fields(int start, int count) {
		var t = Il2CppType.Of<Assets.Scripts.Models.Model>();
		var _nameField = t.GetField("_name", bindFlags);
		var childDependantsField = t.GetField("childDependants", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Model)m[i+start];
			_nameField.SetValue(v,br.ReadBoolean() ? null : String.Intern(br.ReadString()));
			childDependantsField.SetValue(v,(List<Assets.Scripts.Models.Model>) m[br.ReadInt32()]);
		}
	}
	
	private void Set_v_TowerModel_Fields(int start, int count) {
		Set_v_Model_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.TowerModel)m[i+start];
			v.display = ModContent.CreatePrefabReference(br.ReadString());
			v.baseId = br.ReadBoolean() ? null : br.ReadString();
			v.cost = br.ReadSingle();
			v.radius = br.ReadSingle();
			v.radiusSquared = br.ReadSingle();
			v.range = br.ReadSingle();
			v.ignoreBlockers = br.ReadBoolean();
			v.isGlobalRange = br.ReadBoolean();
			v.tier = br.ReadInt32();
			v.tiers = (Il2CppStructArray<int>) m[br.ReadInt32()];
			v.towerSet = br.ReadBoolean() ? null : br.ReadString();
			v.areaTypes = (Il2CppStructArray<Assets.Scripts.Models.Map.AreaType>) m[br.ReadInt32()];
			v.icon = ModContent.CreateSpriteReference(br.ReadString());
			v.portrait = ModContent.CreateSpriteReference(br.ReadString());
			v.instaIcon = ModContent.CreateSpriteReference(br.ReadString());
			v.mods = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Mods.ApplyModModel>) m[br.ReadInt32()];
			v.ignoreTowerForSelection = br.ReadBoolean();
			v.behaviors = (Il2CppReferenceArray<Assets.Scripts.Models.Model>) m[br.ReadInt32()];
			v.footprint = (Assets.Scripts.Models.Towers.Behaviors.FootprintModel) m[br.ReadInt32()];
			v.dontDisplayUpgrades = br.ReadBoolean();
			v.emoteSpriteSmall = ModContent.CreateSpriteReference(br.ReadString());
			v.emoteSpriteLarge = ModContent.CreateSpriteReference(br.ReadString());
			v.doesntRotate = br.ReadBoolean();
			v.upgrades = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Upgrades.UpgradePathModel>) m[br.ReadInt32()];
			v.appliedUpgrades = (Il2CppStringArray) m[br.ReadInt32()];
			v.targetTypes = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.TargetType>) m[br.ReadInt32()];
			v.paragonUpgrade = (Assets.Scripts.Models.Towers.Upgrades.UpgradePathModel) m[br.ReadInt32()];
			v.isSubTower = br.ReadBoolean();
			v.isBakable = br.ReadBoolean();
			v.powerName = br.ReadBoolean() ? null : br.ReadString();
			v.showPowerTowerBuffs = br.ReadBoolean();
			v.animationSpeed = br.ReadSingle();
			v.towerSelectionMenuThemeId = br.ReadBoolean() ? null : br.ReadString();
			v.ignoreCoopAreas = br.ReadBoolean();
			v.canAlwaysBeSold = br.ReadBoolean();
			v.blockSelling = br.ReadBoolean();
			v.isParagon = br.ReadBoolean();
			v.ignoreMaxSellPercent = br.ReadBoolean();
			v.isStunned = br.ReadBoolean();
			v.geraldoItemName = br.ReadBoolean() ? null : br.ReadString();
			v.sellbackModifierAdd = br.ReadSingle();
			v.skinName = br.ReadBoolean() ? null : br.ReadString();
		}
	}
	
	private void Set_v_ApplyModModel_Fields(int start, int count) {
		Set_v_Model_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Mods.ApplyModModel)m[i+start];
			v.mod = br.ReadBoolean() ? null : br.ReadString();
			v.target = br.ReadBoolean() ? null : br.ReadString();
		}
	}
	
	private void Set_v_TowerBehaviorModel_Fields(int start, int count) {
		Set_v_Model_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.TowerBehaviorModel)m[i+start];
		}
	}
	
	private void Set_v_TowerRadiusModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.TowerRadiusModel)m[i+start];
		}
	}
	
	private void Set_v_CreateSoundOnSellModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.CreateSoundOnSellModel)m[i+start];
			v.sound = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_SoundModel_Fields(int start, int count) {
		Set_v_Model_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Audio.SoundModel)m[i+start];
			v.assetId = ModContent.CreateAudioSourceReference(br.ReadString());
		}
	}
	
	private void Set_v_CreateSoundOnUpgradeModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.CreateSoundOnUpgradeModel)m[i+start];
			v.sound = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			v.sound1 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			v.sound2 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			v.sound3 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			v.sound4 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			v.sound5 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			v.sound6 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			v.sound7 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			v.sound8 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_CreateSoundOnAttachedModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.CreateSoundOnAttachedModel)m[i+start];
			v.sound = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			v.altSound = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_CreateEffectOnSellModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnSellModel)m[i+start];
			v.effectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_EffectModel_Fields(int start, int count) {
		Set_v_Model_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Effects.EffectModel)m[i+start];
			v.assetId = ModContent.CreatePrefabReference(br.ReadString());
			v.scale = br.ReadSingle();
			v.lifespan = br.ReadSingle();
			v.fullscreen = br.ReadBoolean();
			v.useCenterPosition = br.ReadBoolean();
			v.useTransformPosition = br.ReadBoolean();
			v.useTransfromRotation = br.ReadBoolean();
			v.destroyOnTransformDestroy = br.ReadBoolean();
			v.alwaysUseAge = br.ReadBoolean();
			v.useRoundTime = br.ReadBoolean();
		}
	}
	
	private void Set_v_CreateEffectOnPlaceModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnPlaceModel)m[i+start];
			v.effectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_OverrideCamoDetectionModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.OverrideCamoDetectionModel)m[i+start];
			v.detectCamo = br.ReadBoolean();
		}
	}
	
	private void Set_v_CreateSoundOnTowerPlaceModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.CreateSoundOnTowerPlaceModel)m[i+start];
			v.sound1 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			v.sound2 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			v.heroSound1 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			v.heroSound2 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_CreateEffectOnUpgradeModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnUpgradeModel)m[i+start];
			v.effectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_AttackModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.AttackModel)m[i+start];
			v.weapons = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Weapons.WeaponModel>) m[br.ReadInt32()];
			v.behaviors = (Il2CppReferenceArray<Assets.Scripts.Models.Model>) m[br.ReadInt32()];
			v.range = br.ReadSingle();
			v.targetProvider = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetSupplierModel) m[br.ReadInt32()];
			v.offsetX = br.ReadSingle();
			v.offsetY = br.ReadSingle();
			v.offsetZ = br.ReadSingle();
			v.attackThroughWalls = br.ReadBoolean();
			v.fireWithoutTarget = br.ReadBoolean();
			v.framesBeforeRetarget = br.ReadInt32();
			v.addsToSharedGrid = br.ReadBoolean();
			v.sharedGridRange = br.ReadSingle();
		}
	}
	
	private void Set_v_WeaponModel_Fields(int start, int count) {
		Set_v_Model_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Weapons.WeaponModel>();
		var animationOffsetField = t.GetField("animationOffset", bindFlags);
		var rateField = t.GetField("rate", bindFlags);
		var customStartCooldownField = t.GetField("customStartCooldown", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Weapons.WeaponModel)m[i+start];
			v.animation = br.ReadInt32();
			animationOffsetField.SetValue(v,br.ReadSingle().ToIl2Cpp());
			v.emission = (Assets.Scripts.Models.Towers.Behaviors.Emissions.EmissionModel) m[br.ReadInt32()];
			v.ejectX = br.ReadSingle();
			v.ejectY = br.ReadSingle();
			v.ejectZ = br.ReadSingle();
			v.projectile = (Assets.Scripts.Models.Towers.Projectiles.ProjectileModel) m[br.ReadInt32()];
			v.fireWithoutTarget = br.ReadBoolean();
			v.fireBetweenRounds = br.ReadBoolean();
			v.behaviors = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Weapons.WeaponBehaviorModel>) m[br.ReadInt32()];
			rateField.SetValue(v,br.ReadSingle().ToIl2Cpp());
			v.useAttackPosition = br.ReadBoolean();
			v.startInCooldown = br.ReadBoolean();
			customStartCooldownField.SetValue(v,br.ReadSingle().ToIl2Cpp());
			v.animateOnMainAttack = br.ReadBoolean();
			v.isStunned = br.ReadBoolean();
		}
	}
	
	private void Set_v_RandomArcEmissionModel_Fields(int start, int count) {
		Set_v_ArcEmissionModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.RandomArcEmissionModel)m[i+start];
			v.randomAngle = br.ReadSingle();
			v.startOffset = br.ReadSingle();
		}
	}
	
	private void Set_v_ProjectileModel_Fields(int start, int count) {
		Set_v_Model_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.ProjectileModel)m[i+start];
			v.display = ModContent.CreatePrefabReference(br.ReadString());
			v.id = br.ReadBoolean() ? null : br.ReadString();
			v.maxPierce = br.ReadSingle();
			v.pierce = br.ReadSingle();
			v.scale = br.ReadSingle();
			v.ignoreBlockers = br.ReadBoolean();
			v.usePointCollisionWithBloons = br.ReadBoolean();
			v.canCollisionBeBlockedByMapLos = br.ReadBoolean();
			v.filters = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Filters.FilterModel>) m[br.ReadInt32()];
			v.behaviors = (Il2CppReferenceArray<Assets.Scripts.Models.Model>) m[br.ReadInt32()];
			v.collisionPasses = (Il2CppStructArray<int>) m[br.ReadInt32()];
			v.canCollideWithBloons = br.ReadBoolean();
			v.radius = br.ReadSingle();
			v.vsBlockerRadius = br.ReadSingle();
			v.hasDamageModifiers = br.ReadBoolean();
			v.dontUseCollisionChecker = br.ReadBoolean();
			v.checkCollisionFrames = br.ReadInt32();
			v.ignoreNonTargetable = br.ReadBoolean();
			v.ignorePierceExhaustion = br.ReadBoolean();
			v.saveId = br.ReadBoolean() ? null : br.ReadString();
		}
	}
	
	private void Set_v_FilterModel_Fields(int start, int count) {
		Set_v_Model_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Filters.FilterModel)m[i+start];
		}
	}
	
	private void Set_v_FilterInvisibleModel_Fields(int start, int count) {
		Set_v_FilterModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Filters.FilterInvisibleModel)m[i+start];
			v.isActive = br.ReadBoolean();
			v.ignoreBroadPhase = br.ReadBoolean();
		}
	}
	
	private void Set_v_ProjectileBehaviorModel_Fields(int start, int count) {
		Set_v_Model_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.ProjectileBehaviorModel)m[i+start];
			v.collisionPass = br.ReadInt32();
		}
	}
	
	private void Set_v_TravelStraitModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Projectiles.Behaviors.TravelStraitModel>();
		var speedField = t.GetField("speed", bindFlags);
		var lifespanField = t.GetField("lifespan", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.TravelStraitModel)m[i+start];
			speedField.SetValue(v,br.ReadSingle().ToIl2Cpp());
			lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
		}
	}
	
	private void Set_v_DamageModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.DamageModel)m[i+start];
			v.damage = br.ReadSingle();
			v.maxDamage = br.ReadSingle();
			v.distributeToChildren = br.ReadBoolean();
			v.overrideDistributeBlocker = br.ReadBoolean();
			v.createPopEffect = br.ReadBoolean();
			v.immuneBloonProperties = (BloonProperties) (br.ReadInt32());
			v.immuneBloonPropertiesOriginal = (BloonProperties) (br.ReadInt32());
		}
	}
	
	private void Set_v_ProjectileFilterModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.ProjectileFilterModel)m[i+start];
			v.filters = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Filters.FilterModel>) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_KnockbackModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Projectiles.Behaviors.KnockbackModel>();
		var lifespanField = t.GetField("lifespan", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.KnockbackModel)m[i+start];
			v.moabMultiplier = br.ReadSingle();
			v.heavyMultiplier = br.ReadSingle();
			v.lightMultiplier = br.ReadSingle();
			v.mutationId = br.ReadBoolean() ? null : br.ReadString();
			lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
		}
	}
	
	private void Set_v_ShowTextOnHitModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.ShowTextOnHitModel)m[i+start];
			v.assetId = ModContent.CreatePrefabReference(br.ReadString());
			v.lifespan = br.ReadSingle();
			v.useTowerPosition = br.ReadBoolean();
			v.text = br.ReadBoolean() ? null : br.ReadString();
		}
	}
	
	private void Set_v_DamageModifierModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.DamageModifierModel)m[i+start];
		}
	}
	
	private void Set_v_DamageModifierForTagModel_Fields(int start, int count) {
		Set_v_DamageModifierModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.DamageModifierForTagModel)m[i+start];
			v.tag = br.ReadBoolean() ? null : br.ReadString();
			v.tags = (Il2CppStringArray) m[br.ReadInt32()];
			v.damageMultiplier = br.ReadSingle();
			v.damageAddative = br.ReadSingle();
			v.mustIncludeAllTags = br.ReadBoolean();
			v.applyOverMaxDamage = br.ReadBoolean();
		}
	}
	
	private void Set_v_DisplayModel_Fields(int start, int count) {
		Set_v_Model_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.GenericBehaviors.DisplayModel)m[i+start];
			v.display = ModContent.CreatePrefabReference(br.ReadString());
			v.layer = br.ReadInt32();
			v.positionOffset = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
			v.scale = br.ReadSingle();
			v.ignoreRotation = br.ReadBoolean();
			v.animationChanges = (List<Assets.Scripts.Models.GenericBehaviors.AnimationChange>) m[br.ReadInt32()];
			v.delayedReveal = br.ReadSingle();
		}
	}
	
	private void Set_v_WeaponBehaviorModel_Fields(int start, int count) {
		Set_v_Model_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Weapons.WeaponBehaviorModel)m[i+start];
		}
	}
	
	private void Set_v_UseAttackRotationModel_Fields(int start, int count) {
		Set_v_WeaponBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.UseAttackRotationModel)m[i+start];
		}
	}
	
	private void Set_v_CritMultiplierModel_Fields(int start, int count) {
		Set_v_WeaponBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.CritMultiplierModel)m[i+start];
			v.damage = br.ReadSingle();
			v.lower = br.ReadInt32();
			v.upper = br.ReadInt32();
			v.display = ModContent.CreatePrefabReference(br.ReadString());
			v.distributeToChildren = br.ReadBoolean();
		}
	}
	
	private void Set_v_AttackBehaviorModel_Fields(int start, int count) {
		Set_v_Model_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.AttackBehaviorModel)m[i+start];
		}
	}
	
	private void Set_v_RotateToMiddleOfTargetsModel_Fields(int start, int count) {
		Set_v_AttackBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.RotateToMiddleOfTargetsModel)m[i+start];
			v.onlyRotateDuringThrow = br.ReadBoolean();
		}
	}
	
	private void Set_v_RotateToTargetModel_Fields(int start, int count) {
		Set_v_AttackBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.RotateToTargetModel)m[i+start];
			v.onlyRotateDuringThrow = br.ReadBoolean();
			v.useThrowMarkerHeight = br.ReadBoolean();
			v.rotateOnlyOnThrow = br.ReadBoolean();
			v.additionalRotation = br.ReadInt32();
			v.rotateTower = br.ReadBoolean();
			v.useMainAttackRotation = br.ReadBoolean();
		}
	}
	
	private void Set_v_AttackFilterModel_Fields(int start, int count) {
		Set_v_AttackBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.AttackFilterModel)m[i+start];
			v.filters = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Filters.FilterModel>) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_TargetSupplierModel_Fields(int start, int count) {
		Set_v_AttackBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetSupplierModel)m[i+start];
			v.isOnSubTower = br.ReadBoolean();
		}
	}
	
	private void Set_v_TargetRightHandModel_Fields(int start, int count) {
		Set_v_TargetSupplierModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetRightHandModel)m[i+start];
			v.isSelectable = br.ReadBoolean();
		}
	}
	
	private void Set_v_TargetCamoModel_Fields(int start, int count) {
		Set_v_TargetSupplierModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetCamoModel)m[i+start];
		}
	}
	
	private void Set_v_TargetFirstPrioCamoModel_Fields(int start, int count) {
		Set_v_TargetCamoModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetFirstPrioCamoModel)m[i+start];
			v.isSelectable = br.ReadBoolean();
		}
	}
	
	private void Set_v_TargetLastPrioCamoModel_Fields(int start, int count) {
		Set_v_TargetCamoModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetLastPrioCamoModel)m[i+start];
			v.isSelectable = br.ReadBoolean();
		}
	}
	
	private void Set_v_TargetClosePrioCamoModel_Fields(int start, int count) {
		Set_v_TargetCamoModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetClosePrioCamoModel)m[i+start];
			v.isSelectable = br.ReadBoolean();
		}
	}
	
	private void Set_v_TargetStrongPrioCamoModel_Fields(int start, int count) {
		Set_v_TargetCamoModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetStrongPrioCamoModel)m[i+start];
			v.isSelectable = br.ReadBoolean();
		}
	}
	
	private void Set_v_TargetLeftHandModel_Fields(int start, int count) {
		Set_v_TargetSupplierModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetLeftHandModel)m[i+start];
			v.isSelectable = br.ReadBoolean();
		}
	}
	
	private void Set_v_EjectEffectModel_Fields(int start, int count) {
		Set_v_WeaponBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.EjectEffectModel)m[i+start];
			v.assetId = ModContent.CreatePrefabReference(br.ReadString());
			v.effectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
			v.lifespan = br.ReadSingle();
			v.fullscreen = br.ReadBoolean();
			v.rotateToWeapon = br.ReadBoolean();
			v.useEjectPoint = br.ReadBoolean();
			v.useEmittedFrom = br.ReadBoolean();
			v.useMainAttackRotation = br.ReadBoolean();
		}
	}
	
	private void Set_v_AbilityModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Behaviors.Abilities.AbilityModel>();
		var cooldownSpeedScaleField = t.GetField("cooldownSpeedScale", bindFlags);
		var animationOffsetField = t.GetField("animationOffset", bindFlags);
		var cooldownField = t.GetField("cooldown", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Abilities.AbilityModel)m[i+start];
			v.displayName = br.ReadBoolean() ? null : br.ReadString();
			v.description = br.ReadBoolean() ? null : br.ReadString();
			v.icon = ModContent.CreateSpriteReference(br.ReadString());
			v.behaviors = (Il2CppReferenceArray<Assets.Scripts.Models.Model>) m[br.ReadInt32()];
			v.activateOnPreLeak = br.ReadBoolean();
			v.activateOnLeak = br.ReadBoolean();
			v.addedViaUpgrade = br.ReadBoolean() ? null : br.ReadString();
			v.livesCost = br.ReadInt32();
			v.maxActivationsPerRound = br.ReadInt32();
			v.animation = br.ReadInt32();
			v.enabled = br.ReadBoolean();
			v.canActivateBetweenRounds = br.ReadBoolean();
			v.resetCooldownOnTierUpgrade = br.ReadBoolean();
			v.activateOnLivesLost = br.ReadBoolean();
			v.sharedCooldown = br.ReadBoolean();
			v.dontShowStacked = br.ReadBoolean();
			v.animateOnMainAttackDisplay = br.ReadBoolean();
			v.restrictAbilityAfterMaxRoundTimer = br.ReadBoolean();
			cooldownSpeedScaleField.SetValue(v,br.ReadSingle().ToIl2Cpp());
			animationOffsetField.SetValue(v,br.ReadSingle().ToIl2Cpp());
			cooldownField.SetValue(v,br.ReadSingle().ToIl2Cpp());
		}
	}
	
	private void Set_v_AbilityBehaviorModel_Fields(int start, int count) {
		Set_v_Model_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Abilities.AbilityBehaviorModel)m[i+start];
		}
	}
	
	private void Set_v_DarkshiftModel_Fields(int start, int count) {
		Set_v_AbilityBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.DarkshiftModel)m[i+start];
			v.restrictToTowerRadius = br.ReadBoolean();
			v.placementZoneAssetRadius = br.ReadSingle();
			v.placementZoneAsset = ModContent.CreatePrefabReference(br.ReadString());
			v.darkshiftSound = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			v.disappearEffectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
			v.reappearEffectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_FootprintModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.FootprintModel)m[i+start];
			v.doesntBlockTowerPlacement = br.ReadBoolean();
			v.ignoresPlacementCheck = br.ReadBoolean();
			v.ignoresTowerOverlap = br.ReadBoolean();
		}
	}
	
	private void Set_v_CircleFootprintModel_Fields(int start, int count) {
		Set_v_FootprintModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.CircleFootprintModel)m[i+start];
			v.radius = br.ReadSingle();
		}
	}
	
	private void Set_v_UpgradePathModel_Fields(int start, int count) {
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Upgrades.UpgradePathModel>();
		var towerField = t.GetField("tower", bindFlags);
		var upgradeField = t.GetField("upgrade", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Upgrades.UpgradePathModel)m[i+start];
			towerField.SetValue(v,br.ReadBoolean() ? null : br.ReadString());
			upgradeField.SetValue(v,br.ReadBoolean() ? null : br.ReadString());
		}
	}
	
	private void Set_v_EmissionModel_Fields(int start, int count) {
		Set_v_Model_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.EmissionModel)m[i+start];
			v.behaviors = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Behaviors.Emissions.EmissionBehaviorModel>) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_EmissionWithOffsetsModel_Fields(int start, int count) {
		Set_v_EmissionModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.EmissionWithOffsetsModel)m[i+start];
			v.throwMarkerOffsetModels = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Weapons.Behaviors.ThrowMarkerOffsetModel>) m[br.ReadInt32()];
			v.projectileCount = br.ReadInt32();
			v.rotateProjectileWithTower = br.ReadBoolean();
			v.randomRotationCone = br.ReadSingle();
		}
	}
	
	private void Set_v_ThrowMarkerOffsetModel_Fields(int start, int count) {
		Set_v_WeaponBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.ThrowMarkerOffsetModel)m[i+start];
			v.ejectX = br.ReadSingle();
			v.ejectY = br.ReadSingle();
			v.ejectZ = br.ReadSingle();
			v.rotation = br.ReadSingle();
		}
	}
	
	private void Set_v_ImmunityModel_Fields(int start, int count) {
		Set_v_AbilityBehaviorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.ImmunityModel>();
		var lifespanField = t.GetField("lifespan", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.ImmunityModel)m[i+start];
			lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
			v.effectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_CreateEffectOnAbilityModel_Fields(int start, int count) {
		Set_v_AbilityBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.CreateEffectOnAbilityModel)m[i+start];
			v.effectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
			v.randomRotation = br.ReadBoolean();
			v.centerEffect = br.ReadBoolean();
			v.destroyOnEnd = br.ReadBoolean();
			v.useAttackTransform = br.ReadBoolean();
			v.canSave = br.ReadBoolean();
		}
	}
	
	private void Set_v_ActivateAttackModel_Fields(int start, int count) {
		Set_v_AbilityBehaviorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.ActivateAttackModel>();
		var lifespanField = t.GetField("lifespan", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.ActivateAttackModel)m[i+start];
			v.attacks = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Behaviors.Attack.AttackModel>) m[br.ReadInt32()];
			v.processOnActivate = br.ReadBoolean();
			v.cancelIfNoTargets = br.ReadBoolean();
			v.turnOffExisting = br.ReadBoolean();
			v.endOnRoundEnd = br.ReadBoolean();
			v.endOnDefeatScreen = br.ReadBoolean();
			v.isOneShot = br.ReadBoolean();
			lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
		}
	}
	
	private void Set_v_SingleEmissionModel_Fields(int start, int count) {
		Set_v_EmissionModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.SingleEmissionModel)m[i+start];
		}
	}
	
	private void Set_v_AgeModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Projectiles.Behaviors.AgeModel>();
		var lifespanField = t.GetField("lifespan", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.AgeModel)m[i+start];
			v.rounds = br.ReadInt32();
			v.useRoundTime = br.ReadBoolean();
			lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
			v.endOfRoundClearBypassModel = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.EndOfRoundClearBypassModel) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_DistributeToChildrenBloonModifierModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.DistributeToChildrenBloonModifierModel)m[i+start];
			v.bloonTag = br.ReadBoolean() ? null : br.ReadString();
			v.bloonTags = (Il2CppStringArray) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_CreateSoundOnAbilityModel_Fields(int start, int count) {
		Set_v_AbilityBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.CreateSoundOnAbilityModel)m[i+start];
			v.sound = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			v.heroSound = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			v.heroSound2 = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_RotateToDefaultPositionTowerModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.RotateToDefaultPositionTowerModel)m[i+start];
			v.rotation = br.ReadSingle();
			v.onlyOnReachingTier = br.ReadInt32();
		}
	}
	
	private void Set_v_RectangleFootprintModel_Fields(int start, int count) {
		Set_v_FootprintModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.RectangleFootprintModel)m[i+start];
			v.xWidth = br.ReadSingle();
			v.yWidth = br.ReadSingle();
		}
	}
	
	private void Set_v_ProjectileBehaviorWithOverlayModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.ProjectileBehaviorWithOverlayModel)m[i+start];
			v.overlayType = br.ReadBoolean() ? null : br.ReadString();
		}
	}
	
	private void Set_v_WindModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorWithOverlayModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.WindModel)m[i+start];
			v.distanceMin = br.ReadSingle();
			v.distanceMax = br.ReadSingle();
			v.chance = br.ReadSingle();
			v.affectMoab = br.ReadBoolean();
			v.distanceScaleForTags = br.ReadSingle();
			v.distanceScaleForTagsTags = br.ReadBoolean() ? null : br.ReadString();
			v.distanceScaleForTagsTagsList = (Il2CppStringArray) m[br.ReadInt32()];
			v.speedMultiplier = br.ReadSingle();
		}
	}
	
	private void Set_v_CheckTempleCanFireModel_Fields(int start, int count) {
		Set_v_WeaponBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.CheckTempleCanFireModel)m[i+start];
		}
	}
	
	private void Set_v_MonkeyTempleModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Behaviors.MonkeyTempleModel>();
		var weaponDelayField = t.GetField("weaponDelay", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.MonkeyTempleModel)m[i+start];
			v.towerGroupCount = br.ReadInt32();
			v.effectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
			v.towerEffectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
			v.heroEffectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
			v.darkTransformSound = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			v.darkAltTransformSound = (Assets.Scripts.Models.Audio.SoundModel) m[br.ReadInt32()];
			v.animation = br.ReadInt32();
			v.upgradeAnimation = br.ReadInt32();
			weaponDelayField.SetValue(v,br.ReadSingle().ToIl2Cpp());
			v.templeId = br.ReadBoolean() ? null : br.ReadString();
			v.checkForThereCanOnlyBeOne = br.ReadBoolean();
			v.transformationEffect = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
			v.theAntiBloonSacrificeEffect = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
			v.legendOfTheNightSacrificeEffect = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
			v.transformationAnimation = br.ReadInt32();
			v.transformationWeaponDelay = br.ReadSingle();
			v.heroOverlapYAdjustment = br.ReadSingle();
		}
	}
	
	private void Set_v_TempleTowerMutatorGroupModel_Fields(int start, int count) {
		Set_v_TowerMutatorGroupModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.TempleTowerMutatorGroupModel)m[i+start];
			v.cost = br.ReadInt32();
			v.towerSet = br.ReadBoolean() ? null : br.ReadString();
		}
	}
	
	private void Set_v_TowerMutatorGroupModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.TowerMutatorGroupModel)m[i+start];
			v.mutators = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.Mutators.TowerMutatorModel>) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_TempleTowerMutatorGroupTierOneModel_Fields(int start, int count) {
		Set_v_TempleTowerMutatorGroupModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.TempleTowerMutatorGroupTierOneModel)m[i+start];
		}
	}
	
	private void Set_v_TowerMutatorModel_Fields(int start, int count) {
		Set_v_Model_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Mutators.TowerMutatorModel)m[i+start];
			v.mutationId = br.ReadBoolean() ? null : br.ReadString();
			v.conditionalId = (Assets.Scripts.Models.Towers.Mutators.Conditions.ConditionalModel) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_PierceTowerMutatorModel_Fields(int start, int count) {
		Set_v_TowerMutatorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Mutators.PierceTowerMutatorModel>();
		var lifespanField = t.GetField("lifespan", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Mutators.PierceTowerMutatorModel)m[i+start];
			v.pierce = br.ReadInt32();
			lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
		}
	}
	
	private void Set_v_DamageTowerMutatorModel_Fields(int start, int count) {
		Set_v_TowerMutatorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Mutators.DamageTowerMutatorModel>();
		var lifespanField = t.GetField("lifespan", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Mutators.DamageTowerMutatorModel)m[i+start];
			v.damage = br.ReadSingle();
			lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
		}
	}
	
	private void Set_v_AddAttackTowerMutatorModel_Fields(int start, int count) {
		Set_v_TowerMutatorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Mutators.AddAttackTowerMutatorModel>();
		var lifespanField = t.GetField("lifespan", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Mutators.AddAttackTowerMutatorModel)m[i+start];
			v.attackModel = (Assets.Scripts.Models.Towers.TowerBehaviorModel) m[br.ReadInt32()];
			lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
		}
	}
	
	private void Set_v_ArcEmissionModel_Fields(int start, int count) {
		Set_v_EmissionModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Behaviors.Emissions.ArcEmissionModel>();
		var CountField = t.GetField("Count", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.ArcEmissionModel)m[i+start];
			v.angle = br.ReadSingle();
			v.offset = br.ReadSingle();
			v.useProjectileRotation = br.ReadBoolean();
			CountField.SetValue(v,br.ReadInt32().ToIl2Cpp());
		}
	}
	
	private void Set_v_RotateModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Projectiles.Behaviors.RotateModel>();
		var angleField = t.GetField("angle", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.RotateModel)m[i+start];
			angleField.SetValue(v,br.ReadSingle().ToIl2Cpp());
		}
	}
	
	private void Set_v_UseTowerRangeModel_Fields(int start, int count) {
		Set_v_AttackBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.UseTowerRangeModel)m[i+start];
		}
	}
	
	private void Set_v_TargetFirstModel_Fields(int start, int count) {
		Set_v_TargetSupplierModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetFirstModel)m[i+start];
			v.isSelectable = br.ReadBoolean();
		}
	}
	
	private void Set_v_TargetLastModel_Fields(int start, int count) {
		Set_v_TargetSupplierModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetLastModel)m[i+start];
			v.isSelectable = br.ReadBoolean();
		}
	}
	
	private void Set_v_TargetCloseModel_Fields(int start, int count) {
		Set_v_TargetSupplierModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetCloseModel)m[i+start];
			v.isSelectable = br.ReadBoolean();
		}
	}
	
	private void Set_v_TargetStrongModel_Fields(int start, int count) {
		Set_v_TargetSupplierModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetStrongModel)m[i+start];
			v.isSelectable = br.ReadBoolean();
		}
	}
	
	private void Set_v_ConditionalModel_Fields(int start, int count) {
		Set_v_Model_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Mutators.Conditions.ConditionalModel)m[i+start];
		}
	}
	
	private void Set_v_CheckTempleUnderLevelModel_Fields(int start, int count) {
		Set_v_ConditionalModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Mutators.Conditions.Behaviors.CheckTempleUnderLevelModel)m[i+start];
			v.cost = br.ReadInt32();
			v.towerSet = br.ReadBoolean() ? null : br.ReadString();
			v.templeType = br.ReadBoolean() ? null : br.ReadString();
		}
	}
	
	private void Set_v_ReloadTimeTowerMutatorModel_Fields(int start, int count) {
		Set_v_TowerMutatorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Mutators.ReloadTimeTowerMutatorModel>();
		var lifespanField = t.GetField("lifespan", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Mutators.ReloadTimeTowerMutatorModel)m[i+start];
			v.multiplier = br.ReadSingle();
			lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
		}
	}
	
	private void Set_v_SingleEmmisionTowardsTargetModel_Fields(int start, int count) {
		Set_v_EmissionModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Emissions.SingleEmmisionTowardsTargetModel)m[i+start];
			v.offset = br.ReadSingle();
		}
	}
	
	private void Set_v_RetargetOnContactModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Projectiles.Behaviors.RetargetOnContactModel>();
		var delayField = t.GetField("delay", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.RetargetOnContactModel)m[i+start];
			v.distance = br.ReadSingle();
			v.maxBounces = br.ReadInt32();
			delayField.SetValue(v,br.ReadSingle().ToIl2Cpp());
			v.targetType.id = br.ReadString();
			v.targetType.actionOnCreate = br.ReadBoolean();
			v.expireIfNoTargetFound = br.ReadBoolean();
		}
	}
	
	private void Set_v_FollowPathModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Projectiles.Behaviors.FollowPathModel>();
		var speedField = t.GetField("speed", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.FollowPathModel)m[i+start];
			v.path = (Il2CppStructArray<Assets.Scripts.Simulation.SMath.Vector3>) m[br.ReadInt32()];
			v.easePath = (Il2CppStructArray<Assets.Scripts.Simulation.SMath.Vector3>) m[br.ReadInt32()];
			v.destroyAtEndOfPath = br.ReadBoolean();
			speedField.SetValue(v,br.ReadSingle().ToIl2Cpp());
		}
	}
	
	private void Set_v_UseParentEjectModel_Fields(int start, int count) {
		Set_v_WeaponBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.UseParentEjectModel)m[i+start];
		}
	}
	
	private void Set_v_ProjectileSizeTowerMutatorModel_Fields(int start, int count) {
		Set_v_TowerMutatorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Mutators.ProjectileSizeTowerMutatorModel>();
		var lifespanField = t.GetField("lifespan", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Mutators.ProjectileSizeTowerMutatorModel)m[i+start];
			v.sizeModifier = br.ReadSingle();
			v.assetSizeModifier = br.ReadSingle();
			lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
		}
	}
	
	private void Set_v_ProjectileSpeedTowerMutatorModel_Fields(int start, int count) {
		Set_v_TowerMutatorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Mutators.ProjectileSpeedTowerMutatorModel>();
		var lifespanField = t.GetField("lifespan", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Mutators.ProjectileSpeedTowerMutatorModel)m[i+start];
			v.speedModifier = br.ReadSingle();
			lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
		}
	}
	
	private void Set_v_FilterAllExceptTargetModel_Fields(int start, int count) {
		Set_v_FilterModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Filters.FilterAllExceptTargetModel)m[i+start];
		}
	}
	
	private void Set_v_TrackTargetModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Projectiles.Behaviors.TrackTargetModel>();
		var turnRateField = t.GetField("turnRate", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.TrackTargetModel)m[i+start];
			v.distance = br.ReadSingle();
			v.trackNewTargets = br.ReadBoolean();
			v.constantlyAquireNewTarget = br.ReadBoolean();
			v.maxSeekAngle = br.ReadSingle();
			v.ignoreSeekAngle = br.ReadBoolean();
			v.overrideRotation = br.ReadBoolean();
			v.useLifetimeAsDistance = br.ReadBoolean();
			turnRateField.SetValue(v,br.ReadSingle().ToIl2Cpp());
		}
	}
	
	private void Set_v_CreateProjectileOnContactModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateProjectileOnContactModel)m[i+start];
			v.projectile = (Assets.Scripts.Models.Towers.Projectiles.ProjectileModel) m[br.ReadInt32()];
			v.emission = (Assets.Scripts.Models.Towers.Behaviors.Emissions.EmissionModel) m[br.ReadInt32()];
			v.passOnCollidedWith = br.ReadBoolean();
			v.dontCreateAtBloon = br.ReadBoolean();
			v.passOnDirectionToContact = br.ReadBoolean();
		}
	}
	
	private void Set_v_CreateEffectOnContactModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateEffectOnContactModel)m[i+start];
			v.effectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_FilterWithTagModel_Fields(int start, int count) {
		Set_v_FilterModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Filters.FilterWithTagModel)m[i+start];
			v.moabTag = br.ReadBoolean();
			v.camoTag = br.ReadBoolean();
			v.growTag = br.ReadBoolean();
			v.fortifiedTag = br.ReadBoolean();
			v.tag = br.ReadBoolean() ? null : br.ReadString();
			v.inclusive = br.ReadBoolean();
			v.hasMoabTag = br.ReadBoolean();
		}
	}
	
	private void Set_v_CreateTowerModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateTowerModel)m[i+start];
			v.tower = (Assets.Scripts.Models.Towers.TowerModel) m[br.ReadInt32()];
			v.height = br.ReadSingle();
			v.positionAtTarget = br.ReadBoolean();
			v.destroySubTowersOnCreateNewTower = br.ReadBoolean();
			v.useProjectileRotation = br.ReadBoolean();
			v.useParentTargetPriority = br.ReadBoolean();
			v.carryMutatorsFromDestroyedTower = br.ReadBoolean();
		}
	}
	
	private void Set_v_SavedSubTowerModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.SavedSubTowerModel)m[i+start];
		}
	}
	
	private void Set_v_TowerExpireOnParentDestroyedModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.TowerExpireOnParentDestroyedModel)m[i+start];
		}
	}
	
	private void Set_v_FireFromAirUnitModel_Fields(int start, int count) {
		Set_v_WeaponBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.FireFromAirUnitModel)m[i+start];
		}
	}
	
	private void Set_v_AlternateProjectileModel_Fields(int start, int count) {
		Set_v_WeaponBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.AlternateProjectileModel)m[i+start];
			v.projectile = (Assets.Scripts.Models.Towers.Projectiles.ProjectileModel) m[br.ReadInt32()];
			v.emissionModel = (Assets.Scripts.Models.Towers.Behaviors.Emissions.EmissionModel) m[br.ReadInt32()];
			v.interval = br.ReadInt32();
			v.alternateAnimation = br.ReadInt32();
		}
	}
	
	private void Set_v_CirclePatternModel_Fields(int start, int count) {
		Set_v_TargetSupplierModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.CirclePatternModel)m[i+start];
			v.radius = br.ReadSingle();
			v.isSelectable = br.ReadBoolean();
			v.reverse = br.ReadBoolean();
			v.display = ModContent.CreatePrefabReference(br.ReadString());
			v.displayCount = br.ReadInt32();
		}
	}
	
	private void Set_v_TargetFirstAirUnitModel_Fields(int start, int count) {
		Set_v_TargetSupplierModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetFirstAirUnitModel)m[i+start];
			v.isSelectable = br.ReadBoolean();
		}
	}
	
	private void Set_v_AirUnitModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.AirUnitModel)m[i+start];
			v.behaviors = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.TowerBehaviorModel>) m[br.ReadInt32()];
			v.display = ModContent.CreatePrefabReference(br.ReadString());
		}
	}
	
	private void Set_v_PathMovementModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.PathMovementModel)m[i+start];
			v.speed = br.ReadSingle();
			v.rotation = br.ReadSingle();
			v.bankRotation = br.ReadSingle();
			v.bankRotationMul = br.ReadSingle();
			v.ignoreTargetType = br.ReadBoolean();
			v.catchUpSpeed = br.ReadSingle();
			v.takeOffTime = br.ReadSingle();
			v.takeOffExponent = br.ReadSingle();
			v.takeOffAnimTime = br.ReadSingle();
			v.takeOffScale = br.ReadSingle();
			v.takeOffScaleExponent = br.ReadSingle();
			v.takeOffPitch = br.ReadSingle();
			v.takeOffPitchExponent = br.ReadSingle();
			v.fixedPathSupplierId = br.ReadBoolean() ? null : br.ReadString();
		}
	}
	
	private void Set_v_SubTowerFilterModel_Fields(int start, int count) {
		Set_v_WeaponBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.SubTowerFilterModel)m[i+start];
			v.baseSubTowerId = br.ReadBoolean() ? null : br.ReadString();
			v.baseSubTowerIds = (Il2CppStringArray) m[br.ReadInt32()];
			v.maxNumberOfSubTowers = br.ReadSingle();
			v.checkForPreExisting = br.ReadBoolean();
		}
	}
	
	private void Set_v_WindChanceTowerMutatorModel_Fields(int start, int count) {
		Set_v_TowerMutatorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Mutators.WindChanceTowerMutatorModel>();
		var lifespanField = t.GetField("lifespan", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Mutators.WindChanceTowerMutatorModel)m[i+start];
			v.windChance = br.ReadSingle();
			lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
		}
	}
	
	private void Set_v_FilterOutTagModel_Fields(int start, int count) {
		Set_v_FilterModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Filters.FilterOutTagModel)m[i+start];
			v.tag = br.ReadBoolean() ? null : br.ReadString();
			v.disableWhenSupportMutatorIDs = (Il2CppStringArray) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_RemoveMutatorsFromBloonModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.RemoveMutatorsFromBloonModel)m[i+start];
			v.key = br.ReadBoolean() ? null : br.ReadString();
			v.keys = (Il2CppStringArray) m[br.ReadInt32()];
			v.mutatorIds = br.ReadBoolean() ? null : br.ReadString();
			v.mutatorIdList = (Il2CppStringArray) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_FilterAllModel_Fields(int start, int count) {
		Set_v_FilterModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Filters.FilterAllModel)m[i+start];
		}
	}
	
	private void Set_v_TowerExpireModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Behaviors.TowerExpireModel>();
		var lifespanField = t.GetField("lifespan", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.TowerExpireModel)m[i+start];
			v.expireOnRoundComplete = br.ReadBoolean();
			v.expireOnDefeatScreen = br.ReadBoolean();
			lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
			v.rounds = br.ReadInt32();
		}
	}
	
	private void Set_v_Assets_Scripts_Models_Towers_Behaviors_CreateEffectOnExpireModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnExpireModel)m[i+start];
			v.effectModel = (Assets.Scripts.Models.Effects.EffectModel) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_CreditPopsToParentTowerModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.CreditPopsToParentTowerModel)m[i+start];
		}
	}
	
	private void Set_v_InstantModel_Fields(int start, int count) {
		Set_v_ProjectileBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Projectiles.Behaviors.InstantModel)m[i+start];
			v.destroyIfInvalid = br.ReadBoolean();
		}
	}
	
	private void Set_v_WeaponRateMinModel_Fields(int start, int count) {
		Set_v_WeaponBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Weapons.Behaviors.WeaponRateMinModel)m[i+start];
			v.min = br.ReadSingle();
		}
	}
	
	private void Set_v_RandomPositionModel_Fields(int start, int count) {
		Set_v_TargetSupplierModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.RandomPositionModel)m[i+start];
			v.minDistance = br.ReadSingle();
			v.maxDistance = br.ReadSingle();
			v.targetRadius = br.ReadSingle();
			v.targetRadiusSquared = br.ReadSingle();
			v.isSelectable = br.ReadBoolean();
			v.pointDistance = br.ReadSingle();
			v.dontUseTowerPosition = br.ReadBoolean();
			v.areaType = br.ReadBoolean() ? null : br.ReadString();
			v.useInverted = br.ReadBoolean();
			v.ignoreTerrain = br.ReadBoolean();
			v.idealDistanceWithinTrack = br.ReadSingle();
		}
	}
	
	private void Set_v_RangeTowerMutatorModel_Fields(int start, int count) {
		Set_v_TowerMutatorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Mutators.RangeTowerMutatorModel>();
		var lifespanField = t.GetField("lifespan", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Mutators.RangeTowerMutatorModel)m[i+start];
			v.rangeIncrease = br.ReadSingle();
			lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
		}
	}
	
	private void Set_v_AddBehaviorToTowerMutatorModel_Fields(int start, int count) {
		Set_v_TowerMutatorModel_Fields(start, count);
		var t = Il2CppType.Of<Assets.Scripts.Models.Towers.Mutators.AddBehaviorToTowerMutatorModel>();
		var lifespanField = t.GetField("lifespan", bindFlags);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Mutators.AddBehaviorToTowerMutatorModel)m[i+start];
			lifespanField.SetValue(v,br.ReadSingle().ToIl2Cpp());
			v.behaviors = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.TowerBehaviorModel>) m[br.ReadInt32()];
		}
	}
	
	private void Set_v_TowerBehaviorBuffModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.TowerBehaviorBuffModel)m[i+start];
			v.buffLocsName = br.ReadBoolean() ? null : br.ReadString();
			v.buffIconName = br.ReadBoolean() ? null : br.ReadString();
			v.maxStackSize = br.ReadInt32();
			v.isGlobalRange = br.ReadBoolean();
		}
	}
	
	private void Set_v_DiscountZoneModel_Fields(int start, int count) {
		Set_v_TowerBehaviorBuffModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.DiscountZoneModel)m[i+start];
			v.discountMultiplier = br.ReadSingle();
			v.stackLimit = br.ReadInt32();
			v.stackName = br.ReadBoolean() ? null : br.ReadString();
			v.groupName = br.ReadBoolean() ? null : br.ReadString();
			v.affectSelf = br.ReadBoolean();
			v.tierCap = br.ReadInt32();
		}
	}
	
	private void Set_v_BuffIndicatorModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.GenericBehaviors.BuffIndicatorModel)m[i+start];
			v.buffName = br.ReadBoolean() ? null : br.ReadString();
			v.iconName = br.ReadBoolean() ? null : br.ReadString();
			v.stackable = br.ReadBoolean();
			v.maxStackSize = br.ReadInt32();
			v.globalRange = br.ReadBoolean();
			v.onlyShowBuffIfMutated = br.ReadBoolean();
		}
	}
	
	private void Set_v_SupportModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.SupportModel)m[i+start];
			v.filters = (Il2CppReferenceArray<Assets.Scripts.Models.Towers.TowerFilters.TowerFilterModel>) m[br.ReadInt32()];
			v.isGlobal = br.ReadBoolean();
			v.isCustomRadius = br.ReadBoolean();
			v.customRadius = br.ReadSingle();
			v.appliesToOwningTower = br.ReadBoolean();
			v.showBuffIcon = br.ReadBoolean();
			v.buffLocsName = br.ReadBoolean() ? null : br.ReadString();
			v.buffIconName = br.ReadBoolean() ? null : br.ReadString();
			v.maxStackSize = br.ReadInt32();
			v.onlyShowBuffIfMutated = br.ReadBoolean();
		}
	}
	
	private void Set_v_RateSupportModel_Fields(int start, int count) {
		Set_v_SupportModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.RateSupportModel)m[i+start];
			v.multiplier = br.ReadSingle();
			v.isUnique = br.ReadBoolean();
			v.mutatorId = br.ReadBoolean() ? null : br.ReadString();
			v.priority = br.ReadInt32();
		}
	}
	
	private void Set_v_PerRoundCashBonusTowerModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.PerRoundCashBonusTowerModel)m[i+start];
			v.cashPerRound = br.ReadSingle();
			v.cashRoundBonusMultiplier = br.ReadSingle();
			v.lifespan = br.ReadSingle();
			v.assetId = ModContent.CreatePrefabReference(br.ReadString());
			v.distributeCash = br.ReadBoolean();
		}
	}
	
	private void Set_v_BonusCashZoneModel_Fields(int start, int count) {
		Set_v_TowerBehaviorModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.BonusCashZoneModel)m[i+start];
			v.multiplier = br.ReadSingle();
			v.stackName = br.ReadBoolean() ? null : br.ReadString();
			v.groupName = br.ReadBoolean() ? null : br.ReadString();
			v.stackLimit = br.ReadInt32();
		}
	}
	
	private void Set_v_PierceSupportModel_Fields(int start, int count) {
		Set_v_SupportModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.PierceSupportModel)m[i+start];
			v.pierce = br.ReadSingle();
			v.mutatorId = br.ReadBoolean() ? null : br.ReadString();
			v.isUnique = br.ReadBoolean();
		}
	}
	
	private void Set_v_RangeSupportModel_Fields(int start, int count) {
		Set_v_SupportModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.RangeSupportModel)m[i+start];
			v.multiplier = br.ReadSingle();
			v.additive = br.ReadSingle();
			v.mutatorId = br.ReadBoolean() ? null : br.ReadString();
			v.isUnique = br.ReadBoolean();
		}
	}
	
	private void Set_v_DamageSupportModel_Fields(int start, int count) {
		Set_v_SupportModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.DamageSupportModel)m[i+start];
			v.increase = br.ReadSingle();
			v.mutatorId = br.ReadBoolean() ? null : br.ReadString();
			v.isUnique = br.ReadBoolean();
		}
	}
	
	private void Set_v_TempleTowerMutatorGroupTierTwoModel_Fields(int start, int count) {
		Set_v_TempleTowerMutatorGroupModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.TempleTowerMutatorGroupTierTwoModel)m[i+start];
		}
	}
	
	private void Set_v_FigureEightPatternModel_Fields(int start, int count) {
		Set_v_TargetSupplierModel_Fields(start, count);
		for (var i=0; i<count; i++) {
			var v = (Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.FigureEightPatternModel)m[i+start];
			v.radius = br.ReadSingle();
			v.isSelectable = br.ReadBoolean();
			v.rotated = br.ReadBoolean();
			v.display = ModContent.CreatePrefabReference(br.ReadString());
			v.displayCount = br.ReadInt32();
			v.useTowerPosition = br.ReadBoolean();
		}
	}
	
	#endregion
	
	protected override Assets.Scripts.Models.Towers.TowerModel Load(byte[] bytes) {
		using (var s = new MemoryStream(bytes)) {
			using (var reader = new BinaryReader(s)) {
				this.br = reader;
				var totalCount = br.ReadInt32();
				m = new object[totalCount];
				
				//##  Step 1: create empty collections
				CreateArraySet<Assets.Scripts.Models.Model>();
				Read_a_Int32_Array();
				Read_a_AreaType_Array();
				CreateArraySet<Assets.Scripts.Models.Towers.Mods.ApplyModModel>();
				CreateArraySet<Assets.Scripts.Models.Towers.Weapons.WeaponModel>();
				CreateArraySet<Assets.Scripts.Models.Towers.Filters.FilterModel>();
				Read_a_String_Array();
				CreateArraySet<Assets.Scripts.Models.Towers.Weapons.WeaponBehaviorModel>();
				CreateArraySet<Assets.Scripts.Models.Towers.Upgrades.UpgradePathModel>();
				Read_a_TargetType_Array();
				CreateArraySet<Assets.Scripts.Models.Towers.Weapons.Behaviors.ThrowMarkerOffsetModel>();
				CreateArraySet<Assets.Scripts.Models.Towers.Behaviors.Attack.AttackModel>();
				CreateArraySet<Assets.Scripts.Models.Towers.Mutators.TowerMutatorModel>();
				Read_a_Vector3_Array();
				CreateArraySet<Assets.Scripts.Models.Towers.TowerBehaviorModel>();
				CreateListSet<Assets.Scripts.Models.Model>();
				
				//##  Step 2: create empty objects
				Create_Records<Assets.Scripts.Models.Towers.TowerModel>();
				Create_Records<Assets.Scripts.Models.Towers.Mods.ApplyModModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.TowerRadiusModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateSoundOnSellModel>();
				Create_Records<Assets.Scripts.Models.Audio.SoundModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateSoundOnUpgradeModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateSoundOnAttachedModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnSellModel>();
				Create_Records<Assets.Scripts.Models.Effects.EffectModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnPlaceModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.OverrideCamoDetectionModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateSoundOnTowerPlaceModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnUpgradeModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.AttackModel>();
				Create_Records<Assets.Scripts.Models.Towers.Weapons.WeaponModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Emissions.RandomArcEmissionModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.ProjectileModel>();
				Create_Records<Assets.Scripts.Models.Towers.Filters.FilterInvisibleModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.TravelStraitModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.DamageModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.ProjectileFilterModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.KnockbackModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.ShowTextOnHitModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.DamageModifierForTagModel>();
				Create_Records<Assets.Scripts.Models.GenericBehaviors.DisplayModel>();
				Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.UseAttackRotationModel>();
				Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.CritMultiplierModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.RotateToMiddleOfTargetsModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.RotateToTargetModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.AttackFilterModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetRightHandModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetFirstPrioCamoModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetLastPrioCamoModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetClosePrioCamoModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetStrongPrioCamoModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetLeftHandModel>();
				Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.EjectEffectModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Abilities.AbilityModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.DarkshiftModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.CircleFootprintModel>();
				Create_Records<Assets.Scripts.Models.Towers.Upgrades.UpgradePathModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Emissions.EmissionWithOffsetsModel>();
				Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.ThrowMarkerOffsetModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.ImmunityModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.CreateEffectOnAbilityModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.ActivateAttackModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Emissions.SingleEmissionModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.AgeModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.DistributeToChildrenBloonModifierModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors.CreateSoundOnAbilityModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.RotateToDefaultPositionTowerModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.RectangleFootprintModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.WindModel>();
				Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.CheckTempleCanFireModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.MonkeyTempleModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.TempleTowerMutatorGroupTierOneModel>();
				Create_Records<Assets.Scripts.Models.Towers.Mutators.PierceTowerMutatorModel>();
				Create_Records<Assets.Scripts.Models.Towers.Mutators.DamageTowerMutatorModel>();
				Create_Records<Assets.Scripts.Models.Towers.Mutators.AddAttackTowerMutatorModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Emissions.ArcEmissionModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.RotateModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.UseTowerRangeModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetFirstModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetLastModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetCloseModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetStrongModel>();
				Create_Records<Assets.Scripts.Models.Towers.Mutators.Conditions.Behaviors.CheckTempleUnderLevelModel>();
				Create_Records<Assets.Scripts.Models.Towers.Mutators.ReloadTimeTowerMutatorModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Emissions.SingleEmmisionTowardsTargetModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.RetargetOnContactModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.FollowPathModel>();
				Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.UseParentEjectModel>();
				Create_Records<Assets.Scripts.Models.Towers.Mutators.ProjectileSizeTowerMutatorModel>();
				Create_Records<Assets.Scripts.Models.Towers.Mutators.ProjectileSpeedTowerMutatorModel>();
				Create_Records<Assets.Scripts.Models.Towers.Filters.FilterAllExceptTargetModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.TrackTargetModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateProjectileOnContactModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateEffectOnContactModel>();
				Create_Records<Assets.Scripts.Models.Towers.Filters.FilterWithTagModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.CreateTowerModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.SavedSubTowerModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.TowerExpireOnParentDestroyedModel>();
				Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.FireFromAirUnitModel>();
				Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.AlternateProjectileModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.CirclePatternModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.TargetFirstAirUnitModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.AirUnitModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.PathMovementModel>();
				Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.SubTowerFilterModel>();
				Create_Records<Assets.Scripts.Models.Towers.Mutators.WindChanceTowerMutatorModel>();
				Create_Records<Assets.Scripts.Models.Towers.Filters.FilterOutTagModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.RemoveMutatorsFromBloonModel>();
				Create_Records<Assets.Scripts.Models.Towers.Filters.FilterAllModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.TowerExpireModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreateEffectOnExpireModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.CreditPopsToParentTowerModel>();
				Create_Records<Assets.Scripts.Models.Towers.Projectiles.Behaviors.InstantModel>();
				Create_Records<Assets.Scripts.Models.Towers.Weapons.Behaviors.WeaponRateMinModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.RandomPositionModel>();
				Create_Records<Assets.Scripts.Models.Towers.Mutators.RangeTowerMutatorModel>();
				Create_Records<Assets.Scripts.Models.Towers.Mutators.AddBehaviorToTowerMutatorModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.DiscountZoneModel>();
				Create_Records<Assets.Scripts.Models.GenericBehaviors.BuffIndicatorModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.RateSupportModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.PerRoundCashBonusTowerModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.BonusCashZoneModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.PierceSupportModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.RangeSupportModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.DamageSupportModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.TempleTowerMutatorGroupTierTwoModel>();
				Create_Records<Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors.FigureEightPatternModel>();
				
				Set_v_TowerModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_ApplyModModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_TowerRadiusModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_CreateSoundOnSellModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_SoundModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_CreateSoundOnUpgradeModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_CreateSoundOnAttachedModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_CreateEffectOnSellModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_EffectModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_CreateEffectOnPlaceModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_OverrideCamoDetectionModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_CreateSoundOnTowerPlaceModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_CreateEffectOnUpgradeModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_AttackModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_WeaponModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_RandomArcEmissionModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_ProjectileModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_FilterInvisibleModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_TravelStraitModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_DamageModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_ProjectileFilterModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_KnockbackModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_ShowTextOnHitModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_DamageModifierForTagModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_DisplayModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_UseAttackRotationModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_CritMultiplierModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_RotateToMiddleOfTargetsModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_RotateToTargetModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_AttackFilterModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_TargetRightHandModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_TargetFirstPrioCamoModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_TargetLastPrioCamoModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_TargetClosePrioCamoModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_TargetStrongPrioCamoModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_TargetLeftHandModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_EjectEffectModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_AbilityModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_DarkshiftModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_CircleFootprintModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_UpgradePathModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_EmissionWithOffsetsModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_ThrowMarkerOffsetModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_ImmunityModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_CreateEffectOnAbilityModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_ActivateAttackModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_SingleEmissionModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_AgeModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_DistributeToChildrenBloonModifierModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_CreateSoundOnAbilityModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_RotateToDefaultPositionTowerModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_RectangleFootprintModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_WindModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_CheckTempleCanFireModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_MonkeyTempleModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_TempleTowerMutatorGroupTierOneModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_PierceTowerMutatorModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_DamageTowerMutatorModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_AddAttackTowerMutatorModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_ArcEmissionModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_RotateModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_UseTowerRangeModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_TargetFirstModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_TargetLastModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_TargetCloseModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_TargetStrongModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_CheckTempleUnderLevelModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_ReloadTimeTowerMutatorModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_SingleEmmisionTowardsTargetModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_RetargetOnContactModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_FollowPathModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_UseParentEjectModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_ProjectileSizeTowerMutatorModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_ProjectileSpeedTowerMutatorModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_FilterAllExceptTargetModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_TrackTargetModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_CreateProjectileOnContactModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_CreateEffectOnContactModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_FilterWithTagModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_CreateTowerModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_SavedSubTowerModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_TowerExpireOnParentDestroyedModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_FireFromAirUnitModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_AlternateProjectileModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_CirclePatternModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_TargetFirstAirUnitModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_AirUnitModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_PathMovementModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_SubTowerFilterModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_WindChanceTowerMutatorModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_FilterOutTagModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_RemoveMutatorsFromBloonModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_FilterAllModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_TowerExpireModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_Assets_Scripts_Models_Towers_Behaviors_CreateEffectOnExpireModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_CreditPopsToParentTowerModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_InstantModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_WeaponRateMinModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_RandomPositionModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_RangeTowerMutatorModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_AddBehaviorToTowerMutatorModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_DiscountZoneModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_BuffIndicatorModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_RateSupportModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_PerRoundCashBonusTowerModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_BonusCashZoneModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_PierceSupportModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_RangeSupportModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_DamageSupportModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_TempleTowerMutatorGroupTierTwoModel_Fields(br.ReadInt32(), br.ReadInt32());
				Set_v_FigureEightPatternModel_Fields(br.ReadInt32(), br.ReadInt32());
				
				//##  Step 4: link object collections e.g Product[]. Note: requires object data e.g dictionary<string, value> where string = model.name
				LinkArray<Assets.Scripts.Models.Model>();
				LinkArray<Assets.Scripts.Models.Towers.Mods.ApplyModModel>();
				LinkArray<Assets.Scripts.Models.Towers.Weapons.WeaponModel>();
				LinkArray<Assets.Scripts.Models.Towers.Filters.FilterModel>();
				LinkArray<Assets.Scripts.Models.Towers.Weapons.WeaponBehaviorModel>();
				LinkArray<Assets.Scripts.Models.Towers.Upgrades.UpgradePathModel>();
				LinkArray<Assets.Scripts.Models.Towers.Weapons.Behaviors.ThrowMarkerOffsetModel>();
				LinkArray<Assets.Scripts.Models.Towers.Behaviors.Attack.AttackModel>();
				LinkArray<Assets.Scripts.Models.Towers.Mutators.TowerMutatorModel>();
				LinkArray<Assets.Scripts.Models.Towers.TowerBehaviorModel>();
				LinkList<Assets.Scripts.Models.Model>();
				
				var resIndex = br.ReadInt32();
				UnityEngine.Debug.Assert(br.BaseStream.Position == br.BaseStream.Length);
				return (Assets.Scripts.Models.Towers.TowerModel) m[resIndex];
			}
		}
	}
}
