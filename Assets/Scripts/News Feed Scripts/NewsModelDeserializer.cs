using System;
using System.Collections.Generic;
/**
 * Parser / Deserializer for {@link NewsItemModel}
 */

public class NewsList
{
    public void setNewsItemModels(List<NewsItemModel> articles)
    {

    }
}

public class NewsItemModelDeserializer{
    //private static string TAG = NewsItemModelDeserializer.getSimpleName();

    private static string SMALL = "480x270";
    private static string MEDIUM = "640x360";
    private static string LARGE = "960x540";

    private static string TYPE = "type";
    private static string ARTICLE = "article";
    private static string TIMESTAMP = "timestamp";
    private static string USER_DATE = "userDate";
    private static string CONTENT_ID = "contentId";
    private static string HEADLINE = "headline";
    private static string SUB_HEAD = "subhead";
    private static string BYLINE = "byline";
    private static string TAGLINE = "tagline";
    private static string BLURB = "blurb";
    private static string URL = "url";
    private static string MOBILE_URL = "mobile-url";
    private static string BODY = "body";
    private static string APPROVAL = "approval";
    private static string TOKEN_DATA = "token-data";
    private static string PHOTOS = "photos";
    private static string CUTS = "cuts";
    private static string MEDIA_URLS = "media-urls";
    private static string RELATED_MEDIA = "related-media";
    private static string DURATION = "duration";
    private static string GUID = "guid";
    private static string SRC = "src";
    private static string MMTAX = "mmtax";
    private static string SHARED_URL = "shared-url";
    private static string ITEM_TAGS = "itemTags";
    private static string BREAKING = "breaking";
    private static string THUMBNAILS = "thumbnails";

    private static string ASPECT_RATIO = "aspectRatio";
    private static string WIDTH = "width";
    private static string HEIGHT = "height";

    private DateTime dateTimeFormatter;
    //private OverrideStrings overrideStrings;
    /*
    public NewsItemModelDeserializer(OverrideStrings overrideStrings)
    {
        this.overrideStrings = overrideStrings;
        dateTimeFormatter = DateTimeFormat.forPattern(overrideStrings.getString(R.string.dateformat_MMM_dd_h_mm_a));
    }*/

    public NewsList getNewsListFromJson(Dictionary<string, object> jsonObject)
    {
        NewsList newsList = new NewsList();
        newsList.setNewsItemModels(getNewsItemModelsFromJson(jsonObject));
        return newsList;
    }

    public List<NewsItemModel> getNewsItemModelsFromJson(Dictionary<string,object> jsonObject)
    {
        List<NewsItemModel> models = new List<NewsItemModel>();
        List<Dictionary<string, object>> articlesJson = jsonObject["list"] as List<Dictionary<string, object>>;
        if (articlesJson != null && articlesJson.Count > 0)
        {
            for (int i = 0; i < articlesJson.Count; i++)
            {
                models.Add(new NewsItemModel(articlesJson[i]));
            }
        }

        return models;
    }

