using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace AnimiationSwapper
{
    public class AnimationSwaper : EditorWindow
    {
        #region Variables
        private Object originalFBXFile;
        private AnimatorController animatorController;
        private Object newAnimationFBXFile;
        #endregion

        #region Tab View

        [MenuItem("Window/Animation Swap")]
        public static void ShowWindow() => GetWindow(typeof(AnimationSwaper), false, "Animation Swapper");

        #endregion

        #region Editor layout
        private void OnGUI()
        {
            originalFBXFile = EditorGUILayout.ObjectField("Original Animation File", originalFBXFile, typeof(Object), false);
            animatorController = EditorGUILayout.ObjectField("Main AnimatorController", animatorController, typeof(AnimatorController), false) as AnimatorController;
            newAnimationFBXFile = EditorGUILayout.ObjectField("New Animation File", newAnimationFBXFile, typeof(Object), false);


            if (GUILayout.Button("Check & Swap Animations"))
            {
                CheckAnimatorController();
                return;
            }

            if (GUILayout.Button("Check Animation Count"))
            {
                CheckAnimatorForAnimation();
                return;
            }
        }
        #endregion

        private void CheckAnimatorController()
        {
            if (animatorController is null || originalFBXFile is null || newAnimationFBXFile is null)
            {
                Debug.LogError("Something is wrong. Please assign all the valus in the field");
                return;
            }

            var newClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GetAssetPath(newAnimationFBXFile));
            int count = 0;
            var animiationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GetAssetPath(originalFBXFile));

            for (int i = 0; i < animatorController.animationClips.Length; i++)
            {
                if (animatorController.animationClips[i] == animiationClip)
                {
                    count++;

                    // Get the state machine, layer, and state information for the matched animation clip.
                    AnimatorStateMachine stateMachine = null;
                    AnimatorControllerLayer layer = null;
                    AnimatorState state = null;

                    // Iterate through layers of the AnimatorController.
                    for (int layerIndex = 0; layerIndex < animatorController.layers.Length; layerIndex++)
                    {
                        layer = animatorController.layers[layerIndex];

                        if (animatorController.layers[layerIndex].stateMachine.states.Any(s => s.state.motion == animiationClip))
                        {
                            state = animatorController.layers[layerIndex].stateMachine.states.First(s => s.state.motion == animiationClip).state;
                            animatorController.SetStateEffectiveMotion(state, newClip);
                            break;
                        }
                        else
                        {
                            // Iterate through state machines in the layer.
                            for (int machineIndex = 0; machineIndex < layer.stateMachine.stateMachines.Length; machineIndex++)
                            {
                                stateMachine = layer.stateMachine.stateMachines[machineIndex].stateMachine;

                                // Check if the animation clip is present in the state machine.
                                if (stateMachine.states.Any(s => s.state.motion == animiationClip))
                                {
                                    // Find the state that uses the animation clip.
                                    state = stateMachine.states.First(s => s.state.motion == animiationClip).state;
                                    animatorController.SetStateEffectiveMotion(state, newClip);
                                    break;
                                }
                            }
                        }
                        // Break if the state has been found in any layer.
                        if (state != null)
                            break;
                    }
                }
            }

            Debug.Log($"Number of animation in the animator is {count}");
            Debug.Log($"Total animations in this animator is {animatorController.animationClips.Length}");

        }

        private void CheckAnimatorForAnimation()
        {
            if (animatorController is null || originalFBXFile is null)
            {
                Debug.LogError("Something is wrong. Please assign all the valus in the field");
                return;
            }

            int count = 0;
            var animiationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GetAssetPath(originalFBXFile));

            for (int i = 0; i < animatorController.animationClips.Length; i++)
            {
                if (animatorController.animationClips[i] == animiationClip)
                    count++;
            }
            Debug.Log($"Number of animation in the animator is {count}");
            Debug.Log($"Total animations in this animator is {animatorController.animationClips.Length}");

        }
    }
}