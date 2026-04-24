<p align="center">
  <img src="https://img.shields.io/badge/BepInEx-5.4.x-blue?style=for-the-badge">
  <img src="https://img.shields.io/badge/Gorilla%20Tag-mod-green?style=for-the-badge">
</p>

---

# Overstocked — BepInEx

this is **not my project**. Overstocked was made by [**NotABird**](https://github.com/Not-A-Bird-07).
i just ported it from MelonLoader to BepInEx because i couldn't be bothered switching loaders every time i wanted to use it.
all the actual work was done by him, i just swapped some imports around.

---

<details>
  <summary><b>💾 Installation</b></summary>

### the easy way
1. grab the latest `Overstocked.dll` from [releases](#)
2. drop it into:
```
Gorilla Tag/BepInEx/plugins/
```
3. launch the game

### from source
1. clone the repo
2. build with `Ctrl + Shift + B`
   - the DLL copies itself into your plugins folder automatically on a successful build
   - if it doesn't, grab it from `bin/` and drop it in manually

</details>

<details>
  <summary><b>🧱 BepInEx Setup (if you don't have it)</b></summary>

1. download **BepInEx 5.4.x (x86)** from [here](https://github.com/BepInEx/BepInEx/releases)
   - make sure it's x86, not x64
2. extract into your Gorilla Tag folder so it looks like this:
```
Gorilla Tag/
├── BepInEx/
├── doorstop_config.ini
├── winhttp.dll
└── Gorilla Tag.exe
```
3. launch the game once and close it so BepInEx generates its folders
4. install the mod as above

</details>

<details>
  <summary><b>🛠️ Project Setup (for developers)</b></summary>

1. clone the repo
2. copy the following DLLs into the `assemblies/` folder at the project root

   from your game:
   ```
   Gorilla Tag/Gorilla Tag_Data/Managed/
   ```
   from BepInEx:
   ```
   Gorilla Tag/BepInEx/core/
   ```
   the `.csproj` already points to that folder, you just need the files there

3. open in Visual Studio or Rider and build
   - successful build = DLL lands in plugins automatically, no manual copying

</details>

---

## uninstall

delete `Overstocked.dll` from `BepInEx/plugins/`. done.

---


> this project is not affiliated with Gorilla Tag or Another Axiom LLC and is not endorsed or otherwise sponsored by Another Axiom LLC. Portions of the materials contained herein are property of Another Axiom LLC. © 2026 Another Axiom LLC.
