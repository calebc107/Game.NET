// D3DInterop.cpp : Defines the exported functions for the DLL application.
//

//Most of this code has been aquired from https://msdn.microsoft.com/en-us/library/windows/desktop/hh780339(v=vs.85).aspx


#include "stdafx.h"
#include <exception>
#include "d2d1.h"
#include "d2d1_1.h"
#include "d2d1_1helper.h"
#include "d2d1effects_2.h"
#include "d3d11.h"
#include "d3dInterop.h"
#include "dxgi.h"
#include "dxgi1_2.h"
#include "dwrite.h"
#include "wincodec.h"
#include "wrl.h"
#include "xaudio2.h"
using Microsoft::WRL::ComPtr;
using D2D1::BitmapProperties1;
using D2D1::PixelFormat;

namespace DX
{
	inline void ThrowIfFailed(HRESULT hr)
	{
		if (FAILED(hr))
		{
			// Set a breakpoint on this line to catch DirectX API errors
			//throw std::exception();
		}
	}
}
template <class T> void SafeRelease(T **ppT)
{
	if (*ppT)
	{
		(*ppT)->Release();
		*ppT = NULL;
	}
}

ComPtr<ID2D1DeviceContext> m_d2dContext;
ComPtr<ID2D1BitmapRenderTarget> m_d2dBitmapRender;
ComPtr<IDXGISwapChain1> m_swapChain;
ComPtr<ID2D1Bitmap1> m_d2dTargetBitmap;
ComPtr<IWICImagingFactory> m_pWICFactory;
ComPtr<IDWriteFactory> m_DWriteFactory;
ComPtr<IXAudio2> m_XAudio2;
IXAudio2MasteringVoice* m_MasteringVoice;
ID2D1Effect* lightingEffect;
ID2D1Effect* invert;

