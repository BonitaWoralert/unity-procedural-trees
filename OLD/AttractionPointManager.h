#pragma once
#include <directxmath.h>
#include <vector>

using namespace DirectX;

struct Point
{
	XMFLOAT4X4 _matrix;
	XMFLOAT3 _position;
};

class AttractionPointManager
{
private:
	float influenceDist;
	float killDist;
	std::vector<Point> _attractionPoints;
public:
	AttractionPointManager();
	~AttractionPointManager();

	void FindNearestBranch();
	bool EnteredKillDist(XMFLOAT4X4 point, XMFLOAT3 branchPosition); //check if branch is in range, if true then delete point
	bool EnteredInfluenceDist();

	void AddPoint(XMFLOAT4X4 matrix, XMFLOAT3 position);
	Point GetPoint(int i) { return _attractionPoints[i]; }
};

