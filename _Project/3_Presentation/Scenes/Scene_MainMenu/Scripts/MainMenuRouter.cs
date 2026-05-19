using System;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuRouter
{
    public enum ShowStyle
    {
        FromLeft,
        FromRight,
        FromTop,
        FromBottom,
        Instant
    }

    public enum HideStyle
    {
        ToLeft,
        ToRight,
        ToTop,
        ToBottom,
        Instant
    }

    private Dictionary<Type, SsBaseMenu> _menus = new Dictionary<Type, SsBaseMenu>();
    private SsBaseMenu _activeMenu;

    public int MenuCount => _menus.Count;

    public void RegisterMenu(SsBaseMenu menu)
    {
        var type = menu.GetType();
        if (!_menus.ContainsKey(type))
        {
            _menus.Add(type, menu);
        }
    }

    // UPDATED: Added Enum parameters with defaults
    public void TransitionTo<T>(ShowStyle showStyle = ShowStyle.Instant, HideStyle hideStyle = HideStyle.Instant) where T : SsBaseMenu
    {
        var targetType = typeof(T);

        if (!_menus.TryGetValue(targetType, out var nextMenu))
        {
            Debug.LogError($"[Router] Transition Failed! {targetType.Name} not found in dictionary.");
            return;
        }

        if (_activeMenu == nextMenu)
        {
            Debug.LogWarning($"[Router] Already looking at {targetType.Name}. Ignoring.");
            return;
        }

        // 1. Hide the current menu using the requested animation
        if (_activeMenu != null)
        {
            ApplyHideAnimation(_activeMenu, hideStyle);
        }

        // 2. Show the new menu using the requested animation
        ApplyShowAnimation(nextMenu, showStyle);

        // 3. Update state
        _activeMenu = nextMenu;
    }

    // --- HELPER SWITCH STATEMENTS ---

    private void ApplyHideAnimation(SsBaseMenu menu, HideStyle style)
    {
        switch (style)
        {
            case HideStyle.ToLeft: menu.HideToLeft(); break;
            case HideStyle.ToRight: menu.HideToRight(); break;
            case HideStyle.ToTop: menu.HideToTop(); break;
            case HideStyle.ToBottom: menu.HideToBottom(); break;
            case HideStyle.Instant: menu.HideImmediate(); break;
        }
    }

    private void ApplyShowAnimation(SsBaseMenu menu, ShowStyle style)
    {
        switch (style)
        {
            case ShowStyle.FromLeft: menu.ShowFromLeft(); break;
            case ShowStyle.FromRight: menu.ShowFromRight(); break;
            case ShowStyle.FromTop: menu.ShowFromTop(); break;
            case ShowStyle.FromBottom: menu.ShowFromBottom(); break;
            case ShowStyle.Instant: menu.JustShow(); break;
        }
    }

    // Optional: Update SetInitialMenu to use the new Instant method
    public void SetInitialMenu<T>() where T : SsBaseMenu
    {
        var targetType = typeof(T);
        if (_menus.TryGetValue(targetType, out var initialMenu))
        {
            foreach (var menu in _menus.Values)
            {
                if (menu != initialMenu) menu.HideImmediate();
            }
            _activeMenu = initialMenu;
            _activeMenu.JustShow();
        }
    }

    public MainMenuRouter()
    {
        Debug.Log($"[Router] A NEW Router instance has been created! HashCode: {this.GetHashCode()}");
    }
}





















