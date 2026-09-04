using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using BetterExperience.BPatchGUI;
using evt;
using m2d;
using nel;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityModBase.HGuiSpace;
using UnityModBase.HotkeyManager;
using XX;

namespace BetterExperience.Patches
{
    /// <summary>
    /// 主线程移动控制。仅持有当前穿墙会话，使用 BE 专属碰撞/重力锁，不写入存档。
    /// 热键由插件 Update 轮询，飞行在玩家物理更新前后执行。
    /// </summary>
    internal static class PlayerMovementController
    {
        private static readonly object NoclipKey = new object();
        private static PR _player;
        private static Map2d _map;
        private static M2Phys _physics;
        private static bool _previousIgnoreWallCheck;
        private static Vector2 _entryPosition;
        private static Vector2 _lastSafePosition;
        private static Vector2 _direction;
        private static bool _initialized;
        private static GuiHostBase[] _guiHosts = Array.Empty<GuiHostBase>();
        private static float _nextGuiRefresh;

        internal static void Initialize()
        {
            if (_initialized)
                return;
            GameSaveLoadManager.OnGameSaveLoadCompleted += CancelForMapChange;
            GameSaveProtectionManager.OnSavingActivated += Stop;
            _initialized = true;
        }

        internal static void Dispose()
        {
            Stop();
            NoticeGUI.Clear(NoclipKey);
            GameSaveLoadManager.OnGameSaveLoadCompleted -= CancelForMapChange;
            GameSaveProtectionManager.OnSavingActivated -= Stop;
            _initialized = false;
            _guiHosts = Array.Empty<GuiHostBase>();
        }

        internal static void Update()
        {
            if (!_initialized)
                return;
            try
            {
                if (ConfigManager.EnableBetterExperience?.Value != true)
                {
                    Stop();
                    return;
                }

                bool toggle = ConfigManager.EnableNoclip?.Value == true &&
                    ConfigManager.ToggleNoclipHotkey?.Value?.WasPressedThisFrame() == true;
                bool teleport = ConfigManager.EnableMouseTeleport?.Value == true &&
                    ConfigManager.MouseTeleportHotkey?.Value?.WasPressedThisFrame() == true;
                if (_physics == null && !toggle && !teleport)
                    return;

                var player = (M2DBase.Instance as NelM2DBase)?.curMap?.getKeyPr() as PR;
                if (_physics != null && !IsCurrentPlayer(player))
                    EndNoclip(false);

                if (!CanControl(player) || !CanUseInput())
                {
                    Stop();
                    return;
                }

                if (_physics != null && (toggle || ConfigManager.EnableNoclip?.Value != true))
                {
                    Stop();
                    return;
                }
                if (toggle && _physics == null)
                    StartNoclip(player);

                _direction = ReadDirection();
                if (teleport)
                    Teleport(player);
            }
            catch (Exception ex)
            {
                Stop();
                BLog.Error("Player movement control failed.", ex);
            }
        }

        private static bool CanControl(PR player)
        {
            return player != null && player.Mp?.M2D != null && player.getPhysic() != null &&
                player.is_alive && player.isNormalState() &&
                Map2d.can_handle && !EV.isActive() && !player.isMoveScriptActive() &&
                !player.Mp.M2D.stopping_game && !player.Mp.M2D.transferring_game_stopping &&
                player.Mp.M2D.curMap == player.Mp && !player.getPhysic().isPausing();
        }

        private static bool IsGuiVisible()
        {
            if (Time.unscaledTime >= _nextGuiRefresh || _guiHosts.Length == 0)
            {
                _guiHosts = UnityEngine.Object.FindObjectsByType<GuiHostBase>(FindObjectsSortMode.None);
                _nextGuiRefresh = Time.unscaledTime + 1f;
            }
            foreach (var host in _guiHosts)
                if (host != null && host.IsVisible)
                    return true;
            return false;
        }

        private static bool CanUseInput() => Application.isFocused && Hotkey.GlobalValid && !IsGuiVisible();

