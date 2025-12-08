# Unity Pro Tools + MCP Server Setup Guide

## Your Configuration

You're using the **optimal setup** for VS Code + Unity:

1. **Unity Pro Tools** - Direct connection to Unity Editor
2. **Unity MCP Server** - Model Context Protocol for AI assistance
3. **OmniSharp** - C# language analysis

This gives you **real-time error reporting** from the Unity Compiler.

---

## How to Get Errors in VS Code

### Step 1: Ensure Unity Pro Tools is Connected

1. In **Unity Editor**: Window → General → Editor Logs
2. Check for message: `"VS Code connection established"`
3. In **VS Code**: Should see "Unity" in Activity Bar (left sidebar)

### Step 2: View Problems Panel

```
View → Problems
OR
Ctrl+Shift+M
```

You should see compilation errors from Unity here.

### Step 3: Verify MCP Server Connection

```
Ctrl+Shift+P → "MCP: Show Server Status"
```

Should show **Unity MCP** as **Connected**

---

## Why Errors Are Now Working

**Unity Pro Tools connects directly to the Unity Compiler**, so:

✅ **Compile errors appear immediately** when you save  
✅ **No delay** - real-time feedback  
✅ **All Unity-specific errors** are captured  
✅ **IntelliSense includes** Unity APIs  

---

## Optimized Settings (Already Applied)

Your `settings.json` now has:

```jsonc
"unity.automaticallyOpenUnityProblemsPanel": true,
"unity.buildOutputVerbosity": "Minimal",
"unity.compileErrorsVisibility": "All",
"unity.enableCodeLens": true,
"unity.showUnityLogs": true,
```

**What this does:**
- ✅ Auto-opens Problems panel when errors occur
- ✅ Shows all compilation errors
- ✅ Enables code lens (references above functions)
- ✅ Shows Unity Editor logs in VS Code

---

## Quick Test

1. **Open any .cs file** with an error
2. **Save the file** (Ctrl+S)
3. **View → Problems** (Ctrl+Shift+M)
4. **Error should appear** with filename, line, and message

---

## Troubleshooting

### Errors Not Showing?

1. **Is Unity Editor still open?**
   - Unity Pro Tools needs the Editor running
   - Check in taskbar

2. **Is VS Code connected to Unity?**
   ```
   Ctrl+Shift+P → "Unity: Show Editor Logs"
   ```
   Look for connection messages

3. **Check Activity Bar**
   - Click "Unity" icon in left sidebar
   - Should show connection status

### MCP Server Issues

```
Ctrl+Shift+P → "MCP: Show Server Status"
→ Should show "unity" with green checkmark
```

If not connected:
1. Restart VS Code
2. Close and reopen Unity Editor
3. Run: `Ctrl+Shift+P → "MCP: Restart Server"`

---

## Pro Tips

### Real-Time Error Checking
- Errors update as you type (if auto-save is on)
- Or save to trigger recompile
- Watch Problems panel for live updates

### Jump to Error
```
Click error in Problems panel → Jump directly to code location
```

### Quick Fix
```
Hover over error → Click lightbulb → Select fix
```

### Filter Errors
```
In Problems panel:
- Click filter icon
- Show errors/warnings/info as needed
```

---

## Daily Workflow

1. **Code** in VS Code
2. **Save** (Ctrl+S)
3. **View Problems** (Ctrl+Shift+M)
4. **Unity Editor** compiles automatically
5. **Play mode** - test in Editor

---

## Extensions to Keep Installed

✅ **Required:**
- Unity Tools (Unity Technologies)
- C# Dev Kit (Microsoft)

✅ **Optional but helpful:**
- Pylance
- Better Comments
- Code Spell Checker

Remove if causing issues:
- Other C# extensions (conflicts with C# Dev Kit)
- OmniSharp (C# Dev Kit includes it)

---

## Contact Unity Pro Tools Support

If errors still don't appear:
1. Extensions → Unity Tools → Click Settings gear
2. Check: **"Unity Project"** is set correctly
3. Verify: **Editor path** points to your Unity installation

---

## Summary

You have **the best setup** for seeing Unity errors in VS Code:

✅ Real-time compilation errors  
✅ Direct Editor connection  
✅ MCP integration for AI assistance  
✅ Full IntelliSense for Unity APIs  

**Errors should appear automatically in Problems panel whenever you save.**

