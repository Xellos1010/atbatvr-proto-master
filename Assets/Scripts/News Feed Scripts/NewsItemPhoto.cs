using System;
using System.Collections.Generic;

/**
 * Created by bah on 2/23/16.
 * Class that holds information about a {@link NewsItemModel} photo
 */
 [System.Serializable]
public class NewsItemPhoto
{
    private String aspectRatio;
    private int? width;
    private int? height;
    private String src;


    public String getAspectRatio()
    {
        return aspectRatio;
    }

    public void setAspectRatio(String aspectRatio)
    {
        this.aspectRatio = aspectRatio;
    }

    public int? getWidth()
    {
        return width;
    }

    public void setWidth(int? width)
    {
        this.width = width;
    }

    public int? getHeight()
    {
        return height;
    }

    public void setHeight(int? height)
    {
        this.height = height;
    }

    public String getSrc()
    {
        return src;
    }

    public void setSrc(String src)
    {
        this.src = src;
    }

    /*@Override
        public int describeContents()
    {
        return 0;
    }

    @Override
        public void writeToParcel(Parcel dest, int flags)
    {
        dest.writeString(this.aspectRatio);
        dest.writeInt(this.width);
        dest.writeInt(this.height);
        dest.writeString(this.src);
    }*/

    public NewsItemPhoto()
    {
    }

    /*protected NewsItemPhoto(Parcel in)
    {
        this.aspectRatio = in.readString();
        this.width = in.readInt();
        this.height = in.readInt();
        this.src = in.readString();
    }

    public static final Parcelable.Creator<NewsItemPhoto> CREATOR = new Parcelable.Creator<NewsItemPhoto>()
    {
            public NewsItemPhoto createFromParcel(Parcel source)
    {
        return new NewsItemPhoto(source);
    }*/

    public NewsItemPhoto[] newArray(int size)
    {
        return new NewsItemPhoto[size];
    }
}

