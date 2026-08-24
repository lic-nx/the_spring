# Pot Action Menu Implementation Guide

This guide explains how to set up the **Pot Action Menu** system in Unity for your game. This system replaces the direct dragging mechanic with a click-based menu that allows players to:
1. **Delete** a pot
2. **Replace** the pot's sprite
3. **Move** the pot to a new zone

---

## Files Modified/Created

### Modified Files
1. **`Pot.cs`**
   - Removed inheritance from `WorldDraggable`
   - Added `IPointerClickHandler` interface for click detection
   - Added methods for toggling the action menu visibility
   - Added methods for **Delete**, **Replace Sprite**, and **Move** actions

2. **`PotDragManager.cs`**
   - Added support for moving existing pots
   - Added `StartMovingExistingPot()` method
   - Added `TryPlaceExistingPot()` method
   - Updated `PotGhostDragHandler` to handle existing pots

3. **`Zone_for_pot.cs`**
   - Updated `OnPotDrop()` to handle moving pots between zones

### New Files
1. **`PotActionMenu.cs`**
   - Handles the UI menu logic for pot actions
   - Manages button clicks and sprite replacement

---

## Setup Instructions

### Step 1: Add the Menu as a Child of the Pot Prefab

1. **Open your Pot Prefab** in Unity:
   - Double-click the pot prefab in the Project window to open it in the Prefab Editor.

2. **Create the Action Menu as a Child Object**:
   - Right-click on the **Pot** object in the Hierarchy → **Create Empty**
   - Rename it to `ActionMenu`
   - Position it above the pot (e.g., set its **Transform** to `X: 0, Y: 1.5, Z: 0`)

3. **Add UI Buttons to the Action Menu**:
   - Right-click on the `ActionMenu` → **UI** → **Button** (3 times)
   - Rename the buttons:
     - `DeleteButton`
     - `ReplaceSpriteButton`
     - `MoveButton`
   - Set their **Text** components to display "Delete", "Replace Sprite", and "Move"

4. **Create the Sprite Replace Panel**:
   - Right-click on the `ActionMenu` → **Create Empty** → Rename to `SpriteReplacePanel`
   - Add **4-6 buttons** (for sprite options) as children of `SpriteReplacePanel`
   - Rename them to `SpriteOption1`, `SpriteOption2`, etc.
   - **Disable** the `SpriteReplacePanel` by default (uncheck the checkbox in the Inspector)

5. **Attach the `PotActionMenu` Script**:
   - Select the `ActionMenu` object in the Pot prefab
   - Click **Add Component** → **Scripts** → **PotActionMenu**
   - In the Inspector, assign the following fields:
     - **Delete Button**: Drag `DeleteButton`
     - **Replace Sprite Button**: Drag `ReplaceSpriteButton`
     - **Move Button**: Drag `MoveButton`
     - **Sprite Replace Panel**: Drag `SpriteReplacePanel`
     - **Sprite Option Buttons**: Drag all sprite option buttons (e.g., `SpriteOption1`, `SpriteOption2`)
     - **Available Pot Sprites**: Drag your pot sprites from the Project window

