using System;
using System.Collections.Generic;

namespace IFC.Systems
{
    public interface IGameEvent { }

    public static class GameEventHub
    {
        private static readonly Dictionary<Type, List<Delegate>> Subscribers = new Dictionary<Type, List<Delegate>>();

        public static void Publish<T>(T evt) where T : struct, IGameEvent
        {
            if (Subscribers.TryGetValue(typeof(T), out var list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is Action<T> action)
                    {
                        action(evt);
                    }
                }
            }
        }

        public static void Subscribe<T>(Action<T> handler) where T : struct, IGameEvent
        {
            if (handler == null)
            {
                return;
            }

            if (!Subscribers.TryGetValue(typeof(T), out var list))
            {
                list = new List<Delegate>();
                Subscribers.Add(typeof(T), list);
            }

            if (!list.Contains(handler))
            {
                list.Add(handler);
            }
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct, IGameEvent
        {
            if (handler == null)
            {
                return;
            }

            if (Subscribers.TryGetValue(typeof(T), out var list))
            {
                list.Remove(handler);
            }
        }
    }

    public struct BuildingUpgradedEvent : IGameEvent
    {
        public string cityId;
        public string buildingKey;
        public int fromLevel;
        public int toLevel;
    }

    public struct BuildingConstructedEvent : IGameEvent
    {
        public string cityId;
        public string buildingKey;
        public int level;
    }

    public struct TileCapChangedEvent : IGameEvent
    {
        public string cityId;
        public int townHallLevel;
        public int cap;
    }

    public struct MissionProgressEvent : IGameEvent
    {
        public string missionId;
        public int current;
        public int target;
    }
}
