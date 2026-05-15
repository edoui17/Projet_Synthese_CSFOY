# Audio Assets Guide - Task 14

## Recommended Sources
To maintain a high quality without requiring budget allocation, here are excellent sources for royalty-free (CC0) audio assets suitable for a 2D Roguelike like IslandSurvivor:

1.  **Kenney.nl (https://kenney.nl/assets/category:audio):**
    *   **Impact Sounds:** "Impact Sounds", "RPGs Audio" packs are perfect for wood, stone, and hit sounds.
    *   **UI/Upgrades:** "UI Audio" pack contains harmonius chimes great for stat upgrades.
2.  **Freesound.org (https://freesound.org/):**
    *   *Note: Always filter by "Creative Commons 0" (CC0) license to avoid copyright issues.*
    *   **Search terms:** "Wood chop", "Rock smash", "Sword hit flesh", "Magic level up", "Portal hum loop".
3.  **Sonniss GDC Audio Archive (https://sonniss.com/gameaudiogdc):**
    *   Massive, high-quality archives released yearly. Excellent for ambient loops (like the portal).

## Recommended Sound Matrix

| Event | Recommended Sound Profile | Suggested Format |
| :--- | :--- | :--- |
| **Wood Impact** | Sharp "Thwack" or "Chop" | `.wav` / `.ogg` |
| **Wood Destroy** | Splintering wood crackle | `.wav` / `.ogg` |
| **Rock Impact** | Dull "Clink" or metallic scrape | `.wav` / `.ogg` |
| **Rock Destroy** | Heavy rumble or crumbling stone | `.wav` / `.ogg` |
| **Enemy Hit** | Fleshy impact or armor ping | `.wav` / `.ogg` |
| **Enemy Death** | Short groan or heavy thud | `.wav` / `.ogg` |
| **Player Hit** | Sharp gasp or wince | `.wav` / `.ogg` |
| **Player Death** | Dramatic downward sweep/thud | `.wav` / `.ogg` |
| **Stat Upgrade** | Harmonious chime or ascending arpeggio | `.wav` / `.ogg` |
| **Portal Active** | Low frequency hum/drone (Looping) | `.ogg` (Better for looping) |

## Implementation Note
Godot natively supports `.wav`, `.ogg`, and `.mp3`.
*   Use **.wav** for short, punchy SFX (impacts, hits) because they have no decoding overhead.
*   Use **.ogg** for longer ambience loops (like the portal) as it has better compression and seamless looping natively supported in Godot.
*   *Avoid .mp3* for looping sounds as the format inherently adds tiny silent padding at the beginning/end, breaking seamless loops.
