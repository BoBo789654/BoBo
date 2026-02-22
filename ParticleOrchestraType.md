# Particle Orchestra Showcase

## Sample Code

```cs
ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings
{
	PositionInWorld = player.Center,
	MovementVector = Vector2.One
});
```

`PositionInWorld` and `MovementVector` are both `Vector2` types. An additional constructor `UniqueInfoPiece` can be added and is `int` type.

# Types
`Vector2.One` was used for the MovementVector in all of these demos. That is the reason some of the particles move down and to the right. The left and the right side aren't neccessarily the exact same instance, it's just to show what it looks like on solid white and black.

## Keybrand

![KeyBrand](https://user-images.githubusercontent.com/11262234/232678257-123f5c9d-11eb-482d-b53a-61b88b131ae0.png)

## FlameWaders

![FlameWaders](https://user-images.githubusercontent.com/11262234/232678574-6aed5fc4-64e9-47a5-b93f-0aa384486fbe.png)

## StellarTune

![StellarTune](https://user-images.githubusercontent.com/11262234/232678918-b4af1381-5777-42d1-87b5-8a391305c174.png)

## WallOfFleshGoatMountFlames

![WallOfFleshGoatMountFlames](https://user-images.githubusercontent.com/11262234/232679350-4950f04d-4d62-4292-890b-dcc0979ca4d8.png)

## BlackLightningHit

![BlackLightningHit](https://user-images.githubusercontent.com/11262234/232679571-63c15249-c647-4ee5-8432-dc9c22310785.png)

## RainbowRodHit

![RainbowRodHit](https://user-images.githubusercontent.com/11262234/232679889-b8af853e-cdda-488f-acd4-abfbbe2be048.png)

## BlackLightningSmall

![BlackLightningSmall](https://user-images.githubusercontent.com/11262234/232680268-2853800d-7870-417f-89dd-00e262b84ad1.png)

## StardustPunch

![StardustPunch](https://user-images.githubusercontent.com/11262234/232680467-5dc2ad92-8141-4df6-b9d5-486bab771917.png)

## PrincessWeapon

![PrincessWeapon](https://user-images.githubusercontent.com/11262234/232680910-f2a478c9-c49b-4b65-bc7f-b2cd4da51de0.png)

## PaladinsHammer

![PaladinsHammer](https://user-images.githubusercontent.com/11262234/232681220-80b0abd2-dc55-48a6-b160-8baab00baf8c.png)

## NightsEdge

![NightsEdge](https://user-images.githubusercontent.com/11262234/232681692-0bfd82ce-d948-4aed-924c-6775c31bcb87.png)

## SilverBulletSparkle

![SilverBulletSparkle](https://user-images.githubusercontent.com/11262234/232681859-43078278-e0de-4eee-8fd2-842365d60ab2.png)

## TrueNightsEdge

![TrueNightsEdge](https://user-images.githubusercontent.com/11262234/232682324-6c54c1dd-d01c-4c26-9828-101b2103dc09.png)

## Excalibur

![Excalibur](https://user-images.githubusercontent.com/11262234/232682448-fcb34957-b382-4110-b45c-3b03db4773bf.png)

## TrueExcalibur

![TrueExcalibur](https://user-images.githubusercontent.com/11262234/232682665-50b9738d-f9ef-4c64-9a4a-23bdce372499.png)

## TerraBlade

![TerraBlade](https://user-images.githubusercontent.com/11262234/232682897-4f55d6f2-a691-4cf0-91c4-3009c0cfa676.png)

## ChlorophyteLeafCrystalPassive

![ChlorophyteLeafCrystalPassive](https://user-images.githubusercontent.com/11262234/232683386-9be44c6d-2cc3-4e82-9a62-22c1174c7384.png)

## ChlorophyteLeafCrystalShot

![ChlorophyteLeafCrystalShot](https://user-images.githubusercontent.com/11262234/232683594-42872e44-8eb5-4e9e-ae02-39cca679f8f8.png)

`UniqueInfoPiece` controls the hue. `0` to `255` for the hue. `511` or above will make the color gray.

## AshTreeShake

![AshTreeShake](https://user-images.githubusercontent.com/11262234/232683910-12ac24c0-d5a1-4c68-892a-80a24de88a7a.png)

## PetExchange

![PetExchange](https://user-images.githubusercontent.com/11262234/232684207-a486ac83-608e-4757-9978-380001f0db54.png)

## SlapHand

Doesn't actually look like anything, but it does play that comical slapping sound.
[Item_175](https://terraria.wiki.gg/wiki/File:Item_175.wav)

## FlyMeal

Doesn't actually look like anything, but it does play that comical farting sound.
[Item_16](https://terraria.wiki.gg/wiki/File:Item_16.wav)

## GasTrap

![GasTrap6](https://user-images.githubusercontent.com/11262234/232686464-f8e89a95-bddf-4d72-abd8-6cc08431ab6a.png)

Also makes that comical farting sound.
[Item_16](https://terraria.wiki.gg/wiki/File:Item_16.wav)

## ItemTransfer

![ItemTransfer](https://user-images.githubusercontent.com/11262234/232696054-5e40400b-93a5-442a-999f-f97d5e7ef24e.png)

`UniqueInfoPiece` is the item ID for which item to show. 

## ShimmerArrow

![ShimmerArrow](https://user-images.githubusercontent.com/11262234/232686991-10eded25-1a2d-4406-8d5c-61efa5d35053.png)

## TownSlimeTransform

### UniqueInfoPiece = 0
Used by the Nerdy Slime

![TownSlimeTransform](https://user-images.githubusercontent.com/11262234/232687356-45838b53-1764-479b-9abb-0711bc974583.png)

### UniqueInfoPiece = 1
Used by the Squire Slime

![TownSlimeTransform1](https://user-images.githubusercontent.com/11262234/232694330-25a8ee04-55dd-43e9-a7ac-de363911018b.png)

### UniqueInfoPiece = 2
Used by the Elder Slime

![TownSlimeTransform2](https://user-images.githubusercontent.com/11262234/232694647-aefb389d-7182-4451-93ce-5afa46c956a4.png)

## LoadoutChange

![LoadoutChange](https://user-images.githubusercontent.com/11262234/232691683-e2396cb3-7557-41db-8c3d-34332f67425b.png)

Always spawns on the player no matter what the position is set to.

## ShimmerBlock

![ShimmerBlock](https://user-images.githubusercontent.com/11262234/232687688-ca09dedd-3aaa-4fba-b3ae-836cfef5e505.png)

## Digestion

![Digestion](https://user-images.githubusercontent.com/11262234/232687958-e47f6c64-e01d-4704-8683-62d77ea04788.png)

## WaffleIron

Doesn't actually look like anything, but it does play a sound of the pan hitting enemies.
[Item_178](https://terraria.wiki.gg/wiki/File:Item_178.wav)

## PooFly

![PooFly](https://user-images.githubusercontent.com/11262234/232688594-78419bb2-66c5-496d-ab57-5ebfee182f2f.png)

## ShimmerTownNPC

![ShimmerTownNPC](https://user-images.githubusercontent.com/11262234/232688869-66f7c4db-aa7e-40e7-bc7a-c8387965a2e0.png)

Also makes a magic *shing* sound.
[Item_29](https://terraria.wiki.gg/wiki/File:Item_29.wav)

## ShimmerTownNPCSend

![ShimmerTownNPCSend](https://user-images.githubusercontent.com/11262234/232689595-fd4750c6-7590-43d2-a655-7fd1c9b2272a.png)

Does not make any sound.