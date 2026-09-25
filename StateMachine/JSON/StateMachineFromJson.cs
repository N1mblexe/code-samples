using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MyStateMachine.FromJson
{
    [Serializable]
    public class AnimatorJson
    {
        public List<LayerData> layers;
    }

    [Serializable]
    public class LayerData
    {
        public string name;
        public List<StateData> states;
    }

    [Serializable]
    public class StateData
    {
        public string name;
        public List<TransitionData> transitions;
    }

    [Serializable]
    public class TransitionData
    {
        public string toState;
        public List<ConditionData> conditions;
    }

    [Serializable]
    public class ConditionData
    {
        public string parameter;
        public string mode;
        public float threshold;
    }


    public class DynamicState
    {
        public string Name { get; private set; }
        public List<DynamicTransition> Transitions { get; private set; } = new();

        public DynamicState(string name)
        {
            Name = name;
        }
    }

    public class DynamicTransition
    {
        public DynamicState Target { get; private set; }
        public List<ConditionData> Conditions { get; private set; }

        public DynamicTransition(DynamicState target, List<ConditionData> conditions)
        {
            Target = target;
            Conditions = conditions;
        }
    }

    public class DynamicStateMachine
    {
        private Dictionary<string, DynamicState> _states = new();
        private DynamicState _currentState;

        public void LoadFromJsonFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"JSON dosyası bulunamadı: {filePath}");
                return;
            }

            string jsonText = File.ReadAllText(filePath);
            LoadFromJson(jsonText);
        }


        public void LoadFromJson(string jsonText)
        {
            var data = JsonUtility.FromJson<AnimatorJson>(jsonText);
            if (data.layers == null || data.layers.Count == 0)
            {
                Debug.LogError("JSON içinde geçerli layer yok!");
                return;
            }

            var layer = data.layers[0];

            foreach (var s in layer.states)
                _states[s.name] = new DynamicState(s.name);

            foreach (var s in layer.states)
            {
                var sourceState = _states[s.name];
                foreach (var t in s.transitions)
                {
                    if (_states.TryGetValue(t.toState, out var target))
                        sourceState.Transitions.Add(new DynamicTransition(target, t.conditions));
                }
            }

            if (layer.states.Count > 0)
                _currentState = _states[layer.states[0].name];

            Debug.Log($"State machine oluşturuldu. İlk state: {_currentState.Name}");
        }

        public void Update()
        {
            if (_currentState == null) return;

            foreach (var transition in _currentState.Transitions)
            {
                ChangeState(transition.Target.Name);
                break;
            }
        }

        public void ChangeState(string stateName)
        {
            if (_states.TryGetValue(stateName, out var nextState))
            {
                Debug.Log($"Geçiş: {_currentState.Name} → {nextState.Name}");
                _currentState = nextState;
            }
        }

        public string GetCurrentState() => _currentState?.Name;
    }
}