# VS Code - Unity Compilation Errors Integration

## Why aren't compilation errors showing in VS Code?

There are several reasons Unity errors might not appear in VS Code's Problems panel:

---

## Issue Checklist

### 1. **Missing C# Extension** ❌ → ✅
VS Code needs proper C# support.

**Required Extensions:**
- `ms-dotnettools.csharp` (C# Dev Kit)
- `ms-dotnettools.vscode-dotnet-runtime` (Dotnet Runtime)

**How to check:**
1. Open Extensions (Ctrl+Shift+X)
2. Search for "C# Dev Kit"
3. If not installed → Install it
4. Reload VS Code (Ctrl+K, Ctrl+R)

---

### 2. **Solution Not Associated** ❌ → ✅
VS Code doesn't know which solution to analyze.

**How to fix:**
1. Settings → `dotnet.defaultSolution`
2. Set to: `Lineage Ancestral Legacies (Unity).slnx`
3. (Already configured in your settings.json)

---

### 3. **OmniSharp Not Analyzing** ❌ → ✅
The language server isn't running.

**How to fix:**
1. Press `Ctrl+Shift+P`
2. Type: `OmniSharp: Restart OmniSharp`
3. Wait for "OmniSharp initialized" message
4. Check Output panel → "OmniSharp" tab

---

### 4. **Assembly Definitions Misconfigured** ❌ → ✅
VS Code can't resolve assembly references.

**Check:**
1. Do all your .asmdef files exist?
   - `LineageScripts.asmdef`
   - `Lineage.Core.Utilities.asmdef`
   - `Tests.asmdef`
   - etc.

2. Are they syntactically valid?
   ```json
   {
     "name": "LineageScripts",
     "references": ["Lineage.Core.Utilities"],
     "includePlatforms": [],
     "excludePlatforms": []
   }
   ```

---

### 5. **Project References Missing** ❌ → ✅
The .csproj files don't reference all necessary projects.

**Solution:**
Let Visual Studio regenerate them:
1. In Unity: Assets → Open C# Project
2. This regenerates all .csproj files
3. Back in VS Code: Restart OmniSharp

---

## Troubleshooting Steps (In Order)

### Step 1: Ensure C# Extension Installed
```
Extensions → Search "C#" → Install "C# Dev Kit"
```

### Step 2: Restart OmniSharp
```
Ctrl+Shift+P → "OmniSharp: Restart OmniSharp"
```

### Step 3: Check Output Panel
```
View → Output → Select "OmniSharp" dropdown
Look for errors like:
  - "Project loading failed"
  - "Could not find project"
  - "Unresolved assembly"
```

### Step 4: Verify Solution File
```
In explorer, right-click .slnx file
→ "Generate Solution" from context menu
(if available)
```

### Step 5: Manual Build Test
```
Ctrl+Shift+P → "Tasks: Run Build Task"
→ "dotnet: build solution"
```

This forces a compile and shows errors directly.

---

## Expected Behavior

### When Configured Correctly:

1. **C# file opens** → OmniSharp analyzes it
2. **Error found** → Red squiggly under code
3. **Problems panel updates** → Shows line/column/message
4. **Hover error** → Shows full error message
5. **Quick fixes available** → Lightbulb appears

### Example Error Display:
```
Assets/_Project/Scripts/Core/Systems/Movement/SimpleMovementController.cs (45,10)
error CS0103: The name 'UnityEngine' does not exist in the current context
```

---

## Quick Diagnostics Script

Run this to identify issues:

```powershell
# Check if C# extension is installed
code --list-extensions | findstr /i "csharp"

# Check if .slnx exists
ls "Lineage Ancestral Legacies (Unity).slnx"

# Check if LineageScripts.csproj exists
ls LineageScripts.csproj

# Verify dotnet is installed
dotnet --version
```

---

## Advanced: Force Full Rescan

If errors still don't show:

1. **Close VS Code**
2. **Delete OmniSharp cache:**
   ```powershell
   rm -r $env:USERPROFILE\.omnisharp\
   ```
3. **Delete .vs folder:**
   ```powershell
   rm -r ".vs" -ErrorAction SilentlyContinue
   ```
4. **Reopen VS Code**
5. **Wait for OmniSharp initialization** (watch Output panel)

---

## Verify Integration with Unity

### In Unity Editor:
1. **Window → Code Editor → Preferences**
2. Check: **External Script Editor** = Visual Studio Code
3. Check: **Generate .csproj files** = ✓ Enabled

### In VS Code:
1. **Open folder** = Project root
2. **Solution open** = Lineage Ancestral Legacies (Unity).slnx
3. **OmniSharp running** = Check Output panel

---

## Common Error Messages & Fixes

| Error | Cause | Fix |
|-------|-------|-----|
| `"Project loading failed"` | .csproj syntax error | Regenerate from Unity |
| `"Unresolved assembly 'UnityEngine'"` | Missing Unity references | Close VS Code, open in Unity |
| `"Could not find project 'LineageScripts'"` | Wrong working directory | Open project root folder |
| `"OmniSharp is not running"` | Extension not activated | Restart OmniSharp (Ctrl+Shift+P) |

---

## Validate Your Setup

✅ **All should be true:**

- [ ] C# Dev Kit extension installed
- [ ] OmniSharp running (Output panel shows "initialized")
- [ ] Project root open in VS Code
- [ ] Lineage Ancestral Legacies (Unity).slnx visible in explorer
- [ ] Errors appear with red squiggles in editor
- [ ] Problems panel shows issues
- [ ] Hover over error shows tooltip
- [ ] Lightbulb appears for quick fixes

If any are false, follow troubleshooting steps above.

---

## Still Not Working?

1. **Check editor logs:**
   - View → Toggle Developer Tools
   - Console tab for errors

2. **Check OmniSharp logs:**
   - Output panel → OmniSharp dropdown
   - Look for initialization errors

3. **Verify dotnet SDK:**
   ```powershell
   dotnet --version
   dotnet --list-sdks
   ```

4. **Verify solution validity:**
   ```powershell
   dotnet build "Lineage Ancestral Legacies (Unity).slnx"
   ```

---

## Next Steps

Once errors are showing:

1. **View → Problems** → See all errors
2. **Ctrl+Shift+M** → Toggle Problems panel
3. Click error → Jump to code location
4. Hover → See full message
5. Select all → Copy for documentation

