using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Bettr.Editor.generators
{
    public static class BettrAnimatorController
    {
        public static AnimatorController CreateOrLoadAnimatorController(string fileName, List<AnimationState> animationStates, List<AnimatorTransition> animationTransitions, string runtimeAssetPath)
        {
            var animatorControllerName = $"{fileName}_anims.controller";
            var animatorControllerPath = $"{runtimeAssetPath}/Animators/{animatorControllerName}";
            
            var animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(animatorControllerPath);
            if (animatorController != null)
            {
                return animatorController;
            }
            
            animatorController = AnimatorController.CreateAnimatorControllerAtPath(animatorControllerPath);
            
            var stateMachine = animatorController.layers[0].stateMachine;
            AssetDatabase.SaveAssets();

            var frameRate = 60;
            
            foreach (var animationState in animationStates)
            {
                var animationClip = new AnimationClip
                {
                    name = animationState.Name,
                    wrapMode = animationState.IsLoop ? WrapMode.Loop : WrapMode.Once,
                    frameRate = frameRate,
                };
                
                var dopesheets = animationState.Dopesheet;
                if (dopesheets != null)
                {
                    foreach (var dopesheet in dopesheets)
                    {
                        AnimationCurve curve = new AnimationCurve
                        {
                            keys = dopesheet.Keyframes.Times.Select((t, i) => new Keyframe(t/frameRate, dopesheet.Keyframes.Values[i])).ToArray()
                        };
                        for (int i = 0; i < curve.length; i++)
                        {
                            Keyframe key = curve[i];
                            key.inTangent = 0;  // Set to desired inTangent
                            key.outTangent = 0; // Set to desired outTangent
                            curve.MoveKey(i, key);
                        }

                        // Automatically smooth the curve's tangents
                        for (int i = 0; i < curve.length; i++)
                        {
                            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                        }
                        switch (dopesheet.Type)
                        {
                            case "GameObject":
                                AnimationUtility.SetEditorCurve(animationClip, EditorCurveBinding.FloatCurve(dopesheet.Path, typeof(GameObject), dopesheet.Property), curve);
                                break;
                            case "Transform":
                                AnimationUtility.SetEditorCurve(animationClip, EditorCurveBinding.FloatCurve(dopesheet.Path, typeof(Transform), dopesheet.Property), curve);
                                break;
                            case "MeshRenderer":
                                AnimationUtility.SetEditorCurve(animationClip, EditorCurveBinding.FloatCurve(dopesheet.Path, typeof(MeshRenderer), dopesheet.Property), curve);
                                break;
                        }
                    }
                }
                
                var animationStateName = animationClip.name;
                var state = stateMachine.AddState(animationStateName);
                state.speed = animationState.Speed;
                state.motion = animationClip;
                if (animationClip.wrapMode == WrapMode.Loop)
                {
                    SerializedObject serializedClip = new SerializedObject(animationClip);
                    var properties = serializedClip.GetIterator();
                    while (properties.NextVisible(true))
                    {
                        if (properties.name == "m_AnimationClipSettings")
                        {
                            properties.FindPropertyRelative("m_LoopTime").boolValue = true;
                            serializedClip.ApplyModifiedProperties();
                        }
                    }
                }
                if (stateMachine.defaultState == null && animationState.IsDefault)
                {
                    stateMachine.defaultState = state;
                }

                if (animationStateName != "noop")
                {
                    var animationClipName = $"{fileName}_anim_{animationStateName}.anim";
                    var animationClipPath = $"{runtimeAssetPath}/Animators/{animationClipName}";
                    AssetDatabase.CreateAsset(animationClip, animationClipPath);
                }
                AssetDatabase.SaveAssets();
            }
            
            AssetDatabase.SaveAssets();

            foreach (var animationStateTransition in animationTransitions)
            {
                var stateTransition = animationStateTransition;
                var transitionFrom = stateMachine.states.FirstOrDefault(s => s.state.name == stateTransition.TransitionFrom);
                var transitionTo = stateMachine.states.FirstOrDefault(s => s.state.name == animationStateTransition.TransitionTo);
                var transition = transitionFrom.state.AddTransition(transitionTo.state);
                if (animationStateTransition.TransitionDuration > 0)
                {
                    transition.duration = animationStateTransition.TransitionDuration;
                }
            }
            
            AssetDatabase.SaveAssets();

            var runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(animatorControllerPath);

            return runtimeAnimatorController;
        }
        
        public static AnimatorController AddAnimationState(string fileName, List<AnimationState> animationStates, List<AnimatorTransition> animationTransitions, string runtimeAssetPath)
        {
            var animatorControllerName = $"{fileName}_anims.controller";
            var animatorControllerPath = $"{runtimeAssetPath}/Animators/{animatorControllerName}";
            
            var animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(animatorControllerPath);
            if (animatorController == null)
            {
                throw new Exception($"Animator controller {fileName} not found.");
            }
            
            var stateMachine = animatorController.layers[0].stateMachine;
            AssetDatabase.SaveAssets();

            var frameRate = 60;
            
            foreach (var animationState in animationStates)
            {
                var animationClip = new AnimationClip
                {
                    name = animationState.Name,
                    wrapMode = animationState.IsLoop ? WrapMode.Loop : WrapMode.Once,
                    frameRate = frameRate,
                };
                
                var dopesheets = animationState.Dopesheet;
                if (dopesheets != null)
                {
                    foreach (var dopesheet in dopesheets)
                    {
                        AnimationCurve curve = new AnimationCurve
                        {
                            keys = dopesheet.Keyframes.Times.Select((t, i) => new Keyframe(t/frameRate, dopesheet.Keyframes.Values[i])).ToArray()
                        };
                        for (int i = 0; i < curve.length; i++)
                        {
                            Keyframe key = curve[i];
                            key.inTangent = 0;  // Set to desired inTangent
                            key.outTangent = 0; // Set to desired outTangent
                            curve.MoveKey(i, key);
                        }

                        // Automatically smooth the curve's tangents
                        for (int i = 0; i < curve.length; i++)
                        {
                            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                        }
                        switch (dopesheet.Type)
                        {
                            case "GameObject":
                                AnimationUtility.SetEditorCurve(animationClip, EditorCurveBinding.FloatCurve(dopesheet.Path, typeof(GameObject), dopesheet.Property), curve);
                                break;
                            case "Transform":
                                AnimationUtility.SetEditorCurve(animationClip, EditorCurveBinding.FloatCurve(dopesheet.Path, typeof(Transform), dopesheet.Property), curve);
                                break;
                            case "MeshRenderer":
                                AnimationUtility.SetEditorCurve(animationClip, EditorCurveBinding.FloatCurve(dopesheet.Path, typeof(MeshRenderer), dopesheet.Property), curve);
                                break;
                        }
                    }
                }
                
                var animationStateName = animationClip.name;
                var state = stateMachine.AddState(animationStateName);
                state.speed = animationState.Speed;
                state.motion = animationClip;
                if (animationClip.wrapMode == WrapMode.Loop)
                {
                    SerializedObject serializedClip = new SerializedObject(animationClip);
                    var properties = serializedClip.GetIterator();
                    while (properties.NextVisible(true))
                    {
                        if (properties.name == "m_AnimationClipSettings")
                        {
                            properties.FindPropertyRelative("m_LoopTime").boolValue = true;
                            serializedClip.ApplyModifiedProperties();
                        }
                    }
                }
                if (stateMachine.defaultState == null && animationState.IsDefault)
                {
                    stateMachine.defaultState = state;
                }

                if (animationStateName != "noop")
                {
                    var animationClipName = $"{fileName}_anim_{animationStateName}.anim";
                    var animationClipPath = $"{runtimeAssetPath}/Animators/{animationClipName}";
                    AssetDatabase.CreateAsset(animationClip, animationClipPath);
                }
                AssetDatabase.SaveAssets();
            }
            
            AssetDatabase.SaveAssets();

            foreach (var animationStateTransition in animationTransitions)
            {
                var stateTransition = animationStateTransition;
                var transitionFrom = stateMachine.states.FirstOrDefault(s => s.state.name == stateTransition.TransitionFrom);
                var transitionTo = stateMachine.states.FirstOrDefault(s => s.state.name == animationStateTransition.TransitionTo);
                var transition = transitionFrom.state.AddTransition(transitionTo.state);
                if (animationStateTransition.TransitionDuration > 0)
                {
                    transition.duration = animationStateTransition.TransitionDuration;
                }
            }
            
            AssetDatabase.SaveAssets();

            var runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(animatorControllerPath);

            return runtimeAnimatorController;
        }
    }
}