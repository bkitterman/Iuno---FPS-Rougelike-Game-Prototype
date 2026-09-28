Idea for how to incorporate rouge like elements into this game

Basically, sci-fi world, post apocalypse, player is AI construct in robotic body
The enemy could be something like Siva or something.

## Components
Each hardware component specifies gameplay
- Cores - Contains passive abilities towards program. IE use less memory, increase effects by %, etc.
- Program - The buff/boon, like dela more damage, shoot 2 shots, etc.
	- Routine - instances of a single program, equivalent to stack counter (Essentially, increase effects of program by # of routines, like stacks of an item in risk of rain). Increase may be linear, hyperbolic, or logarithmic.. The faster scalers will be more logarithmic wherein heavy slowdown after time.
- Application Suite - If # of programs belong to a set are in memory, activate a buff (Like a relic set in genshin)
	- IE Offensive Suite = Pure damage out increase
- Storage - The number of programs in inventory (Each program takes different amounts of storage depending on strength). If maxed -> Pickup goes to memory if available, or is not picked up until space is made or player leaves area, in which it de-spawns.
- Memory - The number of programs/coroutines available to be run at once (Active buffs and their stacks). This varies dependent on programs 'game balance'. I full, go to storage
- Power - Describes how many components can be had, or how strong each is. Every component has its own power draw. Light, not very good components take little power, whereas heavy components that grant strong benefits draw a lot more. Upgrades can be unlocked over time.
- Firewall - Customized like hardware. Describes how long/strong a malware affects the player - Malware - Harms player for set amount of time until Firewall can patch it, then it becomes buff
- Scripts - Essentially a memory save file. A single click will load an entire memory save into active memory, similar to a loadouts system. They can be saved midrun, and can be used across all runs if 
	1. Chosen to be saved system wide (Can be done in run or after it, cleared or failed)
	2. All required programs are present in storage/memory of the active run. Routines will not be capped, but will auto-fill in as they are gained if a setting for the script is enabled (Toggle auto-add routines)
	3. There is enough memory available to fit the script.
- OS Settings - Used to make the game easier or better. 

## Memory/Storage
If one is full, a new drop will attempt to go to the other. If both are full, it will remain on the ground, so long as it remains within a radius of the player. The player can chose to drop something they have already picked up if they wish to take in the new item.

Programs/Virus/Malware may destroy programs/routines as well during gameplay without intervention from the player, for better or worse. A program will only be destroyed if it has no routines, and there is a slight RNG favor in destroying a routine over a program, unless explicitly stated otherwise.

Programs will always take up the same amount of storage, but will only take up memory based on if they are active. Routines do not require storage, but do require more memory. 

Memory and storage will be scaled to match real life standards, as will programs.

## Loot System
Loot will come in a rarity system of multiple steps from Common -> Legendary (Names and steps to be decided later). Common is weaker, but drops more often.

Loot is unlocked through runs by chance, or completing objectives, like defeating a boss, or surviving a set time, and so on. The chance items will be weight slightly to promote items that are yet to be acquired. 

Drops will be first determined by loot rarity, then from there, decide based on owned, previously acquired by player (Not-run, but game/save wide). If a player has a suite, there will be a very gentle increase of chance for other *discovered* applications of the same suite.

Loot can drop from special enemies, exploration, or completing encounters.

May be influenced by cores/programs active.

May also indicate a 'quality' system, where programs can occasionally drop with a greater memory effeciency or slightly (2-5% for fairness) buff strength.

## Application Suites
There will be an in-game codex to help show which are which, but all programs will have a sub-title on the item description that says what suite it belongs to. Perhaps if a button is pressed to show a full breakdown of item (IE: "Item increases crit chance" v "Item increases crit chance by 10% (+5% per routine)") will also list application suite effect and other programs in the suite. 

The codex, and the information of what items do, their suite, and the suite affects, only unlock/become visible once trigged. IE on item pickup, it will display affect, but the suites other programs will not be shown if they have not been collected.

## Malware
When granted to the player, it will be a de-buff of some item, be it disabling programs, destroying them, or overall lowering stats. After a time, determined by Firewall performance passes, it will be transformed into a buff of similar power to the virus. 

Firewall has three values and has no effect on power
- De-buff reduction -> Lowers the affect of the virus/malware
- Cure Time -> Lowers the amount of Virus/malware affect time/Time before transformation into buff
- Effectiveness -> How well the de-buff gets transformed into a buff. IE High efficiency means better buff, lower means smaller buff.

Firewalls with higher efficiency come at the cost of greater reduction/cure time.

The cure will be visually noted by a toast popup, or other visual notification, as well as a full popup after combat sequence ends to show how Corruption -> Buff.

Any de-buffs will be immediately visible, with countdown, above gun (Or someplace always visible) to denote effects, similar to destiny.

Malware and Virus's cannot be dropped or removed until cured.
## Gameplay Loop
I'm hoping for fast action, but with tactical build crafting between, like Destiny raids. Player starts with nothing, and unlocks items as they go, adding them to the loot pool. Start with minimum hardware that can be upgraded with each run.

There will be downtime between particularly dense encounters, and in general, so player has time to build craft and read the buff they acquired. 

## Modifications (Mods)
Can be decided prior to run, or at save file level, unsure yet. This will be used to change the fundamental gameplay in a way, but will also prevent updating the codex or unlocking anything when enabled. Similar to a system in dead cells i forgot the name of.
- Remove hardware limitations
- Randomize programs
- Reduce power requirements of hardware
- More to be added

Unlocked via milestones in the game. Meant to be a cheat code system for fun runs, like RoR2 artifacts.

## Operating System Settings
Settings unlock as game progresses (With more QOL features frontloaded so their early, but late enough to be a reward). This is considered a program and will take up memory and storage. Dropping it will result in death (There should be warning popup, like Nier Automata). The more items active, the more memory OS will take.
	- Auto-run routines - Automatically add a routine to memory if the program is already loaded and space is available.
	- Auto-run programs - Automatically add a program to memory if space is available.
	- Scripts will be through this feature
	- Backing Store/Paging - (Later game?) If there is not enough space for a full program, one can still be run so long as available memory is at least a quarter the required memory for the program, or 1 byte is free for a routine to be added. Comes at reduced performance to the program relative to amount of memory missing. (How many pages in store = % reduction)
	- Quarantine - If a virus or malware is acquired, remove it and place it in quarantine (Takes up storage). Firewall will not progress on this malware until in memory. Only one can be in quarantine at a time. Not all viruses can be quarantined (Such as boss or even-specific de-buffs).
		- Later upgraded to auto-prioritize certain more damaging de-buffs.
	- Virtual Desktops - Enable a hotkey to quick-swap scripts. (Very late game?). Potential drawback is programs must unload and then reload, leaving player vulnerable during the short swap period (Ie 2-3 seconds, depending on program count). Has cooldown of a few seconds. Can only have 2 scripts at a time (So two builds to swap between)
	- Firewall will be upgraded through this method since its software
	- Reduce Memory Overhead - As the OS is upgraded, its amount of memory and storage can be reduced
	- File Compression - Reduce storage space of inactive files, but increase time to swap programs in and out.

Any auto-run items should only be executed on item pickup to ensure it doesn't overrule player build crafting.

## HUD/Screens
There will need to be a very clean gameplay screen that shows minimal items like
- Ammo count
- Health
- Active debuffs, denoted by clean name and timer
- Abilities

A inventory screen split in two
- one for storage/memory management
- One for scripts management
- One for OS settings
- One for hardware (Locked during run, but still visible)
- Codex
- Stats page