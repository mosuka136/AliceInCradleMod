using BetterExperience.HProvider;
using UnityEngine;

namespace BetterExperience.HGuiSpace
{
    public interface IStyleResource
    {
        IUnityGuiProvider UnityGui { get; }
        GUIStyle ToastStyle { get; }
        GUIStyle TooltipStyle { get; }
    }
}
