# Lineage: Ancestral Legacies - Gameplay Guide

## 🎮 How to Play

### Game Overview
Lineage is a god game/colony simulator where you manage a population of Kaari (pops) as they gather resources, survive, and grow their settlement. You must balance resource management with population growth to thrive.

---

## 🕹️ Controls

### Camera Controls
- **WASD** or **Arrow Keys**: Pan the camera
- **Mouse Scroll**: Zoom in/out
- **Right-Click + Drag**: Pan camera (alternative)

### Selection & Commands
- **Left-Click**: Select a single pop
- **Click + Drag**: Box select multiple pops
- **Shift + Click**: Add/remove from selection
- **Right-Click (with pops selected)**: Command pops to move to location

---

## 📊 Resources

### Primary Resources
1. **Food** 🍎
   - Consumed passively by pops (1.0 per pop per second)
   - Gathered automatically by pops from berry bushes
   - Required for pop survival - pops starve without food!

2. **Faith Points** ✨
   - Generated when pops have their needs met (hunger > 30%, thirst > 30%)
   - Rate: 0.5 faith per pop per second
   - Used for: Improving shelter, researching technologies

3. **Wood** 🪵
   - Future resource for construction
   - Currently not implemented in gathering

---

## 👥 Population Management

### Pop Needs
Each pop has essential needs that must be maintained:
- **Hunger** (0-100%): Decreases over time, restored by eating food
- **Thirst** (0-100%): Decreases over time, restored by drinking water
- **Health**: Pops die when health reaches 0 or needs become critical (<10%)

### Death Conditions
Pops will die if:
- Health reaches 0
- Hunger falls below 10% (critical starvation)
- Thirst falls below 10% (critical dehydration)

---

## 🤖 AI Behavior

Pops have autonomous AI that will:
1. **Seek Food**: When hunger < 50%, pops automatically find and gather from berry bushes
2. **Wander**: When needs are met, pops explore the area
3. **Gather Resources**: Pops spend ~3 seconds gathering food from resource nodes
4. **Rest**: Pops idle when satisfied

You can override AI by selecting pops and right-clicking to command movement.

---

## 🏘️ Settlement Actions

### Menu Buttons (Bottom of Screen)

1. **Spawn Pop** 👶
   - Cost: Free (but must be under population cap)
   - Adds a new pop to your settlement
   - Cannot exceed current population cap

2. **Improve Shelter** 🏠
   - Cost: 10 Faith Points
   - Increases population cap by 1
   - Required to grow your settlement beyond initial cap (5)

3. **Research: Efficient Gathering** 🔬
   - Cost: 15 Faith Points
   - One-time upgrade
   - Increases food gathering rate by 50%
   - Button becomes locked after researched

---

## 🎯 Gameplay Loop

### Early Game (Days 1-10)
1. Your 3 starting pops will automatically gather food from berry bushes
2. Monitor food levels - ensure you have enough to feed your population
3. Build up faith points (requires pops to have needs met)
4. Use faith to research Efficient Gathering first (15 faith)
5. Improve shelter once to increase population cap (10 faith)

### Mid Game (Days 11-30)
1. Spawn additional pops as you can afford to feed them
2. Continue improving shelter to increase cap
3. Balance population growth with food production
4. Experiment with commanding pops to specific locations

### Advanced Strategy
- **Faith Generation**: Keep pops well-fed (>30% hunger) to maximize faith generation
- **Food Management**: Each pop consumes 1 food/second - ensure gathering keeps pace
- **Population Control**: Don't spawn more pops than you can feed!
- **Resource Nodes**: Berry bushes regenerate at 5 food/second - manage carefully

---

## ⚙️ Game Speed Controls

- **Pause**: Pause/unpause the game
- **Speed 1x**: Normal speed
- **Speed 2x**: Double speed
- **Speed 3x**: Triple speed

Time progresses with day/night cycles and seasons.

---

## 🐛 Known Issues

1. **Visual Scripting Errors**: Some UI components use Visual Scripting and may show coroutine warnings - these are non-critical
2. **ButtonGroupManager Errors**: Legacy UI code - doesn't affect new functionality
3. **NavMesh Warnings**: Pops may occasionally report NavMesh warnings if spawned off-mesh - they auto-recover

---

## 💡 Tips & Tricks

1. **Right-click commands** override AI temporarily - useful for positioning pops
2. **Berry bushes regenerate** - don't worry if they get depleted temporarily
3. **Faith generation is key** - well-fed pops = steady faith income
4. **Start slow** - Don't spawn too many pops at once or you'll starve them
5. **Watch the hunger bars** on pops to know when food is running low
6. **Selection indicator** shows which pops are currently selected

---

## 📈 Victory Conditions

Current build is a sandbox - focus on:
- Achieving a stable, self-sustaining population
- Reaching maximum population cap through shelter improvements
- Unlocking all research technologies
- Surviving through multiple seasons

---

## 🔧 Technical Details

### New Systems Implemented
- **PopAIBehavior**: Autonomous resource gathering AI
- **ResourceNode**: Harvestable berry bushes with regeneration
- **UIManager**: Dynamic resource display system
- **MenuButtonHandler**: Interactive menu buttons
- **Enhanced PopulationManager**: Food consumption and needs processing
- **Improved EntityDataComponent**: Stat management and needs tracking

### Components Added to Scene
- Pop GameObject: PopAIBehavior component
- Berry Bushes: ResourceNode component + "ResourceNode" tag
- UIManager: UIManager component
- Managers: MenuButtonHandler component

---

**Enjoy building your ancestral legacy!** 🏛️
