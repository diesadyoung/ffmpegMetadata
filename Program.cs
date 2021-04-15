using System;
using System.Drawing;
using FFmpeg.AutoGen;

static void ReciveMetadata(string url)
{
    this.Dispose();


    ffmpeg.av_register_all();
    ffmpeg.avcodec_register_all();
    ffmpeg.avformat_network_init();

    // Открытие
    AVFormatContext* pFormatContext = ffmpeg.avformat_alloc_context();
    if (ffmpeg.avformat_open_input(&pFormatContext, url, null, null) != 0)
        throw new Exception("File or URL not found!");

    if (ffmpeg.avformat_find_stream_info(pFormatContext, null) != 0)
        throw new Exception("No stream information found!");

    // Сбор гланой инфы
    this.Filename = new String(pFormatContext->filename);
    this.Metadata = ToDictionary(pFormatContext->metadata);
    this.Duration = ToTimeSpan(pFormatContext->duration);
    this.BitRate = pFormatContext->bit_rate;

    // Сбор инфы

    for (int i = 0; i < pFormatContext->nb_streams; i++)
    {

        FFmpegStreamInfo info = new FFmpegStreamInfo();
        info.AVStream = pFormatContext->streams[i];
        info.Index = pFormatContext->streams[i]->index;
        switch (pFormatContext->streams[i]->codec->codec_type)
        {
            case AVMediaType.AVMEDIA_TYPE_VIDEO:
                info.StreamType = FFmpegStreamType.Video;
                info.Video_Width = pFormatContext->streams[i]->codec->width;
                info.Video_Height = pFormatContext->streams[i]->codec->height;
                info.Video_FPS = ToDouble(pFormatContext->streams[i]->codec->framerate);
                if (isFirstVideoStream)
                {
                    this.VideoResolution = new Size(info.Video_Width, info.Video_Height);
                    isFirstVideoStream = false;
                }
                break;
        }
        info.MetaData = ToDictionary(pFormatContext->streams[i]->metadata);
        info.BitRate = pFormatContext->streams[i]->codec->bit_rate;
        this.Streams.Add(info);
    }

    this.AVFormatContext = pFormatContext;
    isDisposed = false;
}