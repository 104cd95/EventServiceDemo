using System;
using EventServiceDemo.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EventServiceDemo
{
    // Simple UI
    public class PopupController : MonoBehaviour
    {
        private const string COLOR_RED = "#FF0000";
        private const string COLOR_YELLOW = "#FFFF00";
        private const string COLOR_WHITE = "#FFFFFF";
        
        [SerializeField] private TMP_InputField levelInputField;
        [SerializeField] private TMP_InputField rewardBundleInputField;
        [SerializeField] private TMP_InputField coinNumberInputField;
        [SerializeField] private TMP_Text log;
        [SerializeField] private ScrollRect logScrollRect;
        [SerializeField] private RectTransform logVerticalLayout;
        
        [Header("DebugMode Settings")]
        [SerializeField] private Toggle debugModeToggle;
        [SerializeField] private Toggle longRequest;
        [SerializeField] private Toggle failedRequest;

        private void Awake()
        {
            DebugUtility.LogEvent += OnLog;
        }

        void Start()
        {
            debugModeToggle.isOn = EventService.Instance.debugMode;
            longRequest.isOn = EventService.Instance.longRequest;
            failedRequest.isOn = EventService.Instance.failedRequest;
        }

        private void OnDestroy()
        {
            DebugUtility.LogEvent -= OnLog;
        }

        public void OnDebugModeToggleChanged(bool value)
        {
            EventService.Instance.debugMode = value;
        }

        public void OnLongRequestToggleChanged(bool value)
        {
            EventService.Instance.longRequest = value;
        }

        public void OnFailedRequestToggleChanged(bool value)
        {
            EventService.Instance.failedRequest = value;
        }
        
        public void OnStartLevelButtonClicked()
        {
            EventServiceUtility.TrackLevelStart(Convert.ToInt32(levelInputField.text));
        }
        
        public void OnClaimRewardsButtonClicked()
        {
            EventServiceUtility.TrackRewardClaim(rewardBundleInputField.text);
        }
        
        public void OnSpendCoinsButtonClicked()
        {
            EventServiceUtility.TrackCoinsSpending(Convert.ToInt32(coinNumberInputField.text));
        }

        private void OnLog(LogType logType, string message)
        {
            string color = logType switch
            {
                LogType.Error => COLOR_RED,
                LogType.Warning => COLOR_YELLOW,
                _ => COLOR_WHITE
            }; 
            
            log.text += $"<color={color}>{message}</color><br>";
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(logVerticalLayout);
            Canvas.ForceUpdateCanvases();
            logScrollRect.normalizedPosition = new Vector2(0, 0);
        }
    }
}