6. **Disable the Action Menu by Default**:
   - Select the `ActionMenu` object in the Pot prefab
   - Uncheck the **Active** checkbox in the Inspector (so it's hidden by default)

---

### Step 2: Assign the Action Menu in the Pot Script

1. **Select the Pot Prefab** in the Prefab Editor
2. In the Inspector, find the **Pot** component
3. Assign the `actionMenu` field with the `ActionMenu` child object you created

---

### Step 3: Save the Prefab
1. Click **Save** in the Prefab Editor to apply changes to all instances of the pot prefab

---

## How It Works

### Click Handling
- When a player **left-clicks** on a pot, the `OnPointerClick` method in `Pot.cs` is triggered
- This calls `ToggleActionMenu()`, which toggles the visibility of the `ActionMenu` child object

### Menu Actions
1. **Delete**:
   - Calls `DeletePot()` in `Pot.cs`
   - Frees the zone (if occupied)
   - Destroys the pot and any flower inside it

2. **Replace Sprite**:
   - Opens the `SpriteReplacePanel`
   - Player selects a new sprite from the options
   - Calls `ReplaceSprite()` in `Pot.cs` to update the pot's appearance

3. **Move**:
   - Calls `StartMoving()` in `Pot.cs`
   - Activates the `PotDragManager` to start dragging the pot
   - Shows available zones (via `DropZoneManager`)
   - Player can place the pot in a new zone

---

## Testing

1. **Test Toggle Menu**:
   - Click a pot → Menu should appear above it
   - Click the pot again → Menu should disappear

2. **Test Delete**:
   - Click a pot → Menu appears → Select "Delete" → Pot should disappear
   - Check if the zone is freed (can place a new pot there)

3. **Test Replace Sprite**:
   - Click a pot → Menu appears → Select "Replace Sprite" → Choose a new sprite
   - Verify the pot's appearance updates

4. **Test Move**:
   - Click a pot → Menu appears → Select "Move" → Drag the pot to a new zone
   - Verify the pot moves and the old zone is freed

---

## Notes
- The menu is **toggled** (shown/hidden) instead of instantiated/destroyed
- The **Move** action uses the same dragging logic as buying a new pot
- Sprite replacement **saves automatically** (via `YG2.SaveProgress()`)
- The menu is **part of the pot prefab**, so all pot instances will have it

---

### Step 3: Configure the Shop (Optional)

If you want to ensure new pots also use the menu system:
1. Open the **Shop** script
2. Ensure that the `potDragDropPrefab` (the pot prefab) has the updated `Pot.cs` script attached

---

## How It Works

### Click Handling
- When a player **left-clicks** on a pot, the `OnPointerClick` method in `Pot.cs` is triggered
- This calls `ShowActionMenu()`, which instantiates the `PotActionMenu` prefab

### Menu Actions
1. **Delete**:
   - Calls `DeletePot()` in `Pot.cs`
   - Frees the zone (if occupied)
   - Destroys the pot and any flower inside it

2. **Replace Sprite**:
   - Opens the `SpriteReplacePanel`
   - Player selects a new sprite from the options
   - Calls `ReplaceSprite()` in `Pot.cs` to update the pot's appearance

3. **Move**:
   - Calls `StartMoving()` in `Pot.cs`
   - Activates the `PotDragManager` to start dragging the pot
   - Shows available zones (via `DropZoneManager`)
   - Player can place the pot in a new zone

---

## Troubleshooting

### Issue: Menu doesn't appear when clicking a pot
- **Solution**: Ensure the pot has:
  - A **Collider2D** component (e.g., BoxCollider2D)
  - The `Pot.cs` script attached
  - The `potActionMenuPrefab` field assigned

### Issue: Buttons don't work
- **Solution**: Ensure:
  - The buttons in the prefab have the `PotActionMenu.cs` script assigned
  - The buttons are assigned to the correct fields in the Inspector
  - The `EventSystem` exists in the scene (required for UI button clicks)

### Issue: Sprite replacement doesn't save
- **Solution**: The `ReplaceSprite()` method in `Pot.cs` calls `currentZone.OnPotDrop(gameObject)` to re-save the pot's state. Ensure your `Zone_for_pot.cs` script is properly saving the sprite name.

---

## Testing

1. **Test Delete**:
   - Click a pot → Select "Delete" → Pot should disappear
   - Check if the zone is freed (can place a new pot there)

2. **Test Replace Sprite**:
   - Click a pot → Select "Replace Sprite" → Choose a new sprite
   - Verify the pot's appearance updates

3. **Test Move**:
   - Click a pot → Select "Move" → Drag the pot to a new zone
   - Verify the pot moves and the old zone is freed

---

## Notes
- The menu is **destroyed** after an action is performed (no lingering menus)
- The **Move** action uses the same dragging logic as buying a new pot
- Sprite replacement **saves automatically** (via `YG2.SaveProgress()`)
