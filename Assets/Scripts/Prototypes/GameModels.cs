using System;
using System.Collections.Generic;

[Serializable]
public class ResourcePair {
    public string key;
    public float value;
}

[Serializable]
public class PlayerModel {
    public string id;
    public string username;
    public List<ResourcePair> resources;
    public float premiumCurrency;
}

[Serializable]
public class CityModel {
    public string id;
    public string ownerId;
    public string name;
    public int level;
    public int laborTotal;
    public int laborIdle;
}

[Serializable]
public class BuildingModel {
    public string id;
    public string cityId;
    public string type;
    public int level;
}
