﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace levelListExtension.UI
{
    internal class selectUI
    {
        static public selectUI instance = new selectUI();

        public void Create(LevelSelectionNavigationController resultsView)
        {
            if (root != null) return;
            BSMLParser.Instance.Parse(
                Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), $"levelListExtension.UI.selectUI.bsml"),
                resultsView.gameObject, instance
            );

            resultsView.didActivateEvent += ResultsView_didActivateEvent;
            resultsView.didDeactivateEvent += ResultsView_didDeactivateEvent;

            root.localPosition = new Vector3(5,23F,0);
            root.name = "selectButton";

            setDiffName();

            if (Settings.Configuration.Instance.refresh)
            {
                Plugin.GetSongStatsBl(Settings.Configuration.Instance.count, statusText);
                Plugin.GetSongStats(Settings.Configuration.Instance.count, statusText);
                Settings.Configuration.Instance.refresh = false;
            }
        }

        private void setDiffName()
        {
            string result = "";
            switch (Settings.Configuration.Instance.selectDiff)
            {
                case 0:
                    result = "Easy";
                    break;
                case 1:
                    result = "Normal";
                    break;
                case 2:
                    result = "Hard";
                    break;
                case 3:
                    result = "Ex";
                    break;
                case 4:
                    result = "Ex+";
                    break;
            }
            selectButton.text = result;
        }
        private void ResultsView_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            root.gameObject.SetActive(true);
        }
        private void ResultsView_didDeactivateEvent(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            root.gameObject.SetActive(false);
        }

        [UIAction("onClick")]
        protected async Task onClick()
        {
            Settings.Configuration.Instance.selectDiff += 1;
            if (Settings.Configuration.Instance.selectDiff > 4) Settings.Configuration.Instance.selectDiff = 0;

            setDiffName();
        }

        [UIComponent("root")]
        protected RectTransform root = null;
        [UIComponent("statusTextGrid")]
        protected RectTransform grid = null;
        [UIComponent("selectButton")]
        protected TextMeshProUGUI selectButton = null;
        [UIComponent("statusText")]
        protected TextMeshProUGUI statusText = null;
    }
}
