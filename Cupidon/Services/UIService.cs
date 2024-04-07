using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace Cupidon.Services
{
    internal class UIToggle
    {
        public GameObject ToggleGO { get; set; } = null!;
        public LocalizeStringEvent LocalizeString { get; set; } = null!;
        public Toggle UnityToggle { get; set; } = null!;
    }

    internal class UIText
    {
        public GameObject TextGO { get; set; } = null!;
        public LocalizeStringEvent LocalizeString { get; set; } = null!;
        public TextMeshProUGUI TMPText { get; set; } = null!;
    }

    internal class UIService
    {
        public static UIService Instance
        {
            get
            {
                _instance ??= new UIService();
                return _instance;
            }
        }

        private UIService() { }
        private static UIService? _instance;

        public UIToggle AddToggleToGameSettings(string localizationEntry, UnityAction<bool> onValueChanged)
        {
            var toggle = CreateGameSettingsToggleClone() ?? throw new Exception("Could not create Toggle element");
            toggle.LocalizeString.SetEntry(localizationEntry);
            toggle.UnityToggle.onValueChanged.AddListener(onValueChanged);

            return toggle;
        }

        public UIText AddTextToMainUI(string localizationEntry, Color? textColor = null)
        {
            var uiText = CreateGameTextClone() ?? throw new Exception("Could not create Text element");
            uiText.LocalizeString.SetEntry(localizationEntry);
            uiText.TMPText.color = textColor ?? Color.white;

            return uiText;
        }

        private UIToggle? CreateGameSettingsToggleClone()
        {
            var orig = GameManager.Instance.gameUI.gameSettingsMenu
                .transform.Find("LayoutGroup/Body/TaskPanel/Holder/LayoutGroup/ShowAllySetting");

            if (orig == null)
            {
                Log.Error("Could not get a Toggle copy to duplicate!");
                return null;
            }

            var clone = UnityEngine.Object.Instantiate(orig, orig.parent);
            var textToggle = clone.Find("LayoutGroup/SettingNameText")?.GetComponent<LocalizeStringEvent>();
            var uiToggle = clone.Find("LayoutGroup/ToggleContainer/Toggle")?.GetComponent<Toggle>();

            if (textToggle == null || uiToggle == null)
            {
                Log.Error("Copied object does not contains required text or toggle");
                UnityEngine.Object.Destroy(clone.gameObject);
                return null;
            }

            // Remove default behaviour from copy
            for (int i = 0; i < uiToggle.onValueChanged.GetPersistentEventCount(); i++)
            {
                uiToggle.onValueChanged.SetPersistentListenerState(i, UnityEventCallState.Off);
            }

            // Uncheck the option when creating it
            uiToggle.SetIsOnWithoutNotify(false);

            return new UIToggle
            {
                ToggleGO = clone.gameObject,
                LocalizeString = textToggle,
                UnityToggle = uiToggle,
            };
        }

        private UIText? CreateGameTextClone()
        {
            var orig = GameManager.Instance.gameUI.transform.Find("Canvas/Game/Role");

            if (orig == null)
            {
                Log.Error("Could not get a Text copy to duplicate!");
                return null;
            }

            var clone = UnityEngine.Object.Instantiate(orig, orig.parent);
            var localizedString = clone.GetComponent<LocalizeStringEvent>();
            var renderText = clone.GetComponent<TextMeshProUGUI>();

            if (localizedString == null || renderText == null)
            {
                Log.Error("Copied object does not contains required text or localized string");
                UnityEngine.Object.Destroy(clone.gameObject);
                return null;
            }

            return new UIText
            {
                TextGO = clone.gameObject,
                LocalizeString = localizedString,
                TMPText = renderText,
            };
        }
    }
}