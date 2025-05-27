# Lucky Blocks

<img width="720" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/47c57c25-c8c3-44e4-9482-13684ab3f23e" alt="Lucky blocks" />

*They may be unlucky for some players*  
© *Old superfigher saying*

## Description

That script adds much of insteresting things.  
When a loot crate spawns on the map, there is `30%` (by default) chance that it become a lucky block  
There are a lot of different drop types you may receive:

1. [Player Buffs](#player-buffs) `(make us harder, better, faster, stronger)`
2. [Weapons](#weapons) `(as a drop or improvement)`
3. [Magic](#magic) `(wind, fire, and more...)`
4. [Items](#items) `(have you ever used expired pills?)`
5. [World Events](#world-events) `(the God of the match is watching you)`

**Recommendations**: don't use teams, scripts that can modify objects, players, basically other mods. Lucky blocks is a global standalone mod that changes the game in many aspects. [There are](https://steamcommunity.com/id/mysterymarshak/myworkshopfiles/?appid=855860) my other scripts, that can improve gameplay experience and works fine together (except BazookaGame, which is, in fact, also mod), i'm not responsible for the rest  

All not-standart textures that you will see in GIFs aren't included to this repo

## Player Buffs

Buffs can be obtained from lucky block if player breaks it by kicking or melee atacking  
All durable buffs can be stacked, so you can got a buff again from lucky block, but duration can't be higher than initial value  
Incompatibility means that incompatible buffs can't be obtained from lucky block when player have it

### Full HP

Fully recovers player's HP

**Applies**: once  
**Can be obtained**: if player's HP aren't full

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/13312994/ac7d1fec-0224-451d-9dae-01b011d06b50" alt="Demo of buff usage" />

### Freeze

Freezes the player, preventing them from moving or using weapons

**Duration**: 5s  
**Can be obtained**: if player hasn't got an immunity to freeze  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/50cfe493-622c-4415-901b-9ae8dbb57730" alt="Demo of buff usage" />

<p>

> [!TIP]
> You can get warm!
> 
> <img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/71abe2ee-720e-4e85-b6be-ce94eb5a7c87" alt="Demo of unfreezing" />

</p>

### High Jumps

Increases the player's jump height

**Duration**: 10s  
**Immunities**: to fall damage

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/72e88a45-b1fc-4a44-b099-ad4d09744f8f" alt="Demo of buff usage" />

### Totem of Undying

Instantly revives the player upon death

**Applies**: once  
**Can be obtained**: if player hasn't got a totem yet  

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/70364d8e-758d-4dd9-8ee4-20106145bf9f" alt="Demo of buff usage" />

<p>

> [!WARNING]
> After revival you will lost all your applied buffs!

> [!NOTE]
> The totem will not revive you if the player's body was destroyed
> 
> <img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/6320470a-153a-4a2c-a77f-7de7eee9ff9e" alt="Demo of note" />

</p>

### Shield

Gives complete invulnerability to damage and magic

**Duration**: 7s  
**Incompatibility**: with [Hulk](#hulk)

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/2fa5bc38-c819-472e-b601-ddd177fb89ca" alt="Demo of buff usage (against weapon)" />
<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/334e4ea5-8db4-4267-8b73-8c0ee7d3e16b" alt="Demo of buff usage (against magic)" />

<p>
  
> [!NOTE]
> The shield doesn't protect against the destruction of the player's body and time stop magic

</p>

### Vampirism

The vampire recovers his health equal to the damage dealt to other players

**Duration**: 15s  
**Incompatibility**: with [Hulk](#hulk) and [Dwarf](#dwarf)

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/75ad22ce-3714-4d7b-81b6-78e087f1f2ec" alt="Demo of buff usage" />

<p>

> [!WARNING]
> Pills and medkits are poisoning you with damage equals to heal! `(25 for pills and 50 for medkit)`
>
> <img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/8d8f3d7f-6fe1-4932-9e0d-e4b3b1a8b5ae" alt="Demo of note" />

</p>

### Strong Man

Makes the player insanely strong  
Kills enemies with one hit in melee attack  
Increased shooting damage and also magic range and propagation speed  
Strongly kicks objects on the map

**Duration**: 5s  
**Incompatibility**: with [Hulk](#hulk) and [Dwarf](#dwarf)  
**Immunities**: to [Wind magic](#wind-wizard)

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/89812c1e-0152-4bed-bfc0-cde74af427f2" alt="Demo of buff usage (strength)" />
<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/42417982-cee0-421d-9f72-f645bca210f4" alt="Demo of buff usage (immunity to wind)" />
<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/46f96d86-6b79-487e-a784-dfee3097fae8" alt="Demo of buff usage (magic range and propagation speed)" />

### Hulk

Makes the player strong, big and green  
Melee damage increased threefold  
Damage taken is reduced by 20%  
Movement speed is reduced  

**Duration**: 10s  
**Incompatibility**: with [Dwarf](#dwarf), [Vampirism](#vampirism), [Strong Man](#strong-man) and [Shield](#shield)

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/cf19bf85-8715-4190-af83-0acda322e509" alt="Demo of buff usage" />

### Dwarf

Makes the player small and fast  
Melee damage reduced by 30%

**Duration**: 10s  
**Incompatibility**: with [Hulk](#hulk), [Vampirism](#vampirism) and [Strong Man](#strong-man)

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/a8741de3-4b76-47e2-876f-b29a1655b19b" alt="Demo of buff usage" />

## Weapons

Weapon can be obtained by any type of lucky block destruction  

### Legendary weapon

Drops a random legendary weapon `(e.g are Bazooka, Grenade launcher, Sniper, M60, Chainsaw, Magnum, Flaregun)`

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/22d44abb-1f19-4008-9f16-ec62647f460e" alt="Demo of weapon" />

<p>
  
> [!WARNING]
> Weapon can be booby-trapped with 10% chance! `(it explodes after shoot, or in case of chainsaw after picking up)`
>
> <img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/95e2f2a0-8f40-4e3d-9371-60e9756f8185" alt="Demo of booby-trap" />

</p>

### Sticky grenades

Drops a sticky grenades, that can be attached to player or any surface

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/7deb3b79-075f-4d51-89e6-6acb408340e5" alt="Demo of weapon (attaching to player)" />
<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/561879c7-6cbe-460e-b612-4789d0b7ff51" alt="Demo of weapon (attaching to surface)" />

<p>
  
> [!TIP]
> Sticky grenade has pink "glue" on it

</p>

### Bow with infinite bouncing bullets

Drops a bow with infinite bouncing bullets

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/ac3dfbfb-3cdc-480b-be05-db0ff8d3ffdc" alt="Demo of weapon" />

### Grenade launcher with infinite bouncing bullets

Same as [Bow with infinite bouncing bullets](#bow-with-infinite-bouncing-bullets)

### Random weapon with random powerup

Drops a random weapon with random available [powerup](https://github.com/mysterymarshak/LuckyBlocks?tab=readme-ov-file#weapon-powerups)

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/1efdc942-a6ba-46cf-9d1c-1b7c7b83f81f" alt="Demo of weapon" />

## Weapon (powerups)

Can be obtained if player have a firearm  
Poweruped bullets count depends on weapon, unless otherwise stated  

> [!NOTE]
> One firearm can has got many type of powerups, if its compatible with each other
>
> <img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/44604b4b-50b8-4d76-8305-a71e3e30cf64" alt="Demo of many powerups on same weapon" />

### Explosive bullets

Bullets trigger an explosion on colliding with any object

**Incompatibility**: with [TripleRicochetBullets](#triple-ricochet-bullets) and [InfiniteRicochetBullets](#infinite-ricochet-bullets)

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/23ac71a7-01a7-4e74-ac30-f0e1929a9f24" alt="Demo of powerup" />

### Triple ricochet bullets

When bullets collide with a surface, its ricochet, scattering into three

**Incompatibility**: with [ExplosiveBullets](#explosive-bullets), [FreezeBullets](#freeze-bullets), [InfiniteRicochetBullets](#infinite-ricochet-bullets), [AimBullets](#aim-bullets), [PushBullets](#push-bullets)

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/1eb60c5e-e281-4757-a85e-007a6f35f6d6" alt="Demo of powerup" />

### Freeze bullets

When collides with player, he become [frozen](#freeze)

**Can be obtained**: if alive players more than 1  
**Incompatibility**: with [TripleRicochetBullets](#triple-ricochet-bullets) and [PushBullets](#push-bullets)

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/8809bc8b-f971-47a5-b8a0-02cd5cc57491" alt="Demo of powerup" />

### Push bullets

Pushing objects on its path

**Incompatibility**: with [TripleRicochetBullets](#triple-ricochet-bullets) and [FreezeBullets](#freeze-bullets)

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/bc976848-08fe-45b5-b188-90dc54f24bad" alt="Demo of powerup" />

### Aim bullets

Follow the nearest player in some radius

**Can be obtained**: if alive players more than 1  
**Incompatibility**: with [TripleRicochetBullets](#triple-ricochet-bullets)

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/016c0874-96bb-4c2c-8f19-e42e50656656" alt="Demo of powerup" />

### Infinite ricochet bullets

Bullets which can infinite ricochet

**Quantity**: all weapon ammo  
**Incompatibility**: with [ExplosiveBullets](#explosive-bullets) and [TripleRicochetBullets](#triple-ricochet-bullets)

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/8ff2916f-b218-4ea0-a3ed-f080ef6e7bdd" alt="Demo of powerup" />

## Magic

You can have only one type of magic in same time  
Casts can be stacked as buffs, but there are not limits to casts count  
Each wizard is immune to his own magic (see [Wind wizard example](#wind-wizard))  
To use, hide weapon (except melee) and press `[ALT + A]`

### Wind wizard

Emits strong currents of wind that destroy everything in its path  
Disarms enemies
Reflects bullets  
Puts out the fire

**Casts count**: 3

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/62c01d6f-8346-4ef3-b9e0-2b865d2a8cb6" alt="Demo of magic" />
<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/1984848d-f241-4457-b99e-a0d7677d66d4" alt="Demo of magic immunity" />

### Fire wizard

Spews out a fireball, setting everything on fire in its path

**Casts count**: 3  
**Immunities**: to fire, and [Freeze](#freeze)

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/b728173d-8d66-4eeb-8a5d-8d6e978bcb99" alt="Demo of magic" />

### Electric wizard

Releases an electrical wave, shocking players and electrolyzing objects  
Shock makes it impossible to move and deals 3 damage every 0.3 seconds  
When touching an electrified object, the player receives a shock

**Casts count**: 3

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/a34d8dff-3a14-4976-be2b-cab946c00156" alt="Demo of magic (agains players)" />
<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/cfde6763-e0ab-43cc-ae1f-e0ecbac602ec" alt="Demo of magic (agains objects)" />

### Decoy wizard

Spawns three illusions of the player, making it possible to confuse the enemy

**Casts count**: 3  
**Can be obtained**: if no one else have got this type of magic

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/d64d834c-5608-4d66-b91b-28adb41692aa" alt="Demo of magic" />

<p>

> [!NOTE]
> The illusions don't damage

</p>

### Time stop wizard

Stops the time  
Yes, it's jojo reference, if u know, then this magic works exactly as you think, if no, so:  
In time stop thrown objects first slow down, then its stop completely, and after time resumes its continue to move as usual  
In time stop fire don't damage objects, after time resumes it continue to be fire as usual  
In time stop bullets first travel some distance, slow down, then its stop completely, and after time resumes its continue to be bullets as usual  
If wizard will die during time stop, time will automatically resume  

**Casts count**: 1

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/da677d26-1661-4767-9642-6274a7653771" alt="Demo of magic" />

<p>

> [!TIP]
> You can resume time for object by kicking it with pressed alt `[ALT + S]`  
> You can disarm players by attacking them with pressed alt `[ALT + A]`  
> You can stay on liquids like Jesus

> [!NOTE]
> Time stop doesn't work for other time stop wizards  
> If someone grab you during first 1.5s, time will resume

</p>

## Magic collision

### Fire magic + Electric magic = Explosion

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/22177ff5-5499-4089-ba71-e21c6d48948d" alt="Demo of magic colliding" />

### Wind magic + Fire magic = Fire absorption

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/293e9a55-9404-45e6-99eb-2b0a67ecba3d" alt="Demo of magic colliding" />

## Items

### Medkit

Spawns pills (25 hp) or medkit (50 hp)

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/d4a302fd-68ad-4b51-b3e0-03031123b4dc" alt="Demo of medkit" />

<p>

> [!WARNING]
> Can be poisoned with 50% chance!
>
> <img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/c3a3900a-c305-4032-b541-e73eb7a254b3" alt="Demo of medkit" />

</p>

## World events

### Respawn random player

Respawns random dead player with half hp

**Can be obtained**: if dead player exists

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/ce3c8a5e-7307-4a54-81ed-0180d32b7666" alt="Demo of event" />

### Explosion

Triggers an explosion

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/f414fdec-817f-4303-a4bb-5773cdedb6b5" alt="Demo of event" />

### Explode random barrel

Triggers an explosion of random barrel

**Can be obtained**: if explosive barrels exists

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/5bce758f-4b20-4127-ac81-5228150584d2" alt="Demo of event" />

### Increase spawn chance

Increases spawn chance of lucky block `(30% -> 45% -> 60%)`

**Can be obtained**: if next level of chance exists

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/70de0737-faee-4d2c-ba62-db738c3faed2" alt="Demo of event" />

### Shuffle positions

Shuffles the player positions

**Can be obtained**: if alive players more than 1

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/81759f06-34e3-40c8-8e3f-9c14dd16b9d9" alt="Demo of event" />

### Shuffle weapons

Shuffles players weapons 

**Can be obtained**: if alive players more than 1 and anyone have got any weapon

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/82627e13-a44f-4ebc-8ce3-d43fcd01c50b" alt="Demo of event" />

### Ignite random player

Ignites a random player

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/920fe7f3-e9d3-45a5-95dc-7d1afe74de65" alt="Demo of event" />

### Bloody bath

Spawns many of explosive barrels in the top of the map

<img width="370" src="https://github.com/mysterymarshak/LuckyBlocks/assets/37479500/9de5df30-c15b-41fc-834c-9dd8144f6bf1" alt="Demo of event" />

## About script

This script is a script with a huge history that began in 2021, thanks to it I learned a lot in c#, even picked up linear algebra and repeated quite a bit of physics. that's hundreds of hours of code. Now I have absolutely no time left to do or change anything, this script should have been released at the end of summer - a little more than six months ago. but in three days I finally managed to finish what I had to postpone the release because of. I can now say that I like the time stopping magic and that it is done quite well. in lucky blocks, in general, no matter how hard I tried, there were always some shortcomings, for example, source generators, which I mastered in the summer, I never completed, unfortunately. many ideas had to be abandoned. but the release can no longer be delayed. I don’t even know when we last played SFD, probably a very long time ago. this script is what will remain in the SFD from our creative association of ["shiza"](https://www.youtube.com/@to_shiza). I want to believe that it will bring as much fun as it did for us in its time. The script can only be seen with your eyes, I do not plan to publish a way to compile it. those who want to try - good luck, I'll be interested to see
