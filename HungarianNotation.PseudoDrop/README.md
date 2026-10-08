This mod adjusts the vanilla game's bugged Pseudo Drop algorithm that is used for rare items to restore their intended pre-1.0 drop rates. It does this without disabling the new mechanic that puts an upper limit on how many kills you can get without a drop.

It also mitigates the issue where killing starred enemies can actually **reduce** your drop rate for some specific items.

If you simply want to play with purely random pre-1.0 drop mechanics, you don't need a mod! You can use `setkey NoPseudoDrops` to disable the new behavior for your world.

## Overview

The current version of the game has a bug that makes the first drop of a rare item after a game or server restart take (on average) much longer than it should to spawn, with odds being reduced by up to 50% in the worst case.

This means that the observed drop rates for items dropped by rarer mobs are much lower than intended. This is most disruptive for items like the bear and cultist trophies, but it affects many more items.

## Installation

As this mod is intended to simply fix broken vanilla behavior, there is limited configuration. The mod can be disabled with a setting, and there is a (disabled by default) setting for writing the generated kill counters to the BepInEx console.

This mod doesn't allow any configuration of drop rates, but it is compatible (and complimentary) with mods like [Drop That](https://thunderstore.io/c/valheim/p/ASharpPen/Drop_That/) which can be used to configure the drop rates of items.

This mod should be installed on both clients and servers. Jotunn is used to validate this.

## Bug Reports

If you encounter any issues with this mod, please open an issue on [the mod's GitHub page](https://github.com/hungarian-notation/Valheim.PseudoDrop/issues/new). Before doing so, please ensure that the issue can be recreated when running only this mod, Jotunn, and BepInExPack.

This mod should be natively compatible with other mods, though it might cause unexpected behavior when used alongside other mods that manipulate the pseudo-drop counters.

This mod has a very narrow scope, so non-bug feature requests are very likely to be rejected.

## Details

The game's Pseudo-Drop algorithm decides how many kills you need to get a certain drop the first time you kill a mob after a restart, and again after each drop. The issue is that the method it uses to pick the first drop counter is incorrect, resulting in the first drop of any play session coming later on average than it should. 

For rarer mobs, a much higher percentage of your drops are the first drop in any given game session, meaning that the overall drop rate ends up being much lower than it should be.

This mod patches the item drop logic to ensure that the overall drop rate matches the intended pre-1.0 drop rate, all while retaining the new logic's upper limit on kills between drops.

## Give me the Math!

The game falls afoul of the [inspection paradox](https://en.wikipedia.org/wiki/Renewal_theory#Inspection_paradox). When picking the required kills for the first interval, it assumes that the first drop always occurs following the longest possible delay, and randomly assigns the counter to a possible kill in that interval with an even distribution. This means that the first kill after a restart ends up with half the intended odds, and this nerf compounds over time.

The correct method is to pick the first required kills with a triangular distribution, simulating all possible positions in *all possible intervals*.

[Developer Jonathan Smårs has revealed on reddit](https://www.reddit.com/r/valheim/comments/1wt7erc/comment/pcsf3e8/) that a future patch (that seemingly has already been submitted for review to the console platforms) will work to partially mitigate this issue, but from his description it will only ameliorate the compounding nature of this bug. The first drop will still have the bugged reduced odds, which is significant for non-respawning rare mobs like the cultists. That patch will likely break this mod, so expect an update to be required for compatibility.

### Valheim 1.17 Drop Rates

Here's the current behavior. Any datapoints in these plots that fall below the blue "Nominal" series represent a nerf.

![Plot: First Drop Probability](https://github.com/hungarian-notation/valheim-statistics/blob/main/plots/pseudo_drop.first.png?raw=true)

This plot shows how the bugged algorithm affects total observed drop rates over time.

![Plot: Average Drop Rate](https://github.com/hungarian-notation/valheim-statistics/blob/main/plots/pseudo_drop.average.png?raw=true)

Unfortunately, nearly **all** datapoints fall below the nominal series.

Technically you now have a better chance to receive your first drop before kill 17 or later, but due to how much less likely you are to receieve a drop overall, you still end up with fewer drops on average in all scenarios.

### Patched Drop Rates

Here's the behavior after installing this mod.

The first drop odds are always as good as or better than the pre-1.0 odds, and the upper bound on single-session consecutive kills without a drop is preserved.

![Plot: First Drop Probability (Fixed)](https://github.com/hungarian-notation/valheim-statistics/blob/main/plots/pseudo_drop_random_arrival.first.png?raw=true)

Even though the grind for that first drop is now likely shorter than it would have been before 1.0, the long-term observed drop rate matches the pre-1.0 odds exactly in all cases:

![Plot: Average Drop Rate (Fixed)](https://github.com/hungarian-notation/valheim-statistics/blob/main/plots/pseudo_drop_random_arrival.average.png?raw=true)

## AI Disclosure

Absolutely no generative AI was used in the making of this mod.