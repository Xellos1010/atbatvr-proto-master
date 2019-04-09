
using System;
using System.Collections.Generic;
/*
**
 * News Tokens Used For Articles
 */
public class NewsToken
{
    private String guid;
    private eTokenType type;

    // PLAYER CARD
    private String id;
    private String seoName;

    // LINK FIELD
    private String href;
    // LINK FIELD
    private String mobileHref;

    //VIDEO
    private String videoId;
    private String headline;
    private String duration;
    private String blurb;
    private String bigBlurb;
    private String previewImage;
    private String includeHtmlId;
    private String includeHtml;
    private bool? sharable;
    private Dictionary<String, String> mediaUrlMap;

    public Dictionary<String, String> getMediaUrlMap()
    {
        return mediaUrlMap;
    }

    public void setMediaUrlMap(Dictionary<String, String> mediaUrlMap)
    {
        this.mediaUrlMap = mediaUrlMap;
    }

    public enum eTokenType
    {
        None = -1,
        playerCard,
        hyperLink,
        video,
        includeHtml,
        Length
    }

    /*
            private String key;

            TokenType(String key)
    {
        key = key;*/

    public static eTokenType ReturnTokenType(String key)
    {
        foreach (eTokenType t in Enum.GetValues(typeof(eTokenType)))
        {
            if (key == t.ToString())
            {
                return t;
            }
        }
        return eTokenType.None;
    }

    string CheckAddKey(Dictionary<string, object> jsonObject, string keyToCheck)
    {
        if (jsonObject.ContainsKey(keyToCheck))
            return jsonObject[keyToCheck] as string;
        else
            return null;
    }

    public NewsToken(Dictionary<string, object> jsonObject)
    {
        guid = CheckAddKey(jsonObject, "tokenGUID");//jsonObject["tokenGUID"] as string;
        type = ReturnTokenType(CheckAddKey(jsonObject, "type")); //ReturnTokenType(jsonObject["type"] as string);
        id = CheckAddKey(jsonObject, "id");//jsonObject["id"] as string;
        seoName = CheckAddKey(jsonObject, "seoName"); //jsonObject["seoName"] as string;
        href = CheckAddKey(jsonObject, "href"); //jsonObject["href"] as string;
        mobileHref = CheckAddKey(jsonObject, "hrefMobile"); //jsonObject["hrefMobile"] as string;
        videoId = CheckAddKey(jsonObject, "videoId"); //jsonObject["videoId"] as string;
        headline = CheckAddKey(jsonObject, "headline"); //jsonObject["headline"] as string;
        duration = CheckAddKey(jsonObject, "duration"); //jsonObject["duration"] as string;
        blurb = CheckAddKey(jsonObject, "blurb"); //jsonObject["blurb"] as string;
        bigBlurb = CheckAddKey(jsonObject, "bigBlurb"); //jsonObject["bigBlurb"] as string;
        previewImage = CheckAddKey(jsonObject, "previewImage"); //jsonObject["previewImage"] as string;
        includeHtml = CheckAddKey(jsonObject, "includeHtml"); //jsonObject["includeHtml"] as string;
        includeHtmlId = CheckAddKey(jsonObject, "includeHtmlId"); //jsonObject["includeHtmlId"] as string;
                                                                  //TODO verify if the actual field "shareable" once #JIRA-HMB-3275 is resolved
        if (jsonObject.ContainsKey("shareable"))
            sharable = ((jsonObject["shareable"] as bool?) != null) ? jsonObject["shareable"] as bool? : false;
        else
            sharable = false;
        Dictionary<string, object> mediaUrls = new Dictionary<string, object>();
        if (jsonObject.ContainsKey("mediaURLS"))
        {
            mediaUrlMap = new Dictionary<string, string>();
            mediaUrls = jsonObject["mediaURLS"] as Dictionary<string, object>;
        }
        if (mediaUrls.Keys.Count < 1)
        {
            //TODO Implement GamedayApplication
            /*mediaUrlMap[
                    GamedayApplication.getInstance().getVODPlaybackScenario(false)] =
                    jsonObject["playbackUrlMobile"] as Dictionary<string, object>;
            mediaUrlMap[
                    GamedayApplication.getInstance().getVODPlaybackScenarioTablets(false)] =
                    jsonObject["playbackUrlTablet"] as Dictionary<string, object>;
                    */
        }
        else
        {
            List<String> allowedPlaybackScenarios = new List<String>();
            //TODO implement Playback Scenario
            /*
            List<String> allowedPlaybackScenarios =
                    PlaybackScenario.getPlaybacksForVOD(GamedayApplication.getInstance());
            */
        for (int i = 0; i < allowedPlaybackScenarios.Count; i++)
            {
                if (mediaUrls.ContainsKey(allowedPlaybackScenarios[i]))
                {
                    mediaUrlMap[allowedPlaybackScenarios[i]] = mediaUrls[allowedPlaybackScenarios[i]] as string;
                }
            }
        }
    }

