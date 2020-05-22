using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class HubScene : MonoBehaviour
{
    public static HubScene Instance { set; get; }

    [SerializeField] private TextMeshProUGUI selfInformation;
    [SerializeField] private TMP_InputField addFollowInput;

    [SerializeField] private GameObject followPrefab;
    [SerializeField] private Transform followContainer;

    private Dictionary<string, GameObject> uiFollows = new Dictionary<string, GameObject>();
    void Start()
    {
        Instance = this;
        selfInformation.text = Client.Instance.self.Username + "#" + Client.Instance.self.Discriminator;
        Client.Instance.SendRequestFollow();
    }

    public void AddFollowToUi(Account follow)
    {
        GameObject followItem = Instantiate(followPrefab, followContainer);
        //Debug.Log("addfollowtoui");
        
        followItem.GetComponentInChildren<TextMeshProUGUI>().text = follow.Username + "#" + follow.Discriminator;
        followItem.transform.GetChild(1).GetComponent<Image>().color = (follow.Status != 0) ? Color.green : Color.gray;

        followItem.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate { Destroy(followItem); });
        followItem.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate { OnClickRemoveFollow(follow.Username, follow.Discriminator); });

        uiFollows.Add(follow.Username + "#" + follow.Discriminator, followItem);
    }

    public void UpdateFollow(Account follow)
    {
        uiFollows[follow.Username + "#" + follow.Discriminator].transform.GetChild(1).GetComponent<Image>().color = (follow.Status != 0) ? Color.green : Color.gray;
    }

    #region Button
    public void OnClickAddFollow()
    {
        //Debug.Log("onclickaddfollow");
        string usernameDiscriminator = addFollowInput.text;

        if(!Utility.IsUsernameAndDiscriminator(usernameDiscriminator) && !Utility.IsEmail(usernameDiscriminator))
        {
            //Debug.Log("Invalid format");
            return;           
        }

        //Client instance send add follow
        Client.Instance.SendAddFollow(usernameDiscriminator);
        
    }
    public void OnClickRemoveFollow(string username, string discriminator)
    {
        //Client instance send remove follow
        Client.Instance.SendRemoveFollow(username + "#" + discriminator);
        uiFollows.Remove(username + "#" + discriminator);
    }

    public void OnClickSaveMap()
    {
        //Client.Instance.SendHomeSetup();
        Net_HomeSetup hs = new Net_HomeSetup();

        //hs.Token = token;
        hs.item1PosX = GameObject.Find("Items/item1").transform.position.x;
        hs.item1PosY = GameObject.Find("Items/item1").transform.position.y;
        hs.item2PosX = GameObject.Find("Items/item2").transform.position.x;
        hs.item2PosY = GameObject.Find("Items/item2").transform.position.y;
        hs.item3PosX = GameObject.Find("Items/item3").transform.position.x;
        hs.item3PosY = GameObject.Find("Items/item3").transform.position.y;

        Debug.LogError(hs.item1PosX + " " + hs.item1PosY);
        Debug.LogError(hs.item2PosX + " " + hs.item2PosY);
        Debug.LogError(hs.item3PosX + " " + hs.item3PosY);
    }
    #endregion

}
