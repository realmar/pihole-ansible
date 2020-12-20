using JetBrains.Annotations;
using JulianSchoenbaechler.MicDecode;
using M2MqttUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt.Messages;

[RequireComponent(typeof(MicDecode))]
public class NoiseCollector : MonoBehaviour
{
    public const string SELECTED_MICROPHONE = "selected_microphone";

    [SerializeField] private MicDecode _microphone;
    [SerializeField] private Text _dbText;
    [SerializeField] private InputField _baseDb;
    [SerializeField] private InputField _topic;
    [SerializeField] private Dropdown _micDropdown;
    [SerializeField] private GameObject _mqttClientPrefab;
    private M2MqttUnityClient _mqttClient;
    private GameObject _obj;
    private readonly TimeSpan _minutes = TimeSpan.FromMinutes(8);
    private readonly TimeSpan _sendInterval = TimeSpan.FromSeconds(2);
    private DateTime _startTime = DateTime.MinValue;
    private DateTime _previousSendTime = DateTime.MinValue;
    private readonly List<double> _buffer = new List<double>();

    [UsedImplicitly]
    private void Start()
    {
        var devices = Microphone.devices.ToList();
        _micDropdown.options = devices.Select(s => new Dropdown.OptionData(s)).ToList();
        _micDropdown.onValueChanged.AddListener(index =>
        {
            var inputDevice = devices[index];

            _microphone.StopRecording();
            _microphone.InputDevice = inputDevice;
            _microphone.StartRecording();
        });

        var storedInputDevice = PlayerPrefs.GetString(SELECTED_MICROPHONE);
        var storedInputDeviceIndex = devices.IndexOf(storedInputDevice);

        if (PlayerPrefs.HasKey(SELECTED_MICROPHONE) && storedInputDeviceIndex > -1)
        {
            _micDropdown.value = storedInputDeviceIndex;
        }

        _microphone.StartRecording();
    }

    [UsedImplicitly]
    private void Update()
    {
        try
        {
            var db = _microphone.VolumeDecibel + int.Parse(_baseDb.text);
            _buffer.Add(db);

            _dbText.text = db.ToString();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        try
        {
            if (DateTime.UtcNow - _startTime > _minutes)
            {
                RespawnMqtt();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        try
        {
            if (DateTime.UtcNow - _previousSendTime > _sendInterval)
            {
                _previousSendTime = DateTime.UtcNow;

                if (_mqttClient?.client?.IsConnected ?? false)
                {
                    _mqttClient?.client?.Publish(_topic.text, Encoding.UTF8.GetBytes(JsonUtility.ToJson(new MQTTData { Decibels = _buffer.Average() })), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
                    _buffer.Clear();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private void RespawnMqtt()
    {
        if (_obj != null)
        {
            Destroy(_obj);
        }

        _obj = Instantiate(_mqttClientPrefab);
        _mqttClient = _obj.GetComponent<M2MqttUnityClient>();

        _startTime = DateTime.UtcNow;
    }

    [UsedImplicitly]
    private void OnDestroy()
    {
        _microphone.StopRecording();
    }

    [Serializable]
    private class MQTTData
    {
        public double Decibels;
    }
}
