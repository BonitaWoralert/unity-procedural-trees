#pragma once
#include <windows.h>
#include <d3d11_1.h>
#include <d3dcompiler.h>
#include <directxmath.h>
#include <directxcolors.h>
#include "resource.h"
using namespace DirectX;

class Camera
{
public:
	//constructor and destructor for camera
	Camera();
	~Camera();

	//getset world cam pos
	XMVECTOR GetPositionXM() const { return XMLoadFloat3(&mPosition); }
	XMFLOAT3 GetPosition() const { return mPosition; }
	void SetPosition(float x, float y, float z);
	void SetPosition(const XMFLOAT3& v) { mPosition = v; }

	//get cam basis vectors
	XMVECTOR GetRightXM() const { return XMLoadFloat3(&mRight); }
	XMFLOAT3 GetRight() const { return mRight; }
	XMVECTOR GetUpXM() const { return XMLoadFloat3(&mUp); }
	XMFLOAT3 GetUp() const { return mUp; }
	XMVECTOR GetLookXM() const { return XMLoadFloat3(&mLook); }
	XMFLOAT3 GetLook() const { return mLook; }

	//get frustum properties
	float GetNearZ() const { return mNearZ; }
	float GetFarZ() const { return mFarZ; }
	float GetAspect() const { return mAspect; }
	float GetFovY() const { return mFovY; }
	float GetFovX() const;

	//get near and far plane dimensions in view space coords
	float GetNearWindowWidth() const;
	float GetNearWindowHeight() const;
	float GetFarWindowWidth() const;
	float GetFarWindowHeight() const;

	//set frustum
	void SetLens(float fovY, float aspect, float zn, float zf);

	//define camera space via LookAt parameters
	void LookAt(FXMVECTOR pos, FXMVECTOR target, FXMVECTOR worldUp);
	void LookAt(const XMFLOAT3& pos, const XMFLOAT3& target, const XMFLOAT3& up);

	//get view/proj matrices
	XMMATRIX View() const { return XMLoadFloat4x4(&mView); }
	XMMATRIX Proj() const { return XMLoadFloat4x4(&mProj); }
	XMMATRIX ViewProj() const { return XMMatrixMultiply(View(), Proj()); }

	//strafe/walk camera distance 'd'
	void Strafe(float d);
	void Walk(float d);

	//rotate camera
	void Pitch(float angle);
	void RotateY(float angle);

	//rebuild view matrix once per frame
	void UpdateViewMatrix();

private:

	//cam coords relative to world space
	XMFLOAT3 mPosition; //view space origin
	XMFLOAT3 mRight; //view space x axis
	XMFLOAT3 mUp; //view space y axis
	XMFLOAT3 mLook; //view space z axis

	//cache frustum properties
	float mNearZ;
	float mFarZ;
	float mAspect;
	float mFovY;
	float mNearWindowHeight;
	float mFarWindowHeight;

	//cache view / proj matrices
	XMFLOAT4X4 mView;
	XMFLOAT4X4 mProj;

	/*
	* private:
	//private attributes to store the camera position and view 

	XMFLOAT3 _eye;
	XMFLOAT3 _at;
	XMFLOAT3 _up;

	FLOAT _windowWidth;
	FLOAT _windowHeight;
	FLOAT _nearDepth;
	FLOAT _farDepth;

	//attributes to hold view and projection matrices to pass to the shader
	XMFLOAT4X4 _view;
	XMFLOAT4X4 _projection;


	//update function for current view/projection matrices
	void Update();

	//set and return position, lookat, up
	//get
	XMFLOAT3 GetPos();
	XMFLOAT3 GetLookAt();
	XMFLOAT3 GetUp();
	//set
	void SetPos(XMFLOAT3 newPos);
	void SetLookAt(XMFLOAT3 newLookAt);
	void SetUp(XMFLOAT3 newUp);

	//get view and projection matrices
	XMMATRIX GetViewMatrix();
	XMMATRIX GetProjMatrix();
	*/
};