void Init(void* handle, int x, int y, int fps)
{

	// This flag adds support for surfaces with a different color channel ordering than the API default.
	// You need it for compatibility with Direct2D.
	UINT creationFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;

	// This array defines the set of DirectX hardware feature levels this app  supports.
	// The ordering is important and you should  preserve it.
	// Don't forget to declare your app's minimum required feature level in its
	// description.  All apps are assumed to support 9.1 unless otherwise stated.
	D3D_FEATURE_LEVEL featureLevels[] =
	{
		D3D_FEATURE_LEVEL_11_1,
		D3D_FEATURE_LEVEL_11_0,
		D3D_FEATURE_LEVEL_10_1,
		D3D_FEATURE_LEVEL_10_0,
		D3D_FEATURE_LEVEL_9_3,
		D3D_FEATURE_LEVEL_9_2,
		D3D_FEATURE_LEVEL_9_1
	};

	// Create the DX11 API device object, and get a corresponding context.
	ComPtr<ID3D11Device> device;
	ComPtr<ID3D11DeviceContext> context;

	DX::ThrowIfFailed(
		D3D11CreateDevice(
			nullptr,                    // specify null to use the default adapter
			D3D_DRIVER_TYPE_HARDWARE,
			0,
			creationFlags,              // optionally set debug and Direct2D compatibility flags
			featureLevels,              // list of feature levels this app can support
			ARRAYSIZE(featureLevels),   // number of possible feature levels
			D3D11_SDK_VERSION,
			&device,                    // returns the Direct3D device created
			NULL,            // returns feature level of device created
			&context                    // returns the device immediate context
		)
	);

	ComPtr<IDXGIDevice1> dxgiDevice;
	// Obtain the underlying DXGI device of the Direct3D11 device.
	DX::ThrowIfFailed(
		device.As(&dxgiDevice)
	);

	ID2D1Factory1* m_d2dFactory;
	D2D1CreateFactory(D2D1_FACTORY_TYPE_MULTI_THREADED, &m_d2dFactory);

	ComPtr<ID2D1Device> m_d2dDevice;

	// Obtain the Direct2D device for 2-D rendering.
	DX::ThrowIfFailed(
		m_d2dFactory->CreateDevice(dxgiDevice.Get(), &m_d2dDevice)
	);

	// Get Direct2D device's corresponding device context object.

	DX::ThrowIfFailed(
		m_d2dDevice->CreateDeviceContext(
			D2D1_DEVICE_CONTEXT_OPTIONS_NONE | D2D1_DEVICE_CONTEXT_OPTIONS_ENABLE_MULTITHREADED_OPTIMIZATIONS,
			&m_d2dContext
		)
	);
	// Allocate a descriptor.
	DXGI_SWAP_CHAIN_DESC1 swapChainDesc = { 0 };
	swapChainDesc.Width = 0;                           // use automatic sizing
	swapChainDesc.Height = 0;
	swapChainDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM; // this is the most common swapchain format
	swapChainDesc.Stereo = false;
	swapChainDesc.SampleDesc.Count = 1;                // don't use multi-sampling
	swapChainDesc.SampleDesc.Quality = 0;
	swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
	swapChainDesc.BufferCount = 2;                     // use double buffering to enable flip
	swapChainDesc.Scaling = DXGI_SCALING_NONE;
	swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL; // all apps must use this SwapEffect
	swapChainDesc.Flags = 0;

	// Identify the physical adapter (GPU or card) this device is runs on.
	ComPtr<IDXGIAdapter> dxgiAdapter;
	DX::ThrowIfFailed(
		dxgiDevice->GetAdapter(&dxgiAdapter)
	);

	// Get the factory object that created the DXGI device.
	ComPtr<IDXGIFactory2> dxgiFactory;
	DX::ThrowIfFailed(
		dxgiAdapter->GetParent(IID_PPV_ARGS(&dxgiFactory))
	);

	// Get the final swap chain for this window from the DXGI factory.

	DX::ThrowIfFailed(
		dxgiFactory->CreateSwapChainForHwnd(
			device.Get(),
			(HWND)handle,
			&swapChainDesc,
			NULL,
			nullptr,
			&m_swapChain)
		/*dxgiFactory->CreateSwapChainForCoreWindow(
		device.Get(),
		reinterpret_cast<IUnknown*>(m_window),
		&swapChainDesc,
		nullptr,    // allow on all displays
		&m_swapChain
		)*/
	);

	// Ensure that DXGI doesn't queue more than one frame at a time.

	DX::ThrowIfFailed(
		dxgiDevice->SetMaximumFrameLatency(1)
	);

	// Get the backbuffer for this window which is be the final 3D render target.
	ComPtr<ID3D11Texture2D> backBuffer;
	DX::ThrowIfFailed(
		m_swapChain->GetBuffer(0, IID_PPV_ARGS(&backBuffer))
	);

	// Now we set up the Direct2D render target bitmap linked to the swapchain. 
	// Whenever we render to this bitmap, it is directly rendered to the 
	// swap chain associated with the window.
	FLOAT m_dpi = 100;
	D2D1_BITMAP_PROPERTIES1 bitmapProperties =
		BitmapProperties1(
			D2D1_BITMAP_OPTIONS_TARGET | D2D1_BITMAP_OPTIONS_CANNOT_DRAW,
			PixelFormat(DXGI_FORMAT_B8G8R8A8_UNORM, D2D1_ALPHA_MODE_IGNORE),
			m_dpi,
			m_dpi
		);

	// Direct2D needs the dxgi version of the backbuffer surface pointer.
	ComPtr<IDXGISurface> dxgiBackBuffer;
	DX::ThrowIfFailed(
		m_swapChain->GetBuffer(0, IID_PPV_ARGS(&dxgiBackBuffer))
	);

	// Get a D2D surface from the DXGI back buffer to use as the D2D render target.

	DX::ThrowIfFailed(
		m_d2dContext->CreateBitmapFromDxgiSurface(
			dxgiBackBuffer.Get(),
			&bitmapProperties,
			&m_d2dTargetBitmap
		)
	);

	// Now we can set the Direct2D render target.
	m_d2dContext->SetTarget(m_d2dTargetBitmap.Get());

	//setbackbuffer
	m_d2dContext->CreateCompatibleRenderTarget(&m_d2dBitmapRender);

	//create effects for lighting
	m_d2dContext->CreateEffect(CLSID_D2D1LuminanceToAlpha, &lightingEffect);
	m_d2dContext->CreateEffect(CLSID_D2D1Invert, &invert);

	CoCreateInstance(
		CLSID_WICImagingFactory, nullptr,
		CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&m_pWICFactory));

	//directwrite factory
	DWriteCreateFactory(DWRITE_FACTORY_TYPE_SHARED, __uuidof(IDWriteFactory), &m_DWriteFactory);

	//Xaudio2
	XAudio2Create(&m_XAudio2);
	m_XAudio2->CreateMasteringVoice(&m_MasteringVoice);
}