    public NewsItemModel deserialize(Dictionary<string,object> json,
                                     Type typeOfT)//, // TODO Implement JsonDeserializationContext
                                                  //JsonDeserializationContext context)
    {
        NewsItemModel model = new NewsItemModel();
        Dictionary<string,object> jsonItem = json;

        String type = jsonItem[TYPE] as string;
        model.setType(!String.IsNullOrEmpty(type) ? type : ARTICLE);
        //date
        //Per https://jira.mlbam.com/browse/ANDROID-4521 use timestamp first
        String articleRealseDate = jsonItem[TIMESTAMP] as string;
        if (String.IsNullOrEmpty(articleRealseDate))
        {
            articleRealseDate = jsonItem[USER_DATE] as string;
        }
        model.setDisplayDate(DateTime.Parse(articleRealseDate));

        model.setContentId(jsonItem[CONTENT_ID] as string);

        //TODO review naming convention in AtBat.
        model.setTitle(jsonItem[HEADLINE] as string);
        model.setSubheading(jsonItem[SUB_HEAD] as string);
        model.setByline(jsonItem[BYLINE] as string);
        model.setTagline(jsonItem[TAGLINE] as string);
        model.setBlurb(jsonItem[BLURB] as string);
        if (model.TYPE_SHORT_CONTENT.ToUpper() == model.getType())
        {
            String mobileUrl = jsonItem[MOBILE_URL] as string;
            if (!String.IsNullOrEmpty(mobileUrl))
            {
                if (containsHttp(mobileUrl))
                {
                    model.setMobileUrl(mobileUrl);
                }
                else
                {
                    //TODO Implement Override String
                    model.setMobileUrl(mobileUrl);// overrideStrings.getStringWithFormat(R.string.teampage_image_prefix, mobileUrl));
                    throw new Exception("Override Strings not implemented");
                }
            }
        }
        else if (model.TYPE_BLOG_CONTENT.ToUpper() == model.getType().ToUpper())
        {
            String mobileUrl = jsonItem[URL] as string;
            if (!String.IsNullOrEmpty(mobileUrl))
            {
                if (containsHttp(mobileUrl))
                {
                    model.setMobileUrl(mobileUrl);
                }
                else
                {
                    model.setMobileUrl(mobileUrl);
                    throw new Exception("Override Strings not implemented");
                    //TODO implement Override Stringsx
                    //model.setMobileUrl(overrideStrings.getStringWithFormat(R.string.news_articleprefix, mobileUrl));
                }
            }
        }
        else
        {
            //do nothing
        }

        //TODO questions about this from old implementation
        if (model.TYPE_SHORT_CONTENT.ToUpper() == model.getType()
                || model.TYPE_BLOG_CONTENT.ToUpper() ==model.getType())
        {
            model.setBody(model.getBlurb());
        }
        else
        {
            String articleBody = jsonItem[BODY] as string;
            if (!String.IsNullOrEmpty(articleBody))
            {
                //TODO Write a disable Links Helper (removes a tags)
                model.setBody(articleBody); //HtmlHelper.disableLinks(articleBody));
            }
        }

        model.setDisclaimer(jsonItem[APPROVAL] as string);

        //TODO Figure this out...
        /*
        Type type1 = new TypeToken<Map<String, NewsToken>>()
        {
        }.getType();
        model.setTokenIdToToken((Map<String, NewsToken>)context.deserialize(jsonItem[TOKEN_DATA], type1));
        */
        //TODO review all this
        Dictionary<string, object> photos = (jsonItem[PHOTOS] as List<Dictionary<string, object>>)[0] as Dictionary<string,object>;
        Dictionary<string, object> cuts = photos[CUTS] as Dictionary<string,object>;
        String imageUrl = deserializeImageUrl(cuts, SMALL, MEDIUM, LARGE);

        //TODO Setup proper URL image format
        model.setImageUrl(imageUrl);//String.format(GamedayApplication.getInstance().getString(R.string
                        //.teampage_image_prefix), imageUrl));

        //set default thumbnail
        String thumnailUrl = deserializeImageUrl(cuts, NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_270_154],
                NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_320_180], NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_480_270]);
        model.setThumbnailUrl(sanitizeNewsImageUrl(thumnailUrl));
        //set other photo items.
        Dictionary<String, NewsItemPhoto> itemPhotoDictionary = deserializeNewsItemPhotoDict(cuts);
        model.setNewsItemPhotoDictionary(itemPhotoDictionary);

        //set breaking news.
        Dictionary<string, object> itemTags = jsonItem[ITEM_TAGS] as Dictionary<string, object>;
        model.setIsBreakingNews(deserializeBreaking(itemTags));
        //related media
        List<Dictionary<string,object>> videos = jsonItem[RELATED_MEDIA] as List<Dictionary<string, object>> ;

        //TODO Implement Playback Scenario
        String[] allowedPlaybacks = new String[0];
                //PlaybackScenario.getPlaybacksForVOD(GamedayApplication.getInstance());
        Dictionary<String, Dictionary<string,object>> mediaUrlMap = new Dictionary<String, Dictionary<string, object>>();
        for (int i = 0; i < videos.Count; i++)
        {
            Dictionary<string, object> video = videos[i] as Dictionary<string,object>;
            Dictionary<string, object> mediaUrls = video[MEDIA_URLS] as Dictionary<string, object>;
            bool foundPlayableMedia = false;
            foreach (String pbs in allowedPlaybacks)
            {
                if (mediaUrls.ContainsKey(pbs))
                {
                    Dictionary<string, object> media = mediaUrls[pbs] as Dictionary<string, object>;
                    mediaUrlMap[pbs] = media[SRC] as Dictionary<string, object>;
                    foundPlayableMedia = true;
                }
            }
            if (foundPlayableMedia)
            {
                model.setMediaUrlDict(mediaUrlMap);
                model.setVideoCid(video[CONTENT_ID] as string);
                model.setVideoDuration(video[DURATION] as string);
                model.setGid(video[GUID] as string);
                //checking if video is sharable
                try
                {
                    List<String> mmtax = (video[ITEM_TAGS] as Dictionary<string,object>)[MMTAX] as List<String>;
                    for (int j = 0; j < mmtax.Count; j++)
                    {
                        if (model.CONST_SHARABLE.ToUpper() ==(mmtax[j] as string).ToUpper())
                        {
                            model.setVideoUrlSharable(true);
                        }
                    }
                }
                catch (Exception e)
                {
                    model.setVideoUrlSharable(false);
                }
                //
                model.setImageCaption(video[HEADLINE] as string);
                Dictionary<string, object> photoToUse = (video[THUMBNAILS] as Dictionary<string, object>)
                        [eNewsPhotoConstants.IMAGE_960_540.ToString()] as Dictionary<string, object>;
                model.setImageUrl(photoToUse[SRC] as string);
                 
                // if we previously could not find photos.
                // use thumbnails.
                if (model.getNewsItemPhotoDictionary().Count == 0)
                {
                    model.getNewsItemPhotoDictionary(
                            deserializeNewsItemPhotoDict(video[THUMBNAILS] as Dictionary<string,object>));
                }
                break;
            }

        }

        //
        String shareUrl = getSharedUrl(jsonItem[SHARED_URL] as string);

        if (!String.IsNullOrEmpty(shareUrl))
        {
            model.setShareUrl(shareUrl);
        }
        else
        {
            shareUrl = getSharedUrl(jsonItem[URL] as string);
            if (!String.IsNullOrEmpty(shareUrl))
            {
                model.setShareUrl(shareUrl);
            }
            throw new Exception("shared-url does not exist. Fallback to url");
        }

        return model;
    }

    public static bool deserializeBreaking(Dictionary<string, object> data)
    {
        bool isBreaking = false;
        try
        {
            List<String> breaking = data[BREAKING] as List<String>;
            for (int i = 0; i < breaking.Count; i++)
            {
                if (isBreakingNewsItem(breaking[i] as string))
                {
                    isBreaking = true;
                }
            }
        }
        catch (Exception e)
        {
            isBreaking = false;
            throw e;
        }
        return isBreaking;
    }

    //TODO Verify Works
    public static System.Collections.ObjectModel.ReadOnlyCollection<String> SET_BREAKING_NEWS_TAGS = createBreakingNewsTagsSet();

    private static System.Collections.ObjectModel.ReadOnlyCollection<String> createBreakingNewsTagsSet()
    {
        List<String> set = new List<String>();
        set.Add("mlb_alert");
        set.Add("team_alert");
        set.Add("global_alert");

        return new System.Collections.ObjectModel.ReadOnlyCollection<String>(set);
    }

    public static bool isBreakingNewsItem(String value)
    {
        return SET_BREAKING_NEWS_TAGS.Contains(value.ToLower());
    }

    public static Dictionary<String, NewsItemPhoto> deserializeNewsItemPhotoDict(Dictionary<string,object> data)
    {
            Dictionary<String, NewsItemPhoto> map = new Dictionary<String, NewsItemPhoto>();
        try
        {
            map[NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_1280_720]]=
                    deserializeNewsItemPhoto(data[NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_1280_720]]as Dictionary<string, object>);
            map[NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_960_540]] = 
                    deserializeNewsItemPhoto(data[NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_960_540]]as Dictionary<string, object>);
            map[NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_640_360]] = 
                    deserializeNewsItemPhoto(data[NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_640_360]]as Dictionary<string, object>);
            map[NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_480_270]] = 
                    deserializeNewsItemPhoto(data[NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_480_270]]as Dictionary<string, object>);
            map[NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_320_180]] = 
                    deserializeNewsItemPhoto(data[NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_320_180]]as Dictionary<string, object>);
            map[NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_270_154]] = 
                    deserializeNewsItemPhoto(data[NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_270_154]]as Dictionary<string, object>);
        }
        catch (Exception e)
        {
                throw e;
            //Timber.e(e.getMessage());
        }
        return map;
    }

    private static List<NewsItemPhoto> deserializeNewsItemPhotoList(Dictionary<string,object> data)
    {

        List<NewsItemPhoto> list = new List<NewsItemPhoto>();

        list.Add(deserializeNewsItemPhoto(
                data[NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_1280_720]] as Dictionary<string,object>));
        list.Add(deserializeNewsItemPhoto(
                data[NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_960_540]] as Dictionary<string,object>));
        list.Add(deserializeNewsItemPhoto(
                data[NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_640_360]] as Dictionary<string,object>));
        list.Add(deserializeNewsItemPhoto(
                data[NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_480_270]] as Dictionary<string,object>));
        list.Add(deserializeNewsItemPhoto(
                data[NewsPhotoConstants.ReturnImageResolutionsConvertion[eNewsPhotoConstants.IMAGE_320_180]] as Dictionary<string,object>));
        return list;
    }

    private static NewsItemPhoto deserializeNewsItemPhoto(Dictionary<string,object> data)
    {
        if (data == null)
        {
            return null;
        }

        NewsItemPhoto photo = new NewsItemPhoto();
        photo.setAspectRatio(data[ASPECT_RATIO] as string);
        photo.setHeight(data[HEIGHT] as int?); //Nullable int
        photo.setWidth(data[WIDTH] as int?);
        photo.setSrc(sanitizeNewsImageUrl(data[SRC] as string));
        return photo;
    }

    public static String sanitizeNewsImageUrl(String imageUrl)
    {
        if (!String.IsNullOrEmpty(imageUrl))
        {
            if (containsHttp(imageUrl))
            {
                return imageUrl;
            }
            else
            {
                return imageUrl;
                //TODO Implement GamedayApplication
                //return String.format(GamedayApplication.getInstance().getString(R
                //        .string.teampage_image_prefix), imageUrl);
            }
        }
        return imageUrl;
    }

    public static String deserializeImageUrl(Dictionary<string,object> data, String small, String medium, String
            large)
    {
            int screenDensity = UnityEngine.Mathf.RoundToInt(UnityEngine.Screen.dpi);

        switch (screenDensity)
        {
            case (int)DisplayMetrics.DENSITY_XXXHIGH:
            case (int)DisplayMetrics.DENSITY_560:
            case (int)DisplayMetrics.DENSITY_XXHIGH:
                return (data[large] as Dictionary<string,object>)[SRC] as string;
            case (int)DisplayMetrics.DENSITY_420:
            case (int)DisplayMetrics.DENSITY_400:
            case (int)DisplayMetrics.DENSITY_360:
            case (int)DisplayMetrics.DENSITY_XHIGH:
                return (data[medium] as Dictionary<string,object>)[SRC] as string;
            default:
                return (data[small] as Dictionary<string,object>)[SRC] as string;
        }
    }

    public static String getSharedUrl(String originalUrl)
    {
        if (!String.IsNullOrEmpty(originalUrl))
        {
            if (containsHttp(originalUrl))
            {
                return originalUrl;
            }
            else
            {
                return originalUrl;
                //TODO Implement GamedayApplication
                //return String.format(GamedayApplication.getInstance().getString(R.string
                //        .teampage_image_prefix), originalUrl.trim());
            }
        }
        return null;
    }

    public static bool containsHttp(String check)
    {
        return check.Contains("http") || check.Contains("https");
    }
}

