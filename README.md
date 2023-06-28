# ACTSkill
ACTSkill editor.

## Feature
- Visual editing attack/body range.
- Timeline to set action.
- Support undo redo.

## Editor Window
![image](Screenshots~/EditorWindow.png)

## Usage
- Click "ACTSkill/Skill Editor" menu item to open editor window.
- Add state, edit state values, and edit frame count.
- Select frame to edit attack/body range.
- Add action.
    - Inherit `ActionBase` class to define action config class.
      Must add `[Serializable]` attribute to config class.
    - Implement `IActionHandler` interface to handle action logic.
- Implement `IMachineController` interface to drive machine.
  Some lifecycle events can be listened from `MachineBehaviour` and `StateHandler`.

## Animation
You can inherit `AnimationProcessor` to handle animation during editing.
Default use Animator to play animation.

## Version Support
Unity 2019_3_OR_NEWER

## Dependencies
- "com.mr-sb.customizationinspector": "https://github.com/Mr-sB/CustomizationInspector.git",
- "com.unity.editorcoroutines": "1.0.0"

## Demo
https://github.com/Mr-sB/ACTSkillDemo