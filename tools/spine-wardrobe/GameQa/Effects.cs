using BetterExperience.Patches.ReplaceTexture;
using nel;
using Spine.Unity;
using System;
using System.Collections;
using System.Linq;

namespace Wardrobe.QA
{
    public sealed partial class Plugin
    {
        private IEnumerator CaptureEffects()
        {
            var routine = EffectsCore();
            try
            {
                while (true)
                {
                    bool more;
                    try { more = routine.MoveNext(); }
                    catch (Exception ex)
                    {
                        errors.Add(ex.ToString());
                        Logger.LogError(ex);
                        WriteResult(false, "effects-failed");
                        break;
                    }
                    if (!more) break;
                    yield return routine.Current;
                }
            }
            finally
            {
                (routine as IDisposable)?.Dispose();
                capturing = false;
                request = null;
            }
        }

        private IEnumerator EffectsCore()
        {
            foreach (var pair in isolatedViewers)
            {
                var viewer = pair.Value;
                var manager = viewer.getBetobetoManager();
                var texture = viewer.getSvTexture();
                try
                {
                    foreach (string effect in new[] { "clean", "mud", "washed", "frozen", "stone", "restored" })
                    {
                        manager.cleanAll(false);
                        texture.cleanExecute(true);
                        viewer.clearAnim("stand", -1000, pair.Key == "stand_weak" ? "arm1A" : "default");
                        if (effect == "mud")
                            for (int i = 0; i < 8; i++) manager.Check(BetoInfo.Mud, true, false);
                        if (effect == "frozen") manager.frozen_lv = 3;
                        if (effect == "stone") manager.stone_lv = 3;
                        int attempts = 0;
                        while (!viewer.checkNeedUpdateTexture(true))
                        {
                            if (++attempts > 300) throw new InvalidOperationException("Timed out updating effect texture: " + effect);
                            yield return null;
                        }
                        viewer.updateAnim(true, 0);
                        var animation = Animator.GetValue(viewer) as SkeletonAnimation;
                        var observation = Describe(viewer, animation);
                        var expected = PortraitJson.Array(request["targets"]).Select(PortraitJson.Object)
                            .Single(t => (string)t["target"] == pair.Key);
                        if (!Equals(observation["activePackageId"], expected["packageId"]))
                            throw new InvalidOperationException("Effects captured another active package.");
                        observation["effect"] = effect;
                        observation["frozenLevel"] = (int)manager.frozen_lv;
                        observation["stoneLevel"] = (int)manager.stone_lv;
                        observation["effectDirtCount"] = manager.get_current_dirt();
                        var capture = CaptureCore(viewer, animation, observation, true, true);
                        try { while (capture.MoveNext()) yield return capture.Current; }
                        finally { (capture as IDisposable)?.Dispose(); }
                    }
                }
                finally
                {
                    // Only the separately named QA dirt manager is cleaned.
                    manager.cleanAll(false);
                    texture.cleanExecute(true);
                    viewer.checkNeedUpdateTexture(true);
                }
            }
            WriteResult(false, "effects-captured; visual review required");
        }
    }
}
