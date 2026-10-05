# ZKTris Unforgivable

A Tetris remake where the holes you leave behind are marked and can never be cleared.

Play the web version or download the Windows build at **https://zizmantk.itch.io/zktris**.

## The rule

Every gap you leave under a block turns into a crossed-out hole. Holes are permanent: a row that contains one can never be cleared. The board is 21 × 39, so you have room, but every mistake stays with you until the end.

## Controls

| Action | Keys |
| --- | --- |
| Move | ← → or A D (hold to slide) |
| Rotate | ↑ / W / X (clockwise), Z (counter-clockwise) |
| Soft drop | ↓ or S |
| Hard drop | Space |
| Hold | C or Shift |
| Pause menu | Esc or P (↑↓ and Enter to pick, ←→ to switch a setting) |
| Restart after game over | Enter or R |

## Features

- Score, level, lines and hole counter; best score is saved.
- Ghost piece, hold piece, 3-piece Next queue, 7-bag randomizer, lock delay and wall kicks.
- Speed goes up every 10 lines.
- Screen shake, particles and row flashes on clears and holes.

## Development

- Unity **6000.6.3f1**, Built-in render pipeline, uGUI.
- `Assets/Scripts/Core` is the game model (board, pieces, scoring) as plain C# with no scene dependencies.
- `Assets/Scripts` holds the MonoBehaviours that drive the scene and UI.
- Unit tests live in `Assets/Tests/Editor` (Window > General > Test Runner, EditMode).

Run the tests from the command line:

```bash
Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -testResults results.xml
```

Build WebGL and Windows into `Builds/` (also available from the **ZKTris** menu in the editor):

```bash
Unity.exe -batchmode -quit -projectPath . -executeMethod BuildScript.BuildAll
```

## Credits

UI font: [Barlow Condensed](https://github.com/jpt/barlow) by The Barlow Project Authors, SIL Open Font License 1.1 (`Assets/Fonts/OFL.txt`).