void DeInit() {
	m_MasteringVoice->DestroyVoice();
	m_XAudio2.Reset();
	m_DWriteFactory.Reset();
	m_pWICFactory.Reset();
	m_d2dBitmapRender.Reset();
	m_d2dTargetBitmap.Reset();
	m_swapChain.Reset();
	m_d2dContext.Reset();
}

//from https://msdn.microsoft.com/en-us/library/windows/desktop/dd940435(v=vs.85).aspx
void LoadBitmapFromFile(
	PCWSTR uri,
	ID2D1Bitmap **ppBitmap
)
{
	IWICBitmapDecoder *pDecoder = NULL;
	IWICBitmapFrameDecode *pSource = NULL;
	IWICFormatConverter *pConverter = NULL;

	m_pWICFactory->CreateDecoderFromFilename(
		uri,
		NULL,
		GENERIC_READ,
		WICDecodeMetadataCacheOnDemand,
		&pDecoder
	);

	// Create the initial frame.
	pDecoder->GetFrame(0, &pSource);

	// Convert the image format to 32bppPBGRA
	// (DXGI_FORMAT_B8G8R8A8_UNORM + D2D1_ALPHA_MODE_PREMULTIPLIED).
	m_pWICFactory->CreateFormatConverter(&pConverter);

	pConverter->Initialize(
		pSource,
		GUID_WICPixelFormat32bppPBGRA,
		WICBitmapDitherTypeNone,
		NULL,
		0.f,
		WICBitmapPaletteTypeMedianCut);

	m_d2dContext->CreateBitmapFromWicBitmap(
		pConverter,
		NULL,
		ppBitmap
	);

	SafeRelease(&pDecoder);
	SafeRelease(&pSource);
	SafeRelease(&pConverter);
}

void* BitmapFromGDI(int width, int height, void* stream, int stride) {
	D2D1_SIZE_U* size = new D2D1_SIZE_U;
	size->height = height;
	size->width = width;
	D2D1_PIXEL_FORMAT* pixfmt = new D2D1_PIXEL_FORMAT;
	pixfmt->alphaMode = D2D1_ALPHA_MODE::D2D1_ALPHA_MODE_PREMULTIPLIED;
	pixfmt->format = DXGI_FORMAT::DXGI_FORMAT_B8G8R8A8_UNORM;
	D2D1_BITMAP_PROPERTIES* props = new D2D1_BITMAP_PROPERTIES;
	props->pixelFormat = *pixfmt;
	props->dpiX = 96;
	props->dpiY = 96;
	ID2D1Bitmap* pBitmap;
	HRESULT hr = m_d2dContext->CreateBitmap(*size, stream, stride, props, &pBitmap);
	return pBitmap;
}