public enum DisplayMetrics
{
    None = -1,
    DENSITY_XXXHIGH,
    DENSITY_560 = 560,
    DENSITY_XXHIGH,
    DENSITY_420 = 420,
    DENSITY_400 = 400,
    DENSITY_360 = 360,
    DENSITY_XHIGH,
    Length
}

public enum eItemTags
{
    None = -1,
    DATE_2016,
    DATE_2017,
    embeddable,
    shareable,
    Length
}

public static class NewsPhotoConstants
{
    public static Dictionary<eNewsPhotoConstants, string> ReturnImageResolutionsConvertion
    {
        get
        {
            if (_imageResolutionsConvertion == null)
                _imageResolutionsConvertion = buildResolutionsDictionary();
            return _imageResolutionsConvertion;
        }
    }
    private static Dictionary<eNewsPhotoConstants, string> _imageResolutionsConvertion;
    private static Dictionary<eNewsPhotoConstants, string> buildResolutionsDictionary()
    {
        Dictionary<eNewsPhotoConstants, string> returnValue = new Dictionary<eNewsPhotoConstants, string>();
        foreach (eNewsPhotoConstants resolution in Enum.GetValues(typeof(eNewsPhotoConstants)))
        {
            returnValue[resolution] = resolution.ToString().Replace("IMAGE_", "").Replace('_', 'x');
        }
        return returnValue;
    }
}

public enum eNewsPhotoConstants {
    None = -1,
    IMAGE_74_56,
    IMAGE_96_72,
    IMAGE_124_70,
    IMAGE_135_77,
    IMAGE_148_112,
    IMAGE_160_90,
    IMAGE_192_144,
    IMAGE_209_118,
    IMAGE_215_121,
    IMAGE_222_168,
    IMAGE_248_138,
    IMAGE_265_150,
    IMAGE_270_154,
    IMAGE_320_180,
    IMAGE_400_224,
    IMAGE_430_242,
    IMAGE_480_270,
    IMAGE_496_279,
    IMAGE_640_360,
    IMAGE_800_448,
    IMAGE_960_540,
    IMAGE_1024_576,
    IMAGE_1280_720,
    IMAGE_1600_900,
    Length
}
public class JsonParseException : Exception
{
    public JsonParseException()
    {

    }

    public JsonParseException(string message) : base(message)
    {

    }

    public JsonParseException(string message, Exception inner)
        :base(message,inner)
    {

    }
}