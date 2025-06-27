# UnityUnusedAssetsFinder
Unity Editor tool to find and delete unused assets and scripts in your project.

---

## Features

- Scans your entire `Assets/` folder and lists unused assets  
- Optionally includes unused `.cs` scripts by searching references  
- Allows selecting multiple assets for batch deletion  
- "Ping" button highlights asset in Project window  

---

## Installation

1. Clone or download this repository  
2. Copy the `UnusedAssetFinderWindow.cs` file into your Unity project at `Assets/Editor/` folder (create it if it doesn't exist)  
3. Open Unity, let it compile scripts

---

## Usage

1. In Unity Editor, go to menu `Tools > Unused Assets Finder`  
2. In the window that appears, press **Find Unused Assets**  
3. (Optional) Toggle **Include unused .cs (script) files** checkbox if you want to detect unused scripts  
4. Select the unused assets you want to delete by ticking the checkboxes  
5. Click **Delete Selected Assets** to remove them from your project  
6. Use **Ping** button next to each asset to highlight it in Project window  

---

## Notes

- Only scans assets that are NOT referenced in enabled scenes from Build Settings  
- Does not detect assets used only at runtime via code (e.g. dynamically loaded resources)  
- Script reference detection is simple text search and might miss some indirect uses  
- Always backup your project or use version control before deleting assets!  

---

## Repository Structure
