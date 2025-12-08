# Lineage Project Reorganization Script
# Date: December 5, 2025
# Purpose: Reorganize Assets folder for better maintainability

# IMPORTANT: Run this from project root directory
# Create backup first: Copy-Item "Assets" "Assets_Backup_$(Get-Date -Format 'yyyy-MM-dd-HHmm')" -Recurse

Write-Host "=== Lineage Project Reorganization ===" -ForegroundColor Cyan
Write-Host ""

# Check if we're in the correct directory
if (-not (Test-Path "Assets")) {
    Write-Host "ERROR: Assets folder not found. Run this from project root!" -ForegroundColor Red
    exit
}

Write-Host "Step 1: Creating new folder structure..." -ForegroundColor Yellow

# Create main _Project folder
$folders = @(
    "Assets/_Project",
    "Assets/_Project/Art",
    "Assets/_Project/Art/Sprites",
    "Assets/_Project/Art/Sprites/Characters",
    "Assets/_Project/Art/Sprites/Environment",
    "Assets/_Project/Art/Sprites/UI",
    "Assets/_Project/Art/Sprites/Icons",
    "Assets/_Project/Art/Materials",
    "Assets/_Project/Art/Textures",
    
    "Assets/_Project/Audio",
    "Assets/_Project/Audio/Music",
    "Assets/_Project/Audio/SFX",
    "Assets/_Project/Audio/Ambient",
    
    "Assets/_Project/Data",
    "Assets/_Project/Data/Entities",
    "Assets/_Project/Data/Entities/Pops",
    "Assets/_Project/Data/Entities/Buildings",
    "Assets/_Project/Data/Entities/Resources",
    "Assets/_Project/Data/GameDatabase",
    "Assets/_Project/Data/AI",
    "Assets/_Project/Data/Configuration",
    
    "Assets/_Project/Prefabs",
    "Assets/_Project/Prefabs/Characters",
    "Assets/_Project/Prefabs/Characters/Pops",
    "Assets/_Project/Prefabs/Environment",
    "Assets/_Project/Prefabs/UI",
    "Assets/_Project/Prefabs/UI/Panels",
    "Assets/_Project/Prefabs/UI/Buttons",
    "Assets/_Project/Prefabs/UI/HUD",
    "Assets/_Project/Prefabs/Systems",
    
    "Assets/_Project/Scenes",
    "Assets/_Project/Scenes/Game",
    "Assets/_Project/Scenes/Menus",
    "Assets/_Project/Scenes/Testing",
    
    "Assets/_Project/Scripts",
    "Assets/_Project/Scripts/Core",
    "Assets/_Project/Scripts/Core/Managers",
    "Assets/_Project/Scripts/Core/Systems",
    "Assets/_Project/Scripts/Core/Systems/SaveSystem",
    "Assets/_Project/Scripts/Core/Systems/InventorySystem",
    "Assets/_Project/Scripts/Core/Systems/QuestSystem",
    "Assets/_Project/Scripts/Core/Utilities",
    
    "Assets/_Project/Scripts/Entities",
    "Assets/_Project/Scripts/Entities/Pop",
    "Assets/_Project/Scripts/Entities/Pop/AI",
    "Assets/_Project/Scripts/Entities/Buildings",
    "Assets/_Project/Scripts/Entities/Resources",
    
    "Assets/_Project/Scripts/UI",
    "Assets/_Project/Scripts/UI/Core",
    "Assets/_Project/Scripts/UI/Panels",
    "Assets/_Project/Scripts/UI/Buttons",
    "Assets/_Project/Scripts/UI/HUD",
    
    "Assets/_Project/Scripts/Gameplay",
    "Assets/_Project/Scripts/Gameplay/Selection",
    "Assets/_Project/Scripts/Gameplay/Construction",
    "Assets/_Project/Scripts/Gameplay/Miracles",
    
    "Assets/_Project/Scripts/AI",
    "Assets/_Project/Scripts/AI/States",
    "Assets/_Project/Scripts/AI/Decisions",
    "Assets/_Project/Scripts/AI/Actions",
    
    "Assets/_Project/Scripts/Database",
    "Assets/_Project/Scripts/Database/Core",
    "Assets/_Project/Scripts/Database/Stats",
    "Assets/_Project/Scripts/Database/Items",
    
    "Assets/_Project/Scripts/Editor",
    "Assets/_Project/Scripts/Editor/Tools",
    "Assets/_Project/Scripts/Editor/Inspectors",
    "Assets/_Project/Scripts/Editor/Windows",
    
    "Assets/Plugins",
    "Assets/Plugins/ModernUIPack"
)

