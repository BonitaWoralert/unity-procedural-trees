#pragma once
#include <directxmath.h>
#include <vector>

using namespace DirectX;

class AttractionPointManager
{
private:
	float influenceDist;
	float killDist;
public:
	AttractionPointManager();
	~AttractionPointManager();

	std::vector<XMFLOAT4X4> _points;

	bool EnteredKillDist(XMFLOAT4X4 point, XMFLOAT3 branchPosition); //check if branch is in range, if true then delete point
	bool EnteredInfluenceDist();
	void CreatePoints();
};