HRESULT FindChunk(HANDLE hFile, DWORD fourcc, DWORD & dwChunkSize, DWORD & dwChunkDataPosition)
{
	HRESULT hr = S_OK;
	if (INVALID_SET_FILE_POINTER == SetFilePointer(hFile, 0, NULL, FILE_BEGIN))
		return HRESULT_FROM_WIN32(GetLastError());

	DWORD dwChunkType;
	DWORD dwChunkDataSize;
	DWORD dwRIFFDataSize = 0;
	DWORD dwFileType;
	DWORD bytesRead = 0;
	DWORD dwOffset = 0;


	while (hr == S_OK)
	{
		DWORD dwRead;
		if (0 == ReadFile(hFile, &dwChunkType, sizeof(DWORD), &dwRead, NULL))
			hr = HRESULT_FROM_WIN32(GetLastError());

		if (0 == ReadFile(hFile, &dwChunkDataSize, sizeof(DWORD), &dwRead, NULL))
			hr = HRESULT_FROM_WIN32(GetLastError());

		switch (dwChunkType)
		{
		case 'FFIR':
			dwRIFFDataSize = dwChunkDataSize;
			dwChunkDataSize = 4;
			if (0 == ReadFile(hFile, &dwFileType, sizeof(DWORD), &dwRead, NULL))
				hr = HRESULT_FROM_WIN32(GetLastError());
			break;

		default:
			if (INVALID_SET_FILE_POINTER == SetFilePointer(hFile, dwChunkDataSize, NULL, FILE_CURRENT))
				return HRESULT_FROM_WIN32(GetLastError());
		}

		dwOffset += sizeof(DWORD) * 2;

		if (dwChunkType == fourcc)
		{
			dwChunkSize = dwChunkDataSize;
			dwChunkDataPosition = dwOffset;
			return S_OK;
		}

		dwOffset += dwChunkDataSize;

		if (bytesRead >= dwRIFFDataSize) return S_FALSE;

	}

	return S_OK;

}

HRESULT ReadChunkData(HANDLE hFile, void * buffer, DWORD buffersize, DWORD bufferoffset)
{
	HRESULT hr = S_OK;
	if (INVALID_SET_FILE_POINTER == SetFilePointer(hFile, bufferoffset, NULL, FILE_BEGIN))
		return HRESULT_FROM_WIN32(GetLastError());
	DWORD dwRead;
	if (0 == ReadFile(hFile, buffer, buffersize, &dwRead, NULL))
		hr = HRESULT_FROM_WIN32(GetLastError());
	return hr;
}

void DrawBitmap(void* bitmapPtr, int x, int y, int rot) {
	ID2D1Bitmap* bitmap = (ID2D1Bitmap*)bitmapPtr;
	D2D1_SIZE_F size = bitmap->GetSize();
	D2D1_RECT_F rect = D2D1::Rect(
		x - size.width / 2,
		y - size.height / 2,
		x + size.width / 2,
		y + size.height / 2);
	D2D_MATRIX_3X2_F* transform = new D2D1_MATRIX_3X2_F;
	if (rot != 0) {
		D2D1MakeRotateMatrix(-1 * rot, D2D1::Point2F(x, y), transform);
		m_d2dContext->SetTransform(*transform);
	}
	m_d2dContext->DrawBitmap(
		bitmap,
		rect,
		1);
		delete(transform);
}

