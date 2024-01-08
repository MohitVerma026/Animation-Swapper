using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

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
                Debug.LogError("Something is wrong. Please assign all the values in the field");
                return;
            }

            var animiationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GetAssetPath(originalFBXFile));
            var newClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GetAssetPath(newAnimationFBXFile));
            AnimatorState state = new AnimatorState();
            int totalStateMachines = 0;

            for (int layerIndex = 0; layerIndex < animatorController.layers.Length; layerIndex++)
            {
                AnimatorControllerLayer layer = animatorController.layers[layerIndex];

                if (layer.stateMachine != null)
                {

                    if (layer.stateMachine.states.Any(s => s.state.motion == animiationClip))
                        animatorController.SetStateEffectiveMotion(layer.stateMachine.states.First(s => s.state.motion == animiationClip).state, newClip);

                    totalStateMachines += CountStateMachines(layer.stateMachine);

                }

            }
            Debug.Log("Done");
            int CountStateMachines(AnimatorStateMachine stateMachine)
            {
                int count = 1;

                foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
                {
                    for (int i = 0; i < childStateMachine.stateMachine.states.Length; i++)
                    {
                        if (childStateMachine.stateMachine.states[i].state.motion == animiationClip)
                            childStateMachine.stateMachine.states[i].state.motion = newClip;
                    }
                    count += CountStateMachines(childStateMachine.stateMachine);
                }
                return count;
            }

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