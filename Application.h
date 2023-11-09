#pragma once

#include <windows.h>
#include <d3d11_1.h>
#include <d3dcompiler.h>
#include <directxmath.h>
#include <directxcolors.h>
#include "resource.h"
#include "OBJLoader.h"
#include "Camera.h"
#include "AttractionPointManager.h"

#include <GeometricPrimitive.h>

using namespace DirectX;

class Application
{
private:
	HINSTANCE               _hInst;
	HWND                    _hWnd;
	D3D_DRIVER_TYPE         _driverType;
	D3D_FEATURE_LEVEL       _featureLevel;
	ID3D11Device*           _pd3dDevice;
	ID3D11DeviceContext*    _pImmediateContext;
	IDXGISwapChain*         _pSwapChain;
	ID3D11RenderTargetView* _pRenderTargetView;
	ID3D11VertexShader*     _pVertexShader;
	ID3D11PixelShader*      _pPixelShader;
	ID3D11InputLayout*      _pVertexLayout;
	ID3D11Buffer*           _pConstantBuffer;
	XMFLOAT4X4              _world, _world2, _world3; //worlds for 3 different .obj files
	
	//attraction points
	AttractionPointManager* _attractionPointManager;
	
	//std::vector<XMFLOAT4X4> _attractionPoints;

	//wireframe + solid states
	ID3D11RasterizerState* _wireFrame;
	ID3D11RasterizerState* _solid;
	
	//depth buffer
	ID3D11Texture2D* _depthStencilBuffer;
	ID3D11DepthStencilView* _depthStencilView;

	//texturing
	ID3D11ShaderResourceView* _pTextureRV;
	ID3D11SamplerState* _pSamplerLinear;

	//obj loads 
	MeshData _Mesh1;
	MeshData _Mesh2;
	MeshData _Mesh3;

	GeometricPrimitive::VertexCollection crownVertices;
	GeometricPrimitive::IndexCollection crownIndices;

	//camera
	Camera* _camera;

private:
	HRESULT InitWindow(HINSTANCE hInstance, int nCmdShow);
	HRESULT InitDevice();
	void Cleanup();
	HRESULT CompileShaderFromFile(WCHAR* szFileName, LPCSTR szEntryPoint, LPCSTR szShaderModel, ID3DBlob** ppBlobOut);
	HRESULT InitShadersAndInputLayout();

	//size of window
	UINT _WindowHeight;
	UINT _WindowWidth;

	//lighting section! 

	//diffuse floats
	XMFLOAT4 DiffuseMaterial;
	XMFLOAT4 DiffuseLight;
	XMFLOAT3 directionToLight;

	//ambient floats
	XMFLOAT4 AmbientLight;
	XMFLOAT4 AmbientMaterial;

	//specular floats
	XMFLOAT4 SpecularMaterial;
	XMFLOAT4 SpecularLight;
	FLOAT SpecularPower; //Power to raise specular falloff by
	XMFLOAT3 EyeWorldPos; //Camera's eye position in the world

	//prim geo
	std::unique_ptr<GeometricPrimitive> shape;
	std::unique_ptr<GeometricPrimitive> custom;
public:
	Application();
	~Application();

	HRESULT Initialise(HINSTANCE hInstance, int nCmdShow);

	void Update();
	void Draw();

	void OnMouseDown(WPARAM btnState, int x, int y);
	void OnMouseUp(WPARAM btnState, int x, int y);
	void OnMouseMove(WPARAM btnState, int x, int y);

	virtual LRESULT MsgProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);

	POINT mLastMousePos;
};

