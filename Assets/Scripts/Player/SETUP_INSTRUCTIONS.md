# Sled Controller Setup Instructions

This guide will help you set up the sled character controller in Unity Editor.

## Part 1: Create the Sled GameObject

1. **Create a new GameObject** in your scene:
   - Right-click in Hierarchy → Create Empty
   - Name it `SledPlayer`
   - Position it at your desired spawn point (or let LevelGenerator handle positioning)

2. **Add the required components**:
   - Add Component → Physics → **Rigidbody**
   - Add Component → Scripts → **SledController**
   - Add Component → Scripts → **GroundDetector**

## Part 2: Configure the Rigidbody

The Rigidbody should be automatically configured by SledController, but verify these settings:

- **Mass**: 50
- **Drag**: 0.5
- **Angular Drag**: 2
- **Use Gravity**: ✓ Checked
- **Interpolation**: Interpolate
- **Collision Detection**: Continuous Dynamic
- **Constraints**:
  - Freeze Position: None
  - Freeze Rotation: **X and Z checked** (only Y rotation allowed)

## Part 3: Add Sphere Colliders (Triangle Formation)

Add **three sphere colliders** to create a realistic sled contact pattern:

### Front Sphere Collider
1. Add Component → Physics → Sphere Collider
2. Configure:
   - **Radius**: 0.3
   - **Center**: X=0, Y=0.3, Z=0.8
   - **Material**: (Optional - for friction tuning)
   - **Is Trigger**: Unchecked

### Rear Left Sphere Collider
1. Add Component → Physics → Sphere Collider
2. Configure:
   - **Radius**: 0.3
   - **Center**: X=-0.4, Y=0.3, Z=-0.6
   - **Material**: (Optional)
   - **Is Trigger**: Unchecked

### Rear Right Sphere Collider
1. Add Component → Physics → Sphere Collider
2. Configure:
   - **Radius**: 0.3
   - **Center**: X=0.4, Y=0.3, Z=0.6
   - **Material**: (Optional)
   - **Is Trigger**: Unchecked

### Visual Guide (Top View)
```
        Front (0.8)
           ●
          /|\
         / | \
        /  |  \
       /   |   \
      ●————+————●
   Left    |   Right
 (-0.4)  Center (0.4)
         (-0.6)
```

## Part 4: Add Trigger Detector (For Chunk Progression)

1. **Create a child GameObject**:
   - Right-click on SledPlayer → Create Empty
   - Name it `TriggerDetector`

2. **Add components to TriggerDetector**:
   - Add Component → Physics → Box Collider
   - Add Component → Scripts → **PlayerTrigger**

3. **Configure the Box Collider**:
   - **Is Trigger**: ✓ Checked
   - **Size**: X=1, Y=2, Z=2 (adjust based on your chunk trigger zones)
   - **Center**: X=0, Y=0, Z=0

4. **Configure PlayerTrigger events** (if needed):
   - In the Inspector, you'll see `OnTriggerEntered` Unity Event
   - Connect this to your MapChunk progression logic if needed

## Part 5: Configure SledController Parameters

In the SledController component, adjust these starting values:

### Movement
- **Acceleration Power**: 15
- **Max Speed**: 40
- **Brake Strength**: 2

### Steering
- **Steer Strength**: 40
- **Max Angular Velocity**: 100

### Speed-Dependent Steering
- **Min Steer Factor**: 0.5 (steering at max speed = 50% of base)
- **Speed For Full Reduction**: 20 (speed when reduction starts)

### Keel Mechanic (The Important Part!)
- **Lateral Resistance Threshold**: 15 (degrees of slope before drifting)
- **Max Resistance Force**: 25 (how hard to resist sliding)
- **Drift Coefficient**: 0.4 (damping when drifting)
- **Keel Transition Range**: 5 (smoothing degrees around threshold)

### Physics
- **Mass**: 50
- **Drag**: 0.5
- **Angular Drag**: 2