    public NewsToken()
    {

    }

    public String getGuid()
    {
        return guid;
    }

    public void setGuid(String guid)
    {
        this.guid = guid;
    }

    public eTokenType getType()
    {
        return type;
    }

    public void setType(eTokenType type)
    {
        this.type = type;
    }

    public String getId()
    {
        return id;
    }

    public void setId(String id)
    {
        this.id = id;
    }

    public String getSeoName()
    {
        return seoName;
    }

    public void setSeoName(String seoName)
    {
        this.seoName = seoName;
    }

    public String getHref()
    {
        return href;
    }

    public void setHref(String href)
    {
        this.href = href;
    }

    public String getMobileHref()
    {
        return mobileHref;
    }

    public void setMobileHref(String mobileHref)
    {
        this.mobileHref = mobileHref;
    }

    public String getVideoId()
    {
        return videoId;
    }

    public void setVideoId(String videoId)
    {
        this.videoId = videoId;
    }

    public String getHeadline()
    {
        return headline;
    }

    public void setHeadline(String headline)
    {
        this.headline = headline;
    }

    public String getDuration()
    {
        return duration;
    }

    public void setDuration(String duration)
    {
        this.duration = duration;
    }

    public String getBlurb()
    {
        return blurb;
    }

    public void setBlurb(String blurb)
    {
        this.blurb = blurb;
    }

    public String getBigBlurb()
    {
        return bigBlurb;
    }

    public void setBigBlurb(String bigBlurb)
    {
        this.bigBlurb = bigBlurb;
    }

    public String getPreviewImage()
    {
        return previewImage;
    }

    public void setPreviewImage(String previewImage)
    {
        this.previewImage = previewImage;
    }

    public String getIncludeHtmlId()
    {
        return includeHtmlId;
    }

    public void setIncludeHtmlId(String includeHtmlId)
    {
        this.includeHtmlId = includeHtmlId;
    }

    public String getIncludeHtml()
    {
        return includeHtml;
    }

    public void setIncludeHtml(String includeHtml)
    {
        this.includeHtml = includeHtml;
    }

    public bool? isSharable()
    {
        return sharable;
    }

    public void setSharable(bool sharable)
    {
        this.sharable = sharable;
    }

    public int describeContents()
    {
        return 0;
    }
    /*
    public void writeToParcel(Parcel dest, int flags)
{
    dest.writeString(guid);
    dest.writeInt(type == null ? -1 : type.ordinal());
    dest.writeString(id);
    dest.writeString(seoName);
    dest.writeString(href);
    dest.writeString(mobileHref);
    dest.writeString(videoId);
    dest.writeString(headline);
    dest.writeString(duration);
    dest.writeString(blurb);
    dest.writeString(bigBlurb);
    dest.writeString(previewImage);
    dest.writeString(includeHtmlId);
    dest.writeString(includeHtml);
    dest.writeByte(sharable ? (byte)1 : (byte)0);
    Bundle output = new Bundle();
    for (String key : mediaUrlMap.keySet())
    {
        output.putString(key, mediaUrlMap.get(key));
    }
    dest.writeBundle(output);
}*/
    /*
    protected NewsToken(object parcel)
    {
        guid = parcel.readString();
        int tmpType = in.readInt();
        type = tmpType == -1 ? null : TokenType.values()[tmpType];
        id = in.readString();
        seoName = in.readString();
        href = in.readString();
        mobileHref = in.readString();
        videoId = in.readString();
        headline = in.readString();
        duration = in.readString();
        blurb = in.readString();
        bigBlurb = in.readString();
        previewImage = in.readString();
        includeHtmlId = in.readString();
        includeHtml = in.readString();
        sharable = in.readByte() != 0;
        mediaUrlMap = new HashMap<String, String>();
        Bundle bundle = in.readBundle();
        for (String key : bundle.keySet())
        {
            mediaUrlMap.put(key, (String)bundle.get(key));
        }
    }*/
    /*
    public static final Creator<NewsToken> CREATOR = new Creator<NewsToken>()
    {
            public NewsToken createFromParcel(Parcel source)
    {
        return new NewsToken(source);
    }
    */
    public NewsToken[] newArray(int size)
    {
        return new NewsToken[size];
    }
}