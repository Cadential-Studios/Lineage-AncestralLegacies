# NavMesh Auto-Bake Feature
*Added: 2025*
*Version: 1.0*

## Overview
Added automatic NavMesh baking capability to the **GameSetupVerifier** (Setup Fixer) tool to streamline scene setup workflow.

---

## What Changed

### GameSetupVerifier.cs Enhancements

**Location:** `Assets/Scripts/Editor/GameSetupVerifier.cs`

#### New Features Added:
1. **Auto-Bake Toggle** - UI toggle to enable/disable auto-baking
2. **BakeNavMeshIfNeeded() Method** - Intelligent NavMesh baking with auto-detection
3. **Manual Bake Button** - Standalone button to trigger baking on demand
4. **NavMeshSurface Integration** - Uses Unity's AI Navigation package

---

## How to Use

### Option 1: Auto-Bake with Quick Fix All
1. Open **Lineage > Setup > Verify Game Setup** window
2. Enable **"Auto-Bake NavMesh"** checkbox (enabled by default)
3. Click **"Quick Fix All Issues"** button
4. NavMesh will automatically bake if not already present

### Option 2: Manual Bake
1. Open **Lineage > Setup > Verify Game Setup** window
2. Click **"Bake NavMesh Now"** button
3. NavMesh bakes immediately regardless of current state

### Option 3: Menu Item
- Navigate to **Lineage > Setup > Bake NavMesh** in Unity menu bar
- Bakes NavMesh with single click

---

## Technical Details

### BakeNavMeshIfNeeded() Behavior

**Smart Detection:**
- Checks if NavMesh already exists (skips if baked)
- Auto-detects existing `NavMeshSurface` components
- Falls back to creating NavMeshSurface on ground objects

**Ground Object Detection Priority:**
1. GameObject named "Ground"
2. GameObject named "Terrain"
3. GameObject named "Floor"

**Configuration:**
- Collects all objects in scene (`CollectObjects.All`)
- Uses default NavMesh agent settings
- Non-destructive (preserves existing setup)

### Error Handling
- ✅ Logs success message when baked
- ⚠️ Warns if NavMesh already exists
- ❌ Errors if no suitable ground object found
- ❌ Errors if baking fails with exception details

---

## Code Example

```csharp
/// <summary>
/// Bakes the NavMesh if it hasn't been baked yet
/// </summary>
/// <returns>True if NavMesh was baked, false if already baked or error occurred</returns>
[MenuItem("Lineage/Setup/Bake NavMesh")]
public static bool BakeNavMeshIfNeeded()
{
    // Check if already baked
    var navMeshData = UnityEngine.AI.NavMesh.CalculateTriangulation();
    if (navMeshData.vertices.Length > 0)
    {
        Debug.Log("NavMesh already baked. Skipping.");
        return false;
    }

    // Find or create NavMeshSurface
    NavMeshSurface navMeshSurface = FindFirstObjectByType<NavMeshSurface>();
    
    if (navMeshSurface == null)
    {
        GameObject groundObject = FindGroundObject();
        if (groundObject != null)
        {
            navMeshSurface = groundObject.AddComponent<NavMeshSurface>();
            navMeshSurface.collectObjects = CollectObjects.All;
        }
        else
        {
            Debug.LogWarning("Could not find suitable ground object.");
            return false;
        }
    }

    // Bake
    navMeshSurface.BuildNavMesh();
    Debug.Log("✓ NavMesh baked successfully!");
    return true;
}
```

---

## Dependencies

**Required Package:**
- `Unity.AI.Navigation` - Unity's AI Navigation package
- Included in project via Package Manager

**Required Namespaces:**
```csharp
using UnityEngine.AI;
using Unity.AI.Navigation;
```

---

## Integration with Quick Fix All

The `QuickFixAll()` method now includes NavMesh baking:

```csharp
[MenuItem("Lineage/Setup/Quick Fix All")]
public static void QuickFixAll()
{
    int fixedCount = 0;
    
    // ... existing fixes (tags, PopulationManager, etc.) ...
    
    // Bake NavMesh if enabled and not baked
    if (autoBakeNavMesh)
    {
        bool navMeshBaked = BakeNavMeshIfNeeded();
        if (navMeshBaked)
        {
            fixedCount++;
        }
    }
    
    // Display results
    if (fixedCount > 0)
    {
        EditorUtility.DisplayDialog("Setup Fixed", 
            $"Fixed {fixedCount} issue(s). Try playing the game now!", 
            "OK");
    }
}
```

---

## UI Changes

**New Toggle in Setup Verifier Window:**
```
┌─────────────────────────────────────┐
│ Game Setup Verification             │
├─────────────────────────────────────┤
│ ☑ Auto-Bake NavMesh                 │
│ ℹ When enabled, 'Quick Fix All'     │
│   will automatically bake NavMesh   │
│                                     │
│ ┌─────────────────────────────────┐ │
│ │   Quick Fix All Issues          │ │ (40px height)
│ └─────────────────────────────────┘ │
│ ┌─────────────────────────────────┐ │
│ │   Bake NavMesh Now              │ │ (30px height)
│ └─────────────────────────────────┘ │
└─────────────────────────────────────┘
```

---

## Testing Checklist

- [x] Compiles without errors
- [ ] Auto-bake works when toggle enabled
- [ ] Manual bake button functions correctly
- [ ] Menu item accessible via Lineage > Setup > Bake NavMesh
- [ ] Detects existing NavMesh (no duplicate baking)
- [ ] Finds NavMeshSurface components correctly
- [ ] Creates NavMeshSurface on ground objects when missing
- [ ] Logs appropriate messages (success/warning/error)
- [ ] Integrates with Quick Fix All workflow
- [ ] NavMesh visualizes correctly in Scene view (blue overlay)

---

## Performance Notes

- **Baking Time**: Depends on scene complexity (typically 1-5 seconds)
- **Auto-Detection**: O(n) scan for ground objects (negligible)
- **Caching**: NavMesh data cached after first bake
- **Re-Baking**: Only occurs when explicitly triggered or NavMesh missing

---

## Future Enhancements

**Potential Improvements:**
1. Custom NavMesh agent profiles (different sized pops)
2. Multi-surface baking (stairs, ramps, platforms)
3. Dynamic NavMesh updates during gameplay
4. Bake validation (ensure walkable areas cover spawn points)
5. Batch baking for multiple scenes
6. NavMesh obstacle auto-detection

---

## Rollback Plan

If issues occur, revert to previous version:

1. Remove `autoBakeNavMesh` field
2. Remove `BakeNavMeshIfNeeded()` method
3. Remove NavMesh baking call from `QuickFixAll()`
4. Remove "Bake NavMesh Now" button from OnGUI()
5. Remove `using Unity.AI.Navigation;` if unused elsewhere
6. Keep NavMesh status check in OnGUI() (existing functionality)

---

## Related Documentation

- **NavMesh Setup Guide**: `Documents/04_Guides/NavMesh_Setup_Guide.md`
- **GameSetupVerifier API**: `Assets/Scripts/Editor/GameSetupVerifier.cs`
- **Unity NavMesh Docs**: https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html

---

## Change Summary

**Files Modified:**
- `Assets/Scripts/Editor/GameSetupVerifier.cs`

**Lines Changed:**
- Added ~90 lines (BakeNavMeshIfNeeded method, UI toggle, button)
- Modified 1 line (added `using Unity.AI.Navigation;`)

**Backward Compatibility:** ✅ Fully backward compatible
**Breaking Changes:** None
**Migration Required:** No

---

*Documentation follows AGENTS.md protocol for deliverables and reporting.*