### Debug
- **Show Debug Gizmos**: ✓ Checked (for visual feedback)
- **Show Debug UI**: ✓ Checked (for on-screen stats in Editor)

## Part 6: Configure GroundDetector Parameters

In the GroundDetector component:

- **Raycast Distance**: 1.5 (how far down to check for ground)
- **Ray Origin Height**: 0.2 (offset above center)
- **Ray Spacing**: 0.5 (distance between raycast points)
- **Ground Layer Mask**: Everything (or create a "Ground" layer)
- **Show Debug Rays**: ✓ Checked (shows raycasts in Scene view)

## Part 7: Connect to LevelGenerator

1. Open your scene and locate the **LevelGenerator** GameObject
2. In the LevelGenerator component:
   - Drag your **SledPlayer** GameObject into the `Player Transform` field
3. The SledController will auto-register with LevelGenerator on Start()

## Part 8: Optional - Add Visual Mesh

If you want a visible sled model:

1. Create a child GameObject under SledPlayer named `Visual`
2. Add your 3D model/mesh to this child
3. Position it so the model aligns with the sphere colliders
4. Keep the visual separate from physics for easier tuning

**Tip**: You can add visual lean to the mesh in SledController's Update() based on steering input:
```csharp
visualTransform.localRotation = Quaternion.Euler(0, 0, -horizontalInput * 10f);
```

## Part 9: Testing

1. **Enter Play Mode**
2. **Controls** (New Input System):
   - **W/S or Up/Down Arrow**: Accelerate / Brake
   - **A/D or Left/Right Arrow**: Steer
   - **Gamepad Left Stick**: Also supported if connected
3. **Check Debug Visualization**:
   - Green line: Ground normal
   - Yellow line: Velocity
   - Blue/Red line: Lateral velocity (blue = resisting, red = drifting)
   - Green/Red sphere above sled: Keel state indicator
4. **Watch the Scene view** to see all Gizmos

## Part 10: Tuning Tips

### If the sled feels too sluggish:
- Increase `Acceleration Power` (try 20-25)
- Decrease `Drag` (try 0.3)

### If the sled feels too twitchy:
- Decrease `Steer Strength` (try 30)
- Increase `Angular Drag` (try 3-4)

### If the sled drifts too easily:
- Increase `Lateral Resistance Threshold` (try 20)
- Increase `Max Resistance Force` (try 30-35)

### If the sled is too sticky on slopes:
- Decrease `Max Resistance Force` (try 20)
- Decrease `Drift Coefficient` (try 0.3)

### If high-speed movement is unstable:
- Increase `Drag` with speed-dependent formula
- Lower `Max Speed` (try 30-35)

## Part 11: Create a Prefab (Optional but Recommended)

1. Drag `SledPlayer` from Hierarchy into `Assets/Prefabs/` folder
2. This creates a reusable prefab
3. Any changes to the prefab will apply to all instances

## Troubleshooting

### Sled falls through terrain:
- Check terrain has a Mesh Collider (convex if needed)
- Verify Rigidbody collision detection is "Continuous Dynamic"
- Increase Rigidbody mass if terrain is too lightweight

### Sled doesn't move:
- Check SledController is enabled
- Ensure Rigidbody "Use Gravity" is checked
- Verify keyboard input is being detected (uses New Input System)

### Raycast not detecting ground:
- Check Ground Layer Mask in GroundDetector
- Increase Raycast Distance if terrain is far below
- Enable "Show Debug Rays" to visualize raycasts

### Keel mechanic not working:
- Verify GroundDetector is calculating lateral slope (check debug UI)
- Adjust threshold values
- Test on steeper terrain

---

## Summary

You now have a fully functional sled controller with:
- ✅ Physics-based movement
- ✅ Speed-dependent steering
- ✅ Keel mechanic (lateral resistance → drift)
- ✅ Multi-point ground detection
- ✅ Debug visualization
- ✅ Integration with chunk-based level system

Test on various slopes and tune the parameters until it feels just right!
