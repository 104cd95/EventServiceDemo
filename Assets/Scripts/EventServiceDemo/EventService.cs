using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using EventServiceDemo.Plugins;
using EventServiceDemo.Utility;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace EventServiceDemo
{
    // Event data
    [Serializable]
    public struct EventData
    {
        public string type;
        public string data;
    }
    
    // Session data. Now it only contains events 
    // but could potentially contain player or session info
    [Serializable]
    public class SessionData
    {
        public List<EventData> events;
    }
    
    public class EventService : MonoBehaviour
    {
        private const string STORAGE_FILE_PATH = "eventServiceStorage.dat";
        
        [SerializeField] private string serverURL;
        [SerializeField] private int cooldownBeforeSend;
        [SerializeField] private int backupEachNumberOfEvents;
        
        // Debug options will only work if EVENT_SERVICE_DEBUG is defined
        [Header("DebugMode Settings")]
        [SerializeField] public bool debugMode;
        [SerializeField] public bool longRequest;
        [SerializeField] public bool failedRequest;

        private SessionData session;
        
        private string storageFilePath;
        private Coroutine sendingRoutine;
        
        private static EventService instance;

#region Unity methods
        private void Awake()
        {
            Assert.IsNull(instance, "[EventService] instance already exists");
            instance = this;
            
            storageFilePath = $"{Application.persistentDataPath}/{STORAGE_FILE_PATH}";
            
            // Subscribe to browser's OnApplicationQuit event. See WebGLEventHandler
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLEventHandler.Subscribe(OnApplicationQuit);
#endif
        }

        private void Start()
        {
            RestoreSession();
        }

        // MonoBehaviour's OnApplicationQuit method can be not called on Android device,
        // so we use OnApplicationFocus that is called when user press home button or tasks list
#if UNITY_ANDROID && !UNITY_EDITOR
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                StoreSession();
            }
        }
#else
        private void OnApplicationQuit()
        {
            StoreSession();
        }
#endif
        #endregion

#region Public API
        public static EventService Instance
        {
            get
            {
                if (null == instance)
                {
                    throw new Exception("[EventService] does not exist in current scene");
                }

                return instance;
            }
        }
                
        public void TrackEvent(string type, string data)
        {
            session.events.Add(new EventData { type = type, data = data });
            DebugUtility.LogFormat(LogType.Log, "[EventService] Event added: {{type: {0}. data: {1}}}", type, data);

            // The ticket does not specify how many events to track and what their frequency is
            // so let's assume there are a lot of them and we don't want to lose them
            // We will create a backup for each N event in the list
            // N = 1 is not recommended though
            if (backupEachNumberOfEvents != 0 && session.events.Count % backupEachNumberOfEvents == 0)
            {
                StoreSession();
            }
            
            CheckForUnsentEvents();
        }
#endregion

#region Private API
        private void RestoreSession()
        {
            // If there is no backup, create a new session. Otherwise, deserialize the session from the read json.
            // Then check for events to send
            if (!File.Exists(storageFilePath))
            {
                session = new SessionData { events = new List<EventData>() };
                DebugUtility.Log(LogType.Log, "[EventService] New session created");
                return;
            }

            try
            {
                using (StreamReader streamReader = new StreamReader(storageFilePath))
                {
                    string json = streamReader.ReadToEnd();
                    session = JsonConvert.DeserializeObject<SessionData>(json) ?? new SessionData { events = new List<EventData>() };
                    DebugUtility.LogFormat(LogType.Log, "[EventService] Session restored from disk {0}", json);
                }
            }
            catch (Exception e)
            {
                // If the backup file was corrupted for some reason, handle an exception and recreate session
                DebugUtility.Log(LogType.Error, e.Message);
                session = new SessionData { events = new List<EventData>() };
            }    
            
            CheckForUnsentEvents();
        }

        private void StoreSession()
        {
            //Don't forget to set config.autoSyncPersistentDataPath = true in WebGL template if in WebGL
            using (StreamWriter streamWriter = new StreamWriter(storageFilePath))
            {
                string json = JsonConvert.SerializeObject(session);
                streamWriter.Write(json);
                DebugUtility.LogFormat(LogType.Log, "[EventService] Session stored to disk {0}", json);
            }
        }

        private void CheckForUnsentEvents()
        {
            // Adding the first event to the list starts the sending routine
            // The routine will fill up the event list during the beforeSendCooldown seconds
            // and then send them all at once
            if (session.events.Count > 0 && sendingRoutine == null)
            {
                sendingRoutine = StartCoroutine(SendingRoutine());
            }
        }

        private IEnumerator SendingRoutine()
        {
            DebugUtility.Log(LogType.Warning, "[EventService] Waiting for other events...");
            
            // Waiting for event list to be filled
            yield return new WaitForSeconds(cooldownBeforeSend);
            
            DebugUtility.Log(LogType.Warning, "[EventService] Sending events using WebRequest...");
            
            // Remember how many events we are going to send and serialize them to json
            int sendingEventNumber = session.events.Count;
            string json = JsonConvert.SerializeObject(session);
            
            WWWForm form = new WWWForm();
            form.AddField("data", json);

            using (UnityWebRequest request = UnityWebRequest.Post(ResolveUri(), form))
            {
                yield return request.SendWebRequest();
                
                
                // If events are sent, remove them from the list
                if (request.result == UnityWebRequest.Result.Success)
                {
                    session.events.RemoveRange(0, sendingEventNumber);
                    DebugUtility.LogFormat(LogType.Warning, "[EventService] Events successfully sent: {0}", json);
                }
                else
                {
                    DebugUtility.LogFormat(LogType.Error, "[EventService] Events not sent! Request error: {0}", request.error);
                }
            }

            // Backup event list, stop the routine and check for events that could be added 
            StoreSession();
            
            sendingRoutine = null;
            
            CheckForUnsentEvents();
        }
        
        // We use httpstat.us to simulate long and failed requests
        private string ResolveUri()
        {
#if EVENT_SERVICE_DEBUG            
            if (debugMode)
            {
                return $"httpstat.us/{(failedRequest ? "503" : "200")}{(longRequest ? "?sleep=3000" : "")}";
            }
#endif
            return serverURL;
        }
#endregion
    }
}