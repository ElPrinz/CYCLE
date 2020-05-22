[System.Serializable]
public class Net_FollowUpdate : NetMsg
{
    public Net_FollowUpdate()
    {
        OP = NetOP.FollowUpdate;
    }

    public byte Success { set; get; }
    public Account Follow { set; get; }
}