        private static Vector2 ReadDirection()
        {
            var direction = Vector2.zero;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) direction.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) direction.x += 1f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) direction.y -= 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) direction.y += 1f;
            }
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                Vector2 pad = gamepad.dpad.ReadValue();
                if (stick.sqrMagnitude > 0.04f)
                    direction += new Vector2(stick.x, -stick.y);
                direction += new Vector2(pad.x, -pad.y);
            }
            return direction;
        }

        private static void Teleport(PR player)
        {
            var mouse = Mouse.current;
            var camera = IN.getGUICamera();
            if (mouse == null || camera == null || player.Mp.M2D.Cam == null)
                return;

            // 最终地图相机绘制到独立 RenderTexture，其像素尺寸不等于游戏窗口尺寸。
            // 使用当前鼠标采样和屏幕 GUI 相机，再走游戏关卡编辑器使用的地图换算。
            Vector2 screen = mouse.position.ReadValue();
            if (!MovementGeometry.IsOnScreen(screen, Screen.width, Screen.height) ||
                !MovementGeometry.TryScreenToViewport(screen, camera.pixelRect, out var viewport))
                return;

            Vector2 guiPosition = camera.ViewportToWorldPoint(new Vector3(viewport.x, viewport.y, 0f));
            Vector2 target = GuiToMap(player.Mp, guiPosition);
            if (!CanOccupy(player, target))
            {
                NoticeGUI.Show(TranslatorResource.TeleportTargetBlocked, owner: NoclipKey);
                return;
            }
            ClearMotion(player);
            player.setTo(target.x, target.y);
            if (_physics != null)
                _lastSafePosition = target;
        }

        private static Vector2 GuiToMap(Map2d map, Vector2 guiPosition)
        {
            var camera = map.M2D.Cam;
            // 最终画面按 -cam_shift 平移，反算时先补回偏移，再应用镜头缩放和基础倍率。
            Vector2 world = camera.Screen2UPos(guiPosition + new Vector2(camera.cam_shift_x, camera.cam_shift_y));
            return new Vector2(map.globaluxToMapx(world.x), map.globaluyToMapy(world.y));
        }

        internal static bool CanOccupy(PR player, Vector2 target)
        {
            var map = player.Mp;
            if (map?.BCC == null || !map.BCC.active || !MovementGeometry.IsInsideMap(target,
                player.sizex, player.sizey, map.clms, map.rows))
                return false;

            // 留出极小的接触容差，允许脚底贴地；整个身体覆盖的格子都必须可通行。
            float rx = Math.Max(player.sizex - 0.01f, player.sizex * 0.95f);
            float ry = Math.Max(player.sizey - 0.01f, player.sizey * 0.95f);
            float left = target.x - rx, top = target.y - ry;
            float right = target.x + rx, bottom = target.y + ry;
            if (!map.canStandArea((int)Math.Floor(left), (int)Math.Floor(top),
                (int)Math.Ceiling(right), (int)Math.Ceiling(bottom)))
                return false;

            // 只扫描落点内部，不能扫描玩家到落点的路径，否则会禁止跨墙传送。
            if (!map.canThroughBcc(left, target.y, right, target.y, 0f, ry,
                check_other_bcc: true, set_gizmo: false) ||
                !map.canThroughBcc(target.x, top, target.x, bottom, rx, 0f,
                check_other_bcc: true, set_gizmo: false))
                return false;

            // 扫描边界无法识别完全位于大块动态障碍内部的矩形，保守排除其实体边界框。
            for (int i = 0; i < map.count_carryable_bcc; i++)
            {
                var bcc = map.getCarryableBCCByIndex(i);
                if (bcc != null && bcc.active && bcc.isBoundsCovering(left, top, right, bottom, 0f))
                    return false;
            }
            return true;
        }

        private static void StartNoclip(PR player)
        {
            if (!CanOccupy(player, new Vector2(player.x, player.y)))
            {
                NoticeGUI.Show(TranslatorResource.NoclipStartBlocked, owner: NoclipKey);
                return;
            }
            _player = player;
            _map = player.Mp;
            _physics = player.getPhysic();
            _previousIgnoreWallCheck = _physics.ignore_wallcheck_if_lockwallhit;
            _entryPosition = _lastSafePosition = new Vector2(player.x, player.y);
            MaintainLocks();
            ClearMotion(player);
            NoticeGUI.SetStatus(NoclipKey, TranslatorResource.NoclipActive);
            NoticeGUI.Show(TranslatorResource.NoclipEnabled, owner: NoclipKey);
        }

        private static void MaintainLocks()
        {
            _physics.addLockWallHitting(NoclipKey, 120f);
            _physics.addLockGravity(NoclipKey, 0f, 120f);
            _physics.ignore_wallcheck_if_lockwallhit = true;
        }

        private static void ClearMotion(PR player)
        {
            player.jumpRaisingQuit(true);
            player.getFootManager()?.initJump(false, true, false);
            var physics = player.getPhysic();
            physics.walk_xspeed = 0f;
            physics.killSpeedForce(kill_phy_translate_stack: true);
        }

        private static bool IsCurrentPlayer(PR player)
        {
            return player != null && player == _player && player.Mp == _map &&
                player.getPhysic() == _physics;
        }

        internal static Vector2? BeforePhysics(PR player)
        {
            if (_physics == null || player != _player)
                return null;
            try
            {
                if (!IsCurrentPlayer(player))
                {
                    EndNoclip(false);
                    return null;
                }
                if (!CanControl(player) || !CanUseInput() ||
                    ConfigManager.EnableBetterExperience?.Value != true || ConfigManager.EnableNoclip?.Value != true)
                {
                    Stop();
                    return null;
                }
                MaintainLocks();
                ClearMotion(player);
                return new Vector2(player.x, player.y);
            }
            catch (Exception ex)
            {
                Stop();
                BLog.Error("Failed to prepare noclip physics.", ex);
                return null;
            }
        }

        internal static void AfterPhysics(PR player, float frameCount, Vector2? origin)
        {
            if (!origin.HasValue || _physics == null || !IsCurrentPlayer(player))
                return;
            try
            {
                if (!CanControl(player))
                {
                    Stop();
                    return;
                }
                Vector2 target = origin.Value + MovementGeometry.FlightStep(_direction,
                    ConfigManager.NoclipSpeed?.Value ?? 8f, frameCount);
                target.x = Math.Max(player.sizex, Math.Min(_map.clms - player.sizex, target.x));
                target.y = Math.Max(player.sizey, Math.Min(_map.rows - player.sizey, target.y));
                ClearMotion(player);
                player.setTo(target.x, target.y);
                if (CanOccupy(player, target))
                    _lastSafePosition = target;
            }
            catch (Exception ex)
            {
                Stop();
                BLog.Error("Failed to update noclip physics.", ex);
            }
        }

        internal static void Stop() => EndNoclip(true);

        internal static void CancelForMapChange()
        {
            EndNoclip(false);
            NoticeGUI.Clear(NoclipKey);
        }

        private static void EndNoclip(bool restorePosition)
        {
            var player = _player;
            var map = _map;
            var physics = _physics;
            if (physics == null)
                return;
            _player = null;
            _map = null;
            _physics = null;
            _direction = Vector2.zero;
            NoticeGUI.RemoveStatus(NoclipKey);
            if (restorePosition)
                NoticeGUI.Show(TranslatorResource.NoclipDisabled, owner: NoclipKey);
            else
                NoticeGUI.Clear(NoclipKey);
            try
            {
                // 切图、读档或角色被替换后，绝不把旧坐标写入新状态。
                if (restorePosition && player != null && player.Mp == map && map.M2D.curMap == map &&
                    player.getPhysic() == physics && player.is_alive && !player.isMoveScriptActive())
                {
                    Func<Vector2, bool> valid = point => CanOccupy(player, point);
                    if (MovementGeometry.TryFindNearby(new Vector2(player.x, player.y), valid, out var target) ||
                        MovementGeometry.TryFindNearby(_lastSafePosition, valid, out target) ||
                        MovementGeometry.TryFindNearby(_entryPosition, valid, out target))
                    {
                        ClearMotion(player);
                        player.setTo(target.x, target.y);
                    }
                    else
                    {
                        // 地形变化使全部已知落点失效时，交给游戏的默认位置恢复机制。
                        ClearMotion(player);
                        player.setToDefaultPosition();
                        NoticeGUI.Show(TranslatorResource.NoclipDefaultPosition, owner: NoclipKey);
                    }
                }
            }
            catch (Exception ex)
            {
                BLog.Error("Failed to restore the noclip exit position.", ex);
            }
            finally
            {
                try { physics.remLockGravity(NoclipKey); }
                catch (Exception ex) { BLog.Error("Failed to release the BE gravity lock.", ex); }
                try { physics.remLockWallHitting(NoclipKey); }
                catch (Exception ex) { BLog.Error("Failed to release the BE wall lock.", ex); }
                physics.ignore_wallcheck_if_lockwallhit = _previousIgnoreWallCheck;
            }
        }

    }
}
