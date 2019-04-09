using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class NewsItemModel : INewsModel {

    //private static String TAG = NewsItemModel.class.getSimpleName();

    private static String BUNDLE_NEWS_TOKENS = "bundleNewsTokens";
    private static String BUNDLE_NEWS_ITEM_PHOTOS = "bundleNewsItemPhotos";
    //TODO remove these once we move to using gson
    private static String SMALL = "640x360";
    private static String MEDIUM = "960x540";
    private static String LARGE = "1280x720";
    private static String ITEM_TAGS = "itemTags";
    private static String BREAKING = "breaking";
    private static String THUMBNAILS = "thumbnails";
    private static String ASPECT_RATIO = "aspectRatio";
    private static String WIDTH = "width";
    private static String HEIGHT = "height";
    private static String SRC = "src";

    private String title;
    //private String displayDate;
    private DateTime displayDate;
    private String imageUrl;
    private String thumbnailUrl;
    private String imageCaption;
    private String byline;
    private String subheading;
    private String contentId;
    private String body;
    private String blurb;
    private String videoCid;
    private String videoDuration;
    private bool videoUrlSharable;
    private bool isBreakingNews;
    private String gid;  // for video url transcode, to tell if something is game highlight
    private String tagline;
    private String shareUrl;
    private String disclaimer;
    private String type;
    private String mobileUrl;
    private Dictionary<string, Dictionary<string, object>> mediaUrlDict;
    private Dictionary<string, NewsItemPhoto> newsItemPhotoDictionary = new Dictionary<string, NewsItemPhoto>();
    private Dictionary<string, NewsToken> tokenIdToToken = new Dictionary<string, NewsToken>();

    enum eArticleTypes
    {
        None = -1,
        article,
        shortcontent,
        blogcontent,
        Length
    }

    public NewsItemModel()
    {
        
    }

    string CheckAddKey(Dictionary<string, object> jsonObject, string keyToCheck)
    {
        if (jsonObject.ContainsKey(keyToCheck))
            return jsonObject[keyToCheck] as string;
        else
            return null;
    }

    public NewsItemModel(Dictionary<string,object> articleJson)//Dictionary<string,object> articleJson)//, OverrideStrings overrideStrings)
    {
        try
        {
            //Per https://jira.mlbam.com/browse/ANDROID-4521 use timestamp first
            String articleReleaseDate = articleJson["timestamp"] as string;
            if (String.IsNullOrEmpty(articleReleaseDate))
            {
                //fall back to userDate
                articleReleaseDate = articleJson["userDate"] as string;
            }
            if (DateTime.TryParse(articleReleaseDate as string, out displayDate))
            {
                Debug.Log("Sucessfully parsed date from timestamp");
            }
            else
                Debug.Log("Date was not parsed");
        }
        catch (Exception e)
        {
            //Timber.w(e.toString());
            Debug.Log(e.Message);
            //displayDate = null;
        }
        type = (articleJson["type"] as string);
        contentId = CheckAddKey(articleJson, "contentId"); //articleJson["contentId"] as string;
        title = CheckAddKey(articleJson, "headline"); //articleJson["headline"] as string;

        subheading = CheckAddKey(articleJson, "subhead"); //articleJson["subhead"] as string;
        byline = CheckAddKey(articleJson, "byline"); //articleJson["byline"] as string;
        tagline = CheckAddKey(articleJson, "tagline"); //articleJson["tagline"] as string;
        blurb = CheckAddKey(articleJson, "blurb"); //articleJson["blurb"] as string;
        
        if(TYPE_SHORT_CONTENT == type)
        {
            String mobileUrl = articleJson["mobile-url"] as string;
            if (!String.IsNullOrEmpty(mobileUrl))
            {
                if (NewsItemModelDeserializer.containsHttp(mobileUrl))
                {
                    this.mobileUrl = mobileUrl;
                }
                else
                {
                    this.mobileUrl = mobileUrl;
                    //TODO Implement Override Strings
                    //mobileUrl = overrideStrings.getStringWithFormat(R.string.teampage_image_prefix, mobileUrl);
                }
            }
        }
        else if (TYPE_BLOG_CONTENT == type)
        {
            String mobileUrl = articleJson["url"] as string;
            if (!String.IsNullOrEmpty(mobileUrl))
            {
                if (NewsItemModelDeserializer.containsHttp(mobileUrl))
                {
                    this.mobileUrl = mobileUrl;
                }
                else
                {
                    this.mobileUrl = mobileUrl;
                    //TODO Implement Override Strings
                    //mobileUrl = String.format(overrideStrings.getString(R.string.news_articleprefix), mobileUrl);
                }
            }
        }

        String articleBody = CheckAddKey(articleJson, "body"); //articleJson["body"] as string;
        if (articleBody != null)
        {
            //TODO Implement HTML Helper
            //body = HtmlHelper.disableLinks(articleBody);
            body = articleBody;
        }
        else
        {
            body = articleBody;
        }

        if (TYPE_SHORT_CONTENT == type
                || TYPE_BLOG_CONTENT == type)
        {
            body = articleJson["blurb"] as string;
            subheading = "";
        }


        disclaimer = CheckAddKey(articleJson, "approval"); //articleJson["approval"] as string;

        try
        {
            Dictionary<string,object> photos = ((articleJson["photos"] as List<object>)[0] as Dictionary<string,object>)["cuts"] as Dictionary<string,object>;
            
            //TODO Implement Override string
            /*imageUrl = String.format(
                    overrideStringss.getString(R.string.teampage_image_prefix),
                    deserializeImageUrl(photos, SMALL, MEDIUM, LARGE));
*/

            //set default thumbnail
            String thumbnailUrl = NewsItemModelDeserializer.deserializeImageUrl(photos,  NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_270_154],
                    NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_320_180], NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_480_270]);
            thumbnailUrl = NewsItemModelDeserializer.sanitizeNewsImageUrl(thumbnailUrl);
            //set other photo items.
            newsItemPhotoDictionary = NewsItemModelDeserializer.deserializeNewsItemPhotoDict(photos);
            //set breaking news.
            Dictionary<string, object> itemTags = articleJson[ITEM_TAGS] as Dictionary<string, object>;
            try
            {
                isBreakingNews = NewsItemModelDeserializer.deserializeBreaking(itemTags);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                isBreakingNews = false;
            }
        }
        catch (Exception e)
        {
            // no need to fl
            //throw e;
            Debug.Log(e.Message);
        }

        if (articleJson.ContainsKey("related-media"))
        {
            try
            {
                List<object> videos = articleJson["related-media"] as List<object>;
                List<String> allowedPlaybacks = new List<string>();
                //TODO Implement PlaybackScenario
                //TODO Implement GamedayApplication
                /*List<String> allowedPlaybacks =
                        PlaybackScenario.getPlaybacksForVOD(GamedayApplication.getInstance());
                */
                for (int i = 0; i < videos.Count; i++)
                {
                    Dictionary<string, object> video = videos[i] as Dictionary<string, object>;
                    Dictionary<string, object> mediaUrls = video["media-urls"] as Dictionary<string, object>;

                    bool foundPlayableMedia = false;
                    for (int j = 0; j < allowedPlaybacks.Count; j++)
                    {
                        if (mediaUrls.ContainsKey(allowedPlaybacks[j]))
                        {
                            Dictionary<string, object> media = mediaUrls[allowedPlaybacks[j]] as Dictionary<string, object>;
                            mediaUrlDict[allowedPlaybacks[j]] = media["src"] as Dictionary<string, object>;
                            foundPlayableMedia = true;
                        }
                    }
                    if (foundPlayableMedia)
                    {
                        videoCid = video["contentId"] as string;
                        videoDuration = video["duration"] as string;
                        gid = video["guid"] as string;
                        // Checking if video is sharable
                        try
                        {
                            List<string> mmtax = (video["itemTags"] as Dictionary<string, object>)["mmtax"] as List<string>;
                            videoUrlSharable = false;
                            for (int j = 0; j < mmtax.Count; j++)
                            {
                                if (CONST_SHARABLE == mmtax[j])
                                {
                                    videoUrlSharable = true;
                                }
                            }

                        }
                        catch (Exception e)
                        {
                            videoUrlSharable = false;
                        }
                        try
                        {

                            imageCaption = video["blurb"] as string;
                            Dictionary<string, object> photoToUse = (video[THUMBNAILS] as Dictionary<string, object>)
                                    [NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_960_540]] as Dictionary<string, object>;
                            imageUrl = photoToUse[SRC] as string;
                            if (newsItemPhotoDictionary.Count == 0)
                            {
                                newsItemPhotoDictionary = NewsItemModelDeserializer.deserializeNewsItemPhotoDict(video
                                        [THUMBNAILS] as Dictionary<string, object>);
                            }
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    }

                }
            }
            catch (NullReferenceException e)
            {
                videoCid = null;
                mediaUrlDict = new Dictionary<String, Dictionary<string, object>>();
                gid = null;
                throw (e);
            }
        }

        try
        {
            String shareUrl = articleJson["shared-url"] as string;
            if (shareUrl != null)
            {
                if (shareUrl.Contains("http") || shareUrl.Contains("https"))
                {
                    this.shareUrl = shareUrl;
                }
                else
                {
                    this.shareUrl = shareUrl;
                    //TODO Implement GamedayApplication
                    /*shareUrl = String.format(GamedayApplication.getInstance(]getString(R
                            .string.teampage_image_prefix), shareUrl.trim());*/
                }
            }
            else
            {
                //Timber.w("shared-url does not exist. Fallback to url");
                shareUrl = articleJson["url"] as string;
                if (shareUrl != null && (shareUrl.Contains("http") || shareUrl.Contains("https")))
                {
                    this.shareUrl = shareUrl;
                }
                else if (shareUrl != null)
                {
                    this.shareUrl = shareUrl;
                    //TODO Implement GamedayApplication
                    /*shareUrl = String.format(GamedayApplication.getInstance(]getString(R
                            .string.teampage_image_prefix), shareUrl.trim());*/
                }
                throw new Exception("shared-url does not exist. Fallback to url");
            }
        }
        catch (Exception e)
        {
            //Timber.w(e.toString());
            shareUrl = null;
            throw e;
        }
        if (articleJson.ContainsKey("token-data"))
        {
            try
            {
                Dictionary<string, object> tokenData = articleJson["token-data"] as Dictionary<string, object>;
                foreach (KeyValuePair<string, object> token in tokenData)
                {
                    NewsToken nt = new NewsToken(token.Value as Dictionary<string, object>);
                    tokenIdToToken[nt.getGuid()] = nt;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }

    public override string getBody()
    {
        return body;
    }

    public override string getBlurb()
    {
        return blurb;
    }

    public override string getByline()
    {
        return byline;
    }

    public override string getTagline()
    {
        return tagline;
    }

    public override string getContentId()
    {
        return contentId;
    }

    public override string getDisplayDate()
    {
        return displayDate.ToShortDateString();
    }

    public override string getGID()
    {
        return gid;
    }

    public override string getImageCaption()
    {
        return imageCaption;
    }

    public override string getImageUrl()
    {
        return imageUrl;
    }

    public override string getThumbnailUrl()
    {
        return thumbnailUrl;
    }

    public override string getSubheading()
    {
        return subheading;
    }

    public override string getTitle()
    {
        return title;
    }

    public override string getVideoCid()
    {
        return videoCid;
    }

    public override bool isVideoUrlShareable()
    {
        return videoUrlSharable;
    }

    public override string getShareUrl()
    {
        return shareUrl;
    }

    public override string getDisclaimerText()
    {
        return disclaimer;
    }

    public override string getType()
    {
        return type;
    }

    public override string getMobileUrl()
    {
        return mobileUrl;
    }

    public override Dictionary<string, NewsToken> getTokens()
    {
        return tokenIdToToken;
    }

    public override string getVideoDuration()
    {
        return videoDuration;
    }

    public override bool isBreaking()
    {
        return isBreakingNews;
    }

    public override Dictionary<string, Dictionary<string, object>> getMediaUrlDict()
    {
        return mediaUrlDict;
    }

    public override Dictionary<string, NewsItemPhoto> getNewsItemPhotoDictionary()
    {
        return newsItemPhotoDictionary;
    }

    public override Dictionary<string, NewsItemPhoto> getNewsItemPhotoDictionary(Dictionary<string, NewsItemPhoto> data)
    {
        newsItemPhotoDictionary = data;
        return getNewsItemPhotoDictionary();
    }

    public override void setMediaUrlDict(Dictionary<string, Dictionary<string, object>> mediaUrlDict)
    {
        this.mediaUrlDict = mediaUrlDict;
    }

    public override void setBody(string body)
    {
        this.body = body;
    }

    public override void setDisclaimer(string disclaimer)
    {
        this.disclaimer = disclaimer;
    }

    public override void setTokenIdToToken(Dictionary<string, NewsToken> token)
    {
        tokenIdToToken = token;
    }

    public override void setImageUrl(string URL)
    {
        imageUrl = URL;
    }

    public override void setThumbnailUrl(string URL)
    {
        thumbnailUrl = URL;
    }

    public override void setNewsItemPhotoDictionary(Dictionary<string, NewsItemPhoto> item)
    {
        newsItemPhotoDictionary = item;
    }

    public override void setIsBreakingNews(bool isBreaking)
    {
        isBreakingNews = isBreaking;
    }

    public override void setVideoCid(string cID)
    {
        videoCid = cID;
    }

    public override void setVideoDuration(string duration)
    {
        videoDuration = duration;
    }

    public override void setGid(string gID)
    {
        gid = gID;
    }

    public override void setVideoUrlSharable(bool sharable)
    {
        videoUrlSharable = sharable;
    }

    public override void setImageCaption(string caption)
    {
        imageCaption = caption;
    }

    public override void setShareUrl(string url)
    {
        shareUrl = url;
    }

    public override void setType(string type)
    {
        this.type = type;
    }

    public override void setDisplayDate(DateTime date)
    {
        displayDate = date;
    }

    public override void setContentId(string contentID)
    {
        contentId = contentID;
    }

    public override void setTitle(string title)
    {
        this.title = title;
    }

    public override void setSubheading(string heading)
    {
        subheading = heading;
    }

    public override void setByline(string byLine)
    {
        byline = byLine;
    }

    public override void setTagline(string tagLine)
    {
        tagline = tagLine;
    }

    public override void setBlurb(string blurb)
    {
        this.blurb = blurb;
    }

    public override void setMobileUrl(string url)
    {
        mobileUrl = url;
    }
}



/**
 * Interface for News Items in List or Detail View
 */
public abstract class INewsModel
{
    public String IMAGE_URL_BASE = "http://mlb.com/";
    public String TYPE_SHORT_CONTENT = "short-content";
    public String TYPE_BLOG_CONTENT = "blog-content";
    public String CONST_SHARABLE = "shareable";

    /**
     * Gets the body text for this article.
     *
     * @return Body text for this article.
     */
    public abstract String getBody();

    /**
     * Gets the Summary for this article
     *
     * @return Blurb/Summary for this article
     */
    public abstract String getBlurb();

    /**
     * Gets the byline for this article.
     *
     * @return Byline for this article.
     */
    public abstract String getByline();

    /**
     * Gets the Tagline for this article.
     *
     * @return Tagline for this article.
     */
    public abstract String getTagline();


    /**
     * Gets the content ID for this article.
     *
     * @return Content ID for this article.
     */
    public abstract String getContentId();

    /**
     * Gets a public abstract String representing this article's date as
     * formatted for display on the UI.
     *
     * @return public abstract String representation of this article's date.
     */
    public abstract String getDisplayDate();

    /**
     * Gets the GID associated with this article.
     *
     * @return GID associated with this article.
     */
    public abstract String getGID();

    /**
     * Gets the caption for this article's associated image.
     *
     * @return Caption for this article's associated image.
     */
    public abstract String getImageCaption();

    /**
     * Gets the URL of image associated with this article.
     *
     * @return URL of this article's associated image.
     */
    public abstract String getImageUrl();

    public abstract String getThumbnailUrl();

    /**
     * Gets the subheading for this article.
     *
     * @return Subheading for this article.
     */
    public abstract String getSubheading();

    /**
     * Gets the title of this article.
     *
     * @return Title of this article.
     */
    public abstract String getTitle();

    /**
     * Gets the content ID of the video associated with this article.
     *
     * @return Content ID of the video associated with this article.
     */
    public abstract String getVideoCid();

    public abstract bool isVideoUrlShareable();

    /**
     * Gets the sharing url
     *
     * @return URL that needs to be used for sharing purposes
     */
    public abstract String getShareUrl();

    /**
     * Gets the disclaimer url
     *
     * @return Disclaimer text
     */
    public abstract String getDisclaimerText();

    public abstract String getType();

    public abstract String getMobileUrl();

    public abstract String getVideoDuration();

    public abstract Dictionary<String, NewsToken> getTokens();
    
    public abstract bool isBreaking();

    public abstract Dictionary<String, Dictionary<string, object>> getMediaUrlDict();

    public abstract Dictionary<string, NewsItemPhoto> getNewsItemPhotoDictionary();

    public abstract Dictionary<string, NewsItemPhoto> getNewsItemPhotoDictionary(Dictionary<string, NewsItemPhoto> data);

    public abstract void setBody(string body);

    public abstract void setDisclaimer(string disclaimer);

    public abstract void setTokenIdToToken(Dictionary<string, NewsToken> token);

    public abstract void setImageUrl(string URL);

    public abstract void setThumbnailUrl(string URL);

    public abstract void setNewsItemPhotoDictionary(Dictionary<String, NewsItemPhoto> item);

    public abstract void setIsBreakingNews(bool isBreaking);

    public abstract void setVideoCid(string cID);

    public abstract void setVideoDuration(string duration);

    public abstract void setGid(string gID);

    public abstract void setVideoUrlSharable(bool sharable);

    public abstract void setImageCaption(string caption);

    public abstract void setMediaUrlDict(Dictionary<String, Dictionary<string, object>> mediaUrlDict);

    public abstract void setShareUrl(string url);

    public abstract void setType(string type);

    public abstract void setDisplayDate(DateTime date);

    public abstract void setContentId(string contentID);

    public abstract void setTitle(string title);

    public abstract void setSubheading(string heading);

    public abstract void setByline(string byLine);

    public abstract void setTagline(string tagLine);

    public abstract void setBlurb(string blurb);

    public abstract void setMobileUrl(string url);

}

