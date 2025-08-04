# Lucky Blocks

<img width="720" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/47c57c25-c8c3-44e4-9482-13684ab3f23e" alt="Lucky blocks" />

*They may be unlucky for some players.*  
© *Old superfigher saying*

## Description

This script adds many insteresting ideas to vanilla SFD.  
When a loot crate spawns on the map, there is `30%` (by default) chance for it to become a lucky block.

There are different drop types you may receive:  

1. [Player Buffs](#player-buffs) `(make us harder, better, faster, stronger)`
2. [Weapons](#weapons) `(as a drop or an improvement)`
3. [Magic](#magic) `(wind, fire, and more...)`
4. [Items](#items) `(have you ever used expired pills?)`
5. [World Events](#world-events) `(the big brother is watching you)`

**About compability with other scripts**  
Lucky Blocks is a global standalone mod that changes SFD gameplay in many aspects. Only following quality of life scripts are well-tested and work fine with this script: [Loot Displayer](https://steamcommunity.com/sharedfiles/filedetails/?id=2545232247), [No Drones](https://steamcommunity.com/sharedfiles/filedetails/?id=2250869022). Any other script or map with complicated logic may cause bugs and will not be fixed on our side.  

Every loot has `weight` attribute, equals to `1` by default.  
The higher value corresponds to higher chance to appear.  
P.S. [Bloody bath](#bloody-bath) and both of [Remove all weapons](#remove-weapons) has lower weight due to balance issues.  

## Player Buffs

Player buffs somehow modify player. Most of them are obtained from lucky block (check source in description) when player breaks it by melee attack. There are buffs that are obtained when certain condition are met.  

There are next player buff types: [instant](#instant-buffs), [durable](#durable-buffs), [situational](#situational-buffs).  
 
* Instant buffs are applied once when added.  
* Durable buffs are working during some time.  
* Situational buffs... Just read their descriptions.  

Incompatible buffs are impossible to get together in the same time.

### Immunity

Some buffs may give an immunity to something (like fire, fall damage, something script-specific or etc). It ends with the buff itself (sometimes with delay).

Every buff may interact with others, like if you has wet-hands, becoming fire wizard will dry them.
  
## Instant Buffs

### Full HP

Fully recovers player's HP.

**Source**: `FullHp` loot  
**If**: player's HP aren't full  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/13312994/ac7d1fec-0224-451d-9dae-01b011d06b50" alt="HP recovery" />

### Ignite

Sets the player on fire.

**Source**: [Ignite random player](#ignite-random-player) loot or [Fire Magic](#fire-wizard)  
**Will be repressed if**: player has an immunity to fire  

> USAGE DEMO

### Poison

Poison player for some HP amount.

**Source**: [Medkit](#medkit) loot when it's poisoned or on picking up any meds when you're a [Vampire](#vampirism)  
**Will be repressed if**: player has an immunity to poison  

> USAGE DEMO

### Disarm

Throws the active player's weapon.

**Source**: [Wind Magic](#wind-wizard)  
**Will be repressed if**: player has an immunity to wind  

> USAGE DEMO

## Durable Buffs

### Freeze

Freezes the player, preventing him from moving or using weapons.  

**Duration**: 5s  
**Source**: `Freeze` loot or [Freeze projectile](#freeze-bullets)  
**Will be repressed if**: player has an immunity to freeze  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/50cfe493-622c-4415-901b-9ae8dbb57730" alt="Freezed player" />

<p>

> [!TIP]
> You can get warm!
> 
> <img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/71abe2ee-720e-4e85-b6be-ce94eb5a7c87" alt="Demo of unfreezing" />

</p>

### High Jumps

Increases the player's jump height.  

**Duration**: 10s  
**Source**: `HighJumps` loot  
**Grants immunity**: to fall damage  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/72e88a45-b1fc-4a44-b099-ad4d09744f8f" alt="High jumps" />

### Shield

Gives immunity to all types of damage and some magic.  

**Duration**: 7s  
**Source**: `Shield` loot  
**Incompatibility**: with [Hulk](#hulk)  
**Grants immunity**: to poison, fall damage, fire, freeze, wind, shock  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/2fa5bc38-c819-472e-b601-ddd177fb89ca" alt="Demo of buff usage (against weapon)" />
<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/334e4ea5-8db4-4267-8b73-8c0ee7d3e16b" alt="Demo of buff usage (against magic)" />

<p>
  
> [!NOTE]
> The shield doesn't protect against the destruction of the player's body (don't jump into three hundred ton hydraulic press)

</p>

### Vampirism

The vampire recovers his health equal to the damage dealt to other players.  

**Duration**: 15s  
**Source**: `Vampirism` loot  
**Incompatibility**: with [Hulk](#hulk) and [Dwarf](#dwarf)  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/75ad22ce-3714-4d7b-81b6-78e087f1f2ec" alt="Demo of buff usage" />

<p>

> [!WARNING]
> Medicine poisons you with damage equals to heal! `(25 HP for pills and 50 HP for medkit)`
>
> <img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/8d8f3d7f-6fe1-4932-9e0d-e4b3b1a8b5ae" alt="Demo of medical negligence" />

</p>

### Strong Man

Makes the player insanely strong:  
* Kill enemies in one hit 
* Kill enemies in one bullet
* Cast magic to the other side of map

*Map props are terrified by his kicks*  

**Duration**: 5s  
**Source**: `StrongMan` loot  
**Incompatibility**: with [Hulk](#hulk) and [Dwarf](#dwarf)  
**Grant immunity**: to wind  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/89812c1e-0152-4bed-bfc0-cde74af427f2" alt="Demo of buff usage (strength)" />
<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/42417982-cee0-421d-9f72-f645bca210f4" alt="Demo of buff usage (immunity to wind)" />
<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/46f96d86-6b79-487e-a784-dfee3097fae8" alt="Demo of buff usage (magic range and propagation speed)" />

### Hulk

Makes the player strong, big and green (as Hulk).

* Melee damage increased threefold
* Feel less pain
* Move slowly  

**Duration**: 10s  
**Source**: `Hulk` loot  
**Incompatibility**: with [Dwarf](#dwarf), [Vampirism](#vampirism), [Strong Man](#strong-man) and [Shield](#shield)  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/cf19bf85-8715-4190-af83-0acda322e509" alt="Demo of buff usage" />

### Dwarf

Makes the player small and fast.  
Melee damage reduced.  

**Duration**: 10s  
**Source**: `Dwarf` loot  
**Incompatibility**: with [Hulk](#hulk), [Vampirism](#vampirism) and [Strong Man](#strong-man)  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/a8741de3-4b76-47e2-876f-b29a1655b19b" alt="Demo of buff usage" />

### Wet Hands

There's a `25%` chance that you will drop active weapon on actions with it.  
`(e.g. Draw, Shoot (firearms), Melee hit (melees), Activate (grenades))`  

**Duration**: 20s  
**Source**: `WetHands` loot  
**Will be repressed if**: player has an immunity to water  

> USAGE DEMO

### Durable poison

Deals `3` damage/s every second.  

**Duration**: 10s  
**Source**: [Poisoned projectile](#poison-bullets)  
**Will be repressed if**: player has an immunity to poison  

> USAGE DEMO

### Shock

Paralyzes player and deals `3` damage every `0.3` seconds.  

**Duration**: 0.05s - 5s (depends on shock charge)  
**Source**: [Electric Magic](#electric-wizard)  
**Will be repressed if**: player has an immunity to shock  

> USAGE DEMO

## Situational Buffs

### Totem of Undying

Instantly revives the player upon death.  

**Source**: `TotemOfUndying` loot  
**If**: player doesn't have totem  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/70364d8e-758d-4dd9-8ee4-20106145bf9f" alt="Demo of buff usage" />

<p>

> [!WARNING]
> After revival you will lost all your applied buffs!

> [!NOTE]
> The totem will not revive you if the body was destroyed (three hundred ton hydraulic press is still your enemy)  
> 
> <img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/6320470a-153a-4a2c-a77f-7de7eee9ff9e" alt="Demo of note" />

</p>

### The Fool

By pressing `[ALT + D]` you can prohibit magic usage for `15` seconds.  

**Source**: `TheFool` loot  
**If**: no one have this buff and magic is allowed on the moment when lucky block was broken  

> USAGE DEMO

## Weapons

Weapons are obtained after lucky block destruction.  

<p>
  
> [!WARNING]
> Any weapon from lucky block can be booby-trapped with `10%` chance!  
> `(it explodes after shoot (firearm), activate (grenades), draw (melees))`  
>
> <img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/95e2f2a0-8f40-4e3d-9371-60e9756f8185" alt="Demo of booby-trap" />

</p>

### Legendary weapon

Drops random legendary weapon: Bazooka, Grenade launcher, Sniper, M60, Chainsaw, Magnum, Flaregun  

**Source**: `LegendaryWeapon` loot  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/22d44abb-1f19-4008-9f16-ec62647f460e" alt="Demo of weapon" />

### Sticky grenades

Sticky grenades are attached to you or any surface.  

**Source**: `StickyGrenades` loot  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/7deb3b79-075f-4d51-89e6-6acb408340e5" alt="Demo of weapon (attaching to player)" />
<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/561879c7-6cbe-460e-b612-4789d0b7ff51" alt="Demo of weapon (attaching to surface)" />

<p>
  
> [!TIP]
> Sticky grenade has pink "glue" on it

</p>

### Banana grenades

Have you ever played Worms? C'mon, just try it  

**Source**: `BananaGrenades` loot  

> USAGE DEMO

<p>
  
> [!TIP]
> Banana grenade has yellow "banana peel" on it

</p>

### Fun weapon

Drops a random fun weapon from templates:
- Bow with [Push bullets](#push-bullets), [Infinite ricochet bullets](#infinite-ricochet-bullets)    
- Bow with [Infinite ricochet bullets](#infinite-ricochet-bullets)  
- Grenade laucher with [Push bullets](#push-bullets), [Infinite ricochet bullets](#infinite-ricochet-bullets)  
- Grenade launcher with [Infinite ricochet bullets](#infinite-ricochet-bullets)  
- Bazooka with [Infinite ricochet bullets](#infinite-ricochet-bullets)  
- Flaregun with [Push bullets](#push-bullets), [Infinite ricochet bullets](#infinite-ricochet-bullets)  
- Sawed-off with [Infinite ricochet bullets](#infinite-ricochet-bullets), [Lost bullets](#lost-bullets)    

**Source**: `FunWeapon` loot  

> USAGE DEMO

> [!NOTE]
> There's `30%` chance that weapon will also get [Lost bullets](#lost-bullets) powerup.  

### Random weapon with random powerups

Drops a random weapon with random set of [powerups](https://github.com/mysterymarshak/LuckyBlocks?tab=readme-ov-file#weapon-powerups).  
After first powerup on weapon each next is added with `30%` chance.

**Source**: `WeaponWithRandomPowerups` loot  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/1efdc942-a6ba-46cf-9d1c-1b7c7b83f81f" alt="Demo of weapon" />

## Weapon Powerups

Weapon powerups are obtained if player have a firearm.  
Poweruped bullets count depends on weapon, unless otherwise stated.  
*There's a formula: `сlamp(3, Weapon.MagSize, Weapon.MaxTotalAmmo / 2)`* (`clamp(min, value, max)`)  

> [!NOTE]
> One firearm may have many type of powerups, if they're compatible with each other  
> Search compatible weapon order: `In hands` -> `Primary` -> `Secondary`  
>
> <img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/44604b4b-50b8-4d76-8305-a71e3e30cf64" alt="Demo of multiple powerups on the weapon" />

### Explosive bullets

Bullet triggers an explosion on hit.  

**Source**: `ExplosiveBullets` loot  
**Incompatibility**: with [Triple Ricochet Bullets](#triple-ricochet-bullets) and [Infinite Ricochet Bullets](#infinite-ricochet-bullets)  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/23ac71a7-01a7-4e74-ac30-f0e1929a9f24" alt="Demo of powerup" />

### Triple ricochet bullets

When bullet hits a surface, it ricochets, dividing into three bullets.  

**Source**: `TripleRicochetBullets` loot  
**Incompatibility**: with [Explosive Bullets](#explosive-bullets), [Freeze Bullets](#freeze-bullets), [Infinite Ricochet Bullets](#infinite-ricochet-bullets), [Aim Bullets](#aim-bullets), [Push Bullets](#push-bullets)  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/1eb60c5e-e281-4757-a85e-007a6f35f6d6" alt="Demo of powerup" />

### Freeze bullets

[Freezes](#freeze) the player on hit.  

**Source**: `FreezeBullets` loot  
**Incompatibility**: with [Triple Ricochet Bullets](#triple-ricochet-bullets) and [Push Bullets](#push-bullets)  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/8809bc8b-f971-47a5-b8a0-02cd5cc57491" alt="Demo of powerup" />

### Push bullets

Pushes objects and players on it's path.  

**Source**: `PushBullets` loot  
**Incompatibility**: with [Triple Ricochet Bullets](#triple-ricochet-bullets) and [Freeze Bullets](#freeze-bullets)  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/bc976848-08fe-45b5-b188-90dc54f24bad" alt="Demo of powerup" />

### Aim bullets

Tries to follow the nearest player in some radius.  

**Source**: `AimBullets` loot  
**Incompatibility**: with [Triple Ricochet Bullets](#triple-ricochet-bullets)  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/016c0874-96bb-4c2c-8f19-e42e50656656" alt="Demo of powerup" />

### Infinite ricochet bullets

Read the title again please.  

**Source**: `InfiniteRicochetBullets` loot  
**Quantity**: all weapon ammo (for shotgun 0.5 weapon ammo)  
**Incompatibility**: with [Explosive Bullets](#explosive-bullets) and [Triple Ricochet Bullets](#triple-ricochet-bullets)  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/8ff2916f-b218-4ea0-a3ed-f080ef6e7bdd" alt="Demo of powerup" />

### Poison bullets

[Poisons](#durable-poison) player on hit.  

**Source**: `PoisonBullets` loot  
**Incompability**: with [Triple Ricochet Bullets](#triple-ricochet-bullets) and [Push Bullets](#push-bullets)  

> USAGE DEMO

### Lost bullets

Bullets will spawn in random place on the map.  

**Source**: `LostBullets` loot  
**Quantity**: all weapon ammo (for shotgun 0.5 weapon ammo)  

> USAGE DEMO

### Flamy katana

Katana + Fire = Flamy Katana  

**Source**: `FlamyKatana` loot  

> USAGE DEMO

## Magic

You're not issekai hero, only one magic.  
Casts are able to be stacked unless otherwise stated. There are no limits to casts count.  
Usage: hide weapon (except melees) and press `[ALT + A]`.  
*You must finish attack animation with holding [ALT], only then magic will be casted*.  

### Wind wizard

Emits strong currents of wind that pushes everything in its path.  
* [Disarms](#disarm) players.
* Reflects projectiles.  
* Puts out the fire.  

**Source**: `WindWizard` loot  
**Casts count**: 3  
**Grants immunity**: to wind  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/62c01d6f-8346-4ef3-b9e0-2b865d2a8cb6" alt="Demo of magic" />
<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/1984848d-f241-4457-b99e-a0d7677d66d4" alt="Demo of magic immunity" />

### Fire wizard

Spews out a fireball, setting everything on fire in its way.  

**Source**: `FireWizard` loot  
**Casts count**: 3  
**Grants immunity**: to fire, water and [Freeze](#freeze)  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/b728173d-8d66-4eeb-8a5d-8d6e978bcb99" alt="Demo of magic" />

### Electric wizard

Releases an electrical wave, [shocking](#shock) players and electrolyzing objects.  
When object touches another electrified object, it becomes shocked too.  

**Source**: `ElectricWizard` loot  
**Casts count**: 3  
**Grant immunity**: to shock  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/a34d8dff-3a14-4976-be2b-cab946c00156" alt="Demo of magic (agains players)" />
<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/cfde6763-e0ab-43cc-ae1f-e0ecbac602ec" alt="Demo of magic (agains objects)" />

<p>
  
> [!NOTE]
> Electrical wave shocks every object in an area, granting to each one 7s of "shock charge" (3.5s for players)  
> When any object without shock touches the shocked object, charge divides for both equally  
> Think about it like irl charging behaviour. There's also an "elementary charge" equals to 50ms that can't be split further  
> But note that two already shocked objects even if there're colliding won't split their charges  

</p>

### Decoy wizard

Spawns three player illusions.  

**Source**: `DecoyWizard` loot  
**If**: no one else is Decoy Wizard  
**Casts count**: 1  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/d64d834c-5608-4d66-b91b-28adb41692aa" alt="Demo of magic" />

<p>

> [!NOTE]
> Illusions don't deal damage

</p>

### Time stop wizard

Stops the time. Every action performed in stopped scene saves impulse and affects further game.  
Time automatically resumes if wizard dies.  

**Source**: `TimeStopWizard` loot  
**Casts count**: 1  
**Grants immunity**: to time stop  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/da677d26-1661-4767-9642-6274a7653771" alt="Demo of magic" />

<p>

> [!TIP]
> You can resume time for object by kicking it with `[ALT + S]`  
> You can disarm players by attacking them with `[ALT + A]`  
> You can walk on liquids

> [!NOTE]
> Time stop doesn't affect other time stop wizards  
> If someone grab you during first 1.5s, time will resume

</p>

### Restore Wizard

You can store your state to return there later.  

**Source**: `RestoreWizard` loot  
**If**: you isn't Restore Wizard already  
**Casts count**: first - save, second - restore  

> USAGE DEMO

### Steal Wizard

You can steal inventory of chosen player.  

**Source**: `StealWizard` loot  
**If**: you aren't Steal Wizard already  
**Casts count**: 1  
**Grants immunity**: to steal  

> USAGE DEMO

### Time Revert Wizard

Reverts last X seconds back.  

**Source**: `TimeRevertWizard` loot  
**If**: no one is Time Revert Wizard  
**Casts count**: 1  

> USAGE DEMO

## Magic collision

### Fire magic + Electric magic = Explosion

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/22177ff5-5499-4089-ba71-e21c6d48948d" alt="Demo of magic colliding" />

### Wind magic + Fire magic = Fire absorption

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/293e9a55-9404-45e6-99eb-2b0a67ecba3d" alt="Demo of magic colliding" />

## Items

### Medkit

Spawns pills (25 HP) or medkit (50 HP)

**Source**: `Medkit` loot  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/d4a302fd-68ad-4b51-b3e0-03031123b4dc" alt="Demo of medkit" />

<p>

> [!WARNING]
> Can be poisoned with `50%` chance!
>
> <img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/c3a3900a-c305-4032-b541-e73eb7a254b3" alt="Demo of medkit" />

</p>

## World Events

### Respawn random player

Respawns random dead player with half hp.  

**Source**: `RespawnRandomPlayer` loot  
**If**: someone died  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/ce3c8a5e-7307-4a54-81ed-0180d32b7666" alt="Demo of event" />

### Explosion

Creates an explosion within broken lucky block.  

**Source**: `Explosion` loot  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/f414fdec-817f-4303-a4bb-5773cdedb6b5" alt="Demo of event" />

### Explode random barrel

Explodes random barrel on the map.  

**Source**: `ExplodeRandomBarrel` loot  
**If**: explosive barrel exists  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/5bce758f-4b20-4127-ac81-5228150584d2" alt="Demo of event" />

### Increase spawn chance

Increases spawn chance of lucky block `(30% -> 45% -> 60%)`

**Source**: `IncreaseSpawnChance` loot  
**If**: next level of chance exists and you don't use manual one  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/70de0737-faee-4d2c-ba62-db738c3faed2" alt="Demo of event" />

### Shuffle positions

Shuffles player positions.  

**Source**: `ShufflePositions` loot  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/81759f06-34e3-40c8-8e3f-9c14dd16b9d9" alt="Demo of event" />

### Shuffle weapons

Shuffles players weapons.  

**Source**: `ShuffleWeapons` loot  
**If**: there's armed player  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/82627e13-a44f-4ebc-8ce3-d43fcd01c50b" alt="Demo of event" />

### Ignite random player

Ignites random player.  

**Source**: `IgniteRandomPlayer` loot  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/920fe7f3-e9d3-45a5-95dc-7d1afe74de65" alt="Demo of event" />

### Bloody bath

Spawns many explosive barrels in the top of the map.  

**Source**: `BloodyBath` loot  
**Weight**: 0.5  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/9de5df30-c15b-41fc-834c-9dd8144f6bf1" alt="Demo of event" />

<p>

> [!NOTE]
> On maps with roof (like Hazardous) barrels will stuck in it.  

</p>

### Remove weapons

Removes weapons from all players on the map.  

**Source**: `RemoveWeapons` loot  
**If**: there's armed player  
**Weight**: 0.25

> USAGE DEMO

### Remove weapons except activator

Makes activator a weapon monopolist.  

**Source**: `RemoveWeaponsExceptActivator` loot  
**If**: there's armed player  
**Weight**: 0.25

> USAGE DEMO

## F.A.Q.

**Q**: What is F.A.Q.?  
**A**: Use google please.

**Q**: I am playing with gamepad, changed key mapping, how to use `[ALT+A]` etc.  
**A**: Actions are actually bound to events. `[ALT+A]` means you have to be in slow-walking state and pressing attack button.

**Q**: Is there any commands in script?  
**A**: Use `/lb_help`.

**Q**: Cool textures! Are they part of script? How to get them?  
**A**: Not a part of script. Meant for "shiza" internal usage only.

**Q**: Cool project! How can I build it?  
**A**: Great question! You can't! Nobody stops you from finding a way to do it though.

**Q**: Time revert works badly, what's happened to my C4? Detonator doesn't work!  
**A**: Game creators stole it :<

**Q**: Freezed player moves! HOW?!?!?  
**A**: Probably they live on south. *(unreproducible bug)*

**Q**: Does this script work with bots?  
**A**: Maybe. Better avoid them. We assumed you will play with real humans (or maybe even touch the real grass).

## About script

This script is a script with a huge history that began in 2021, thanks to it I learned a lot in c#, even picked up linear algebra and repeated quite a bit of physics. that's hundreds of hours of code. Now I have absolutely no time left to do or change anything, this script should have been released at the end of summer - a little more than six months ago. but in three days I finally managed to finish what I had to postpone the release because of. I can now say that I like the time stopping magic and that it is done quite well. in lucky blocks, in general, no matter how hard I tried, there were always some shortcomings, for example, source generators, which I mastered in the summer, I never completed, unfortunately. many ideas had to be abandoned. but the release can no longer be delayed. I don’t even know when we last played SFD, probably a very long time ago. this script is what will remain in the SFD from our creative association of ["shiza"](https://www.youtube.com/@to_shiza). I want to believe that it will bring as much fun as it did for us in its time. The script can only be seen with your eyes, I do not plan to publish a way to compile it. those who want to try - good luck, I'll be interested to see  
\- from marshak in 2024

I'm tired of making changes to `README.md`  
\- from DMax in 2025