foreach ($folder in $folders) {
    if (-not (Test-Path $folder)) {
        New-Item -ItemType Directory -Path $folder -Force | Out-Null
        Write-Host "  Created: $folder" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Step 2: Moving Modern UI Pack to Plugins..." -ForegroundColor Yellow

if (Test-Path "Assets/Modern UI Pack") {
    Move-Item "Assets/Modern UI Pack" "Assets/Plugins/ModernUIPack" -Force
    Write-Host "  Moved: Modern UI Pack -> Plugins/ModernUIPack" -ForegroundColor Green
}

Write-Host ""
Write-Host "Step 3: Organizing Scripts..." -ForegroundColor Yellow

# Move Managers
if (Test-Path "Assets/Scripts/Managers") {
    Move-Item "Assets/Scripts/Managers/*" "Assets/_Project/Scripts/Core/Managers/" -Force
    Write-Host "  Moved: Scripts/Managers -> _Project/Scripts/Core/Managers" -ForegroundColor Green
}

# Move Entities
if (Test-Path "Assets/Scripts/Entities") {
    Move-Item "Assets/Scripts/Entities/*" "Assets/_Project/Scripts/Entities/" -Force
    Write-Host "  Moved: Scripts/Entities -> _Project/Scripts/Entities" -ForegroundColor Green
}

# Move UI Scripts
if (Test-Path "Assets/UI/Scripts") {
    Move-Item "Assets/UI/Scripts/*" "Assets/_Project/Scripts/UI/" -Force
    Write-Host "  Moved: UI/Scripts -> _Project/Scripts/UI" -ForegroundColor Green
}

# Move Database Scripts
if (Test-Path "Assets/Scripts/GameDatabase") {
    Move-Item "Assets/Scripts/GameDatabase/*" "Assets/_Project/Scripts/Database/" -Force
    Write-Host "  Moved: Scripts/GameDatabase -> _Project/Scripts/Database" -ForegroundColor Green
}

# Move Debug Scripts to Editor
if (Test-Path "Assets/Scripts/Debug") {
    Move-Item "Assets/Scripts/Debug/*" "Assets/_Project/Scripts/Editor/Tools/" -Force
    Write-Host "  Moved: Scripts/Debug -> _Project/Scripts/Editor/Tools" -ForegroundColor Green
}

# Move Systems
if (Test-Path "Assets/Scripts/Systems") {
    Move-Item "Assets/Scripts/Systems/*" "Assets/_Project/Scripts/Core/Systems/" -Force
    Write-Host "  Moved: Scripts/Systems -> _Project/Scripts/Core/Systems" -ForegroundColor Green
}

# Move Components
if (Test-Path "Assets/Scripts/Components") {
    Move-Item "Assets/Scripts/Components/*" "Assets/_Project/Scripts/Core/Utilities/" -Force
    Write-Host "  Moved: Scripts/Components -> _Project/Scripts/Core/Utilities" -ForegroundColor Green
}

Write-Host ""
Write-Host "Step 4: Organizing Assets..." -ForegroundColor Yellow

# Move Sprites
if (Test-Path "Assets/Sprites") {
    Move-Item "Assets/Sprites/*" "Assets/_Project/Art/Sprites/" -Force
    Write-Host "  Moved: Sprites -> _Project/Art/Sprites" -ForegroundColor Green
}

# Move Prefabs
if (Test-Path "Assets/Prefabs") {
    Move-Item "Assets/Prefabs/*" "Assets/_Project/Prefabs/" -Force
    Write-Host "  Moved: Prefabs -> _Project/Prefabs" -ForegroundColor Green
}

# Move Entity Prefabs
if (Test-Path "Assets/Entities") {
    Move-Item "Assets/Entities/*" "Assets/_Project/Prefabs/Characters/" -Force
    Write-Host "  Moved: Entities -> _Project/Prefabs/Characters" -ForegroundColor Green
}

# Move Data
if (Test-Path "Assets/Data") {
    Move-Item "Assets/Data/*" "Assets/_Project/Data/" -Force
    Write-Host "  Moved: Data -> _Project/Data" -ForegroundColor Green
}

# Move Scenes
if (Test-Path "Assets/Scenes") {
    Move-Item "Assets/Scenes/*" "Assets/_Project/Scenes/Game/" -Force
    Write-Host "  Moved: Scenes -> _Project/Scenes/Game" -ForegroundColor Green
}

# Move SFX
if (Test-Path "Assets/SFX") {
    Move-Item "Assets/SFX/*" "Assets/_Project/Audio/SFX/" -Force
    Write-Host "  Moved: SFX -> _Project/Audio/SFX" -ForegroundColor Green
}

Write-Host ""
Write-Host "Step 5: Cleaning up empty folders..." -ForegroundColor Yellow

$emptyFolders = @(
    "Assets/Scripts/Managers",
    "Assets/Scripts/Entities",
    "Assets/Scripts/GameDatabase",
    "Assets/Scripts/Debug",
    "Assets/Scripts/Systems",
    "Assets/Scripts/Components",
    "Assets/UI/Scripts",
    "Assets/Sprites",
    "Assets/Prefabs",
    "Assets/Entities",
    "Assets/Data",
    "Assets/Scenes",
    "Assets/SFX"
)

foreach ($folder in $emptyFolders) {
    if (Test-Path $folder) {
        $items = Get-ChildItem $folder -Recurse
        if ($items.Count -eq 0) {
            Remove-Item $folder -Force -Recurse
            Write-Host "  Removed empty: $folder" -ForegroundColor Gray
        }
    }
}

Write-Host ""
Write-Host "Step 6: Creating meta files..." -ForegroundColor Yellow
Write-Host "  (Unity will auto-generate .meta files on next import)" -ForegroundColor Gray

Write-Host ""
Write-Host "=== Reorganization Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "NEXT STEPS:" -ForegroundColor Cyan
Write-Host "1. Open Unity project" -ForegroundColor White
Write-Host "2. Let Unity reimport assets (may take 5-10 minutes)" -ForegroundColor White
Write-Host "3. Check for broken references in prefabs/scenes" -ForegroundColor White
Write-Host "4. Run 'Assets > Reimport All' if needed" -ForegroundColor White
Write-Host "5. Test scene loading and prefab instantiation" -ForegroundColor White
Write-Host ""
Write-Host "If issues occur, restore from backup:" -ForegroundColor Yellow
Write-Host "  Remove-Item 'Assets' -Recurse -Force" -ForegroundColor Gray
Write-Host "  Copy-Item 'Assets_Backup_*' 'Assets' -Recurse" -ForegroundColor Gray
Write-Host ""
