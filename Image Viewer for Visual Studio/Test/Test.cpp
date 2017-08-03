// Test.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

extern "C"
{
#include <libavcodec/avcodec.h>
#include <libavformat/avformat.h>
#include <libavutil/imgutils.h>
#include <libavutil/error.h>
#include <libavutil/opt.h>
#include <libswresample/swresample.h>
#include <libswscale/swscale.h>
}

#include <stdexcept>
#include <cstdio>
#include <cstdlib>
#include <iostream>
#include <fstream>


inline void ThrowOnAVError(int error)
{
	if (error >= 0)
		return;

	char errorMessage[1024];
	av_strerror(error, errorMessage, sizeof(errorMessage));
	
	throw std::runtime_error(errorMessage);
}

int main()
{
	try
	{
		av_register_all();
		//AVOutputFormat* fmt = av_guess_format(0, "D:\\Dropbox\\MULTIMEDIA\\Images 2006-04-01\\Photos\\2017\\(2017-03) Grande Canarie\\20170311_113219.jpg", 0);
		AVFormatContext *pFormatCtx = nullptr;
		ThrowOnAVError(avformat_open_input(&pFormatCtx, "D:\\Dropbox\\MULTIMEDIA\\Images 2006-04-01\\Photos\\2017\\(2017-03) Grande Canarie\\20170311_113219.jpg", NULL, 0));
		ThrowOnAVError(avformat_find_stream_info(pFormatCtx, NULL));
		AVPacket packet;
		av_init_packet(&packet);
		AVCodecParameters* pCodecParameters = pFormatCtx->streams[0]->codecpar;
		AVCodec         *pCodec = avcodec_find_decoder(pCodecParameters->codec_id);

		auto pCodecCtx = avcodec_alloc_context3(pCodec);

		ThrowOnAVError(avcodec_parameters_to_context(pCodecCtx, pCodecParameters));
		ThrowOnAVError(avcodec_open2(pCodecCtx, pCodec, NULL));

		AVPacket* packetIn = av_packet_alloc();
		AVFrame* frameOut = av_frame_alloc();
		ThrowOnAVError(av_read_frame(pFormatCtx, packetIn));
		ThrowOnAVError(avcodec_send_packet(pCodecCtx, packetIn));
		ThrowOnAVError(avcodec_receive_frame(pCodecCtx, frameOut));

		AVFrame* frameOutRGB24 = av_frame_alloc();
		frameOutRGB24->width = 128;
		frameOutRGB24->height = 128;
		frameOutRGB24->format = AV_PIX_FMT_RGB24;
		frameOutRGB24->nb_samples = 1;

		int numBytes;
		ThrowOnAVError(numBytes = av_image_get_buffer_size((AVPixelFormat)frameOutRGB24->format, frameOutRGB24->width, frameOutRGB24->height, 1));
		uint8_t* frameOutRGB24Buffer = (uint8_t *)av_malloc(numBytes);
		av_image_fill_arrays(frameOutRGB24->data, frameOutRGB24->linesize, frameOutRGB24Buffer, (AVPixelFormat)frameOutRGB24->format, frameOutRGB24->width, frameOutRGB24->height, 1);
		SwsContext* img_convert_ctx;
		img_convert_ctx = sws_getContext(frameOut->width, frameOut->height, (AVPixelFormat)frameOut->format, frameOutRGB24->width, frameOutRGB24->height, (AVPixelFormat)frameOutRGB24->format, SWS_BICUBIC, NULL, NULL, NULL);
		auto ret = sws_scale(img_convert_ctx, frameOut->data, frameOut->linesize, 0, frameOut->height, frameOutRGB24->data, frameOutRGB24->linesize);
		std::ofstream out("d:\\out.raw", std::ios::binary);
		out.write((char*)(frameOutRGB24->data[0]), numBytes);
		out.close();
		std::cout << "OK" << std::endl;
	}
	catch (const std::exception& exception)
	{
		std::cerr << exception.what() << std::endl;
	}
	
    return 0;
}