void RtDrawText(char* string, int length, char* fontname, float size, int x, int y, void* brush) {
	IDWriteTextFormat* format;
	IDWriteTextLayout* layout;
	WCHAR* fontstr = new WCHAR[512];
	WCHAR* stringstr = new WCHAR[length];
	MultiByteToWideChar(CP_ACP, 0, fontname, -1, fontstr, 512);
	MultiByteToWideChar(CP_ACP, 0, string, length, stringstr, length);
	HRESULT hr = m_DWriteFactory->CreateTextFormat(fontstr, NULL, DWRITE_FONT_WEIGHT_NORMAL, DWRITE_FONT_STYLE_NORMAL, DWRITE_FONT_STRETCH_NORMAL, size, (WCHAR*)L"", &format);
	hr = m_DWriteFactory->CreateTextLayout(stringstr, length, format, length*size, size, &layout);
	FLOAT* width = new FLOAT();
	DWRITE_TEXT_METRICS* metrics = new DWRITE_TEXT_METRICS();
	hr = layout->GetMetrics(metrics);
	*width = metrics->width;

	D2D1_RECT_F rect = D2D1_RECT_F();
	rect.bottom = y + size / 2;
	rect.top = y - size / 1.5;
	rect.left = x - *width / 2;
	rect.right = x + *width / 2;

	m_d2dContext->DrawTextW(stringstr, (UINT32)length, layout, &rect, (ID2D1Brush*)brush, D2D1_DRAW_TEXT_OPTIONS_NONE, DWRITE_MEASURING_MODE_NATURAL);
	format->Release();
	layout->Release();
	delete(width);
	delete(metrics);
	delete(fontstr);
	delete(stringstr);
}

void BeginDraw() {
	m_d2dContext->BeginDraw();
	m_d2dBitmapRender->BeginDraw();
}

void EndDraw(bool lighting) {

	m_d2dContext->SetTransform(D2D1::Matrix3x2F::Rotation(0, D2D1::Point2F(0, 0)));
	if (lighting) {
		ID2D1Bitmap* bitmap;
		m_d2dBitmapRender->EndDraw();
		m_d2dBitmapRender->GetBitmap(&bitmap);

		ID2D1Image* intermediate;

		invert->SetInput(0, bitmap);
		invert->GetOutput(&intermediate);
		lightingEffect->SetInput(0, intermediate);
		m_d2dContext->DrawImage(lightingEffect, D2D1_INTERPOLATION_MODE_LINEAR);
		intermediate->Release();
		bitmap->Release();
	}
	else {
		m_d2dBitmapRender->EndDraw();
	}

	DX::ThrowIfFailed(
		m_d2dContext->EndDraw()
	);
}

void Clear()
{
	m_d2dContext->Clear();
	m_d2dBitmapRender->Clear(D2D1::ColorF(0, 0, 0, 1));
}

void* LoadBitmapFile(char* path) {
	wchar_t* wString = new wchar_t[4096];
	MultiByteToWideChar(CP_ACP, 0, path, -1, wString, 4096);
	ID2D1Bitmap* bitmapPtr = nullptr;
	LoadBitmapFromFile(wString, &bitmapPtr);
	delete(wString);
	return bitmapPtr;
}

void DrawLight(void* bitmapPtr, int x, int y) {
	ID2D1Bitmap* bitmap = (ID2D1Bitmap*)bitmapPtr;
	D2D1_SIZE_F size = bitmap->GetSize();
	D2D1_RECT_F rect = D2D1::Rect(
		x - size.width / 2,
		y - size.height / 2,
		x + size.width / 2,
		y + size.height / 2);

	m_d2dBitmapRender->DrawBitmap(
		bitmap,
		rect,
		1);
}

void* CreateBrush(float r, float g, float b) {
	ID2D1SolidColorBrush* brush;
	m_d2dContext->CreateSolidColorBrush(D2D1::ColorF(r, g, b, 1.0f), &brush);
	return brush;
}

