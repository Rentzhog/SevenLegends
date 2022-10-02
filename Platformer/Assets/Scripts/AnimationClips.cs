using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationClips : MonoBehaviour, IAnimationClipSource 
{
    public List<AnimationClip> clips;

    public void GetAnimationClips(List<AnimationClip> result) {
        result.AddRange(clips);
    }
}
