using BetterExperience.Patches.ReplaceTexture;
using nel;
using System;
using System.Collections.Generic;
using UnityEngine;
using XX;
using Object = UnityEngine.Object;

namespace Wardrobe.QA
{
    public sealed partial class Plugin
    {
        private readonly Dictionary<string, SpineViewerNel> isolatedViewers = new Dictionary<string, SpineViewerNel>();
        private readonly List<Object> isolatedObjects = new List<Object>();

        private bool EnsureIsolatedViewers()
        {
            // MtrSpineDefault exists on the title screen before Nel shaders do.
            // Check NEL first: MTR.preparedG itself may advance resource loading.
            if (!NEL.loaded || !MTR.preparedG || MTRX.MtrSpineDefault == null) return false;
            foreach (var value in PortraitJson.Array(request["targets"]))
            {
                string target = (string)PortraitJson.Object(value)["target"];
                if (target != "stand_normal" && target != "stand_weak")
                    throw new ArgumentException("Isolated v1 QA supports the two wardrobe targets only.");
                if (isolatedViewers.ContainsKey(target)) continue;
                var holder = new GameObject("WardrobeQA-" + target);
                Object.DontDestroyOnLoad(holder);
                // Keep source viewers outside the capture camera's layer.
                holder.layer = 30;
                holder.transform.position = new Vector3(100000, 100000, 0);
                var material = new Material(MTRX.MtrSpineDefault);
                material.name = "WardrobeQA-" + target;
                isolatedObjects.Add(holder);
                isolatedObjects.Add(material);
                // A separate dirt manager and viewer avoid changing the player's
                // current pose, dirt state or save. Assets still pass through the
                // real game's viewer, texture manager and installed loader hooks.
                var viewer = new SpineViewerNel(null, target, "wardrobe-qa");
                viewer.initGameObject(holder);
                viewer.getSvTexture().initialize_load = true;
                viewer.initializeLoad(material);
                viewer.clearAnim("stand", -1000, target == "stand_weak" ? "arm1A" : "default");
                viewer.updateAnim(true, 0);
                isolatedViewers.Add(target, viewer);
                Register(viewer);
            }
            return true;
        }

        private void ReleaseIsolatedViewers()
        {
            foreach (var viewer in isolatedViewers.Values)
            {
                // Unity may destroy scene objects before this plugin at exit.
                var animation = Animator.GetValue(viewer) as Spine.Unity.SkeletonAnimation;
                if (animation != null && animation.GetComponent<MeshRenderer>() != null)
                {
                    viewer.deactivate();
                    viewer.destruct();
                }
            }
            isolatedViewers.Clear();
            foreach (var value in isolatedObjects)
                if (value != null) Object.Destroy(value);
            isolatedObjects.Clear();
        }
    }
}