void* LoadWAVFile(char* path, float repeatAt, void** bufferLocation, void** xaudiobufloc) {
	WAVEFORMATEXTENSIBLE wfx = { 0 };
	XAUDIO2_BUFFER* buffer = new XAUDIO2_BUFFER();
	wchar_t* strFileName = new wchar_t[64];
	HRESULT hr = MultiByteToWideChar(CP_ACP, 0, path, -1, strFileName, 64);
	// Open the file
	HANDLE hFile = CreateFile(
		strFileName,
		GENERIC_READ,
		FILE_SHARE_READ,
		NULL,
		OPEN_EXISTING,
		0,
		NULL);

	DWORD dwChunkSize;
	DWORD dwChunkPosition;

	//check the file type, should be fourccWAVE or 'XWMA'
	FindChunk(hFile, 'FFIR', dwChunkSize, dwChunkPosition);
	DWORD filetype;
	ReadChunkData(hFile, &filetype, sizeof(DWORD), dwChunkPosition);
	if (filetype != 'EVAW')
		throw std::exception();
	FindChunk(hFile, ' tmf', dwChunkSize, dwChunkPosition);
	ReadChunkData(hFile, &wfx, dwChunkSize, dwChunkPosition);
	IXAudio2SourceVoice* SourceVoice;
	m_XAudio2->CreateSourceVoice(&SourceVoice, (WAVEFORMATEX*)&wfx);
	XAUDIO2_VOICE_DETAILS details;
	SourceVoice->GetVoiceDetails(&details);

	//fill out the audio data buffer with the contents of the fourccDATA chunk
	FindChunk(hFile, 'atad', dwChunkSize, dwChunkPosition);
	BYTE * pDataBuffer = new BYTE[dwChunkSize];
	ReadChunkData(hFile, pDataBuffer, dwChunkSize, dwChunkPosition);
	buffer->AudioBytes = dwChunkSize;  //buffer containing audio data
	buffer->pAudioData = pDataBuffer;  //size of the audio buffer in bytes

	if (repeatAt >= 0) {
		buffer->LoopBegin = details.InputSampleRate*repeatAt;
		buffer->LoopCount = XAUDIO2_LOOP_INFINITE;
	}
	buffer->Flags = XAUDIO2_END_OF_STREAM; // tell the source voice not to expect any data after this buffer

	//SourceVoice->SubmitSourceBuffer(buffer);
	delete(strFileName);
	*bufferLocation = pDataBuffer;
	*xaudiobufloc = buffer;
	return SourceVoice;
}

void PlaySourceVoice(void* voidSourceVoice, void* buffer) {
	IXAudio2SourceVoice* SourceVoice = (IXAudio2SourceVoice*)voidSourceVoice;
	SourceVoice->Stop();
	SourceVoice->FlushSourceBuffers();
	XAUDIO2_BUFFER* xaudiobufloc = (XAUDIO2_BUFFER*)buffer;
	SourceVoice->SubmitSourceBuffer(xaudiobufloc);
	SourceVoice->Start();
}
void StopSourceVoice(void* voidSourceVoice) {
	IXAudio2SourceVoice* SourceVoice = (IXAudio2SourceVoice*)voidSourceVoice;
	SourceVoice->Stop();
}

void Present() {
	DX::ThrowIfFailed(
		m_swapChain->Present(1, 0)
	);
}

void Resize(int w, int h, int fps, bool fullscreen) {
	m_swapChain->ResizeBuffers(0, w, h, DXGI_FORMAT_UNKNOWN, 0);
	DXGI_RATIONAL refreshrate = DXGI_RATIONAL();
	refreshrate.Denominator = fps;
	refreshrate.Numerator = 1;
	DXGI_MODE_DESC* desc = new DXGI_MODE_DESC();
	desc->Height = h;
	desc->Width = w;
	desc->RefreshRate = refreshrate;

	desc->Format = DXGI_FORMAT_B8G8R8A8_UNORM;
	desc->Scaling = DXGI_MODE_SCALING_CENTERED;
	m_swapChain->ResizeTarget(desc);
	m_swapChain->SetFullscreenState(fullscreen, NULL);
}

void DisposeBitmap(void* ptr) {
	ID2D1Resource* bitmap = (ID2D1Resource*)ptr;
	SafeRelease(&bitmap);
}
void DisposeSourceVoice(void* ptr, void* bufferloc, void* vxaudiobuf) {
	IXAudio2SourceVoice* sourcevoice = (IXAudio2SourceVoice*)ptr;
	sourcevoice->Stop();
	sourcevoice->FlushSourceBuffers();
	sourcevoice->DestroyVoice();
	delete(vxaudiobuf);
	delete(bufferloc);
}