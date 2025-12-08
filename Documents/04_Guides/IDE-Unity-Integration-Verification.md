# IDE & Unity Integration Verification Checklist

## VS Code Integration Status

### ✅ Debugger Configuration
- **Launch Type**: `coreclr` (correct for C# debugging)
- **Deprecated Fields**: Removed `engineLogging`, `sourceLanguages`
- **Attach Configurations**: 
  - Attach to Unity Editor
  - Attach to Unity (Localhost)
  - Debug Unity Game

**Test Steps:**
1. Open Unity Editor (play mode optional)
2. In VS Code: F5 → "Attach to Unity Editor"
3. Select Unity process from list
4. Set breakpoint in any C# script
5. Trigger code execution in Unity
6. Verify breakpoint hits and variables display

### ✅ Problems Panel Integration
- **Task Matchers**: Configured for C# compilation errors
- **Unity Pro Tools**: Active extension for error surfacing
- **MCP Server**: Unity MCP integration enabled

**Test Steps:**
1. Introduce syntax error in any `.cs` file (e.g., remove semicolon)
2. Save file
3. Check VS Code Problems panel (Ctrl+Shift+M)
4. Verify error appears with file path and line number
5. Fix error and verify it clears from Problems panel

### ✅ Unity Pro Tools + MCP
- **Extension**: Unity Tools (ms-dotnettools.csdevkit-unity)
- **Settings**: `.vscode/settings.json` optimized for Unity Pro Tools
- **Solution Format**: `.slnx` (Unity 6 XML format)

**Test Steps:**
1. Right-click in Unity Project window → "Regenerate project files"
2. In VS Code: Reload window (Ctrl+Shift+P → "Reload Window")
3. Verify IntelliSense works for Unity APIs (type `UnityEngine.`)
4. Verify "Go to Definition" (F12) works for Unity components

---

## Unity Console Integration Status

### ✅ Runtime Logging Migration
- **Old System**: `AdvancedLogger` (Editor-only, causes runtime errors)
- **New System**: `Lineage.Debug.Log` (runtime-safe)
- **Migration Status**: Complete for all runtime assemblies

**Files Migrated:**
- `Pop.cs` ✅
- `PopController_Refactored.cs` ✅
- `GridPathfinder.cs` ✅
- `SimpleMovementController.cs` ✅
- `PopAIBehavior_Refactored.cs` ✅

**Editor Tools** (still use AdvancedLogger - correct):
- `DebugManager.cs`
- `ProfilingIntegration.cs`
- `RuntimeObjectInspector.cs`

**Test Steps:**
1. Enter Play mode in Unity
2. Trigger Pop movement (select Pop, right-click to move)
3. Verify no `AdvancedLogger` errors in Console
4. Check for movement-related log messages (Info/Warning)
5. Exit Play mode - verify clean shutdown

### ✅ Error-Free Compilation
- **Runtime Assemblies**: No Editor dependencies
- **Assembly Definitions**: Proper separation maintained
- **Namespace Hygiene**: `Lineage.Debug` accessible to runtime

**Test Steps:**
1. In Unity: Assets → Reimport All
2. Wait for full compilation
3. Check Console - should be error-free
4. Open Test Runner (Window → General → Test Runner)
5. Run PlayMode tests - verify all pass

---

## Known Issues & Resolutions

### Issue: "AdvancedLogger does not exist in current context"
**Resolution**: Already fixed - runtime code now uses `Log` API instead.

### Issue: Debugger not attaching
**Resolution**: 
1. Ensure Unity Editor is running
2. Use `coreclr` type (not `vstuc`)
3. Select correct Unity process from picker
4. Check firewall isn't blocking debugger port

### Issue: Problems panel empty despite Unity errors
**Resolution**:
1. Verify Unity Pro Tools extension enabled
2. Check `.vscode/settings.json` has `dotnet.unitySolution` enabled
3. Regenerate project files from Unity
4. Reload VS Code window

---

## Performance Baselines (To Be Measured)

### Target Metrics
- **50 Pops (SimpleMovementController)**: Target <16ms frame time
- **50 Pops (GridPathfinder)**: Target <20ms frame time
- **100 Pops (SimpleMovementController)**: Target <25ms frame time
- **100 Pops (GridPathfinder)**: Target <35ms frame time

**Measurement Steps:**
1. Create scene with specified Pop count
2. Enable Unity Profiler (Window → Analysis → Profiler)
3. Enter Play mode and let Pops wander for 60 seconds
4. Note average CPU frame time and movement system overhead
5. Document in `Documents/02_Tasks/PerformanceBaselines.md`

---

## Quick Diagnostic Commands

### VS Code Terminal
```powershell
# Check for AdvancedLogger in runtime assemblies
Get-ChildItem -Path "Assets\_Project\Scripts" -Recurse -Filter *.cs | Select-String "AdvancedLogger" | Select-Object Path, LineNumber

# Check for AdvancedLogger in Logic assemblies
Get-ChildItem -Path "Assets\Logic" -Recurse -Filter *.cs | Select-String "AdvancedLogger" | Select-Object Path, LineNumber
```

### Unity Console Filters
- **Errors Only**: Click "Error" button in Console toolbar
- **Warnings Only**: Click "Warning" button
- **Collapse Duplicates**: Click "Collapse" button
- **Clear on Play**: Console menu → "Clear on Play"

---

## Sign-Off Checklist

- [ ] VS Code debugger attaches to Unity successfully
- [ ] Breakpoints hit and variables inspect correctly
- [ ] Problems panel surfaces Unity compilation errors
- [ ] Unity Console shows no AdvancedLogger errors in Play mode
- [ ] All PlayMode tests pass in Test Runner
- [ ] Runtime debug overlay toggles with F3 (if enabled)
- [ ] Log system uses configurable LogConfig asset
- [ ] CI workflow executes PlayMode tests on push

**Date Verified**: _____________  
**Verified By**: _____________
