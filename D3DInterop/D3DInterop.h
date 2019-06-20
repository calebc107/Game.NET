#pragma once
extern "C" {
	//Ulilities
	__declspec(dllexport) void Init(void* handle, int x, int y, int fps);
	__declspec(dllexport) void DeInit();
	__declspec(dllexport) void* LoadBitmapFile(char* path);
	__declspec(dllexport) void* BitmapFromGDI(int width, int height, void* stream, int stride);
	__declspec(dllexport) void* LoadWAVFile(char* path, float repeatAt, void** bufferloc, void** xaudiobufloc);
	__declspec(dllexport) void* CreateBrush(float r, float g, float b);
	__declspec(dllexport) void Resize(int w, int h, int fps, bool fullscreen);

	//Video
	__declspec(dllexport) void BeginDraw();
	__declspec(dllexport) void EndDraw(bool ligning);
	__declspec(dllexport) void Clear();
	__declspec(dllexport) void DrawBitmap(void* bitmapPtr, int x, int y, int rot);
	__declspec(dllexport) void DrawLight(void* bitmapPtr, int x, int y);
	__declspec(dllexport) void Present();
	__declspec(dllexport) void DisposeBitmap(void* ptr);
	__declspec(dllexport) void RtDrawText(char* string, int length, char* fontname, float size, int x, int y, void* brush);
	
	//Audio
	__declspec(dllexport) void PlaySourceVoice(void* voidSourceVoice,void* buffer);
	__declspec(dllexport) void DisposeSourceVoice(void* ptr,void* bufferloc, void* vxaudiobufloc);
}
