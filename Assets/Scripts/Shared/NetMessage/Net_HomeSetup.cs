[System.Serializable]
public class Net_HomeSetup : NetMsg
{
    public Net_HomeSetup()
    {
        OP = NetOP.HomeSetup;
    }

    public string Token { set; get; }
    public float item1PosX { set; get; }
    public float item1PosY { set; get; }
    public float item2PosX { set; get; }
    public float item2PosY { set; get; }
    public float item3PosX { set; get; }
    public float item3PosY { set; get; }
}
