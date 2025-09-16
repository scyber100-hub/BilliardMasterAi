# Design Guidelines: Icons and Emoji Replacement

- Goal: Replace any emoji used in UI with high-quality vector or sprite icons.

## Icon System
- Use `IconSet` (ScriptableObject) to map `IconKey` → `Sprite`.
- Use `IconImage` component on `Image` to render the chosen `IconKey`.
- Create an IconSet via: Create → BilliardMasterAi → Icon Set, then assign sprites.
- Suggested keys: Home, Camera, Recommend, Compare, ARGuide, Replay, Save, Dashboard, Assign, Training, Monitor, Report, Video, Calibrate, Overlay, Export, Share, Timer, League, Star, Badge, Warning, Info.

## Emoji Scanner (Editor)
- Menu: BilliardMasterAi → Tools → Scan Emojis in UI Text.
- Scans open scenes’ UI Text (and TMP when available) for emoji/symbol code points.
- Replace flagged text with icons or plain text.

## Asset Recommendations
- Prefer monochrome SVG → sprite atlas (Unity SVG Importer) or 2x/3x PNG sprites.
- Keep sizes consistent (24/32/48 px logical), provide @2x for high DPI.
- Use a neutral color palette; tint via `Image.color` when needed.

## Usage Tips
- Avoid mixing icon and emoji fonts; rely on sprites for consistency.
- For action buttons, pair `IconImage` + label Text; keep spacing 8–12 px.
- For HUD (timer, league), use icons with accessible contrast.
